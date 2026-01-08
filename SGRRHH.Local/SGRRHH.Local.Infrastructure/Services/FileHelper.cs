using System.Collections.Concurrent;

namespace SGRRHH.Local.Infrastructure.Services;

public static class FileHelper
{
    private static readonly ConcurrentDictionary<string, string> MimeTypes = new(new[]
    {
        new KeyValuePair<string, string>(".pdf", "application/pdf"),
        new KeyValuePair<string, string>(".jpg", "image/jpeg"),
        new KeyValuePair<string, string>(".jpeg", "image/jpeg"),
        new KeyValuePair<string, string>(".png", "image/png"),
        new KeyValuePair<string, string>(".gif", "image/gif"),
        new KeyValuePair<string, string>(".bmp", "image/bmp"),
        new KeyValuePair<string, string>(".doc", "application/msword"),
        new KeyValuePair<string, string>(".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"),
        new KeyValuePair<string, string>(".xls", "application/vnd.ms-excel"),
        new KeyValuePair<string, string>(".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
    });

    public static string GetMimeType(string extension)
    {
        extension = NormalizeExtension(extension);
        return MimeTypes.TryGetValue(extension, out var mime) ? mime : "application/octet-stream";
    }

    public static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
    }

    public static string GetUniqueFileName(string directory, string fileName)
    {
        Directory.CreateDirectory(directory);

        var sanitized = SanitizeFileName(fileName);
        var fullPath = Path.Combine(directory, sanitized);
        if (!File.Exists(fullPath)) return fullPath;

        var name = Path.GetFileNameWithoutExtension(sanitized);
        var ext = Path.GetExtension(sanitized);
        var counter = 1;

        while (File.Exists(fullPath))
        {
            fullPath = Path.Combine(directory, $"{name}_{counter}{ext}");
            counter++;
        }

        return fullPath;
    }

    public static string NormalizeExtension(string extension)
    {
        var ext = extension.Trim().ToLowerInvariant();
        return ext.StartsWith('.') ? ext : $".{ext}";
    }
}