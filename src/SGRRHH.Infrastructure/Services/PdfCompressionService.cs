using iLovePdf.Core;
using iLovePdf.Model.Enums;
using iLovePdf.Model.Task;
using iLovePdf.Model.TaskParams;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SGRRHH.Core.Common;
using SGRRHH.Core.Interfaces;
using System.IO;

namespace SGRRHH.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de compresión de PDF usando iLovePDF API
/// </summary>
public class PdfCompressionService : IPdfCompressionService
{
    private readonly ILogger<PdfCompressionService>? _logger;
    private readonly string? _publicKey;
    private readonly string? _secretKey;
    private readonly CompressionLevels _compressionLevel;
    
    /// <summary>
    /// Tamaño máximo antes de comprimir (5 MB)
    /// </summary>
    public long MaxFileSizeBeforeCompression { get; }
    
    /// <summary>
    /// Indica si el servicio está configurado con las credenciales de iLovePDF
    /// </summary>
    public bool IsConfigured => !string.IsNullOrWhiteSpace(_publicKey) && !string.IsNullOrWhiteSpace(_secretKey);
    
    public PdfCompressionService(IConfiguration configuration, ILogger<PdfCompressionService>? logger = null)
    {
        _logger = logger;
        
        // Leer configuración de iLovePDF
        _publicKey = configuration["ILovePDF:PublicKey"];
        _secretKey = configuration["ILovePDF:SecretKey"];
        
        // Leer nivel de compresión (por defecto: Recommended)
        var levelConfig = configuration["ILovePDF:CompressionLevel"] ?? "Recommended";
        _compressionLevel = levelConfig.ToLowerInvariant() switch
        {
            "low" => CompressionLevels.Low,
            "recommended" => CompressionLevels.Recommended,
            "extreme" => CompressionLevels.Extreme,
            _ => CompressionLevels.Recommended
        };
        
        // Leer tamaño máximo (por defecto: 5 MB)
        int maxSizeMb = 5;
        var maxSizeMbStr = configuration["ILovePDF:MaxFileSizeMB"];
        if (!string.IsNullOrEmpty(maxSizeMbStr) && int.TryParse(maxSizeMbStr, out var parsedSize))
        {
            maxSizeMb = parsedSize;
        }
        MaxFileSizeBeforeCompression = maxSizeMb * 1024 * 1024; // Convertir a bytes
        
        if (!IsConfigured)
        {
            _logger?.LogWarning("⚠️ iLovePDF no está configurado. Los PDFs grandes no serán comprimidos automáticamente.");
        }
        else
        {
            _logger?.LogInformation("✅ iLovePDF configurado correctamente. Compresión activada para PDFs > {MaxSize} MB", maxSizeMb);
        }
    }
    
    /// <inheritdoc/>
    public bool NeedsCompression(long fileSizeBytes)
    {
        return fileSizeBytes > MaxFileSizeBeforeCompression;
    }
    
    /// <inheritdoc/>
    public bool IsPdfFile(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;
            
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension == ".pdf";
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<byte[]>> CompressIfNeededAsync(byte[] pdfBytes, string fileName)
    {
        if (pdfBytes == null || pdfBytes.Length == 0)
        {
            return ServiceResult<byte[]>.Fail("El archivo está vacío");
        }
        
        // Verificar si es PDF
        if (!IsPdfFile(fileName))
        {
            _logger?.LogDebug("El archivo {FileName} no es PDF, se omite compresión", fileName);
            return ServiceResult<byte[]>.Ok(pdfBytes, "El archivo no es PDF, no requiere compresión");
        }
        
        // Verificar si necesita compresión
        if (!NeedsCompression(pdfBytes.Length))
        {
            var sizeMb = pdfBytes.Length / (1024.0 * 1024.0);
            _logger?.LogDebug("El PDF {FileName} ({Size:F2} MB) no requiere compresión", fileName, sizeMb);
            return ServiceResult<byte[]>.Ok(pdfBytes, $"El archivo ({sizeMb:F2} MB) no excede el límite de {MaxFileSizeBeforeCompression / (1024.0 * 1024.0):F0} MB");
        }
        
        // Comprimir
        return await CompressAsync(pdfBytes, fileName);
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<byte[]>> CompressAsync(byte[] pdfBytes, string fileName)
    {
        if (!IsConfigured)
        {
            _logger?.LogWarning("iLovePDF no configurado, devolviendo PDF original");
            return ServiceResult<byte[]>.Ok(pdfBytes, "iLovePDF no está configurado, se guarda sin compresión");
        }
        
        if (pdfBytes == null || pdfBytes.Length == 0)
        {
            return ServiceResult<byte[]>.Fail("El archivo está vacío");
        }
        
        if (!IsPdfFile(fileName))
        {
            return ServiceResult<byte[]>.Fail("El archivo no es un PDF");
        }
        
        var originalSizeMb = pdfBytes.Length / (1024.0 * 1024.0);
        _logger?.LogInformation("📄 Comprimiendo PDF: {FileName} ({Size:F2} MB)...", fileName, originalSizeMb);
        
        try
        {
            return await Task.Run(async () =>
            {
                // Crear cliente de iLovePDF
                var api = new iLovePdfApi(_publicKey!, _secretKey!);
                
                // Crear tarea de compresión
                var task = api.CreateTask<CompressTask>();
                
                // Crear archivo temporal
                var tempPath = Path.Combine(Path.GetTempPath(), $"compress_{Guid.NewGuid()}.pdf");
                var outputPath = Path.Combine(Path.GetTempPath(), $"compressed_{Guid.NewGuid()}");
                
                try
                {
                    // Guardar archivo original temporalmente
                    await File.WriteAllBytesAsync(tempPath, pdfBytes);
                    
                    // Subir archivo
                    task.AddFile(tempPath);
                    
                    // Configurar parámetros de compresión
                    var parameters = new CompressParams
                    {
                        CompressionLevel = _compressionLevel,
                        OutputFileName = Path.GetFileNameWithoutExtension(fileName)
                    };
                    
                    // Procesar
                    task.Process(parameters);
                    
                    // Crear directorio de salida
                    Directory.CreateDirectory(outputPath);
                    
                    // Descargar resultado
                    task.DownloadFile(outputPath);
                    
                    // Buscar el archivo descargado
                    var downloadedFiles = Directory.GetFiles(outputPath, "*.pdf");
                    if (downloadedFiles.Length == 0)
                    {
                        throw new Exception("No se encontró el archivo comprimido");
                    }
                    
                    // Leer archivo comprimido
                    var compressedBytes = await File.ReadAllBytesAsync(downloadedFiles[0]);
                    var compressedSizeMb = compressedBytes.Length / (1024.0 * 1024.0);
                    var savings = (1 - (compressedBytes.Length / (double)pdfBytes.Length)) * 100;
                    
                    _logger?.LogInformation(
                        "✅ PDF comprimido exitosamente: {FileName} - Original: {Original:F2} MB → Comprimido: {Compressed:F2} MB (Ahorro: {Savings:F1}%)", 
                        fileName, originalSizeMb, compressedSizeMb, savings);
                    
                    // Eliminar tarea en iLovePDF
                    try { task.DeleteTask(); } catch { /* Ignorar errores de limpieza */ }
                    
                    return ServiceResult<byte[]>.Ok(
                        compressedBytes, 
                        $"PDF comprimido: {originalSizeMb:F2} MB → {compressedSizeMb:F2} MB (Ahorro: {savings:F1}%)");
                }
                finally
                {
                    // Limpiar archivos temporales
                    try
                    {
                        if (File.Exists(tempPath))
                            File.Delete(tempPath);
                        
                        if (Directory.Exists(outputPath))
                            Directory.Delete(outputPath, true);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning("No se pudieron eliminar archivos temporales: {Error}", ex.Message);
                    }
                }
            });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "❌ Error al comprimir PDF {FileName}", fileName);
            
            // En caso de error, devolvemos el archivo original para no perder el documento
            return ServiceResult<byte[]>.Ok(pdfBytes, $"Error al comprimir, se guarda sin compresión: {ex.Message}");
        }
    }
}
