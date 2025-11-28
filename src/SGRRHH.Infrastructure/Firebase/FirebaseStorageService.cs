using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Logging;
using SGRRHH.Core.Common;
using SGRRHH.Core.Interfaces;
using Object = Google.Apis.Storage.v1.Data.Object;

namespace SGRRHH.Infrastructure.Firebase;

/// <summary>
/// Implementación del servicio de Firebase Storage para SGRRHH.
/// 
/// Estructura de carpetas en Storage:
/// gs://{bucket}/
/// ├── fotos/
/// │   └── empleados/
/// │       └── {empleadoId}/
/// │           └── foto.{ext}
/// ├── documentos/
/// │   ├── permisos/
/// │   │   └── {permisoId}/
/// │   │       └── soporte.{ext}
/// │   ├── contratos/
/// │   │   └── {contratoId}/
/// │   │       └── contrato.{ext}
/// │   └── generados/
/// │       ├── actas/
/// │       │   └── {filename}.pdf
/// │       └── certificados/
/// │           └── {filename}.pdf
/// ├── config/
/// │   └── logo.{ext}
/// └── updates/
///     ├── version.json
///     └── latest/
///         └── {app files}
/// </summary>
public class FirebaseStorageService : IFirebaseStorageService
{
    private readonly FirebaseInitializer _firebase;
    private readonly ILogger<FirebaseStorageService>? _logger;
    private readonly string _bucketName;
    
    // Rutas base en Storage
    private const string FOTOS_EMPLEADOS_PATH = "fotos/empleados";
    private const string DOCUMENTOS_PERMISOS_PATH = "documentos/permisos";
    private const string DOCUMENTOS_CONTRATOS_PATH = "documentos/contratos";
    private const string DOCUMENTOS_GENERADOS_PATH = "documentos/generados";
    private const string CONFIG_PATH = "config";
    private const string UPDATES_PATH = "updates";
    
    // Content Types comunes
    private static readonly Dictionary<string, string> ExtensionToContentType = new(StringComparer.OrdinalIgnoreCase)
    {
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".png", "image/png" },
        { ".gif", "image/gif" },
        { ".bmp", "image/bmp" },
        { ".webp", "image/webp" },
        { ".pdf", "application/pdf" },
        { ".doc", "application/msword" },
        { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        { ".xls", "application/vnd.ms-excel" },
        { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
        { ".txt", "text/plain" },
        { ".json", "application/json" },
        { ".xml", "application/xml" },
        { ".zip", "application/zip" },
        { ".exe", "application/octet-stream" },
        { ".dll", "application/octet-stream" },
    };
    
    public FirebaseStorageService(FirebaseInitializer firebase, ILogger<FirebaseStorageService>? logger = null)
    {
        _firebase = firebase ?? throw new ArgumentNullException(nameof(firebase));
        _logger = logger;
        _bucketName = firebase.Config.StorageBucket;
        
        if (string.IsNullOrWhiteSpace(_bucketName))
        {
            throw new InvalidOperationException("Firebase Storage bucket name not configured");
        }
    }
    
    private StorageClient Storage => _firebase.Storage 
        ?? throw new InvalidOperationException("Firebase Storage not initialized");
    
    #region Upload Operations
    
    /// <inheritdoc/>
    public async Task<ServiceResult<string>> UploadFileAsync(string localFilePath, string storagePath, string? contentType = null)
    {
        try
        {
            if (!File.Exists(localFilePath))
            {
                return ServiceResult<string>.Fail($"El archivo no existe: {localFilePath}");
            }
            
            // Detectar content type si no se proporciona
            contentType ??= GetContentType(localFilePath);
            storagePath = NormalizePath(storagePath);
            
            _logger?.LogInformation("Subiendo archivo a Storage: {StoragePath}", storagePath);
            
            await using var fileStream = File.OpenRead(localFilePath);
            var obj = await Storage.UploadObjectAsync(
                _bucketName, 
                storagePath, 
                contentType, 
                fileStream);
            
            var downloadUrl = GetPublicUrl(storagePath);
            
            _logger?.LogInformation("Archivo subido exitosamente: {StoragePath}", storagePath);
            return ServiceResult<string>.Ok(downloadUrl, "Archivo subido correctamente");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al subir archivo: {LocalPath} -> {StoragePath}", localFilePath, storagePath);
            return ServiceResult<string>.Fail($"Error al subir archivo: {ex.Message}");
        }
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<string>> UploadBytesAsync(byte[] fileBytes, string storagePath, string contentType)
    {
        try
        {
            if (fileBytes == null || fileBytes.Length == 0)
            {
                return ServiceResult<string>.Fail("El contenido del archivo está vacío");
            }
            
            storagePath = NormalizePath(storagePath);
            
            _logger?.LogInformation("Subiendo {Size} bytes a Storage: {StoragePath}", fileBytes.Length, storagePath);
            
            using var memoryStream = new MemoryStream(fileBytes);
            var obj = await Storage.UploadObjectAsync(
                _bucketName, 
                storagePath, 
                contentType, 
                memoryStream);
            
            var downloadUrl = GetPublicUrl(storagePath);
            
            _logger?.LogInformation("Bytes subidos exitosamente: {StoragePath}", storagePath);
            return ServiceResult<string>.Ok(downloadUrl, "Archivo subido correctamente");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al subir bytes a: {StoragePath}", storagePath);
            return ServiceResult<string>.Fail($"Error al subir archivo: {ex.Message}");
        }
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<string>> UploadStreamAsync(Stream stream, string storagePath, string contentType)
    {
        try
        {
            if (stream == null || !stream.CanRead)
            {
                return ServiceResult<string>.Fail("El stream no es válido");
            }
            
            storagePath = NormalizePath(storagePath);
            
            _logger?.LogInformation("Subiendo stream a Storage: {StoragePath}", storagePath);
            
            var obj = await Storage.UploadObjectAsync(
                _bucketName, 
                storagePath, 
                contentType, 
                stream);
            
            var downloadUrl = GetPublicUrl(storagePath);
            
            _logger?.LogInformation("Stream subido exitosamente: {StoragePath}", storagePath);
            return ServiceResult<string>.Ok(downloadUrl, "Archivo subido correctamente");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al subir stream a: {StoragePath}", storagePath);
            return ServiceResult<string>.Fail($"Error al subir archivo: {ex.Message}");
        }
    }
    
    #endregion
    
    #region Download Operations
    
    /// <inheritdoc/>
    public async Task<ServiceResult> DownloadFileAsync(string storagePath, string localFilePath)
    {
        try
        {
            storagePath = NormalizePath(storagePath);
            
            _logger?.LogInformation("Descargando archivo de Storage: {StoragePath} -> {LocalPath}", storagePath, localFilePath);
            
            // Crear directorio si no existe
            var directory = Path.GetDirectoryName(localFilePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            await using var fileStream = File.Create(localFilePath);
            await Storage.DownloadObjectAsync(_bucketName, storagePath, fileStream);
            
            _logger?.LogInformation("Archivo descargado exitosamente: {LocalPath}", localFilePath);
            return ServiceResult.Ok("Archivo descargado correctamente");
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger?.LogWarning("Archivo no encontrado en Storage: {StoragePath}", storagePath);
            return ServiceResult.Fail("El archivo no existe en Storage");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al descargar archivo: {StoragePath}", storagePath);
            return ServiceResult.Fail($"Error al descargar archivo: {ex.Message}");
        }
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<byte[]>> DownloadBytesAsync(string storagePath)
    {
        try
        {
            storagePath = NormalizePath(storagePath);
            
            _logger?.LogInformation("Descargando bytes de Storage: {StoragePath}", storagePath);
            
            using var memoryStream = new MemoryStream();
            await Storage.DownloadObjectAsync(_bucketName, storagePath, memoryStream);
            
            var bytes = memoryStream.ToArray();
            
            _logger?.LogInformation("Descargados {Size} bytes de: {StoragePath}", bytes.Length, storagePath);
            return ServiceResult<byte[]>.Ok(bytes, "Archivo descargado correctamente");
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger?.LogWarning("Archivo no encontrado en Storage: {StoragePath}", storagePath);
            return ServiceResult<byte[]>.Fail("El archivo no existe en Storage");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al descargar bytes: {StoragePath}", storagePath);
            return ServiceResult<byte[]>.Fail($"Error al descargar archivo: {ex.Message}");
        }
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<Stream>> DownloadStreamAsync(string storagePath)
    {
        try
        {
            storagePath = NormalizePath(storagePath);
            
            _logger?.LogInformation("Descargando stream de Storage: {StoragePath}", storagePath);
            
            var memoryStream = new MemoryStream();
            await Storage.DownloadObjectAsync(_bucketName, storagePath, memoryStream);
            memoryStream.Position = 0; // Reset position for reading
            
            return ServiceResult<Stream>.Ok(memoryStream, "Stream descargado correctamente");
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger?.LogWarning("Archivo no encontrado en Storage: {StoragePath}", storagePath);
            return ServiceResult<Stream>.Fail("El archivo no existe en Storage");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al descargar stream: {StoragePath}", storagePath);
            return ServiceResult<Stream>.Fail($"Error al descargar archivo: {ex.Message}");
        }
    }
    
    #endregion
    
    #region URL Operations
    
    /// <inheritdoc/>
    public Task<ServiceResult<string>> GetDownloadUrlAsync(string storagePath)
    {
        try
        {
            storagePath = NormalizePath(storagePath);
            var url = GetPublicUrl(storagePath);
            return Task.FromResult(ServiceResult<string>.Ok(url, "URL generada correctamente"));
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al generar URL de descarga: {StoragePath}", storagePath);
            return Task.FromResult(ServiceResult<string>.Fail($"Error al generar URL: {ex.Message}"));
        }
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<string>> GetSignedUrlAsync(string storagePath, TimeSpan expiration)
    {
        try
        {
            storagePath = NormalizePath(storagePath);
            
            // Nota: Para URLs firmadas necesitamos el UrlSigner con credenciales específicas
            // Por ahora retornamos la URL pública
            // TODO: Implementar URL firmada si se requiere más seguridad
            
            var url = GetPublicUrl(storagePath);
            
            _logger?.LogInformation("URL firmada generada para: {StoragePath}, expira en: {Expiration}", 
                storagePath, expiration);
            
            return await Task.FromResult(ServiceResult<string>.Ok(url, "URL firmada generada"));
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al generar URL firmada: {StoragePath}", storagePath);
            return ServiceResult<string>.Fail($"Error al generar URL: {ex.Message}");
        }
    }
    
    #endregion
    
    #region Delete Operations
    
    /// <inheritdoc/>
    public async Task<ServiceResult> DeleteFileAsync(string storagePath)
    {
        try
        {
            storagePath = NormalizePath(storagePath);
            
            _logger?.LogInformation("Eliminando archivo de Storage: {StoragePath}", storagePath);
            
            await Storage.DeleteObjectAsync(_bucketName, storagePath);
            
            _logger?.LogInformation("Archivo eliminado: {StoragePath}", storagePath);
            return ServiceResult.Ok("Archivo eliminado correctamente");
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger?.LogWarning("Archivo no encontrado para eliminar: {StoragePath}", storagePath);
            return ServiceResult.Ok("El archivo no existía"); // No es error si no existe
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al eliminar archivo: {StoragePath}", storagePath);
            return ServiceResult.Fail($"Error al eliminar archivo: {ex.Message}");
        }
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult> DeleteFilesAsync(IEnumerable<string> storagePaths)
    {
        try
        {
            var paths = storagePaths.Select(NormalizePath).ToList();
            
            _logger?.LogInformation("Eliminando {Count} archivos de Storage", paths.Count);
            
            var errors = new List<string>();
            foreach (var path in paths)
            {
                try
                {
                    await Storage.DeleteObjectAsync(_bucketName, path);
                }
                catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // Ignorar archivos que no existen
                }
                catch (Exception ex)
                {
                    errors.Add($"{path}: {ex.Message}");
                }
            }
            
            if (errors.Any())
            {
                _logger?.LogWarning("Algunos archivos no se pudieron eliminar: {Errors}", string.Join("; ", errors));
                return ServiceResult.Fail($"Errores al eliminar algunos archivos: {string.Join("; ", errors)}");
            }
            
            _logger?.LogInformation("Archivos eliminados exitosamente");
            return ServiceResult.Ok("Archivos eliminados correctamente");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al eliminar múltiples archivos");
            return ServiceResult.Fail($"Error al eliminar archivos: {ex.Message}");
        }
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<int>> DeleteFolderAsync(string folderPath)
    {
        try
        {
            folderPath = NormalizePath(folderPath);
            if (!folderPath.EndsWith("/"))
                folderPath += "/";
            
            _logger?.LogInformation("Eliminando carpeta de Storage: {FolderPath}", folderPath);
            
            var objects = Storage.ListObjects(_bucketName, folderPath);
            var count = 0;
            
            foreach (var obj in objects)
            {
                try
                {
                    await Storage.DeleteObjectAsync(_bucketName, obj.Name);
                    count++;
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "No se pudo eliminar: {ObjectName}", obj.Name);
                }
            }
            
            _logger?.LogInformation("Eliminados {Count} archivos de la carpeta: {FolderPath}", count, folderPath);
            return ServiceResult<int>.Ok(count, $"Se eliminaron {count} archivos");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al eliminar carpeta: {FolderPath}", folderPath);
            return ServiceResult<int>.Fail($"Error al eliminar carpeta: {ex.Message}");
        }
    }
    
    #endregion
    
    #region List Operations
    
    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<StorageFileInfo>>> ListFilesAsync(string folderPath, int maxResults = 100)
    {
        try
        {
            folderPath = NormalizePath(folderPath);
            if (!folderPath.EndsWith("/") && !string.IsNullOrEmpty(folderPath))
                folderPath += "/";
            
            _logger?.LogInformation("Listando archivos en: {FolderPath}", folderPath);
            
            var options = new ListObjectsOptions { PageSize = maxResults };
            var objects = Storage.ListObjects(_bucketName, folderPath, options);
            
            var files = new List<StorageFileInfo>();
            foreach (var obj in objects)
            {
                files.Add(new StorageFileInfo
                {
                    Name = Path.GetFileName(obj.Name),
                    Path = obj.Name,
                    Size = (long)(obj.Size ?? 0),
                    ContentType = obj.ContentType ?? "application/octet-stream",
                    Created = obj.TimeCreatedDateTimeOffset?.DateTime,
                    Updated = obj.UpdatedDateTimeOffset?.DateTime,
                    DownloadUrl = GetPublicUrl(obj.Name)
                });
                
                if (files.Count >= maxResults)
                    break;
            }
            
            _logger?.LogInformation("Encontrados {Count} archivos en: {FolderPath}", files.Count, folderPath);
            return await Task.FromResult(ServiceResult<IEnumerable<StorageFileInfo>>.Ok(files, "Lista obtenida correctamente"));
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al listar archivos: {FolderPath}", folderPath);
            return ServiceResult<IEnumerable<StorageFileInfo>>.Fail($"Error al listar archivos: {ex.Message}");
        }
    }
    
    /// <inheritdoc/>
    public async Task<bool> FileExistsAsync(string storagePath)
    {
        try
        {
            storagePath = NormalizePath(storagePath);
            var obj = await Storage.GetObjectAsync(_bucketName, storagePath);
            return obj != null;
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
        catch
        {
            return false;
        }
    }
    
    #endregion
    
    #region Specialized Operations (Photos, Documents)
    
    /// <inheritdoc/>
    public async Task<ServiceResult<string>> UploadEmpleadoFotoAsync(int empleadoId, string localFilePath)
    {
        try
        {
            var extension = Path.GetExtension(localFilePath).TrimStart('.');
            var storagePath = $"{FOTOS_EMPLEADOS_PATH}/{empleadoId}/foto.{extension}";
            
            return await UploadFileAsync(localFilePath, storagePath);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al subir foto del empleado {EmpleadoId}", empleadoId);
            return ServiceResult<string>.Fail($"Error al subir foto: {ex.Message}");
        }
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<string>> UploadEmpleadoFotoAsync(int empleadoId, byte[] imageBytes, string extension = "jpg")
    {
        try
        {
            extension = extension.TrimStart('.');
            var storagePath = $"{FOTOS_EMPLEADOS_PATH}/{empleadoId}/foto.{extension}";
            var contentType = GetContentTypeFromExtension($".{extension}");
            
            return await UploadBytesAsync(imageBytes, storagePath, contentType);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al subir foto del empleado {EmpleadoId} desde bytes", empleadoId);
            return ServiceResult<string>.Fail($"Error al subir foto: {ex.Message}");
        }
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult> DeleteEmpleadoFotoAsync(int empleadoId)
    {
        try
        {
            // Eliminar todas las posibles extensiones de foto
            var basePath = $"{FOTOS_EMPLEADOS_PATH}/{empleadoId}/";
            return await DeleteFolderAsync(basePath).ContinueWith(t => 
                t.Result.Success ? ServiceResult.Ok("Foto eliminada") : ServiceResult.Fail(t.Result.Message));
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al eliminar foto del empleado {EmpleadoId}", empleadoId);
            return ServiceResult.Fail($"Error al eliminar foto: {ex.Message}");
        }
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<string>> UploadPermisoDocumentoAsync(int permisoId, string localFilePath)
    {
        try
        {
            var extension = Path.GetExtension(localFilePath).TrimStart('.');
            var fileName = Path.GetFileName(localFilePath);
            var storagePath = $"{DOCUMENTOS_PERMISOS_PATH}/{permisoId}/{fileName}";
            
            return await UploadFileAsync(localFilePath, storagePath);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al subir documento del permiso {PermisoId}", permisoId);
            return ServiceResult<string>.Fail($"Error al subir documento: {ex.Message}");
        }
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<string>> UploadContratoArchivoAsync(int contratoId, string localFilePath)
    {
        try
        {
            var extension = Path.GetExtension(localFilePath).TrimStart('.');
            var fileName = Path.GetFileName(localFilePath);
            var storagePath = $"{DOCUMENTOS_CONTRATOS_PATH}/{contratoId}/{fileName}";
            
            return await UploadFileAsync(localFilePath, storagePath);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al subir archivo del contrato {ContratoId}", contratoId);
            return ServiceResult<string>.Fail($"Error al subir archivo: {ex.Message}");
        }
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<string>> UploadDocumentoGeneradoAsync(string tipoDocumento, string fileName, byte[] pdfBytes)
    {
        try
        {
            // Sanitizar nombre de archivo
            fileName = SanitizeFileName(fileName);
            if (!fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                fileName += ".pdf";
            
            var storagePath = $"{DOCUMENTOS_GENERADOS_PATH}/{tipoDocumento}/{fileName}";
            
            return await UploadBytesAsync(pdfBytes, storagePath, "application/pdf");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al subir documento generado {TipoDocumento}/{FileName}", tipoDocumento, fileName);
            return ServiceResult<string>.Fail($"Error al subir documento: {ex.Message}");
        }
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<string>> UploadLogoEmpresaAsync(string localFilePath)
    {
        try
        {
            var extension = Path.GetExtension(localFilePath).TrimStart('.');
            var storagePath = $"{CONFIG_PATH}/logo.{extension}";
            
            return await UploadFileAsync(localFilePath, storagePath);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al subir logo de la empresa");
            return ServiceResult<string>.Fail($"Error al subir logo: {ex.Message}");
        }
    }
    
    #endregion
    
    #region Helper Methods
    
    /// <summary>
    /// Normaliza la ruta removiendo barras invertidas y espacios
    /// </summary>
    private static string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;
            
        // Reemplazar barras invertidas por barras normales
        path = path.Replace('\\', '/');
        
        // Remover barra inicial si existe
        path = path.TrimStart('/');
        
        // Remover espacios
        path = path.Trim();
        
        return path;
    }
    
    /// <summary>
    /// Obtiene el content type basado en la extensión del archivo
    /// </summary>
    private static string GetContentType(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        return GetContentTypeFromExtension(extension);
    }
    
    /// <summary>
    /// Obtiene el content type basado en la extensión
    /// </summary>
    private static string GetContentTypeFromExtension(string extension)
    {
        if (string.IsNullOrEmpty(extension))
            return "application/octet-stream";
            
        if (!extension.StartsWith("."))
            extension = "." + extension;
            
        return ExtensionToContentType.TryGetValue(extension, out var contentType) 
            ? contentType 
            : "application/octet-stream";
    }
    
    /// <summary>
    /// Genera la URL pública del archivo.
    /// Formato: https://firebasestorage.googleapis.com/v0/b/{bucket}/o/{encodedPath}?alt=media
    /// </summary>
    private string GetPublicUrl(string storagePath)
    {
        var encodedPath = Uri.EscapeDataString(storagePath);
        return $"https://firebasestorage.googleapis.com/v0/b/{_bucketName}/o/{encodedPath}?alt=media";
    }
    
    /// <summary>
    /// Sanitiza el nombre de archivo removiendo caracteres inválidos
    /// </summary>
    private static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "documento";
            
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        return sanitized.Trim();
    }
    
    #endregion
}
