using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Logging;
using SGRRHH.Core.Interfaces;
using SGRRHH.Core.Models;

namespace SGRRHH.Infrastructure.Firebase;

/// <summary>
/// Servicio para gestionar actualizaciones automáticas de la aplicación
/// via Firebase Storage.
/// 
/// Estructura en Firebase Storage:
/// gs://{bucket}/updates/
/// ├── version.json          # Información de la última versión
/// └── latest/
///     ├── SGRRHH.exe
///     ├── SGRRHH.dll
///     ├── SGRRHH.deps.json
///     ├── SGRRHH.runtimeconfig.json
///     └── ... (otros archivos)
/// </summary>
public class FirebaseUpdateService : IFirebaseUpdateService
{
    private readonly FirebaseInitializer _firebase;
    private readonly ILogger<FirebaseUpdateService>? _logger;
    private readonly string _currentVersion;
    private readonly string _installPath;
    private readonly string _tempUpdatePath;
    private readonly string _storageBucket;
    private readonly HttpClient _httpClient;
    
    private VersionInfo? _pendingUpdate;
    
    // Rutas en Firebase Storage
    private const string UPDATES_BASE_PATH = "updates";
    private const string VERSION_FILE = "updates/version.json";
    private const string LATEST_FOLDER = "updates/latest/";
    
    /// <summary>
    /// Evento que se dispara cuando se detecta una nueva versión
    /// </summary>
    public event EventHandler<VersionInfo>? UpdateAvailable;
    
    /// <summary>
    /// Indica si hay una actualización descargada lista para instalar
    /// </summary>
    public bool HasPendingUpdate => _pendingUpdate != null && 
        Directory.Exists(_tempUpdatePath) && 
        Directory.GetFiles(_tempUpdatePath).Length > 0;
    
    /// <summary>
    /// Indica si el servicio está usando Firebase Storage
    /// </summary>
    public bool IsUsingFirebaseStorage => _firebase.IsInitialized;
    
    /// <summary>
    /// URL base del bucket de Firebase Storage
    /// </summary>
    public string StorageBucketUrl => $"gs://{_storageBucket}";
    
    /// <summary>
    /// Ruta donde se descargan las actualizaciones (local temporal)
    /// </summary>
    public string UpdatesPath => _tempUpdatePath;
    
    public FirebaseUpdateService(
        FirebaseInitializer firebase, 
        string currentVersion,
        ILogger<FirebaseUpdateService>? logger = null)
    {
        _firebase = firebase ?? throw new ArgumentNullException(nameof(firebase));
        _currentVersion = currentVersion ?? "1.0.0";
        _logger = logger;
        _storageBucket = firebase.Config.StorageBucket;
        _installPath = AppDomain.CurrentDomain.BaseDirectory;
        _tempUpdatePath = Path.Combine(_installPath, "update_temp");
        
        // HttpClient para descargas con URLs públicas
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(10) // Timeout largo para archivos grandes
        };
    }
    
    private StorageClient Storage => _firebase.Storage 
        ?? throw new InvalidOperationException("Firebase Storage not initialized");
    
    #region IUpdateService Implementation
    
    /// <inheritdoc/>
    public string GetCurrentVersion() => _currentVersion;
    
    /// <inheritdoc/>
    public async Task<UpdateCheckResult> CheckForUpdatesAsync()
    {
        var result = new UpdateCheckResult
        {
            CurrentVersion = _currentVersion,
            Success = false
        };
        
        try
        {
            if (!_firebase.IsInitialized)
            {
                result.ErrorMessage = "Firebase no está inicializado";
                return result;
            }
            
            _logger?.LogInformation("Verificando actualizaciones en Firebase Storage...");
            
            // Descargar version.json desde Firebase Storage
            var serverVersion = await GetRemoteVersionInfoAsync();
            
            if (serverVersion == null)
            {
                // No hay archivo de versión, no hay actualizaciones disponibles
                result.Success = true;
                result.UpdateAvailable = false;
                _logger?.LogInformation("No hay actualizaciones disponibles");
                return result;
            }
            
            result.Success = true;
            
            // Comparar versiones
            var currentVer = ParseVersion(_currentVersion);
            var serverVer = ParseVersion(serverVersion.Version);
            
            _logger?.LogInformation("Versión actual: {Current}, Versión disponible: {Server}", 
                _currentVersion, serverVersion.Version);
            
            if (serverVer > currentVer)
            {
                result.UpdateAvailable = true;
                result.NewVersion = serverVersion;
                _pendingUpdate = serverVersion;
                
                _logger?.LogInformation("Nueva versión disponible: {Version}", serverVersion.Version);
                
                // Notificar que hay actualización disponible
                UpdateAvailable?.Invoke(this, serverVersion);
            }
            else
            {
                result.UpdateAvailable = false;
                _logger?.LogInformation("La aplicación está actualizada");
            }
            
            return result;
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // No hay archivo de versión
            result.Success = true;
            result.UpdateAvailable = false;
            _logger?.LogInformation("No se encontró archivo de versión en Firebase Storage");
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al verificar actualizaciones");
            result.ErrorMessage = $"Error al verificar actualizaciones: {ex.Message}";
            return result;
        }
    }
    
    /// <inheritdoc/>
    public async Task<bool> DownloadUpdateAsync(IProgress<UpdateProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        if (_pendingUpdate == null)
        {
            _logger?.LogWarning("No hay actualización pendiente para descargar");
            return false;
        }
        
        try
        {
            _logger?.LogInformation("Iniciando descarga de actualización versión {Version}", _pendingUpdate.Version);
            
            // Crear carpeta temporal para la descarga
            if (Directory.Exists(_tempUpdatePath))
            {
                Directory.Delete(_tempUpdatePath, true);
            }
            Directory.CreateDirectory(_tempUpdatePath);
            
            ReportProgress(progress, UpdatePhase.Downloading, 0, "Obteniendo lista de archivos...");
            
            // Obtener lista de archivos desde Firebase Storage
            var files = await GetFilesFromStorageAsync();
            
            if (!files.Any())
            {
                ReportProgress(progress, UpdatePhase.Error, 0, "No se encontraron archivos para descargar");
                _logger?.LogError("No se encontraron archivos en la carpeta de actualización");
                return false;
            }
            
            var totalBytes = files.Sum(f => f.Size);
            long downloadedBytes = 0;
            int fileIndex = 0;
            
            _logger?.LogInformation("Descargando {Count} archivos ({Size} bytes)", files.Count, totalBytes);
            
            foreach (var file in files)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger?.LogWarning("Descarga cancelada por el usuario");
                    CleanupTempFolder();
                    return false;
                }
                
                var relativePath = file.Name;
                var localPath = Path.Combine(_tempUpdatePath, relativePath);
                
                // Crear directorio si no existe
                var localDir = Path.GetDirectoryName(localPath);
                if (!string.IsNullOrEmpty(localDir) && !Directory.Exists(localDir))
                {
                    Directory.CreateDirectory(localDir);
                }
                
                // No copiar appsettings.json (mantener configuración local)
                if (Path.GetFileName(relativePath).Equals("appsettings.json", StringComparison.OrdinalIgnoreCase))
                {
                    fileIndex++;
                    continue;
                }
                
                var percentage = totalBytes > 0 ? (int)((downloadedBytes * 100) / totalBytes) : 0;
                ReportProgress(progress, UpdatePhase.Downloading, percentage, 
                    $"Descargando: {relativePath}", relativePath, downloadedBytes, totalBytes);
                
                // Descargar archivo desde Firebase Storage
                var storageFilePath = $"{LATEST_FOLDER}{relativePath}";
                await DownloadStorageFileAsync(storageFilePath, localPath, cancellationToken);
                
                downloadedBytes += file.Size;
                fileIndex++;
                
                _logger?.LogDebug("Descargado: {File} ({Index}/{Total})", relativePath, fileIndex, files.Count);
            }
            
            // Verificar integridad si hay checksums
            ReportProgress(progress, UpdatePhase.Verifying, 100, "Verificando archivos...");
            
            if (_pendingUpdate.Files.Count > 0)
            {
                foreach (var file in _pendingUpdate.Files)
                {
                    if (cancellationToken.IsCancellationRequested) return false;
                    
                    var localFile = Path.Combine(_tempUpdatePath, file.Name);
                    if (File.Exists(localFile) && !string.IsNullOrEmpty(file.Checksum))
                    {
                        var hash = await ComputeFileHashAsync(localFile);
                        if (!hash.Equals(file.Checksum, StringComparison.OrdinalIgnoreCase))
                        {
                            ReportProgress(progress, UpdatePhase.Error, 0, $"Error de integridad: {file.Name}");
                            _logger?.LogError("Verificación de integridad fallida para: {File}", file.Name);
                            CleanupTempFolder();
                            return false;
                        }
                    }
                }
            }
            
            ReportProgress(progress, UpdatePhase.Completed, 100, "Descarga completada");
            _logger?.LogInformation("Descarga de actualización completada exitosamente");
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al descargar actualización");
            ReportProgress(progress, UpdatePhase.Error, 0, $"Error: {ex.Message}");
            CleanupTempFolder();
            return false;
        }
    }
    
    /// <inheritdoc/>
    public async Task<bool> ApplyUpdateAsync()
    {
        if (!HasPendingUpdate)
        {
            _logger?.LogWarning("No hay actualización pendiente para aplicar");
            return false;
        }
        
        try
        {
            _logger?.LogInformation("Preparando aplicación de actualización...");
            
            // Crear script de actualización que se ejecutará después de cerrar la app
            var updateScript = CreateUpdateScript();
            
            if (string.IsNullOrEmpty(updateScript))
            {
                _logger?.LogError("No se pudo crear el script de actualización");
                return false;
            }
            
            // Guardar la ruta del script para que App.xaml.cs lo ejecute al cerrar
            await File.WriteAllTextAsync(
                Path.Combine(_installPath, "pending_update.txt"),
                updateScript
            );
            
            _logger?.LogInformation("Script de actualización creado: {Script}", updateScript);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al preparar la actualización");
            return false;
        }
    }
    
    #endregion
    
    #region IFirebaseUpdateService Implementation
    
    /// <inheritdoc/>
    public async Task<bool> TestFirebaseConnectionAsync()
    {
        try
        {
            if (!_firebase.IsInitialized)
            {
                return false;
            }
            
            // Intentar listar archivos en la carpeta de actualizaciones
            var objects = Storage.ListObjects(_storageBucket, UPDATES_BASE_PATH);
            
            // Verificar que hay al menos un objeto (aunque sea solo el prefijo)
            await Task.Run(() => objects.Take(1).ToList());
            
            _logger?.LogInformation("Conexión con Firebase Storage verificada");
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "No se pudo verificar conexión con Firebase Storage");
            return false;
        }
    }
    
    /// <inheritdoc/>
    public async Task<VersionInfo?> GetRemoteVersionInfoAsync()
    {
        try
        {
            using var memoryStream = new MemoryStream();
            await Storage.DownloadObjectAsync(_storageBucket, VERSION_FILE, memoryStream);
            
            memoryStream.Position = 0;
            using var reader = new StreamReader(memoryStream);
            var json = await reader.ReadToEndAsync();
            
            var versionInfo = JsonSerializer.Deserialize<VersionInfo>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            _logger?.LogDebug("VersionInfo descargado: {Version}", versionInfo?.Version);
            return versionInfo;
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger?.LogInformation("No se encontró version.json en Firebase Storage");
            return null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener información de versión remota");
            return null;
        }
    }
    
    /// <inheritdoc/>
    public async Task<bool> DownloadFileAsync(string remoteFileName, string localPath, 
        IProgress<UpdateProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var storagePath = $"{LATEST_FOLDER}{remoteFileName}";
            
            ReportProgress(progress, UpdatePhase.Downloading, 0, $"Descargando: {remoteFileName}");
            
            await DownloadStorageFileAsync(storagePath, localPath, cancellationToken);
            
            ReportProgress(progress, UpdatePhase.Completed, 100, "Descarga completada");
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al descargar archivo: {File}", remoteFileName);
            ReportProgress(progress, UpdatePhase.Error, 0, $"Error: {ex.Message}");
            return false;
        }
    }
    
    /// <inheritdoc/>
    public async Task<IEnumerable<UpdateFile>> GetAvailableFilesAsync()
    {
        try
        {
            var files = await GetFilesFromStorageAsync();
            return files.Select(f => new UpdateFile
            {
                Name = f.Name,
                Size = f.Size
            });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener lista de archivos disponibles");
            return Enumerable.Empty<UpdateFile>();
        }
    }
    
    /// <inheritdoc/>
    public Task<int> CleanupTempFilesAsync()
    {
        return Task.Run(() =>
        {
            int count = 0;
            
            try
            {
                // Limpiar carpeta temporal principal
                if (Directory.Exists(_tempUpdatePath))
                {
                    var files = Directory.GetFiles(_tempUpdatePath, "*", SearchOption.AllDirectories);
                    count = files.Length;
                    Directory.Delete(_tempUpdatePath, true);
                    _logger?.LogInformation("Limpiados {Count} archivos temporales", count);
                }
                
                // Limpiar backups antiguos (más de 7 días)
                var backupPattern = "backup_*";
                var backupDirs = Directory.GetDirectories(_installPath, backupPattern);
                foreach (var backupDir in backupDirs)
                {
                    var dirInfo = new DirectoryInfo(backupDir);
                    if (dirInfo.CreationTime < DateTime.Now.AddDays(-7))
                    {
                        var backupFiles = Directory.GetFiles(backupDir, "*", SearchOption.AllDirectories);
                        count += backupFiles.Length;
                        Directory.Delete(backupDir, true);
                        _logger?.LogInformation("Eliminado backup antiguo: {Backup}", backupDir);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error al limpiar archivos temporales");
            }
            
            return count;
        });
    }
    
    /// <inheritdoc/>
    public async Task<bool> HasMandatoryUpdateAsync()
    {
        try
        {
            var versionInfo = await GetRemoteVersionInfoAsync();
            
            if (versionInfo == null || !versionInfo.Mandatory)
            {
                return false;
            }
            
            // Verificar si la versión actual es menor a la mínima requerida
            var currentVer = ParseVersion(_currentVersion);
            var minimumVer = ParseVersion(versionInfo.MinimumVersion ?? "1.0.0");
            
            return currentVer < minimumVer;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al verificar actualización obligatoria");
            return false;
        }
    }
    
    #endregion
    
    #region Helper Methods
    
    /// <summary>
    /// Obtiene la lista de archivos desde Firebase Storage
    /// </summary>
    private async Task<List<StorageFileMetadata>> GetFilesFromStorageAsync()
    {
        var files = new List<StorageFileMetadata>();
        
        try
        {
            var objects = Storage.ListObjects(_storageBucket, LATEST_FOLDER);
            
            await Task.Run(() =>
            {
                foreach (var obj in objects)
                {
                    // Ignorar el "directorio" en sí
                    if (obj.Name.Equals(LATEST_FOLDER, StringComparison.OrdinalIgnoreCase))
                        continue;
                    
                    // Obtener nombre relativo
                    var relativeName = obj.Name;
                    if (relativeName.StartsWith(LATEST_FOLDER))
                    {
                        relativeName = relativeName.Substring(LATEST_FOLDER.Length);
                    }
                    
                    // Ignorar entradas vacías (subdirectorios)
                    if (string.IsNullOrWhiteSpace(relativeName) || relativeName.EndsWith("/"))
                        continue;
                    
                    files.Add(new StorageFileMetadata
                    {
                        Name = relativeName,
                        FullPath = obj.Name,
                        Size = (long)(obj.Size ?? 0),
                        ContentType = obj.ContentType ?? "application/octet-stream"
                    });
                }
            });
            
            _logger?.LogDebug("Encontrados {Count} archivos en Storage", files.Count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al listar archivos de Storage");
        }
        
        return files;
    }
    
    /// <summary>
    /// Descarga un archivo desde Firebase Storage
    /// </summary>
    private async Task DownloadStorageFileAsync(string storagePath, string localPath, CancellationToken cancellationToken)
    {
        // Crear directorio si no existe
        var directory = Path.GetDirectoryName(localPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        await using var fileStream = File.Create(localPath);
        await Storage.DownloadObjectAsync(_storageBucket, storagePath, fileStream, cancellationToken: cancellationToken);
    }
    
    /// <summary>
    /// Crea el script PowerShell para aplicar la actualización
    /// </summary>
    private string? CreateUpdateScript()
    {
        try
        {
            var scriptPath = Path.Combine(_installPath, "update.ps1");
            var exePath = Path.Combine(_installPath, "SGRRHH.exe");
            
            var script = $@"
# Script de actualización automática SGRRHH (Firebase)
# Generado automáticamente - No editar
# Versión: {_pendingUpdate?.Version ?? "desconocida"}

$ErrorActionPreference = 'Stop'

# Esperar a que la aplicación se cierre completamente
Write-Host 'Esperando a que SGRRHH se cierre...'
Start-Sleep -Seconds 3

# Intentar hasta 10 veces
$maxRetries = 10
$retry = 0

while ($retry -lt $maxRetries) {{
    $process = Get-Process -Name 'SGRRHH' -ErrorAction SilentlyContinue
    if ($null -eq $process) {{
        break
    }}
    Write-Host 'Aplicación aún en ejecución, esperando...'
    Start-Sleep -Seconds 2
    $retry++
}}

if ($retry -ge $maxRetries) {{
    Write-Host 'ERROR: No se pudo cerrar la aplicación'
    exit 1
}}

Write-Host 'Aplicando actualización desde Firebase...'

# Crear backup de la versión actual
$backupPath = '{EscapeForPowerShell(_installPath)}\backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')'
New-Item -Path $backupPath -ItemType Directory -Force | Out-Null

# Copiar archivos actuales a backup (excepto carpetas de datos y temporales)
Get-ChildItem -Path '{EscapeForPowerShell(_installPath)}' -File | 
    Where-Object {{ $_.Name -notmatch '^(update|backup|pending_update|data)' }} |
    ForEach-Object {{ Copy-Item $_.FullName -Destination $backupPath -Force }}

Write-Host 'Backup creado en: $backupPath'

# Copiar nuevos archivos
$sourcePath = '{EscapeForPowerShell(_tempUpdatePath)}'
$destPath = '{EscapeForPowerShell(_installPath)}'

Write-Host 'Copiando archivos nuevos...'
Copy-Item -Path ""$sourcePath\*"" -Destination $destPath -Recurse -Force -Exclude 'appsettings.json'

# Limpiar carpeta temporal
Write-Host 'Limpiando archivos temporales...'
Remove-Item -Path $sourcePath -Recurse -Force -ErrorAction SilentlyContinue

# Eliminar archivo de actualización pendiente
Remove-Item -Path '{EscapeForPowerShell(_installPath)}pending_update.txt' -Force -ErrorAction SilentlyContinue

Write-Host 'Actualización completada. Reiniciando aplicación...'

# Reiniciar la aplicación
Start-Process -FilePath '{EscapeForPowerShell(exePath)}'

# Auto-eliminar este script
Remove-Item -Path $MyInvocation.MyCommand.Path -Force -ErrorAction SilentlyContinue
";
            
            File.WriteAllText(scriptPath, script);
            return scriptPath;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al crear script de actualización");
            return null;
        }
    }
    
    /// <summary>
    /// Escapa una cadena para uso seguro en PowerShell
    /// </summary>
    private static string EscapeForPowerShell(string path)
    {
        return path.Replace("\\", "\\\\");
    }
    
    /// <summary>
    /// Calcula el hash SHA256 de un archivo
    /// </summary>
    private async Task<string> ComputeFileHashAsync(string filePath)
    {
        await using var stream = File.OpenRead(filePath);
        var hash = await SHA256.HashDataAsync(stream);
        return $"sha256:{Convert.ToHexString(hash).ToLowerInvariant()}";
    }
    
    /// <summary>
    /// Limpia la carpeta temporal de actualización
    /// </summary>
    private void CleanupTempFolder()
    {
        try
        {
            if (Directory.Exists(_tempUpdatePath))
            {
                Directory.Delete(_tempUpdatePath, true);
                _logger?.LogInformation("Carpeta temporal de actualización eliminada");
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error al limpiar carpeta temporal");
        }
    }
    
    /// <summary>
    /// Reporta progreso de la actualización
    /// </summary>
    private void ReportProgress(IProgress<UpdateProgress>? progress, UpdatePhase phase, int percentage, 
        string message, string? currentFile = null, long bytesTransferred = 0, long totalBytes = 0)
    {
        progress?.Report(new UpdateProgress
        {
            Phase = phase,
            Percentage = percentage,
            Message = message,
            CurrentFile = currentFile,
            BytesTransferred = bytesTransferred,
            TotalBytes = totalBytes
        });
    }
    
    /// <summary>
    /// Parsea una versión semántica a un objeto Version comparable
    /// </summary>
    private static Version ParseVersion(string versionString)
    {
        if (string.IsNullOrWhiteSpace(versionString))
            return new Version(1, 0, 0);
            
        // Limpiar el string de versión
        var cleaned = versionString.TrimStart('v', 'V');
        
        // Separar por guión para eliminar sufijos como -beta, -rc, etc.
        var parts = cleaned.Split('-')[0];
        
        if (Version.TryParse(parts, out var version))
        {
            return version;
        }
        
        return new Version(1, 0, 0);
    }
    
    /// <summary>
    /// Ejecuta el script de actualización pendiente (método estático para uso externo)
    /// </summary>
    public static void ExecutePendingUpdate()
    {
        var installPath = AppDomain.CurrentDomain.BaseDirectory;
        var pendingFile = Path.Combine(installPath, "pending_update.txt");
        
        if (File.Exists(pendingFile))
        {
            try
            {
                var scriptPath = File.ReadAllText(pendingFile).Trim();
                
                if (File.Exists(scriptPath))
                {
                    // Ejecutar script de actualización en proceso separado
                    var startInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-ExecutionPolicy Bypass -WindowStyle Hidden -File \"{scriptPath}\"",
                        UseShellExecute = true,
                        CreateNoWindow = true,
                        WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                    };
                    
                    System.Diagnostics.Process.Start(startInfo);
                }
            }
            catch
            {
                // Si falla, eliminar el archivo pendiente para no quedar en loop
                try { File.Delete(pendingFile); } catch { }
            }
        }
    }
    
    #endregion
    
    /// <summary>
    /// Metadatos de un archivo en Firebase Storage
    /// </summary>
    private class StorageFileMetadata
    {
        public string Name { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public long Size { get; set; }
        public string ContentType { get; set; } = string.Empty;
    }
}
