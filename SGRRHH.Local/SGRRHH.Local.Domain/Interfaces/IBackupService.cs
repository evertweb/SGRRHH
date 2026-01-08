using SGRRHH.Local.Domain.DTOs;
using SGRRHH.Local.Shared;

namespace SGRRHH.Local.Domain.Interfaces;

public interface IBackupService
{
    /// <summary>
    /// Crea un backup completo (BD + archivos)
    /// </summary>
    Task<Result<string>> CreateBackupAsync(string? description = null);
    
    /// <summary>
    /// Crea un backup automático programado
    /// </summary>
    Task<Result<string>> CreateScheduledBackupAsync();
    
    /// <summary>
    /// Obtiene la lista de backups disponibles
    /// </summary>
    Task<Result<IEnumerable<BackupInfo>>> GetAvailableBackupsAsync();
    
    /// <summary>
    /// Restaura un backup específico
    /// </summary>
    Task<Result> RestoreFromBackupAsync(string backupFileName);
    
    /// <summary>
    /// Elimina backups antiguos
    /// </summary>
    Task<Result<int>> CleanupOldBackupsAsync(int keepDays = 30);
    
    /// <summary>
    /// Calcula el tamaño total de la carpeta de backups
    /// </summary>
    Task<Result<long>> GetBackupsFolderSizeAsync();
}
