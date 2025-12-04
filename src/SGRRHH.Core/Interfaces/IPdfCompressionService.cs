using SGRRHH.Core.Common;

namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Servicio para comprimir archivos PDF grandes usando iLovePDF API
/// </summary>
public interface IPdfCompressionService
{
    /// <summary>
    /// Tamaño máximo en bytes antes de aplicar compresión (5MB por defecto)
    /// </summary>
    long MaxFileSizeBeforeCompression { get; }
    
    /// <summary>
    /// Indica si el servicio está configurado correctamente
    /// </summary>
    bool IsConfigured { get; }
    
    /// <summary>
    /// Comprime un archivo PDF si excede el tamaño máximo permitido
    /// </summary>
    /// <param name="pdfBytes">Bytes del archivo PDF</param>
    /// <param name="fileName">Nombre del archivo (para logging)</param>
    /// <returns>Bytes comprimidos del PDF, o los bytes originales si no necesita compresión</returns>
    Task<ServiceResult<byte[]>> CompressIfNeededAsync(byte[] pdfBytes, string fileName);
    
    /// <summary>
    /// Comprime un archivo PDF sin importar el tamaño
    /// </summary>
    /// <param name="pdfBytes">Bytes del archivo PDF</param>
    /// <param name="fileName">Nombre del archivo</param>
    /// <returns>Bytes comprimidos del PDF</returns>
    Task<ServiceResult<byte[]>> CompressAsync(byte[] pdfBytes, string fileName);
    
    /// <summary>
    /// Verifica si un archivo necesita compresión según su tamaño
    /// </summary>
    /// <param name="fileSizeBytes">Tamaño del archivo en bytes</param>
    /// <returns>True si necesita compresión</returns>
    bool NeedsCompression(long fileSizeBytes);
    
    /// <summary>
    /// Verifica si un archivo es un PDF válido
    /// </summary>
    /// <param name="fileName">Nombre del archivo</param>
    /// <returns>True si es un PDF</returns>
    bool IsPdfFile(string fileName);
}
