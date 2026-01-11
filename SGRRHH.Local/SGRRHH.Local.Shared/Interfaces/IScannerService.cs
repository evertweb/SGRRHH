using SGRRHH.Local.Domain.DTOs;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Shared;

namespace SGRRHH.Local.Shared.Interfaces;

/// <summary>
/// Servicio para escanear documentos usando NAPS2.Sdk
/// </summary>
public interface IScannerService : IDisposable
{
    /// <summary>
    /// Evento disparado cuando hay progreso en el escaneo
    /// </summary>
    event EventHandler<ScanProgressEventArgs>? ScanProgress;
    
    /// <summary>
    /// Indica si hay un escaneo en progreso
    /// </summary>
    bool IsScanningInProgress { get; }
    
    /// <summary>
    /// Obtiene la lista de escáneres disponibles en el sistema
    /// </summary>
    /// <returns>Lista de dispositivos de escaneo</returns>
    Task<Result<List<ScannerDeviceDto>>> GetAvailableScannersAsync();
    
    /// <summary>
    /// Escanea una sola página (modo flatbed)
    /// </summary>
    /// <param name="options">Opciones de escaneo</param>
    /// <returns>Documento escaneado</returns>
    Task<Result<ScannedDocumentDto>> ScanSinglePageAsync(ScanOptionsDto? options = null);
    
    /// <summary>
    /// Escanea múltiples páginas (batch mode)
    /// Usa el alimentador automático si está disponible, o permite agregar páginas manualmente
    /// </summary>
    /// <param name="options">Opciones de escaneo</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Enumerador asíncrono de páginas escaneadas</returns>
    IAsyncEnumerable<ScannedPageDto> ScanMultiplePagesAsync(
        ScanOptionsDto? options = null, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Escanea todas las páginas disponibles y retorna el resultado completo
    /// </summary>
    /// <param name="options">Opciones de escaneo</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado con todas las páginas</returns>
    Task<Result<BatchScanResultDto>> ScanBatchAsync(
        ScanOptionsDto? options = null, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Escanea un documento y lo guarda directamente como documento de empleado
    /// </summary>
    /// <param name="empleadoId">ID del empleado</param>
    /// <param name="tipoDocumento">Tipo de documento</param>
    /// <param name="nombreDocumento">Nombre descriptivo del documento</param>
    /// <param name="options">Opciones de escaneo</param>
    /// <returns>El documento guardado</returns>
    Task<Result<DocumentoEmpleado>> ScanAndSaveDocumentAsync(
        int empleadoId,
        TipoDocumentoEmpleado tipoDocumento,
        string nombreDocumento,
        ScanOptionsDto? options = null);
    
    /// <summary>
    /// Escanea múltiples páginas y las combina en un solo PDF
    /// </summary>
    /// <param name="options">Opciones de escaneo</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>PDF con todas las páginas</returns>
    Task<Result<byte[]>> ScanToPdfAsync(
        ScanOptionsDto? options = null, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cancela el escaneo en progreso
    /// </summary>
    void CancelCurrentScan();
    
    /// <summary>
    /// Verifica si hay escáneres disponibles
    /// </summary>
    /// <returns>True si hay al menos un escáner</returns>
    Task<bool> HasAvailableScannersAsync();
    
    /// <summary>
    /// Obtiene información de un escáner específico
    /// </summary>
    /// <param name="deviceId">ID del dispositivo</param>
    /// <returns>Información del escáner</returns>
    Task<Result<ScannerDeviceDto>> GetScannerInfoAsync(string deviceId);
    
    /// <summary>
    /// Realiza un pre-escaneo rápido en baja resolución para vista previa
    /// </summary>
    /// <param name="options">Opciones de escaneo (se forzará DPI bajo)</param>
    /// <returns>Imagen de preview</returns>
    Task<Result<ScannedDocumentDto>> PreviewScanAsync(ScanOptionsDto? options = null);
}
