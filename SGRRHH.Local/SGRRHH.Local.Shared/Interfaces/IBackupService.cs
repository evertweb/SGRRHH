using SGRRHH.Local.Shared;

namespace SGRRHH.Local.Shared.Interfaces;

public interface IBackupService
{
    // Backup
    Task<Result<string>> CreateBackupAsync(string? description = null);
    Task<Result> CreateScheduledBackupAsync(); // Para timer automático
    
    // Restore
    Task<Result<IEnumerable<BackupInfo>>> GetAvailableBackupsAsync();
    Task<Result> RestoreFromBackupAsync(string backupFileName);
    
    // Mantenimiento
    Task<Result<int>> CleanupOldBackupsAsync(int keepDays = 30);
    Task<Result<long>> GetBackupsFolderSizeAsync();
}

public class BackupInfo
{
    public string FileName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public long SizeBytes { get; set; }
    public string? Description { get; set; }
    public bool IncludesFiles { get; set; }
    
    public string SizeFormatted
    {
        get
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = SizeBytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
