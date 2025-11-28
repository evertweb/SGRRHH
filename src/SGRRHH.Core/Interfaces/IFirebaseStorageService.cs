using SGRRHH.Core.Common;

namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Interfaz para el servicio de almacenamiento en Firebase Storage.
/// Maneja la subida, descarga y gestión de archivos en la nube.
/// </summary>
public interface IFirebaseStorageService
{
    #region Upload Operations
    
    /// <summary>
    /// Sube un archivo desde una ruta local a Firebase Storage.
    /// </summary>
    /// <param name="localFilePath">Ruta completa del archivo local</param>
    /// <param name="storagePath">Ruta destino en Storage (ej: "fotos/empleados/EMP001/foto.jpg")</param>
    /// <param name="contentType">Tipo MIME del archivo (opcional, se detecta automáticamente)</param>
    /// <returns>URL de descarga del archivo subido</returns>
    Task<ServiceResult<string>> UploadFileAsync(string localFilePath, string storagePath, string? contentType = null);
    
    /// <summary>
    /// Sube un archivo desde bytes a Firebase Storage.
    /// </summary>
    /// <param name="fileBytes">Contenido del archivo en bytes</param>
    /// <param name="storagePath">Ruta destino en Storage</param>
    /// <param name="contentType">Tipo MIME del archivo</param>
    /// <returns>URL de descarga del archivo subido</returns>
    Task<ServiceResult<string>> UploadBytesAsync(byte[] fileBytes, string storagePath, string contentType);
    
    /// <summary>
    /// Sube un archivo desde un stream a Firebase Storage.
    /// </summary>
    /// <param name="stream">Stream con el contenido del archivo</param>
    /// <param name="storagePath">Ruta destino en Storage</param>
    /// <param name="contentType">Tipo MIME del archivo</param>
    /// <returns>URL de descarga del archivo subido</returns>
    Task<ServiceResult<string>> UploadStreamAsync(Stream stream, string storagePath, string contentType);
    
    #endregion
    
    #region Download Operations
    
    /// <summary>
    /// Descarga un archivo de Firebase Storage a una ruta local.
    /// </summary>
    /// <param name="storagePath">Ruta del archivo en Storage</param>
    /// <param name="localFilePath">Ruta local donde guardar el archivo</param>
    /// <returns>Resultado de la operación</returns>
    Task<ServiceResult> DownloadFileAsync(string storagePath, string localFilePath);
    
    /// <summary>
    /// Descarga un archivo de Firebase Storage como bytes.
    /// </summary>
    /// <param name="storagePath">Ruta del archivo en Storage</param>
    /// <returns>Contenido del archivo en bytes</returns>
    Task<ServiceResult<byte[]>> DownloadBytesAsync(string storagePath);
    
    /// <summary>
    /// Descarga un archivo de Firebase Storage como stream.
    /// </summary>
    /// <param name="storagePath">Ruta del archivo en Storage</param>
    /// <returns>Stream con el contenido del archivo</returns>
    Task<ServiceResult<Stream>> DownloadStreamAsync(string storagePath);
    
    #endregion
    
    #region URL Operations
    
    /// <summary>
    /// Obtiene una URL de descarga pública para un archivo.
    /// Nota: Las URLs de Firebase Storage requieren autenticación por defecto.
    /// </summary>
    /// <param name="storagePath">Ruta del archivo en Storage</param>
    /// <returns>URL de descarga</returns>
    Task<ServiceResult<string>> GetDownloadUrlAsync(string storagePath);
    
    /// <summary>
    /// Obtiene una URL firmada con tiempo de expiración.
    /// </summary>
    /// <param name="storagePath">Ruta del archivo en Storage</param>
    /// <param name="expiration">Tiempo de expiración de la URL</param>
    /// <returns>URL firmada</returns>
    Task<ServiceResult<string>> GetSignedUrlAsync(string storagePath, TimeSpan expiration);
    
    #endregion
    
    #region Delete Operations
    
    /// <summary>
    /// Elimina un archivo de Firebase Storage.
    /// </summary>
    /// <param name="storagePath">Ruta del archivo en Storage</param>
    /// <returns>Resultado de la operación</returns>
    Task<ServiceResult> DeleteFileAsync(string storagePath);
    
    /// <summary>
    /// Elimina múltiples archivos de Firebase Storage.
    /// </summary>
    /// <param name="storagePaths">Lista de rutas de archivos a eliminar</param>
    /// <returns>Resultado de la operación</returns>
    Task<ServiceResult> DeleteFilesAsync(IEnumerable<string> storagePaths);
    
    /// <summary>
    /// Elimina todos los archivos de una carpeta en Storage.
    /// </summary>
    /// <param name="folderPath">Ruta de la carpeta (ej: "fotos/empleados/EMP001/")</param>
    /// <returns>Resultado de la operación con cantidad de archivos eliminados</returns>
    Task<ServiceResult<int>> DeleteFolderAsync(string folderPath);
    
    #endregion
    
    #region List Operations
    
    /// <summary>
    /// Lista los archivos en una carpeta de Storage.
    /// </summary>
    /// <param name="folderPath">Ruta de la carpeta</param>
    /// <param name="maxResults">Máximo número de resultados (por defecto 100)</param>
    /// <returns>Lista de archivos con su información</returns>
    Task<ServiceResult<IEnumerable<StorageFileInfo>>> ListFilesAsync(string folderPath, int maxResults = 100);
    
    /// <summary>
    /// Verifica si un archivo existe en Storage.
    /// </summary>
    /// <param name="storagePath">Ruta del archivo en Storage</param>
    /// <returns>True si el archivo existe</returns>
    Task<bool> FileExistsAsync(string storagePath);
    
    #endregion
    
    #region Specialized Operations (Photos, Documents)
    
    /// <summary>
    /// Sube la foto de un empleado.
    /// </summary>
    /// <param name="empleadoId">ID del empleado</param>
    /// <param name="localFilePath">Ruta local de la foto</param>
    /// <returns>URL de la foto subida</returns>
    Task<ServiceResult<string>> UploadEmpleadoFotoAsync(int empleadoId, string localFilePath);
    
    /// <summary>
    /// Sube la foto de un empleado desde bytes.
    /// </summary>
    /// <param name="empleadoId">ID del empleado</param>
    /// <param name="imageBytes">Bytes de la imagen</param>
    /// <param name="extension">Extensión del archivo (jpg, png, etc.)</param>
    /// <returns>URL de la foto subida</returns>
    Task<ServiceResult<string>> UploadEmpleadoFotoAsync(int empleadoId, byte[] imageBytes, string extension = "jpg");
    
    /// <summary>
    /// Elimina la foto de un empleado.
    /// </summary>
    /// <param name="empleadoId">ID del empleado</param>
    /// <returns>Resultado de la operación</returns>
    Task<ServiceResult> DeleteEmpleadoFotoAsync(int empleadoId);
    
    /// <summary>
    /// Sube el documento soporte de un permiso.
    /// </summary>
    /// <param name="permisoId">ID del permiso</param>
    /// <param name="localFilePath">Ruta local del documento</param>
    /// <returns>URL del documento subido</returns>
    Task<ServiceResult<string>> UploadPermisoDocumentoAsync(int permisoId, string localFilePath);
    
    /// <summary>
    /// Sube el archivo de un contrato.
    /// </summary>
    /// <param name="contratoId">ID del contrato</param>
    /// <param name="localFilePath">Ruta local del archivo</param>
    /// <returns>URL del archivo subido</returns>
    Task<ServiceResult<string>> UploadContratoArchivoAsync(int contratoId, string localFilePath);
    
    /// <summary>
    /// Sube un documento generado (acta, certificado, etc.)
    /// </summary>
    /// <param name="tipoDocumento">Tipo de documento (actas, certificados, etc.)</param>
    /// <param name="fileName">Nombre del archivo</param>
    /// <param name="pdfBytes">Contenido del PDF</param>
    /// <returns>URL del documento subido</returns>
    Task<ServiceResult<string>> UploadDocumentoGeneradoAsync(string tipoDocumento, string fileName, byte[] pdfBytes);
    
    /// <summary>
    /// Sube o actualiza el logo de la empresa.
    /// </summary>
    /// <param name="localFilePath">Ruta local del logo</param>
    /// <returns>URL del logo subido</returns>
    Task<ServiceResult<string>> UploadLogoEmpresaAsync(string localFilePath);
    
    #endregion
}

/// <summary>
/// Información de un archivo en Firebase Storage.
/// </summary>
public class StorageFileInfo
{
    /// <summary>
    /// Nombre del archivo
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Ruta completa en Storage
    /// </summary>
    public string Path { get; set; } = string.Empty;
    
    /// <summary>
    /// Tamaño del archivo en bytes
    /// </summary>
    public long Size { get; set; }
    
    /// <summary>
    /// Tipo MIME del archivo
    /// </summary>
    public string ContentType { get; set; } = string.Empty;
    
    /// <summary>
    /// Fecha de creación
    /// </summary>
    public DateTime? Created { get; set; }
    
    /// <summary>
    /// Fecha de última modificación
    /// </summary>
    public DateTime? Updated { get; set; }
    
    /// <summary>
    /// URL de descarga (si está disponible)
    /// </summary>
    public string? DownloadUrl { get; set; }
}
