using SGRRHH.Core.Models;

namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Servicio extendido para gestionar actualizaciones de la aplicación desde Firebase Storage.
/// Hereda de IUpdateService para mantener compatibilidad.
/// </summary>
public interface IFirebaseUpdateService : IUpdateService
{
    /// <summary>
    /// Indica si el servicio está usando Firebase Storage (true) o carpeta local (false)
    /// </summary>
    bool IsUsingFirebaseStorage { get; }
    
    /// <summary>
    /// URL base del bucket de Firebase Storage
    /// </summary>
    string StorageBucketUrl { get; }
    
    /// <summary>
    /// Verifica la conectividad con Firebase Storage
    /// </summary>
    /// <returns>True si se puede acceder a Firebase Storage</returns>
    Task<bool> TestFirebaseConnectionAsync();
    
    /// <summary>
    /// Obtiene la información de versión directamente desde Firebase Storage
    /// </summary>
    /// <returns>Información de la versión disponible o null si no hay</returns>
    Task<VersionInfo?> GetRemoteVersionInfoAsync();
    
    /// <summary>
    /// Descarga un archivo específico de la actualización desde Firebase Storage
    /// </summary>
    /// <param name="remoteFileName">Nombre del archivo en Firebase Storage</param>
    /// <param name="localPath">Ruta local donde guardar el archivo</param>
    /// <param name="progress">Callback para reportar progreso</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si la descarga fue exitosa</returns>
    Task<bool> DownloadFileAsync(string remoteFileName, string localPath, 
        IProgress<UpdateProgress>? progress = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene la lista de archivos disponibles en la última versión
    /// </summary>
    /// <returns>Lista de archivos con su información</returns>
    Task<IEnumerable<UpdateFile>> GetAvailableFilesAsync();
    
    /// <summary>
    /// Limpia los archivos temporales de actualizaciones anteriores
    /// </summary>
    /// <returns>Número de archivos/carpetas eliminados</returns>
    Task<int> CleanupTempFilesAsync();
    
    /// <summary>
    /// Verifica si hay una actualización obligatoria que impida usar la aplicación
    /// </summary>
    /// <returns>True si hay una actualización obligatoria pendiente</returns>
    Task<bool> HasMandatoryUpdateAsync();
}
