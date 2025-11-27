using SGRRHH.Core.Common;

namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Información de un archivo de backup
/// </summary>
public class BackupInfo
{
    public string NombreArchivo { get; set; } = string.Empty;
    public string RutaCompleta { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public long TamañoBytes { get; set; }
    
    public string TamañoFormateado => TamañoBytes switch
    {
        < 1024 => $"{TamañoBytes} B",
        < 1024 * 1024 => $"{TamañoBytes / 1024.0:F2} KB",
        _ => $"{TamañoBytes / (1024.0 * 1024.0):F2} MB"
    };
}

/// <summary>
/// Interfaz para el servicio de backup de la base de datos
/// </summary>
public interface IBackupService
{
    /// <summary>
    /// Crea un backup de la base de datos
    /// </summary>
    /// <param name="rutaDestino">Ruta donde guardar el backup. Si es null, se guarda en la carpeta por defecto</param>
    /// <returns>Ruta completa del archivo de backup creado</returns>
    Task<ServiceResult<string>> CrearBackupAsync(string? rutaDestino = null);
    
    /// <summary>
    /// Restaura la base de datos desde un backup
    /// </summary>
    /// <param name="rutaBackup">Ruta del archivo de backup</param>
    Task<ServiceResult> RestaurarBackupAsync(string rutaBackup);
    
    /// <summary>
    /// Obtiene la lista de backups disponibles en la carpeta por defecto
    /// </summary>
    Task<List<BackupInfo>> ListarBackupsAsync();
    
    /// <summary>
    /// Elimina un archivo de backup
    /// </summary>
    Task<ServiceResult> EliminarBackupAsync(string rutaBackup);
    
    /// <summary>
    /// Obtiene la ruta de la carpeta de backups
    /// </summary>
    string GetRutaBackups();
    
    /// <summary>
    /// Verifica si un archivo de backup es válido
    /// </summary>
    Task<ServiceResult> ValidarBackupAsync(string rutaBackup);
}
