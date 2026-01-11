namespace SGRRHH.Local.Domain.DTOs;

/// <summary>
/// Representa un dispositivo de escaneo disponible
/// </summary>
public class ScannerDeviceDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Driver { get; set; } = string.Empty; // WIA, TWAIN, ESCL
    public bool IsAvailable { get; set; } = true;
}

/// <summary>
/// Opciones para configurar el escaneo
/// </summary>
public class ScanOptionsDto
{
    /// <summary>
    /// ID del dispositivo a usar. Si es null, usa el primero disponible
    /// </summary>
    public string? DeviceId { get; set; }
    
    /// <summary>
    /// Fuente del escaneo (plano o alimentador)
    /// </summary>
    public ScanSource Source { get; set; } = ScanSource.Flatbed;
    
    /// <summary>
    /// Resolución en DPI
    /// </summary>
    public int Dpi { get; set; } = 300;
    
    /// <summary>
    /// Modo de color
    /// </summary>
    public ScanColorMode ColorMode { get; set; } = ScanColorMode.Color;
    
    /// <summary>
    /// Tamaño de página
    /// </summary>
    public ScanPageSize PageSize { get; set; } = ScanPageSize.Letter;
}

/// <summary>
/// Fuente del documento a escanear
/// </summary>
public enum ScanSource
{
    /// <summary>
    /// Vidrio plano del escáner
    /// </summary>
    Flatbed,
    
    /// <summary>
    /// Alimentador automático de documentos (ADF)
    /// </summary>
    Feeder,
    
    /// <summary>
    /// Automático - usa ADF si hay documentos, sino Flatbed
    /// </summary>
    Auto
}

/// <summary>
/// Modo de color para escaneo
/// </summary>
public enum ScanColorMode
{
    /// <summary>
    /// Color completo (24 bits)
    /// </summary>
    Color,
    
    /// <summary>
    /// Escala de grises (8 bits)
    /// </summary>
    Grayscale,
    
    /// <summary>
    /// Blanco y negro (1 bit)
    /// </summary>
    BlackWhite
}

/// <summary>
/// Tamaño de página para escaneo
/// </summary>
public enum ScanPageSize
{
    Letter,     // 8.5 x 11 pulgadas
    Legal,      // 8.5 x 14 pulgadas
    A4,         // 210 x 297 mm
    A5,         // 148 x 210 mm
    Custom      // Tamaño personalizado
}

/// <summary>
/// Resultado de un escaneo (documento completo o página individual)
/// </summary>
public class ScannedDocumentDto
{
    /// <summary>
    /// Bytes de la imagen escaneada
    /// </summary>
    public byte[] ImageBytes { get; set; } = Array.Empty<byte>();
    
    /// <summary>
    /// Tipo MIME (image/png, image/jpeg, application/pdf)
    /// </summary>
    public string MimeType { get; set; } = "image/png";
    
    /// <summary>
    /// Ancho en píxeles
    /// </summary>
    public int Width { get; set; }
    
    /// <summary>
    /// Alto en píxeles
    /// </summary>
    public int Height { get; set; }
    
    /// <summary>
    /// DPI usado en el escaneo
    /// </summary>
    public int Dpi { get; set; }
    
    /// <summary>
    /// Fecha y hora del escaneo
    /// </summary>
    public DateTime ScannedAt { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Tamaño en bytes
    /// </summary>
    public long FileSizeBytes => ImageBytes.Length;
    
    /// <summary>
    /// Tamaño legible (ej: "1.5 MB")
    /// </summary>
    public string FileSizeFormatted => FormatFileSize(FileSizeBytes);
    
    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}

/// <summary>
/// Página individual en un escaneo batch
/// </summary>
public class ScannedPageDto : ScannedDocumentDto
{
    /// <summary>
    /// Número de página (1-based)
    /// </summary>
    public int PageNumber { get; set; }
}

/// <summary>
/// Evento de progreso durante el escaneo
/// </summary>
public class ScanProgressEventArgs : EventArgs
{
    /// <summary>
    /// Número de página actual siendo escaneada
    /// </summary>
    public int CurrentPage { get; set; }
    
    /// <summary>
    /// Progreso de la página actual (0.0 - 1.0)
    /// </summary>
    public double Progress { get; set; }
    
    /// <summary>
    /// Mensaje de estado
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Indica si el escaneo ha terminado
    /// </summary>
    public bool IsComplete { get; set; }
    
    /// <summary>
    /// Indica si hubo un error
    /// </summary>
    public bool HasError { get; set; }
    
    /// <summary>
    /// Mensaje de error (si aplica)
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Resultado de escaneo batch (múltiples páginas)
/// </summary>
public class BatchScanResultDto
{
    /// <summary>
    /// Lista de páginas escaneadas
    /// </summary>
    public List<ScannedPageDto> Pages { get; set; } = new();
    
    /// <summary>
    /// Total de páginas escaneadas
    /// </summary>
    public int TotalPages => Pages.Count;
    
    /// <summary>
    /// Indica si el escaneo fue exitoso
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Mensaje de error si falló
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Tiempo total del escaneo
    /// </summary>
    public TimeSpan Duration { get; set; }
}
