using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.DTOs;
using SGRRHH.Local.Shared;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Services;

/// <summary>
/// Servicio de impresión que usa System.Drawing.Printing para Windows
/// Soporta impresión silenciosa, selección de impresora y múltiples formatos
/// </summary>
public class PrintService : IPrintService
{
    private readonly ILogger<PrintService> _logger;
    private bool _isPrinting;
    private bool _disposed;
    
    // Para impresión de múltiples imágenes
    private List<Image>? _imagesToPrint;
    private int _currentImageIndex;
    private PrintOptionsDto? _currentOptions;

    public event EventHandler<PrintProgressEventArgs>? PrintProgress;
    
    public bool IsPrintingInProgress => _isPrinting;

    public PrintService(ILogger<PrintService> logger)
    {
        _logger = logger;
    }

    #region Gestión de Impresoras

    public Task<Result<List<PrinterDeviceDto>>> GetAvailablePrintersAsync()
    {
        return Task.Run(() =>
        {
            try
            {
                var printers = new List<PrinterDeviceDto>();
                var defaultPrinter = GetDefaultPrinterName();
                
                foreach (string printerName in PrinterSettings.InstalledPrinters)
                {
                    try
                    {
                        var settings = new PrinterSettings { PrinterName = printerName };
                        var printer = new PrinterDeviceDto
                        {
                            Name = printerName,
                            IsDefault = printerName == defaultPrinter,
                            IsAvailable = settings.IsValid,
                            Status = settings.IsValid ? PrinterStatus.Ready : PrinterStatus.Offline,
                            IsNetworkPrinter = printerName.StartsWith("\\\\") || printerName.Contains("network", StringComparison.OrdinalIgnoreCase)
                        };
                        
                        printers.Add(printer);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error al obtener info de impresora {PrinterName}", printerName);
                        printers.Add(new PrinterDeviceDto
                        {
                            Name = printerName,
                            IsAvailable = false,
                            Status = PrinterStatus.Error
                        });
                    }
                }
                
                // Ordenar: predeterminada primero, luego alfabético
                printers = printers
                    .OrderByDescending(p => p.IsDefault)
                    .ThenByDescending(p => p.IsAvailable)
                    .ThenBy(p => p.Name)
                    .ToList();
                
                _logger.LogInformation("Se encontraron {Count} impresoras instaladas", printers.Count);
                return Result<List<PrinterDeviceDto>>.Ok(printers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar impresoras");
                return Result<List<PrinterDeviceDto>>.Fail($"Error al listar impresoras: {ex.Message}");
            }
        });
    }

    public Task<Result<PrinterDeviceDto?>> GetDefaultPrinterAsync()
    {
        return Task.Run(() =>
        {
            try
            {
                var defaultName = GetDefaultPrinterName();
                if (string.IsNullOrEmpty(defaultName))
                {
                    return Result<PrinterDeviceDto?>.Ok(null);
                }
                
                var settings = new PrinterSettings { PrinterName = defaultName };
                var printer = new PrinterDeviceDto
                {
                    Name = defaultName,
                    IsDefault = true,
                    IsAvailable = settings.IsValid,
                    Status = settings.IsValid ? PrinterStatus.Ready : PrinterStatus.Offline
                };
                
                return Result<PrinterDeviceDto?>.Ok(printer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener impresora predeterminada");
                return Result<PrinterDeviceDto?>.Fail($"Error: {ex.Message}");
            }
        });
    }

    public async Task<bool> HasAvailablePrintersAsync()
    {
        var result = await GetAvailablePrintersAsync();
        return result.IsSuccess && result.Value?.Any(p => p.IsAvailable) == true;
    }

    public Task<Result<PrinterDeviceDto>> GetPrinterInfoAsync(string printerName)
    {
        return Task.Run(() =>
        {
            try
            {
                var settings = new PrinterSettings { PrinterName = printerName };
                
                if (!settings.IsValid)
                {
                    return Result<PrinterDeviceDto>.Fail($"Impresora '{printerName}' no encontrada");
                }
                
                var defaultName = GetDefaultPrinterName();
                var printer = new PrinterDeviceDto
                {
                    Name = printerName,
                    IsDefault = printerName == defaultName,
                    IsAvailable = true,
                    Status = PrinterStatus.Ready
                };
                
                return Result<PrinterDeviceDto>.Ok(printer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener info de impresora {PrinterName}", printerName);
                return Result<PrinterDeviceDto>.Fail($"Error: {ex.Message}");
            }
        });
    }

    private static string? GetDefaultPrinterName()
    {
        var settings = new PrinterSettings();
        return settings.IsValid ? settings.PrinterName : null;
    }

    #endregion

    #region Impresión de Documentos

    public Task<Result<PrintJobResultDto>> PrintPdfAsync(byte[] pdfBytes, PrintOptionsDto? options = null)
    {
        return Task.Run(async () =>
        {
            if (pdfBytes == null || pdfBytes.Length == 0)
            {
                return Result<PrintJobResultDto>.Fail("El PDF está vacío");
            }
            
            // Para PDFs, convertimos a imágenes y las imprimimos
            // Esto es una limitación de System.Drawing.Printing
            // Para impresión nativa de PDF necesitaríamos bibliotecas adicionales
            
            try
            {
                var images = await ConvertPdfToImagesAsync(pdfBytes);
                if (images.Count == 0)
                {
                    // Fallback: guardar temporalmente y usar proceso externo
                    return await PrintPdfExternalAsync(pdfBytes, options);
                }
                
                var imageBytes = images.Select(img => ImageToBytes(img)).ToList();
                var result = await PrintImagesAsync(imageBytes, options);
                
                // Limpiar imágenes
                foreach (var img in images) img.Dispose();
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al convertir PDF a imágenes, usando método externo");
                return await PrintPdfExternalAsync(pdfBytes, options);
            }
        });
    }

    public Task<Result<PrintJobResultDto>> PrintPdfFromFileAsync(string pdfPath, PrintOptionsDto? options = null)
    {
        return Task.Run(async () =>
        {
            if (!File.Exists(pdfPath))
            {
                return Result<PrintJobResultDto>.Fail($"Archivo no encontrado: {pdfPath}");
            }
            
            var pdfBytes = await File.ReadAllBytesAsync(pdfPath);
            return await PrintPdfAsync(pdfBytes, options);
        });
    }

    public Task<Result<PrintJobResultDto>> PrintImageAsync(byte[] imageBytes, PrintOptionsDto? options = null)
    {
        return PrintImagesAsync(new[] { imageBytes }, options);
    }

    public Task<Result<PrintJobResultDto>> PrintImagesAsync(IEnumerable<byte[]> images, PrintOptionsDto? options = null)
    {
        return Task.Run(() =>
        {
            if (_isPrinting)
            {
                return Result<PrintJobResultDto>.Fail("Ya hay una impresión en progreso");
            }
            
            options ??= new PrintOptionsDto();
            var jobResult = new PrintJobResultDto
            {
                JobName = options.JobName ?? $"Impresión_{DateTime.Now:yyyyMMdd_HHmmss}",
                PrinterName = options.PrinterName ?? GetDefaultPrinterName() ?? "Desconocida",
                Copies = options.Copies,
                Status = PrintJobStatus.Pending
            };
            
            try
            {
                _isPrinting = true;
                _currentOptions = options;
                _imagesToPrint = new List<Image>();
                _currentImageIndex = 0;
                
                // Cargar todas las imágenes
                foreach (var imageBytes in images)
                {
                    using var ms = new MemoryStream(imageBytes);
                    var img = Image.FromStream(ms);
                    _imagesToPrint.Add(img);
                }
                
                if (_imagesToPrint.Count == 0)
                {
                    return Result<PrintJobResultDto>.Fail("No hay imágenes para imprimir");
                }
                
                jobResult.PageCount = _imagesToPrint.Count;
                
                // Configurar impresora
                var printDoc = new PrintDocument();
                printDoc.DocumentName = jobResult.JobName;
                
                if (!string.IsNullOrEmpty(options.PrinterName))
                {
                    printDoc.PrinterSettings.PrinterName = options.PrinterName;
                }
                
                if (!printDoc.PrinterSettings.IsValid)
                {
                    jobResult.Status = PrintJobStatus.Failed;
                    jobResult.Success = false;
                    jobResult.ErrorMessage = $"Impresora '{options.PrinterName}' no válida";
                    return Result<PrintJobResultDto>.Fail(jobResult.ErrorMessage);
                }
                
                // Configurar opciones
                printDoc.PrinterSettings.Copies = (short)Math.Max(1, Math.Min(options.Copies, 100));
                
                if (options.Orientation == PrintOrientation.Landscape)
                {
                    printDoc.DefaultPageSettings.Landscape = true;
                }
                
                // Configurar tamaño de papel
                SetPaperSize(printDoc, options.PaperSize);
                
                // Configurar calidad
                SetPrintQuality(printDoc, options.Quality);
                
                // Evento para cada página
                printDoc.PrintPage += (sender, e) =>
                {
                    if (_currentImageIndex < _imagesToPrint!.Count && e.Graphics != null)
                    {
                        var img = _imagesToPrint[_currentImageIndex];
                        var bounds = e.MarginBounds;
                        
                        if (options.FitToPage)
                        {
                            // Escalar imagen para que quepa en la página
                            var scale = Math.Min(
                                (double)bounds.Width / img.Width,
                                (double)bounds.Height / img.Height);
                            
                            var width = (int)(img.Width * scale);
                            var height = (int)(img.Height * scale);
                            var x = bounds.Left + (bounds.Width - width) / 2;
                            var y = bounds.Top + (bounds.Height - height) / 2;
                            
                            e.Graphics.DrawImage(img, x, y, width, height);
                        }
                        else
                        {
                            e.Graphics.DrawImage(img, bounds.Location);
                        }
                        
                        _currentImageIndex++;
                        
                        // Reportar progreso
                        PrintProgress?.Invoke(this, new PrintProgressEventArgs
                        {
                            JobId = jobResult.JobId,
                            CurrentPage = _currentImageIndex,
                            TotalPages = _imagesToPrint.Count,
                            Status = PrintJobStatus.Printing,
                            Message = $"Imprimiendo página {_currentImageIndex} de {_imagesToPrint.Count}"
                        });
                        
                        e.HasMorePages = _currentImageIndex < _imagesToPrint.Count;
                    }
                    else
                    {
                        e.HasMorePages = false;
                    }
                };
                
                // Imprimir
                jobResult.Status = PrintJobStatus.Printing;
                printDoc.Print();
                
                // Éxito
                jobResult.Status = PrintJobStatus.Completed;
                jobResult.Success = true;
                
                _logger.LogInformation("Impresión completada: {JobName}, {PageCount} páginas en {PrinterName}",
                    jobResult.JobName, jobResult.PageCount, jobResult.PrinterName);
                
                PrintProgress?.Invoke(this, new PrintProgressEventArgs
                {
                    JobId = jobResult.JobId,
                    CurrentPage = jobResult.PageCount,
                    TotalPages = jobResult.PageCount,
                    Status = PrintJobStatus.Completed,
                    Message = "Impresión completada"
                });
                
                return Result<PrintJobResultDto>.Ok(jobResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al imprimir");
                jobResult.Status = PrintJobStatus.Failed;
                jobResult.Success = false;
                jobResult.ErrorMessage = ex.Message;
                
                PrintProgress?.Invoke(this, new PrintProgressEventArgs
                {
                    JobId = jobResult.JobId,
                    Status = PrintJobStatus.Failed,
                    Message = ex.Message
                });
                
                return Result<PrintJobResultDto>.Fail($"Error al imprimir: {ex.Message}");
            }
            finally
            {
                // Limpiar
                if (_imagesToPrint != null)
                {
                    foreach (var img in _imagesToPrint) img.Dispose();
                    _imagesToPrint = null;
                }
                _isPrinting = false;
                _currentOptions = null;
            }
        });
    }

    public Task<Result<PrintJobResultDto>> PrintHtmlAsync(string htmlContent, PrintOptionsDto? options = null)
    {
        // Para HTML, necesitaríamos un navegador/WebView
        // Por ahora, retornamos un mensaje indicando la limitación
        return Task.FromResult(Result<PrintJobResultDto>.Fail(
            "La impresión de HTML no está soportada. Use PrintPdfAsync con un PDF generado."));
    }

    #endregion

    #region Cola de Impresión

    public Task<Result<List<PrintQueueItemDto>>> GetPrintQueueAsync(string? printerName = null)
    {
        return Task.Run(() =>
        {
            try
            {
                // System.Drawing.Printing no tiene acceso directo a la cola
                // Se necesitaría P/Invoke a Win32 API o System.Printing
                // Por ahora, retornamos lista vacía
                _logger.LogDebug("GetPrintQueueAsync llamado - funcionalidad limitada en esta implementación");
                return Result<List<PrintQueueItemDto>>.Ok(new List<PrintQueueItemDto>());
            }
            catch (Exception ex)
            {
                return Result<List<PrintQueueItemDto>>.Fail($"Error: {ex.Message}");
            }
        });
    }

    public Task<Result<bool>> CancelPrintJobAsync(string jobId)
    {
        // Limitación de System.Drawing.Printing
        return Task.FromResult(Result<bool>.Fail(
            "Cancelación de trabajos no soportada en esta implementación"));
    }

    public Task<Result<int>> ClearPrintQueueAsync(string printerName)
    {
        // Limitación de System.Drawing.Printing
        return Task.FromResult(Result<int>.Fail(
            "Limpieza de cola no soportada en esta implementación"));
    }

    #endregion

    #region Utilidades

    public Task<Result<bool>> ShowPrintDialogAsync(byte[] pdfBytes)
    {
        return Task.Run(async () =>
        {
            try
            {
                // Guardar PDF temporal y abrir con aplicación predeterminada
                var tempPath = Path.Combine(Path.GetTempPath(), $"print_{Guid.NewGuid():N}.pdf");
                await File.WriteAllBytesAsync(tempPath, pdfBytes);
                
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = tempPath,
                    UseShellExecute = true,
                    Verb = "print"
                };
                
                System.Diagnostics.Process.Start(psi);
                
                // Limpiar después de un tiempo
                _ = Task.Delay(30000).ContinueWith(_ =>
                {
                    try { File.Delete(tempPath); } catch { }
                });
                
                return Result<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Fail($"Error al abrir diálogo: {ex.Message}");
            }
        });
    }

    public Task<Result<byte[]>> GeneratePreviewAsync(byte[] pdfBytes)
    {
        return Task.Run(async () =>
        {
            try
            {
                var images = await ConvertPdfToImagesAsync(pdfBytes);
                if (images.Count == 0)
                {
                    return Result<byte[]>.Fail("No se pudo generar vista previa");
                }
                
                var preview = ImageToBytes(images[0], 150); // Thumbnail a 150 DPI
                foreach (var img in images) img.Dispose();
                
                return Result<byte[]>.Ok(preview);
            }
            catch (Exception ex)
            {
                return Result<byte[]>.Fail($"Error al generar preview: {ex.Message}");
            }
        });
    }

    #endregion

    #region Helpers Privados

    private async Task<Result<PrintJobResultDto>> PrintPdfExternalAsync(byte[] pdfBytes, PrintOptionsDto? options)
    {
        try
        {
            options ??= new PrintOptionsDto();
            var jobResult = new PrintJobResultDto
            {
                JobName = options.JobName ?? $"PDF_{DateTime.Now:yyyyMMdd_HHmmss}",
                PrinterName = options.PrinterName ?? GetDefaultPrinterName() ?? "Predeterminada",
                Copies = options.Copies,
                PageCount = 1 // Desconocido para PDFs externos
            };
            
            // Guardar PDF temporal
            var tempPath = Path.Combine(Path.GetTempPath(), $"print_{Guid.NewGuid():N}.pdf");
            await File.WriteAllBytesAsync(tempPath, pdfBytes);
            
            // Configurar argumentos para SumatraPDF o Adobe Reader (si están instalados)
            // Fallback a impresión del sistema
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = tempPath,
                UseShellExecute = true,
                Verb = options.SilentPrint ? "printto" : "print"
            };
            
            if (options.SilentPrint && !string.IsNullOrEmpty(options.PrinterName))
            {
                psi.Arguments = $"\"{options.PrinterName}\"";
            }
            
            var process = System.Diagnostics.Process.Start(psi);
            
            // Para impresión silenciosa, esperamos que termine
            if (options.SilentPrint && process != null)
            {
                await process.WaitForExitAsync();
            }
            
            // Limpiar archivo temporal después de un tiempo
            _ = Task.Delay(60000).ContinueWith(_ =>
            {
                try { File.Delete(tempPath); } catch { }
            });
            
            jobResult.Success = true;
            jobResult.Status = PrintJobStatus.Completed;
            
            return Result<PrintJobResultDto>.Ok(jobResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al imprimir PDF externamente");
            return Result<PrintJobResultDto>.Fail($"Error al imprimir PDF: {ex.Message}");
        }
    }

    private Task<List<Image>> ConvertPdfToImagesAsync(byte[] pdfBytes)
    {
        // System.Drawing no puede renderizar PDFs directamente
        // Retornamos lista vacía para forzar el método externo
        // Para renderizado de PDF se necesitaría PDFium, Pdfium.Net, etc.
        return Task.FromResult(new List<Image>());
    }

    private void SetPaperSize(PrintDocument doc, PrintPaperSize size)
    {
        var paperName = size switch
        {
            PrintPaperSize.Letter => "Letter",
            PrintPaperSize.Legal => "Legal",
            PrintPaperSize.A4 => "A4",
            PrintPaperSize.A5 => "A5",
            _ => "Letter"
        };
        
        foreach (PaperSize ps in doc.PrinterSettings.PaperSizes)
        {
            if (ps.PaperName.Contains(paperName, StringComparison.OrdinalIgnoreCase))
            {
                doc.DefaultPageSettings.PaperSize = ps;
                break;
            }
        }
    }

    private void SetPrintQuality(PrintDocument doc, PrintQuality quality)
    {
        doc.DefaultPageSettings.PrinterResolution = quality switch
        {
            PrintQuality.Draft => GetResolution(doc, PrinterResolutionKind.Draft),
            PrintQuality.Normal => GetResolution(doc, PrinterResolutionKind.Medium),
            PrintQuality.High => GetResolution(doc, PrinterResolutionKind.High),
            _ => GetResolution(doc, PrinterResolutionKind.Medium)
        };
    }

    private PrinterResolution GetResolution(PrintDocument doc, PrinterResolutionKind kind)
    {
        foreach (PrinterResolution res in doc.PrinterSettings.PrinterResolutions)
        {
            if (res.Kind == kind)
                return res;
        }
        return doc.DefaultPageSettings.PrinterResolution;
    }

    private byte[] ImageToBytes(Image image, int? dpi = null)
    {
        using var ms = new MemoryStream();
        
        if (dpi.HasValue)
        {
            // Crear copia con DPI específico
            using var bmp = new Bitmap(image);
            bmp.SetResolution(dpi.Value, dpi.Value);
            bmp.Save(ms, ImageFormat.Png);
        }
        else
        {
            image.Save(ms, ImageFormat.Png);
        }
        
        return ms.ToArray();
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                if (_imagesToPrint != null)
                {
                    foreach (var img in _imagesToPrint) img.Dispose();
                    _imagesToPrint = null;
                }
            }
            _disposed = true;
        }
    }

    #endregion
}
