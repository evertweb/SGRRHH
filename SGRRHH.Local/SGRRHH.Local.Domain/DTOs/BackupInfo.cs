namespace SGRRHH.Local.Domain.DTOs;

public class BackupInfo
{
    public string FileName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public long SizeBytes { get; set; }
    public string? Description { get; set; }
    public bool IncludesFiles { get; set; }
    
    public string SizeFormatted => FormatBytes(SizeBytes);
    
    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
