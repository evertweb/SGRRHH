using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using SGRRHH.Core.Interfaces;
using SGRRHH.Core.Models;

namespace SGRRHH.Infrastructure.Services;

public class GithubUpdateService : IUpdateService
{
    private readonly ILogger<GithubUpdateService>? _logger;
    private readonly string _currentVersion;
    private readonly HttpClient _httpClient;
    private readonly string _tempUpdatePath;
    private readonly string _installPath;
    
    // Configuración del repositorio
    private const string REPO_OWNER = "evertweb";
    private const string REPO_NAME = "SGRRHH";
    
    private GithubRelease? _pendingUpdate;
    
    // Implementación de propiedades de IUpdateService
    public bool HasPendingUpdate => _pendingUpdate != null && Directory.Exists(Path.Combine(_tempUpdatePath, "extracted"));
    public string UpdatesPath => _tempUpdatePath;
    public event EventHandler<VersionInfo>? UpdateAvailable;

    public GithubUpdateService(string currentVersion, ILogger<GithubUpdateService>? logger = null)
    {
        _currentVersion = currentVersion;
        _logger = logger;
        _installPath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        _tempUpdatePath = Path.Combine(Path.GetTempPath(), "SGRRHH_update_temp");
        
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("SGRRHH-Updater");
    }

    public string GetCurrentVersion() => _currentVersion;

    public async Task<UpdateCheckResult> CheckForUpdatesAsync()
    {
        var result = new UpdateCheckResult
        {
            CurrentVersion = _currentVersion,
            Success = false
        };

        try
        {
            _logger?.LogInformation("Verificando actualizaciones en GitHub...");
            
            var url = $"https://api.github.com/repos/{REPO_OWNER}/{REPO_NAME}/releases/latest";
            var release = await _httpClient.GetFromJsonAsync<GithubRelease>(url);

            if (release == null)
            {
                result.Success = true;
                result.UpdateAvailable = false;
                return result;
            }

            var serverVersionStr = release.TagName.TrimStart('v', 'V');
            var currentVer = ParseVersion(_currentVersion);
            var serverVer = ParseVersion(serverVersionStr);

            _logger?.LogInformation($"Versión actual: {_currentVersion}, Versión GitHub: {serverVersionStr}");

            if (serverVer > currentVer)
            {
                result.Success = true;
                result.UpdateAvailable = true;
                result.NewVersion = new VersionInfo 
                { 
                    Version = serverVersionStr,
                    ReleaseNotes = release.Body,
                    ReleaseDate = release.PublishedAt
                };
                _pendingUpdate = release;
                
                // Disparar evento
                UpdateAvailable?.Invoke(this, result.NewVersion);
            }
            else
            {
                result.Success = true;
                result.UpdateAvailable = false;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error verificando actualizaciones en GitHub");
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    public async Task<bool> DownloadUpdateAsync(IProgress<UpdateProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        if (_pendingUpdate == null) return false;

        try
        {
            // Buscar el asset .zip
            var asset = _pendingUpdate.Assets.FirstOrDefault(a => a.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase));
            if (asset == null)
            {
                _logger?.LogError("No se encontró un archivo .zip en el release.");
                return false;
            }

            _logger?.LogInformation($"Descargando actualización desde: {asset.BrowserDownloadUrl}");

            if (Directory.Exists(_tempUpdatePath)) Directory.Delete(_tempUpdatePath, true);
            Directory.CreateDirectory(_tempUpdatePath);

            string zipPath = Path.Combine(_tempUpdatePath, "update.zip");

            // Descargar
            using (var response = await _httpClient.GetAsync(asset.BrowserDownloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                response.EnsureSuccessStatusCode();
                var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                var canReportProgress = totalBytes != -1 && progress != null;

                using (var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken))
                using (var fileStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                {
                    var buffer = new byte[8192];
                    long totalRead = 0;
                    int read;

                    while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, read, cancellationToken);
                        totalRead += read;

                        if (canReportProgress)
                        {
                            progress?.Report(new UpdateProgress 
                            { 
                                Phase = UpdatePhase.Downloading, 
                                Percentage = (int)((totalRead * 100) / totalBytes),
                                TotalBytes = totalBytes,
                                BytesTransferred = totalRead
                            });
                        }
                    }
                }
            }

            // Verificar integridad del archivo descargado
            progress?.Report(new UpdateProgress { Phase = UpdatePhase.Verifying, Percentage = 0, Message = "Verificando integridad del archivo..." });

            var checksum = CalculateFileSha256(zipPath);
            _logger?.LogInformation($"SHA256 del archivo descargado: {checksum}");

            // Si hay un checksum esperado en el VersionInfo, verificarlo
            if (_pendingUpdate != null && !string.IsNullOrEmpty(_pendingUpdate.Body))
            {
                // Buscar checksum en las notas de versión (formato: SHA256: xxxxx)
                var checksumMatch = System.Text.RegularExpressions.Regex.Match(
                    _pendingUpdate.Body,
                    @"SHA256:\s*([a-fA-F0-9]{64})",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );

                if (checksumMatch.Success)
                {
                    var expectedChecksum = checksumMatch.Groups[1].Value.ToLowerInvariant();
                    if (!VerifyFileIntegrity(zipPath, expectedChecksum))
                    {
                        _logger?.LogError("La verificación de integridad falló. El archivo puede estar corrupto o modificado.");
                        progress?.Report(new UpdateProgress
                        {
                            Phase = UpdatePhase.Error,
                            Percentage = 0,
                            Message = "Error de integridad: el archivo descargado no es válido"
                        });
                        return false;
                    }
                }
                else
                {
                    _logger?.LogWarning("No se encontró checksum en las notas de versión");
                }
            }

            // Extraer
            progress?.Report(new UpdateProgress { Phase = UpdatePhase.Verifying, Percentage = 50, Message = "Extrayendo archivos..." });
            string extractPath = Path.Combine(_tempUpdatePath, "extracted");

            // Limpiar carpeta si existe
            if (Directory.Exists(extractPath))
            {
                Directory.Delete(extractPath, true);
            }
            Directory.CreateDirectory(extractPath);

            System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, extractPath);

            _logger?.LogInformation($"Archivos extraídos en: {extractPath}");
            _logger?.LogInformation($"Total de archivos: {Directory.GetFiles(extractPath, "*.*", SearchOption.AllDirectories).Length}");

            progress?.Report(new UpdateProgress { Phase = UpdatePhase.Completed, Percentage = 100, Message = "Descarga completada y verificada" });
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error descargando actualización");
            return false;
        }
    }

    public async Task<bool> ApplyUpdateAsync()
    {
        try
        {
            string sourceDir = Path.Combine(_tempUpdatePath, "extracted");

            // Verificar que exista la carpeta de actualización
            if (!Directory.Exists(sourceDir))
            {
                _logger?.LogError($"No existe el directorio de actualización: {sourceDir}");
                return false;
            }

            // Buscar el updater primero en la descarga (más confiable)
            string downloadedUpdater = Path.Combine(sourceDir, "SGRRHH.Updater.exe");
            string updaterPath = Path.Combine(_installPath, "SGRRHH.Updater.exe");

            _logger?.LogInformation($"Buscando updater en descarga: {downloadedUpdater}");
            _logger?.LogInformation($"Ruta de instalación: {updaterPath}");

            if (!File.Exists(downloadedUpdater))
            {
                _logger?.LogError($"No se encuentra SGRRHH.Updater.exe en la descarga: {downloadedUpdater}");
                _logger?.LogError($"Archivos en extracted: {string.Join(", ", Directory.GetFiles(sourceDir).Select(Path.GetFileName))}");
                return false;
            }

            // Copiar el updater a la instalación actual (sobrescribir si existe)
            try
            {
                _logger?.LogInformation($"Copiando updater desde {downloadedUpdater} a {updaterPath}");
                File.Copy(downloadedUpdater, updaterPath, overwrite: true);
                _logger?.LogInformation("Updater copiado exitosamente");
            }
            catch (Exception copyEx)
            {
                _logger?.LogError(copyEx, $"Error copiando updater: {copyEx.Message}");
                return false;
            }

            int pid = Process.GetCurrentProcess().Id;
            string exeName = "SGRRHH.exe"; // Nombre del ejecutable principal

            var startInfo = new ProcessStartInfo
            {
                FileName = updaterPath,
                Arguments = $"\"{_installPath}\" \"{sourceDir}\" \"{exeName}\" {pid}",
                UseShellExecute = true,
                WorkingDirectory = _installPath
            };

            _logger?.LogInformation($"Lanzando updater: {updaterPath}");
            _logger?.LogInformation($"Argumentos: {startInfo.Arguments}");

            var process = Process.Start(startInfo);
            if (process == null)
            {
                _logger?.LogError("No se pudo iniciar el proceso del updater");
                return false;
            }

            _logger?.LogInformation($"Updater lanzado exitosamente (PID: {process.Id})");
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Error aplicando actualización: {ex.Message}");
            return false;
        }
    }

    private Version ParseVersion(string versionString)
    {
        if (string.IsNullOrWhiteSpace(versionString)) return new Version(0, 0, 0);
        var cleaned = versionString.TrimStart('v', 'V').Split('-')[0];
        return Version.TryParse(cleaned, out var v) ? v : new Version(0, 0, 0);
    }

    /// <summary>
    /// Calcula el hash SHA256 de un archivo
    /// </summary>
    private string CalculateFileSha256(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hash = sha256.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    /// <summary>
    /// Verifica la integridad del archivo descargado contra un checksum esperado
    /// </summary>
    private bool VerifyFileIntegrity(string filePath, string? expectedChecksum)
    {
        if (string.IsNullOrWhiteSpace(expectedChecksum))
        {
            _logger?.LogWarning("No hay checksum para verificar, omitiendo validación de integridad");
            return true; // No hay checksum que verificar
        }

        try
        {
            var actualChecksum = CalculateFileSha256(filePath);
            var isValid = actualChecksum.Equals(expectedChecksum, StringComparison.OrdinalIgnoreCase);

            if (isValid)
            {
                _logger?.LogInformation($"✓ Integridad verificada: {actualChecksum}");
            }
            else
            {
                _logger?.LogError($"✗ Checksum no coincide!");
                _logger?.LogError($"  Esperado: {expectedChecksum}");
                _logger?.LogError($"  Obtenido: {actualChecksum}");
            }

            return isValid;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error verificando integridad del archivo");
            return false;
        }
    }

    // Clases para deserializar respuesta de GitHub
    private class GithubRelease
    {
        [JsonPropertyName("tag_name")]
        public string TagName { get; set; } = "";
        
        [JsonPropertyName("body")]
        public string Body { get; set; } = "";
        
        [JsonPropertyName("published_at")]
        public DateTime PublishedAt { get; set; }
        
        [JsonPropertyName("assets")]
        public List<GithubAsset> Assets { get; set; } = new();
    }

    private class GithubAsset
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";
        
        [JsonPropertyName("browser_download_url")]
        public string BrowserDownloadUrl { get; set; } = "";
    }
}
