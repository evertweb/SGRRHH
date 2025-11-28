using SGRRHH.Core.Models;

namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Servicio para gestionar actualizaciones de la aplicación
/// </summary>
public interface IUpdateService
{
    /// <summary>
    /// Verifica si hay una nueva versión disponible
    /// </summary>
    /// <returns>Resultado de la verificación</returns>
    Task<UpdateCheckResult> CheckForUpdatesAsync();
    
    /// <summary>
    /// Descarga la actualización disponible
    /// </summary>
    /// <param name="progress">Callback para reportar progreso</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si la descarga fue exitosa</returns>
    Task<bool> DownloadUpdateAsync(IProgress<UpdateProgress>? progress = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Aplica la actualización descargada (requiere reinicio de la app)
    /// </summary>
    /// <returns>True si se puede proceder con la instalación</returns>
    Task<bool> ApplyUpdateAsync();
    
    /// <summary>
    /// Obtiene la versión actual instalada
    /// </summary>
    string GetCurrentVersion();
    
    /// <summary>
    /// Indica si hay una actualización descargada lista para instalar
    /// </summary>
    bool HasPendingUpdate { get; }
    
    /// <summary>
    /// Ruta donde se descargan las actualizaciones
    /// </summary>
    string UpdatesPath { get; }
    
    /// <summary>
    /// Evento que se dispara cuando se detecta una nueva versión
    /// </summary>
    event EventHandler<VersionInfo>? UpdateAvailable;
}
