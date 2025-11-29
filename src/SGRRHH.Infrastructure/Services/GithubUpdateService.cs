using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Json;
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

            // Extraer
            progress?.Report(new UpdateProgress { Phase = UpdatePhase.Verifying, Percentage = 0, Message = "Extrayendo archivos..." });
            string extractPath = Path.Combine(_tempUpdatePath, "extracted");
            Directory.CreateDirectory(extractPath);
            
            System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, extractPath);
            
            // Mover archivos extraídos a la raíz de temp si están dentro de una carpeta (común en zips de github source, pero aquí es release asset)
            // Asumimos que el zip tiene los archivos planos o estructura correcta.
            
            progress?.Report(new UpdateProgress { Phase = UpdatePhase.Completed, Percentage = 100, Message = "Listo para instalar" });
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
            string updaterPath = Path.Combine(_installPath, "SGRRHH.Updater.exe");
            if (!File.Exists(updaterPath))
            {
                // Intentar buscarlo en la actualización descargada si no existe en la instalación actual (primera vez)
                string downloadedUpdater = Path.Combine(_tempUpdatePath, "extracted", "SGRRHH.Updater.exe");
                if (File.Exists(downloadedUpdater))
                {
                    File.Copy(downloadedUpdater, updaterPath, true);
                }
                else
                {
                    _logger?.LogError("No se encuentra SGRRHH.Updater.exe");
                    return false;
                }
            }

            string sourceDir = Path.Combine(_tempUpdatePath, "extracted");
            int pid = Process.GetCurrentProcess().Id;
            string exeName = "SGRRHH.exe"; // Nombre del ejecutable principal

            var startInfo = new ProcessStartInfo
            {
                FileName = updaterPath,
                Arguments = $"\"{_installPath}\" \"{sourceDir}\" \"{exeName}\" {pid}",
                UseShellExecute = true
            };

            Process.Start(startInfo);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error aplicando actualización");
            return false;
        }
    }

    private Version ParseVersion(string versionString)
    {
        if (string.IsNullOrWhiteSpace(versionString)) return new Version(0, 0, 0);
        var cleaned = versionString.TrimStart('v', 'V').Split('-')[0];
        return Version.TryParse(cleaned, out var v) ? v : new Version(0, 0, 0);
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
