using SGRRHH.Local.Domain.DTOs;

namespace SGRRHH.Local.Shared.Interfaces;

/// <summary>
/// Servicio para impresión de documentos
/// Soporta impresión silenciosa, selección de impresora y cola de impresión
/// </summary>
public interface IPrintService : IDisposable
{
    /// <summary>
    /// Evento disparado cuando hay progreso en la impresión
    /// </summary>
    event EventHandler<PrintProgressEventArgs>? PrintProgress;
    
    /// <summary>
    /// Indica si hay una impresión en progreso
    /// </summary>
    bool IsPrintingInProgress { get; }
    
    #region Gestión de Impresoras
    
    /// <summary>
    /// Obtiene la lista de impresoras disponibles en el sistema
    /// </summary>
    /// <returns>Lista de impresoras</returns>
    Task<Result<List<PrinterDeviceDto>>> GetAvailablePrintersAsync();
    
    /// <summary>
    /// Obtiene la impresora predeterminada del sistema
    /// </summary>
    /// <returns>Impresora predeterminada o null si no hay</returns>
    Task<Result<PrinterDeviceDto?>> GetDefaultPrinterAsync();
    
    /// <summary>
    /// Verifica si hay impresoras disponibles
    /// </summary>
    /// <returns>True si hay al menos una impresora</returns>
    Task<bool> HasAvailablePrintersAsync();
    
    /// <summary>
    /// Obtiene información de una impresora específica
    /// </summary>
    /// <param name="printerName">Nombre de la impresora</param>
    /// <returns>Información de la impresora</returns>
    Task<Result<PrinterDeviceDto>> GetPrinterInfoAsync(string printerName);
    
    #endregion
    
    #region Impresión de Documentos
    
    /// <summary>
    /// Imprime un archivo PDF
    /// </summary>
    /// <param name="pdfBytes">Bytes del PDF</param>
    /// <param name="options">Opciones de impresión</param>
    /// <returns>Resultado del trabajo de impresión</returns>
    Task<Result<PrintJobResultDto>> PrintPdfAsync(byte[] pdfBytes, PrintOptionsDto? options = null);
    
    /// <summary>
    /// Imprime un archivo PDF desde disco
    /// </summary>
    /// <param name="pdfPath">Ruta al archivo PDF</param>
    /// <param name="options">Opciones de impresión</param>
    /// <returns>Resultado del trabajo de impresión</returns>
    Task<Result<PrintJobResultDto>> PrintPdfFromFileAsync(string pdfPath, PrintOptionsDto? options = null);
    
    /// <summary>
    /// Imprime una imagen
    /// </summary>
    /// <param name="imageBytes">Bytes de la imagen</param>
    /// <param name="options">Opciones de impresión</param>
    /// <returns>Resultado del trabajo de impresión</returns>
    Task<Result<PrintJobResultDto>> PrintImageAsync(byte[] imageBytes, PrintOptionsDto? options = null);
    
    /// <summary>
    /// Imprime múltiples imágenes (una por página)
    /// </summary>
    /// <param name="images">Lista de imágenes en bytes</param>
    /// <param name="options">Opciones de impresión</param>
    /// <returns>Resultado del trabajo de impresión</returns>
    Task<Result<PrintJobResultDto>> PrintImagesAsync(IEnumerable<byte[]> images, PrintOptionsDto? options = null);
    
    /// <summary>
    /// Imprime un documento HTML
    /// </summary>
    /// <param name="htmlContent">Contenido HTML</param>
    /// <param name="options">Opciones de impresión</param>
    /// <returns>Resultado del trabajo de impresión</returns>
    Task<Result<PrintJobResultDto>> PrintHtmlAsync(string htmlContent, PrintOptionsDto? options = null);
    
    #endregion
    
    #region Cola de Impresión
    
    /// <summary>
    /// Obtiene los trabajos en cola para una impresora
    /// </summary>
    /// <param name="printerName">Nombre de la impresora (null para todas)</param>
    /// <returns>Lista de trabajos en cola</returns>
    Task<Result<List<PrintQueueItemDto>>> GetPrintQueueAsync(string? printerName = null);
    
    /// <summary>
    /// Cancela un trabajo de impresión
    /// </summary>
    /// <param name="jobId">ID del trabajo</param>
    /// <returns>True si se canceló exitosamente</returns>
    Task<Result<bool>> CancelPrintJobAsync(string jobId);
    
    /// <summary>
    /// Limpia todos los trabajos en cola de una impresora
    /// </summary>
    /// <param name="printerName">Nombre de la impresora</param>
    /// <returns>Número de trabajos cancelados</returns>
    Task<Result<int>> ClearPrintQueueAsync(string printerName);
    
    #endregion
    
    #region Utilidades
    
    /// <summary>
    /// Abre el diálogo de impresión del sistema para un PDF
    /// </summary>
    /// <param name="pdfBytes">Bytes del PDF</param>
    /// <returns>True si el usuario imprimió</returns>
    Task<Result<bool>> ShowPrintDialogAsync(byte[] pdfBytes);
    
    /// <summary>
    /// Genera una vista previa del documento (primera página)
    /// </summary>
    /// <param name="pdfBytes">Bytes del PDF</param>
    /// <returns>Imagen de vista previa</returns>
    Task<Result<byte[]>> GeneratePreviewAsync(byte[] pdfBytes);
    
    #endregion
}
