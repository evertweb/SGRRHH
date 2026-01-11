using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using NAPS2.Images;
using NAPS2.Images.Gdi;
using NAPS2.Pdf;
using NAPS2.Scan;
using SGRRHH.Local.Domain.DTOs;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Shared;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de escaneo usando NAPS2.Sdk
/// </summary>
public class ScannerService : IScannerService
{
    private readonly ILogger<ScannerService> _logger;
    private readonly IDocumentoEmpleadoRepository _documentoRepository;
    private readonly ILocalStorageService _storageService;
    
    private CancellationTokenSource? _currentScanCts;
    private bool _disposed;
    
    public event EventHandler<ScanProgressEventArgs>? ScanProgress;
    
    public bool IsScanningInProgress => _currentScanCts != null && !_currentScanCts.IsCancellationRequested;
    
    public ScannerService(
        ILogger<ScannerService> logger,
        IDocumentoEmpleadoRepository documentoRepository,
        ILocalStorageService storageService)
    {
        _logger = logger;
        _documentoRepository = documentoRepository;
        _storageService = storageService;
    }
    
    public async Task<Result<List<ScannerDeviceDto>>> GetAvailableScannersAsync()
    {
        try
        {
            _logger.LogInformation("Buscando escáneres disponibles...");
            
            using var scanningContext = new ScanningContext(new GdiImageContext());
            var controller = new ScanController(scanningContext);
            
            var devices = new List<ScannerDeviceDto>();
            
            // Buscar dispositivos WIA (Windows Image Acquisition)
            try
            {
                var wiaDevices = await controller.GetDeviceList(Driver.Wia);
                foreach (var device in wiaDevices)
                {
                    devices.Add(new ScannerDeviceDto
                    {
                        Id = device.ID,
                        Name = device.Name,
                        Driver = "WIA",
                        IsAvailable = true
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudieron obtener dispositivos WIA");
            }
            
            // Buscar dispositivos TWAIN
            try
            {
                var twainDevices = await controller.GetDeviceList(Driver.Twain);
                foreach (var device in twainDevices)
                {
                    // Evitar duplicados
                    if (!devices.Any(d => d.Name == device.Name))
                    {
                        devices.Add(new ScannerDeviceDto
                        {
                            Id = device.ID,
                            Name = device.Name,
                            Driver = "TWAIN",
                            IsAvailable = true
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudieron obtener dispositivos TWAIN");
            }
            
            // Buscar dispositivos ESCL (eSCL/AirPrint - escáneres de red)
            try
            {
                var esclDevices = await controller.GetDeviceList(Driver.Escl);
                foreach (var device in esclDevices)
                {
                    if (!devices.Any(d => d.Name == device.Name))
                    {
                        devices.Add(new ScannerDeviceDto
                        {
                            Id = device.ID,
                            Name = device.Name,
                            Driver = "ESCL",
                            IsAvailable = true
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudieron obtener dispositivos ESCL");
            }
            
            _logger.LogInformation("Se encontraron {Count} escáneres", devices.Count);
            
            return Result<List<ScannerDeviceDto>>.Ok(devices);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar escáneres");
            return Result<List<ScannerDeviceDto>>.Fail($"Error al buscar escáneres: {ex.Message}");
        }
    }
    
    public async Task<bool> HasAvailableScannersAsync()
    {
        var result = await GetAvailableScannersAsync();
        return result.IsSuccess && result.Value != null && result.Value.Count > 0;
    }
    
    public async Task<Result<ScannerDeviceDto>> GetScannerInfoAsync(string deviceId)
    {
        var result = await GetAvailableScannersAsync();
        if (!result.IsSuccess || result.Value == null)
            return Result<ScannerDeviceDto>.Fail(result.Error ?? "Error desconocido");
        
        var device = result.Value.FirstOrDefault(d => d.Id == deviceId);
        if (device == null)
            return Result<ScannerDeviceDto>.Fail($"Escáner no encontrado: {deviceId}");
        
        return Result<ScannerDeviceDto>.Ok(device);
    }
    
    public async Task<Result<ScannedDocumentDto>> PreviewScanAsync(ScanOptionsDto? options = null)
    {
        options ??= new ScanOptionsDto();
        
        // Forzar configuración para preview rápido (baja resolución)
        var previewOptions = new ScanOptionsDto
        {
            DeviceId = options.DeviceId,
            Source = options.Source,
            Dpi = 75, // Baja resolución para velocidad
            ColorMode = options.ColorMode,
            PageSize = options.PageSize
            // No aplicar correcciones de imagen en preview
        };
        
        _logger.LogInformation("Realizando pre-escaneo de vista previa a {Dpi} DPI", previewOptions.Dpi);
        
        return await ScanSinglePageAsync(previewOptions);
    }
    
    public async Task<Result<ScannedDocumentDto>> ScanSinglePageAsync(ScanOptionsDto? options = null)
    {
        options ??= new ScanOptionsDto();
        
        try
        {
            _logger.LogInformation("Iniciando escaneo de página única. DPI: {Dpi}, Color: {Color}", 
                options.Dpi, options.ColorMode);
            
            _currentScanCts = new CancellationTokenSource();
            
            RaiseScanProgress(1, 0, "Inicializando escáner...");
            
            using var scanningContext = new ScanningContext(new GdiImageContext());
            var controller = new ScanController(scanningContext);
            
            // Obtener dispositivo
            var device = await GetScanDevice(controller, options);
            if (device == null)
            {
                return Result<ScannedDocumentDto>.Fail("No se encontró ningún escáner disponible");
            }
            
            // Configurar eventos de progreso
            controller.PageStart += (s, e) => RaiseScanProgress(e.PageNumber, 0, $"Escaneando página {e.PageNumber}...");
            controller.PageProgress += (s, e) => RaiseScanProgress(e.PageNumber, e.Progress, "Escaneando...");
            
            // Configurar opciones de escaneo
            var scanOptions = BuildScanOptions(device, options);
            
            RaiseScanProgress(1, 0.1, "Conectando con escáner...");
            
            // Escanear
            ProcessedImage? scannedImage = null;
            await foreach (var image in controller.Scan(scanOptions, _currentScanCts.Token))
            {
                scannedImage = image;
                break; // Solo queremos una página
            }
            
            if (scannedImage == null)
            {
                return Result<ScannedDocumentDto>.Fail("No se recibió ninguna imagen del escáner");
            }
            
            RaiseScanProgress(1, 0.9, "Procesando imagen...");
            
            // Convertir a bytes
            var scanResult = await ConvertToScannedDocumentAsync(scannedImage, options.Dpi);
            
            RaiseScanProgress(1, 1.0, "Escaneo completado", isComplete: true);
            
            _logger.LogInformation("Escaneo completado. Tamaño: {Size}", scanResult.FileSizeFormatted);
            
            return Result<ScannedDocumentDto>.Ok(scanResult);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Escaneo cancelado por el usuario");
            RaiseScanProgress(0, 0, "Escaneo cancelado", isComplete: true);
            return Result<ScannedDocumentDto>.Fail("Escaneo cancelado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el escaneo");
            RaiseScanProgress(0, 0, $"Error: {ex.Message}", hasError: true, errorMessage: ex.Message);
            return Result<ScannedDocumentDto>.Fail($"Error de escaneo: {ex.Message}");
        }
        finally
        {
            _currentScanCts?.Dispose();
            _currentScanCts = null;
        }
    }
    
    public async IAsyncEnumerable<ScannedPageDto> ScanMultiplePagesAsync(
        ScanOptionsDto? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        options ??= new ScanOptionsDto();
        
        _logger.LogInformation("Iniciando escaneo de múltiples páginas");
        
        _currentScanCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        
        try
        {
            using var scanningContext = new ScanningContext(new GdiImageContext());
            var controller = new ScanController(scanningContext);
            
            var device = await GetScanDevice(controller, options);
            if (device == null)
            {
                RaiseScanProgress(0, 0, "No se encontró escáner", hasError: true);
                yield break;
            }
            
            var scanOptions = BuildScanOptions(device, options);
            
            int pageNumber = 0;
            
            controller.PageStart += (s, e) => RaiseScanProgress(e.PageNumber, 0, $"Escaneando página {e.PageNumber}...");
            controller.PageProgress += (s, e) => RaiseScanProgress(e.PageNumber, e.Progress, "Escaneando...");
            
            await foreach (var image in controller.Scan(scanOptions, _currentScanCts.Token))
            {
                pageNumber++;
                
                RaiseScanProgress(pageNumber, 0.9, $"Procesando página {pageNumber}...");
                
                var scannedDoc = await ConvertToScannedDocumentAsync(image, options.Dpi);
                
                yield return new ScannedPageDto
                {
                    ImageBytes = scannedDoc.ImageBytes,
                    MimeType = scannedDoc.MimeType,
                    Width = scannedDoc.Width,
                    Height = scannedDoc.Height,
                    Dpi = scannedDoc.Dpi,
                    ScannedAt = scannedDoc.ScannedAt,
                    PageNumber = pageNumber
                };
                
                RaiseScanProgress(pageNumber, 1.0, $"Página {pageNumber} completada");
            }
            
            RaiseScanProgress(pageNumber, 1.0, $"Escaneo completado. {pageNumber} página(s)", isComplete: true);
        }
        finally
        {
            _currentScanCts?.Dispose();
            _currentScanCts = null;
        }
    }
    
    public async Task<Result<BatchScanResultDto>> ScanBatchAsync(
        ScanOptionsDto? options = null,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new BatchScanResultDto();
        
        try
        {
            await foreach (var page in ScanMultiplePagesAsync(options, cancellationToken))
            {
                result.Pages.Add(page);
            }
            
            result.Success = true;
            result.Duration = stopwatch.Elapsed;
            
            _logger.LogInformation("Escaneo batch completado. {Pages} páginas en {Duration}", 
                result.TotalPages, result.Duration);
            
            return Result<BatchScanResultDto>.Ok(result);
        }
        catch (OperationCanceledException)
        {
            result.Success = false;
            result.ErrorMessage = "Escaneo cancelado";
            result.Duration = stopwatch.Elapsed;
            return Result<BatchScanResultDto>.Fail("Escaneo cancelado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en escaneo batch");
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.Duration = stopwatch.Elapsed;
            return Result<BatchScanResultDto>.Fail(ex.Message);
        }
    }
    
    public async Task<Result<DocumentoEmpleado>> ScanAndSaveDocumentAsync(
        int empleadoId,
        TipoDocumentoEmpleado tipoDocumento,
        string nombreDocumento,
        ScanOptionsDto? options = null)
    {
        try
        {
            _logger.LogInformation("Escaneando documento para empleado {EmpleadoId}: {Tipo}", 
                empleadoId, tipoDocumento);
            
            // Escanear
            var scanResult = await ScanSinglePageAsync(options);
            if (!scanResult.IsSuccess || scanResult.Value == null)
            {
                return Result<DocumentoEmpleado>.Fail(scanResult.Error ?? "Error al escanear");
            }
            
            var scannedDoc = scanResult.Value;
            
            // Determinar extensión
            var extension = scannedDoc.MimeType switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "application/pdf" => ".pdf",
                _ => ".png"
            };
            
            // Generar nombre de archivo único
            var fileName = $"{tipoDocumento}_{empleadoId}_{DateTime.Now:yyyyMMdd_HHmmss}{extension}";
            
            // Guardar archivo usando el método existente de ILocalStorageService
            var saveResult = await _storageService.SaveDocumentoEmpleadoAsync(
                empleadoId, 
                tipoDocumento.ToString(), 
                scannedDoc.ImageBytes, 
                fileName);
                
            if (!saveResult.IsSuccess || saveResult.Value == null)
            {
                return Result<DocumentoEmpleado>.Fail($"Error guardando archivo: {saveResult.Error}");
            }
            
            // Crear registro en BD
            var documento = new DocumentoEmpleado
            {
                EmpleadoId = empleadoId,
                TipoDocumento = tipoDocumento,
                Nombre = nombreDocumento,
                Descripcion = $"Documento escaneado el {DateTime.Now:dd/MM/yyyy HH:mm}",
                ArchivoPath = saveResult.Value,
                NombreArchivoOriginal = fileName,
                TamanoArchivo = scannedDoc.ImageBytes.Length,
                TipoMime = scannedDoc.MimeType,
                FechaEmision = DateTime.Today,
                FechaCreacion = DateTime.Now
            };
            
            var savedDocumento = await _documentoRepository.AddAsync(documento);
            
            _logger.LogInformation("Documento escaneado y guardado. ID: {DocId}, Empleado: {EmpleadoId}", 
                savedDocumento.Id, empleadoId);
            
            return Result<DocumentoEmpleado>.Ok(savedDocumento);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al escanear y guardar documento");
            return Result<DocumentoEmpleado>.Fail($"Error: {ex.Message}");
        }
    }
    
    public async Task<Result<byte[]>> ScanToPdfAsync(
        ScanOptionsDto? options = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Iniciando escaneo a PDF");
            
            options ??= new ScanOptionsDto();
            _currentScanCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            
            using var scanningContext = new ScanningContext(new GdiImageContext());
            var controller = new ScanController(scanningContext);
            
            var device = await GetScanDevice(controller, options);
            if (device == null)
            {
                return Result<byte[]>.Fail("No se encontró ningún escáner disponible");
            }
            
            var scanOptions = BuildScanOptions(device, options);
            
            // Escanear todas las páginas
            var images = new List<ProcessedImage>();
            int pageNum = 0;
            
            controller.PageStart += (s, e) => RaiseScanProgress(e.PageNumber, 0, $"Escaneando página {e.PageNumber}...");
            controller.PageProgress += (s, e) => RaiseScanProgress(e.PageNumber, e.Progress, "Escaneando...");
            
            await foreach (var image in controller.Scan(scanOptions, _currentScanCts.Token))
            {
                pageNum++;
                images.Add(image);
                RaiseScanProgress(pageNum, 1.0, $"Página {pageNum} capturada");
            }
            
            if (images.Count == 0)
            {
                return Result<byte[]>.Fail("No se escanearon páginas");
            }
            
            RaiseScanProgress(pageNum, 0.5, "Generando PDF...");
            
            // Exportar a PDF
            var pdfExporter = new PdfExporter(scanningContext);
            using var memoryStream = new MemoryStream();
            await pdfExporter.Export(memoryStream, images);
            
            var pdfBytes = memoryStream.ToArray();
            
            // Limpiar imágenes
            foreach (var img in images)
            {
                img.Dispose();
            }
            
            RaiseScanProgress(pageNum, 1.0, $"PDF generado con {pageNum} página(s)", isComplete: true);
            
            _logger.LogInformation("PDF generado exitosamente. {Pages} páginas, {Size} bytes", 
                pageNum, pdfBytes.Length);
            
            return Result<byte[]>.Ok(pdfBytes);
        }
        catch (OperationCanceledException)
        {
            return Result<byte[]>.Fail("Escaneo cancelado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar PDF");
            return Result<byte[]>.Fail($"Error: {ex.Message}");
        }
        finally
        {
            _currentScanCts?.Dispose();
            _currentScanCts = null;
        }
    }
    
    public void CancelCurrentScan()
    {
        if (_currentScanCts != null && !_currentScanCts.IsCancellationRequested)
        {
            _logger.LogInformation("Cancelando escaneo en progreso...");
            _currentScanCts.Cancel();
        }
    }
    
    #region Private Methods
    
    private async Task<ScanDevice?> GetScanDevice(ScanController controller, ScanOptionsDto options)
    {
        // Si se especificó un dispositivo, buscarlo
        if (!string.IsNullOrEmpty(options.DeviceId))
        {
            // Intentar WIA primero
            var wiaDevices = await controller.GetDeviceList(Driver.Wia);
            var device = wiaDevices.FirstOrDefault(d => d.ID == options.DeviceId);
            if (device != null) return device;
            
            // Luego TWAIN
            var twainDevices = await controller.GetDeviceList(Driver.Twain);
            device = twainDevices.FirstOrDefault(d => d.ID == options.DeviceId);
            if (device != null) return device;
            
            // Finalmente ESCL
            var esclDevices = await controller.GetDeviceList(Driver.Escl);
            device = esclDevices.FirstOrDefault(d => d.ID == options.DeviceId);
            if (device != null) return device;
        }
        
        // Si no se especificó o no se encontró, usar el primero disponible
        var defaultDevices = await controller.GetDeviceList();
        return defaultDevices.FirstOrDefault();
    }
    
    private ScanOptions BuildScanOptions(ScanDevice device, ScanOptionsDto options)
    {
        var scanOptions = new ScanOptions
        {
            Device = device,
            Dpi = options.Dpi,
            PaperSource = options.Source switch
            {
                ScanSource.Feeder => PaperSource.Feeder,
                ScanSource.Auto => PaperSource.Auto,
                _ => PaperSource.Flatbed
            },
            PageSize = options.PageSize switch
            {
                ScanPageSize.A4 => PageSize.A4,
                ScanPageSize.A5 => PageSize.A5,
                ScanPageSize.Legal => PageSize.Legal,
                _ => PageSize.Letter
            },
            BitDepth = options.ColorMode switch
            {
                ScanColorMode.Grayscale => BitDepth.Grayscale,
                ScanColorMode.BlackWhite => BitDepth.BlackAndWhite,
                _ => BitDepth.Color
            }
        };
        
        return scanOptions;
    }
    
    private async Task<ScannedDocumentDto> ConvertToScannedDocumentAsync(ProcessedImage image, int dpi)
    {
        return await Task.Run(() =>
        {
            using var bitmap = image.RenderToBitmap();
            
            using var ms = new MemoryStream();
            
            // Guardar como JPEG para mejor compresión
            var encoder = GetEncoder(ImageFormat.Jpeg);
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, 85L);
            
            bitmap.Save(ms, encoder, encoderParams);
            
            return new ScannedDocumentDto
            {
                ImageBytes = ms.ToArray(),
                MimeType = "image/jpeg",
                Width = bitmap.Width,
                Height = bitmap.Height,
                Dpi = dpi,
                ScannedAt = DateTime.Now
            };
        });
    }
    
    private ImageCodecInfo GetEncoder(ImageFormat format)
    {
        var codecs = ImageCodecInfo.GetImageEncoders();
        return codecs.First(codec => codec.FormatID == format.Guid);
    }
    
    private void RaiseScanProgress(int currentPage, double progress, string status, 
        bool isComplete = false, bool hasError = false, string? errorMessage = null)
    {
        ScanProgress?.Invoke(this, new ScanProgressEventArgs
        {
            CurrentPage = currentPage,
            Progress = progress,
            Status = status,
            IsComplete = isComplete,
            HasError = hasError,
            ErrorMessage = errorMessage
        });
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
        if (_disposed) return;
        
        if (disposing)
        {
            _currentScanCts?.Cancel();
            _currentScanCts?.Dispose();
        }
        
        _disposed = true;
    }
    
    #endregion
}
