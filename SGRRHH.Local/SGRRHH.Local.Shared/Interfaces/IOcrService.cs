using SGRRHH.Local.Domain.DTOs;
using SGRRHH.Local.Shared;

namespace SGRRHH.Local.Shared.Interfaces;

/// <summary>
/// Servicio para reconocimiento óptico de caracteres (OCR) en imágenes escaneadas
/// </summary>
public interface IOcrService
{
    /// <summary>
    /// Indica si el servicio OCR está disponible (Tesseract instalado)
    /// </summary>
    bool IsAvailable { get; }
    
    /// <summary>
    /// Extrae texto de una imagen escaneada
    /// </summary>
    /// <param name="imageBytes">Bytes de la imagen</param>
    /// <param name="language">Código de idioma (spa, eng, por, etc.)</param>
    /// <returns>Texto extraído</returns>
    Task<Result<string>> ExtractTextAsync(byte[] imageBytes, string language = "spa");
    
    /// <summary>
    /// Genera un PDF buscable (PDF/A) a partir de imágenes escaneadas.
    /// El texto OCR se incrusta como capa invisible sobre las imágenes.
    /// </summary>
    /// <param name="pages">Lista de páginas escaneadas</param>
    /// <param name="language">Código de idioma para OCR</param>
    /// <returns>Bytes del PDF con texto buscable</returns>
    Task<Result<byte[]>> GenerateSearchablePdfAsync(List<ScannedPageDto> pages, string language = "spa");
    
    /// <summary>
    /// Obtiene los idiomas disponibles para OCR
    /// </summary>
    Task<List<OcrLanguageDto>> GetAvailableLanguagesAsync();
}

/// <summary>
/// Información de un idioma disponible para OCR
/// </summary>
public class OcrLanguageDto
{
    /// <summary>
    /// Código ISO del idioma (spa, eng, por, etc.)
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Nombre del idioma para mostrar
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Indica si está instalado localmente
    /// </summary>
    public bool IsInstalled { get; set; }
}
