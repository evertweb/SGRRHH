using System.Collections.Concurrent;
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
/// Optimizado para velocidad y fiabilidad con escáneres Canon en red
/// </summary>
public class ScannerService : IScannerService
{
    private readonly ILogger<ScannerService> _logger;
    private readonly IDocumentoEmpleadoRepository _documentoRepository;
    private readonly ILocalStorageService _storageService;
    
    private CancellationTokenSource? _currentScanCts;
    private bool _disposed;
    
    // Caché de dispositivos para evitar búsquedas repetidas
    private static readonly ConcurrentDictionary<string, (List<ScannerDeviceDto> Devices, DateTime CachedAt)> _deviceCache = new();
    private static readonly TimeSpan DeviceCacheExpiration = TimeSpan.FromMinutes(2);
    
    // Timeouts configurables
    private const int DeviceDiscoveryTimeoutMs = 10000; // 10 segundos para buscar dispositivos
    private const int ScanOperationTimeoutMs = 120000;  // 2 minutos para escanear
    
    // Orden de prioridad de drivers (TWAIN primero para Canon - más rápido y estable)
    private static readonly Driver[] DriverPriority = { Driver.Twain, Driver.Wia, Driver.Escl };
    
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
    
    /// <summary>
    /// Invalida la caché de dispositivos para forzar re-escaneo
    /// </summary>
    public void InvalidateDeviceCache()
    {
        _deviceCache.Clear();
        _logger.LogInformation("Caché de dispositivos invalidada");
    }
    
    public async Task<Result<List<ScannerDeviceDto>>> GetAvailableScannersAsync()
    {
        var sw = Stopwatch.StartNew();
        try
        {
            _logger.LogInformation("[SCAN] ========== INICIO GetAvailableScannersAsync ==========");
            _logger.LogInformation("[SCAN] Timestamp: {Timestamp}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            
            // Verificar caché primero
            var cacheKey = "all_devices";
            if (_deviceCache.TryGetValue(cacheKey, out var cached) && 
                DateTime.Now - cached.CachedAt < DeviceCacheExpiration)
            {
                var cacheAge = DateTime.Now - cached.CachedAt;
                _logger.LogInformation("[SCAN] ✓ CACHE HIT - {Count} dispositivos (edad: {Age:F1}s)", 
                    cached.Devices.Count, cacheAge.TotalSeconds);
                foreach (var dev in cached.Devices)
                {
                    _logger.LogInformation("[SCAN]   → {Name} [{Driver}] ID={Id}", dev.Name, dev.Driver, dev.Id);
                }
                return Result<List<ScannerDeviceDto>>.Ok(cached.Devices);
            }
            
            _logger.LogInformation("[SCAN] CACHE MISS - Iniciando búsqueda de dispositivos...");
            _logger.LogInformation("[SCAN] Creando ScanningContext con GdiImageContext...");
            using var scanningContext = new ScanningContext(new GdiImageContext());
            
            // Configurar worker Win32 para TWAIN (requerido en procesos de 64-bit)
            _logger.LogInformation("[SCAN] Configurando Win32 Worker para compatibilidad TWAIN...");
            try
            {
                scanningContext.SetUpWin32Worker();
                _logger.LogInformation("[SCAN] ✓ Win32 Worker configurado OK");
            }
            catch (Exception exWorker)
            {
                _logger.LogWarning("[SCAN] ⚠ No se pudo configurar Win32 Worker: {Message}", exWorker.Message);
            }
            
            var controller = new ScanController(scanningContext);
            _logger.LogInformation("[SCAN] ScanController creado OK");
            
            var devices = new List<ScannerDeviceDto>();
            
            // Buscar en orden de prioridad (TWAIN primero para Canon)
            _logger.LogInformation("[SCAN] Orden de búsqueda: {Drivers}", string.Join(" → ", DriverPriority));
            
            foreach (var driver in DriverPriority)
            {
                var driverSw = Stopwatch.StartNew();
                _logger.LogInformation("[SCAN] ──── Buscando en driver: {Driver} (timeout: {Timeout}ms) ────", 
                    driver, DeviceDiscoveryTimeoutMs);
                try
                {
                    using var cts = new CancellationTokenSource(DeviceDiscoveryTimeoutMs);
                    var driverDevices = await controller.GetDeviceList(driver).WaitAsync(cts.Token);
                    var deviceList = driverDevices.ToList();
                    
                    _logger.LogInformation("[SCAN] {Driver}: Encontrados {Count} dispositivos en {Elapsed}ms", 
                        driver, deviceList.Count, driverSw.ElapsedMilliseconds);
                    
                    foreach (var device in deviceList)
                    {
                        _logger.LogInformation("[SCAN]   ├─ Dispositivo: {Name}", device.Name);
                        _logger.LogInformation("[SCAN]   │    ID: {Id}", device.ID);
                        
                        // Evitar duplicados por nombre
                        if (!devices.Any(d => d.Name == device.Name))
                        {
                            devices.Add(new ScannerDeviceDto
                            {
                                Id = device.ID,
                                Name = device.Name,
                                Driver = driver.ToString().ToUpper(),
                                IsAvailable = true
                            });
                            _logger.LogInformation("[SCAN]   └─ ✓ AGREGADO (nuevo)");
                        }
                        else
                        {
                            _logger.LogInformation("[SCAN]   └─ ⊘ OMITIDO (duplicado)");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("[SCAN] ⚠ {Driver}: TIMEOUT después de {Elapsed}ms", 
                        driver, driverSw.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("[SCAN] ✗ {Driver}: ERROR - {Message} (en {Elapsed}ms)", 
                        driver, ex.Message, driverSw.ElapsedMilliseconds);
                    _logger.LogDebug(ex, "[SCAN] Detalle del error en {Driver}", driver);
                }
            }
            
            // Actualizar caché
            _deviceCache[cacheKey] = (devices, DateTime.Now);
            
            _logger.LogInformation("[SCAN] ========== RESUMEN ==========");
            _logger.LogInformation("[SCAN] Total dispositivos encontrados: {Count}", devices.Count);
            _logger.LogInformation("[SCAN] Tiempo total: {Elapsed}ms", sw.ElapsedMilliseconds);
            foreach (var dev in devices)
            {
                _logger.LogInformation("[SCAN]   ✓ {Name} [{Driver}]", dev.Name, dev.Driver);
            }
            
            return Result<List<ScannerDeviceDto>>.Ok(devices);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SCAN] ✗✗✗ ERROR CRÍTICO al buscar escáneres (en {Elapsed}ms)", sw.ElapsedMilliseconds);
            _logger.LogError("[SCAN] Tipo: {Type}, Mensaje: {Message}", ex.GetType().Name, ex.Message);
            if (ex.InnerException != null)
            {
                _logger.LogError("[SCAN] InnerException: {Inner}", ex.InnerException.Message);
            }
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
        var sw = Stopwatch.StartNew();
        _logger.LogInformation("[PREVIEW] ========== INICIO PreviewScanAsync ==========");
        _logger.LogInformation("[PREVIEW] Timestamp: {Timestamp}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
        
        options ??= new ScanOptionsDto();
        _logger.LogInformation("[PREVIEW] Opciones recibidas - DeviceId: {DeviceId}, Source: {Source}", 
            options.DeviceId ?? "(auto)", options.Source);
        
        // Forzar configuración para preview ultra-rápido
        var previewOptions = new ScanOptionsDto
        {
            DeviceId = options.DeviceId,
            Source = options.Source,
            Dpi = 50, // Muy baja resolución para máxima velocidad
            ColorMode = ScanColorMode.Grayscale, // Escala de grises para reducir datos
            PageSize = options.PageSize
            // No aplicar correcciones de imagen en preview
        };
        
        _logger.LogInformation("[PREVIEW] Configuración preview: DPI={Dpi}, Color={Color}, PageSize={Size}", 
            previewOptions.Dpi, previewOptions.ColorMode, previewOptions.PageSize);
        
        var result = await ScanSinglePageAsync(previewOptions);
        
        _logger.LogInformation("[PREVIEW] Resultado: {Status} en {Elapsed}ms", 
            result.IsSuccess ? "✓ OK" : "✗ FALLO", sw.ElapsedMilliseconds);
        if (!result.IsSuccess)
        {
            _logger.LogWarning("[PREVIEW] Error: {Error}", result.Error);
        }
        
        return result;
    }
    
    public async Task<Result<ScannedDocumentDto>> ScanSinglePageAsync(ScanOptionsDto? options = null)
    {
        var sw = Stopwatch.StartNew();
        var stepSw = Stopwatch.StartNew();
        
        options ??= new ScanOptionsDto();
        
        try
        {
            _logger.LogInformation("[SCAN-SINGLE] ========== INICIO ScanSinglePageAsync ==========");
            _logger.LogInformation("[SCAN-SINGLE] Timestamp: {Timestamp}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            _logger.LogInformation("[SCAN-SINGLE] Configuración: DPI={Dpi}, Color={Color}, Source={Source}, PageSize={Size}", 
                options.Dpi, options.ColorMode, options.Source, options.PageSize);
            _logger.LogInformation("[SCAN-SINGLE] DeviceId: {DeviceId}", options.DeviceId ?? "(auto-selección)");
            
            _currentScanCts = new CancellationTokenSource();
            
            RaiseScanProgress(1, 0, "Inicializando escáner...");
            
            // Paso 1: Crear contexto
            stepSw.Restart();
            _logger.LogInformation("[SCAN-SINGLE] [1/6] Creando ScanningContext...");
            using var scanningContext = new ScanningContext(new GdiImageContext());
            
            // Configurar worker Win32 para TWAIN (requerido en procesos de 64-bit)
            try { scanningContext.SetUpWin32Worker(); }
            catch (Exception ex) { _logger.LogDebug("[SCAN-SINGLE] Win32 Worker: {Msg}", ex.Message); }
            
            var controller = new ScanController(scanningContext);
            _logger.LogInformation("[SCAN-SINGLE] [1/6] ✓ Contexto creado en {Elapsed}ms", stepSw.ElapsedMilliseconds);
            
            // Paso 2: Obtener dispositivo
            stepSw.Restart();
            _logger.LogInformation("[SCAN-SINGLE] [2/6] Buscando dispositivo de escaneo...");
            var device = await GetScanDevice(controller, options);
            if (device == null)
            {
                _logger.LogError("[SCAN-SINGLE] ✗ NO SE ENCONTRÓ ESCÁNER después de {Elapsed}ms", stepSw.ElapsedMilliseconds);
                return Result<ScannedDocumentDto>.Fail("No se encontró ningún escáner disponible");
            }
            _logger.LogInformation("[SCAN-SINGLE] [2/6] ✓ Dispositivo encontrado en {Elapsed}ms: {Name}", 
                stepSw.ElapsedMilliseconds, device.Name);
            
            // Paso 3: Configurar eventos
            _logger.LogInformation("[SCAN-SINGLE] [3/6] Configurando eventos de progreso...");
            controller.PageStart += (s, e) => 
            {
                _logger.LogInformation("[SCAN-SINGLE] >> PageStart: página {PageNumber}", e.PageNumber);
                RaiseScanProgress(e.PageNumber, 0, $"Escaneando página {e.PageNumber}...");
            };
            controller.PageProgress += (s, e) => 
            {
                _logger.LogDebug("[SCAN-SINGLE] >> PageProgress: {Progress:P0}", e.Progress);
                RaiseScanProgress(e.PageNumber, e.Progress, "Escaneando...");
            };
            
            // Paso 4: Configurar opciones de escaneo
            stepSw.Restart();
            _logger.LogInformation("[SCAN-SINGLE] [4/6] Construyendo opciones de escaneo...");
            var scanOptions = BuildScanOptions(device, options);
            _logger.LogInformation("[SCAN-SINGLE] [4/6] ✓ Opciones: DPI={Dpi}, Source={Source}, BitDepth={BitDepth}", 
                scanOptions.Dpi, scanOptions.PaperSource, scanOptions.BitDepth);
            
            RaiseScanProgress(1, 0.1, "Conectando con escáner...");
            
            // Paso 5: Escanear
            stepSw.Restart();
            _logger.LogInformation("[SCAN-SINGLE] [5/6] ▶▶▶ INICIANDO ESCANEO FÍSICO ▶▶▶");
            ProcessedImage? scannedImage = null;
            int imageCount = 0;
            await foreach (var image in controller.Scan(scanOptions, _currentScanCts.Token))
            {
                imageCount++;
                _logger.LogInformation("[SCAN-SINGLE] [5/6] Imagen recibida #{Count} en {Elapsed}ms", 
                    imageCount, stepSw.ElapsedMilliseconds);
                scannedImage = image;
                break; // Solo queremos una página
            }
            
            if (scannedImage == null)
            {
                _logger.LogError("[SCAN-SINGLE] ✗ El escáner no devolvió ninguna imagen (después de {Elapsed}ms)", 
                    stepSw.ElapsedMilliseconds);
                return Result<ScannedDocumentDto>.Fail("No se recibió ninguna imagen del escáner");
            }
            _logger.LogInformation("[SCAN-SINGLE] [5/6] ✓ Escaneo físico completado en {Elapsed}ms", stepSw.ElapsedMilliseconds);
            
            RaiseScanProgress(1, 0.9, "Procesando imagen...");
            
            // Paso 6: Convertir a bytes
            stepSw.Restart();
            _logger.LogInformation("[SCAN-SINGLE] [6/6] Convirtiendo imagen a JPEG...");
            var scanResult = await ConvertToScannedDocumentAsync(scannedImage, options.Dpi);
            _logger.LogInformation("[SCAN-SINGLE] [6/6] ✓ Conversión completada en {Elapsed}ms", stepSw.ElapsedMilliseconds);
            _logger.LogInformation("[SCAN-SINGLE] Resultado: {Width}x{Height}px, {Size}", 
                scanResult.Width, scanResult.Height, scanResult.FileSizeFormatted);
            
            RaiseScanProgress(1, 1.0, "Escaneo completado", isComplete: true);
            
            _logger.LogInformation("[SCAN-SINGLE] ========== COMPLETADO en {TotalElapsed}ms ==========", sw.ElapsedMilliseconds);
            
            return Result<ScannedDocumentDto>.Ok(scanResult);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("[SCAN-SINGLE] ⚠ Escaneo CANCELADO por el usuario (después de {Elapsed}ms)", sw.ElapsedMilliseconds);
            RaiseScanProgress(0, 0, "Escaneo cancelado", isComplete: true);
            return Result<ScannedDocumentDto>.Fail("Escaneo cancelado");
        }
        catch (Exception ex)
        {
            _logger.LogError("[SCAN-SINGLE] ✗✗✗ ERROR CRÍTICO (después de {Elapsed}ms)", sw.ElapsedMilliseconds);
            _logger.LogError("[SCAN-SINGLE] Tipo: {Type}", ex.GetType().FullName);
            _logger.LogError("[SCAN-SINGLE] Mensaje: {Message}", ex.Message);
            _logger.LogError("[SCAN-SINGLE] StackTrace: {Stack}", ex.StackTrace);
            if (ex.InnerException != null)
            {
                _logger.LogError("[SCAN-SINGLE] InnerException: {Type} - {Message}", 
                    ex.InnerException.GetType().Name, ex.InnerException.Message);
            }
            RaiseScanProgress(0, 0, $"Error: {ex.Message}", hasError: true, errorMessage: ex.Message);
            return Result<ScannedDocumentDto>.Fail($"Error de escaneo: {ex.Message}");
        }
        finally
        {
            _logger.LogDebug("[SCAN-SINGLE] Finally: limpiando recursos...");
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
            
            // Configurar worker Win32 para TWAIN (requerido en procesos de 64-bit)
            try { scanningContext.SetUpWin32Worker(); }
            catch { /* Worker ya configurado o no disponible */ }
            
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
            
            // Configurar worker Win32 para TWAIN (requerido en procesos de 64-bit)
            try { scanningContext.SetUpWin32Worker(); }
            catch { /* Worker ya configurado o no disponible */ }
            
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
        _logger.LogInformation("[CANCEL] CancelCurrentScan() llamado");
        if (_currentScanCts != null && !_currentScanCts.IsCancellationRequested)
        {
            _logger.LogWarning("[CANCEL] ⚠ Enviando señal de cancelación...");
            _currentScanCts.Cancel();
            _logger.LogInformation("[CANCEL] Señal de cancelación enviada");
        }
        else
        {
            _logger.LogDebug("[CANCEL] No hay escaneo activo para cancelar");
        }
    }
    
    #region Private Methods
    
    private async Task<ScanDevice?> GetScanDevice(ScanController controller, ScanOptionsDto options)
    {
        var sw = Stopwatch.StartNew();
        _logger.LogInformation("[GET-DEVICE] ── Buscando dispositivo de escaneo ──");
        _logger.LogInformation("[GET-DEVICE] DeviceId solicitado: {DeviceId}", options.DeviceId ?? "(ninguno - auto)");
        _logger.LogInformation("[GET-DEVICE] Orden de drivers: {Order}", string.Join(" → ", DriverPriority));
        
        // Si se especificó un dispositivo, buscarlo en orden de prioridad de drivers
        if (!string.IsNullOrEmpty(options.DeviceId))
        {
            _logger.LogInformation("[GET-DEVICE] Modo: Búsqueda por ID específico");
            foreach (var driver in DriverPriority)
            {
                var driverSw = Stopwatch.StartNew();
                _logger.LogInformation("[GET-DEVICE] Probando {Driver}...", driver);
                try
                {
                    using var cts = new CancellationTokenSource(DeviceDiscoveryTimeoutMs);
                    var devices = await controller.GetDeviceList(driver).WaitAsync(cts.Token);
                    var deviceList = devices.ToList();
                    _logger.LogInformation("[GET-DEVICE] {Driver}: {Count} dispositivos en {Elapsed}ms", 
                        driver, deviceList.Count, driverSw.ElapsedMilliseconds);
                    
                    foreach (var d in deviceList)
                    {
                        _logger.LogDebug("[GET-DEVICE]   - {Name} (ID={Id})", d.Name, d.ID);
                    }
                    
                    var device = deviceList.FirstOrDefault(d => d.ID == options.DeviceId);
                    if (device != null)
                    {
                        _logger.LogInformation("[GET-DEVICE] ✓ ENCONTRADO: {Name} via {Driver} (total: {Elapsed}ms)", 
                            device.Name, driver, sw.ElapsedMilliseconds);
                        return device;
                    }
                    _logger.LogInformation("[GET-DEVICE] {Driver}: ID no coincide, continuando...", driver);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("[GET-DEVICE] ⚠ {Driver}: TIMEOUT después de {Elapsed}ms", 
                        driver, driverSw.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("[GET-DEVICE] ✗ {Driver}: Error - {Message}", driver, ex.Message);
                }
            }
            _logger.LogWarning("[GET-DEVICE] ID '{DeviceId}' no encontrado en ningún driver", options.DeviceId);
        }
        
        // Si no se especificó o no se encontró, usar el primero disponible (preferir TWAIN para Canon)
        _logger.LogInformation("[GET-DEVICE] Modo: Auto-selección del primer dispositivo disponible");
        foreach (var driver in DriverPriority)
        {
            var driverSw = Stopwatch.StartNew();
            _logger.LogInformation("[GET-DEVICE] Probando {Driver}...", driver);
            try
            {
                using var cts = new CancellationTokenSource(DeviceDiscoveryTimeoutMs);
                var devices = await controller.GetDeviceList(driver).WaitAsync(cts.Token);
                var deviceList = devices.ToList();
                _logger.LogInformation("[GET-DEVICE] {Driver}: {Count} dispositivos en {Elapsed}ms", 
                    driver, deviceList.Count, driverSw.ElapsedMilliseconds);
                
                var device = deviceList.FirstOrDefault();
                if (device != null)
                {
                    _logger.LogInformation("[GET-DEVICE] ✓ AUTO-SELECCIONADO: {Name} via {Driver} (total: {Elapsed}ms)", 
                        device.Name, driver, sw.ElapsedMilliseconds);
                    return device;
                }
                _logger.LogInformation("[GET-DEVICE] {Driver}: Lista vacía, continuando...", driver);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("[GET-DEVICE] ⚠ {Driver}: TIMEOUT", driver);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("[GET-DEVICE] ✗ {Driver}: Error - {Message}", driver, ex.Message);
            }
        }
        
        _logger.LogError("[GET-DEVICE] ✗✗✗ NO SE ENCONTRÓ NINGÚN DISPOSITIVO (después de {Elapsed}ms)", sw.ElapsedMilliseconds);
        return null;
    }
    
    private ScanOptions BuildScanOptions(ScanDevice device, ScanOptionsDto options)
    {
        _logger.LogDebug("[BUILD-OPTIONS] Construyendo opciones de escaneo...");
        _logger.LogDebug("[BUILD-OPTIONS] Input - DPI: {Dpi}, Source: {Source}, Color: {Color}, PageSize: {Size}",
            options.Dpi, options.Source, options.ColorMode, options.PageSize);
        
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
        
        _logger.LogInformation("[BUILD-OPTIONS] Output - Device: {Device}, DPI: {Dpi}, Source: {Source}, BitDepth: {BitDepth}, PageSize: {Size}",
            device.Name, scanOptions.Dpi, scanOptions.PaperSource, scanOptions.BitDepth, scanOptions.PageSize);
        
        return scanOptions;
    }
    
    private async Task<ScannedDocumentDto> ConvertToScannedDocumentAsync(ProcessedImage image, int dpi)
    {
        var sw = Stopwatch.StartNew();
        _logger.LogInformation("[CONVERT] Iniciando conversión de imagen...");
        
        return await Task.Run(() =>
        {
            _logger.LogDebug("[CONVERT] Renderizando a Bitmap...");
            var renderSw = Stopwatch.StartNew();
            using var bitmap = image.RenderToBitmap();
            _logger.LogInformation("[CONVERT] Bitmap renderizado en {Elapsed}ms: {Width}x{Height}px", 
                renderSw.ElapsedMilliseconds, bitmap.Width, bitmap.Height);
            
            using var ms = new MemoryStream();
            
            // Usar calidad adaptativa según DPI para balance velocidad/calidad
            long quality = dpi switch
            {
                <= 75 => 60L,   // Preview: máxima velocidad
                <= 150 => 75L,  // Rápido: buena velocidad
                <= 300 => 85L,  // Normal: buen balance
                _ => 92L        // Alta calidad: prioriza fidelidad
            };
            _logger.LogDebug("[CONVERT] Calidad JPEG seleccionada: {Quality}% (para {Dpi} DPI)", quality, dpi);
            
            var encodeSw = Stopwatch.StartNew();
            var encoder = GetEncoder(ImageFormat.Jpeg);
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);
            
            bitmap.Save(ms, encoder, encoderParams);
            _logger.LogInformation("[CONVERT] Codificación JPEG completada en {Elapsed}ms", encodeSw.ElapsedMilliseconds);
            
            var sizeKB = ms.Length / 1024.0;
            _logger.LogInformation("[CONVERT] ✓ Conversión completa: {Width}x{Height} @ {Dpi}DPI → {Size:F1}KB (calidad {Quality}%) en {Total}ms",
                bitmap.Width, bitmap.Height, dpi, sizeKB, quality, sw.ElapsedMilliseconds);
            
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
