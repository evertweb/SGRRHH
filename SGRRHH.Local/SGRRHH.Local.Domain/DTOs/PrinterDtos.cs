namespace SGRRHH.Local.Domain.DTOs;

/// <summary>
/// Representa una impresora disponible en el sistema
/// </summary>
public class PrinterDeviceDto
{
    /// <summary>
    /// Nombre de la impresora (usado como identificador)
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Indica si es la impresora predeterminada del sistema
    /// </summary>
    public bool IsDefault { get; set; }
    
    /// <summary>
    /// Indica si la impresora está disponible/conectada
    /// </summary>
    public bool IsAvailable { get; set; } = true;
    
    /// <summary>
    /// Estado actual de la impresora
    /// </summary>
    public PrinterStatus Status { get; set; } = PrinterStatus.Ready;
    
    /// <summary>
    /// Cantidad de trabajos en cola
    /// </summary>
    public int JobsInQueue { get; set; }
    
    /// <summary>
    /// Indica si es una impresora de red
    /// </summary>
    public bool IsNetworkPrinter { get; set; }
    
    /// <summary>
    /// Puerto de la impresora
    /// </summary>
    public string? PortName { get; set; }
    
    /// <summary>
    /// Driver de la impresora
    /// </summary>
    public string? DriverName { get; set; }
}

/// <summary>
/// Estado de la impresora
/// </summary>
public enum PrinterStatus
{
    Ready,
    Printing,
    Busy,
    Offline,
    PaperOut,
    Error,
    Unknown
}

/// <summary>
/// Opciones para configurar la impresión
/// </summary>
public class PrintOptionsDto
{
    /// <summary>
    /// Nombre de la impresora a usar. Si es null, usa la predeterminada
    /// </summary>
    public string? PrinterName { get; set; }
    
    /// <summary>
    /// Número de copias a imprimir
    /// </summary>
    public int Copies { get; set; } = 1;
    
    /// <summary>
    /// Orientación del papel
    /// </summary>
    public PrintOrientation Orientation { get; set; } = PrintOrientation.Portrait;
    
    /// <summary>
    /// Tamaño de papel
    /// </summary>
    public PrintPaperSize PaperSize { get; set; } = PrintPaperSize.Letter;
    
    /// <summary>
    /// Imprimir en color o blanco y negro
    /// </summary>
    public PrintColorMode ColorMode { get; set; } = PrintColorMode.Auto;
    
    /// <summary>
    /// Calidad de impresión
    /// </summary>
    public PrintQuality Quality { get; set; } = PrintQuality.Normal;
    
    /// <summary>
    /// Imprimir a doble cara
    /// </summary>
    public bool Duplex { get; set; } = false;
    
    /// <summary>
    /// Rango de páginas a imprimir (ej: "1-5", "1,3,5", null para todas)
    /// </summary>
    public string? PageRange { get; set; }
    
    /// <summary>
    /// Escalar al tamaño de página
    /// </summary>
    public bool FitToPage { get; set; } = true;
    
    /// <summary>
    /// Imprimir silenciosamente sin mostrar diálogo
    /// </summary>
    public bool SilentPrint { get; set; } = true;
    
    /// <summary>
    /// Nombre del trabajo de impresión (para cola de impresión)
    /// </summary>
    public string? JobName { get; set; }
}

/// <summary>
/// Orientación del papel
/// </summary>
public enum PrintOrientation
{
    Portrait,
    Landscape
}

/// <summary>
/// Tamaño de papel
/// </summary>
public enum PrintPaperSize
{
    Letter,     // 8.5 x 11 pulgadas
    Legal,      // 8.5 x 14 pulgadas
    A4,         // 210 x 297 mm
    A5,         // 148 x 210 mm
    Custom
}

/// <summary>
/// Modo de color
/// </summary>
public enum PrintColorMode
{
    /// <summary>
    /// Detectar automáticamente
    /// </summary>
    Auto,
    
    /// <summary>
    /// Color
    /// </summary>
    Color,
    
    /// <summary>
    /// Escala de grises
    /// </summary>
    Grayscale
}

/// <summary>
/// Calidad de impresión
/// </summary>
public enum PrintQuality
{
    Draft,
    Normal,
    High
}

/// <summary>
/// Resultado de un trabajo de impresión
/// </summary>
public class ResultadoImpresionDto
{
    /// <summary>
    /// ID único del trabajo de impresión
    /// </summary>
    public string JobId { get; set; } = Guid.NewGuid().ToString("N");
    
    /// <summary>
    /// Nombre del trabajo
    /// </summary>
    public string JobName { get; set; } = string.Empty;
    
    /// <summary>
    /// Indica si fue exitoso
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Mensaje de error si falló
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Nombre de la impresora usada
    /// </summary>
    public string PrinterName { get; set; } = string.Empty;
    
    /// <summary>
    /// Número de páginas enviadas
    /// </summary>
    public int PageCount { get; set; }
    
    /// <summary>
    /// Número de copias
    /// </summary>
    public int Copies { get; set; }
    
    /// <summary>
    /// Fecha y hora de inicio
    /// </summary>
    public DateTime StartTime { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Estado actual del trabajo
    /// </summary>
    public PrintJobStatus Status { get; set; } = PrintJobStatus.Pending;
}

/// <summary>
/// Estado de un trabajo de impresión
/// </summary>
public enum PrintJobStatus
{
    Pending,
    Printing,
    Completed,
    Failed,
    Cancelled
}

/// <summary>
/// Trabajo de impresión en cola
/// </summary>
public class PrintQueueItemDto
{
    /// <summary>
    /// ID del trabajo
    /// </summary>
    public string JobId { get; set; } = string.Empty;
    
    /// <summary>
    /// Nombre del trabajo
    /// </summary>
    public string JobName { get; set; } = string.Empty;
    
    /// <summary>
    /// Nombre de la impresora
    /// </summary>
    public string PrinterName { get; set; } = string.Empty;
    
    /// <summary>
    /// Estado del trabajo
    /// </summary>
    public PrintJobStatus Status { get; set; }
    
    /// <summary>
    /// Número de páginas
    /// </summary>
    public int PageCount { get; set; }
    
    /// <summary>
    /// Páginas impresas
    /// </summary>
    public int PagesPrinted { get; set; }
    
    /// <summary>
    /// Fecha de creación
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Posición en la cola
    /// </summary>
    public int Position { get; set; }
}

/// <summary>
/// Evento de progreso de impresión
/// </summary>
public class PrintProgressEventArgs : EventArgs
{
    public string JobId { get; set; } = string.Empty;
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public double Progress => TotalPages > 0 ? (double)CurrentPage / TotalPages : 0;
    public PrintJobStatus Status { get; set; }
    public string? Message { get; set; }
}
