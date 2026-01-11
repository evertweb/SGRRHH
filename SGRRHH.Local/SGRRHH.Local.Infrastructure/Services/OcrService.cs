using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.DTOs;
using SGRRHH.Local.Shared;
using SGRRHH.Local.Shared.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using Tesseract;

namespace SGRRHH.Local.Infrastructure.Services;

/// <summary>
/// Servicio de OCR para reconocimiento de texto en imágenes escaneadas.
/// Utiliza Tesseract OCR para el reconocimiento de texto.
/// 
/// NOTA: Requiere que los archivos de datos de idioma (.traineddata) estén en la carpeta tessdata.
/// Descargar desde: https://github.com/tesseract-ocr/tessdata_fast
/// </summary>
public class OcrService : IOcrService
{
    private readonly ILogger<OcrService> _logger;
    private readonly string _tessDataPath;
    private bool? _isAvailable;
    
    public OcrService(ILogger<OcrService> logger)
    {
        _logger = logger;
        
        // Buscar tessdata en varias ubicaciones
        var possiblePaths = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "tessdata"),
            Path.Combine(AppContext.BaseDirectory, "..", "tessdata"),
            @"C:\Program Files\Tesseract-OCR\tessdata",
            @"C:\SGRRHH\tessdata"
        };
        
        _tessDataPath = possiblePaths.FirstOrDefault(Directory.Exists) 
                        ?? Path.Combine(AppContext.BaseDirectory, "tessdata");
    }
    
    /// <inheritdoc />
    public bool IsAvailable
    {
        get
        {
            if (_isAvailable.HasValue)
                return _isAvailable.Value;
            
            try
            {
                // Verificar si existe el directorio tessdata y tiene archivos .traineddata
                if (Directory.Exists(_tessDataPath))
                {
                    var trainedDataFiles = Directory.GetFiles(_tessDataPath, "*.traineddata");
                    _isAvailable = trainedDataFiles.Length > 0;
                    
                    if (_isAvailable.Value)
                    {
                        _logger.LogInformation("OCR disponible. Tessdata: {Path}, Idiomas: {Count}", 
                            _tessDataPath, trainedDataFiles.Length);
                    }
                }
                else
                {
                    _isAvailable = false;
                    _logger.LogWarning("Carpeta tessdata no encontrada: {Path}", _tessDataPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando disponibilidad de OCR");
                _isAvailable = false;
            }
            
            return _isAvailable.Value;
        }
    }
    
    /// <inheritdoc />
    public async Task<Result<string>> ExtractTextAsync(byte[] imageBytes, string language = "spa")
    {
        if (!IsAvailable)
        {
            return Result<string>.Fail(
                "OCR no está disponible. Descargue los archivos .traineddata de https://github.com/tesseract-ocr/tessdata_fast " +
                $"y colóquelos en: {_tessDataPath}");
        }
        
        try
        {
            _logger.LogInformation("Extrayendo texto OCR. Idioma: {Language}, Tamaño imagen: {Size} bytes", 
                language, imageBytes.Length);
            
            return await Task.Run(() =>
            {
                using var engine = new TesseractEngine(_tessDataPath, language, EngineMode.Default);
                using var img = Pix.LoadFromMemory(imageBytes);
                using var page = engine.Process(img);
                
                var text = page.GetText();
                var confidence = page.GetMeanConfidence();
                
                _logger.LogInformation("Texto extraído: {Length} caracteres, Confianza: {Confidence:P0}", 
                    text.Length, confidence);
                
                return Result<string>.Ok(text);
            });
        }
        catch (TesseractException ex) when (ex.Message.Contains("traineddata"))
        {
            _logger.LogError(ex, "Archivo de idioma no encontrado: {Language}", language);
            return Result<string>.Fail(
                $"Idioma '{language}' no disponible. Descargue {language}.traineddata de " +
                $"https://github.com/tesseract-ocr/tessdata_fast y colóquelo en: {_tessDataPath}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extrayendo texto OCR");
            return Result<string>.Fail($"Error OCR: {ex.Message}");
        }
    }
    
    /// <inheritdoc />
    public async Task<Result<byte[]>> GenerateSearchablePdfAsync(List<ScannedPageDto> pages, string language = "spa")
    {
        if (pages.Count == 0)
        {
            return Result<byte[]>.Fail("No hay páginas para generar el PDF");
        }
        
        try
        {
            _logger.LogInformation("Generando PDF buscable. Páginas: {Count}, Idioma: {Language}", 
                pages.Count, language);
            
            if (!IsAvailable)
            {
                // Sin OCR, generar PDF simple
                _logger.LogWarning("OCR no disponible, generando PDF sin texto buscable");
                return await GenerateSimplePdfAsync(pages);
            }
            
            // Con OCR: extraer texto de cada página y generar PDF con capa de texto
            return await Task.Run(() =>
            {
                using var engine = new TesseractEngine(_tessDataPath, language, EngineMode.Default);
                var pagesWithText = new List<(ScannedPageDto Page, string Text)>();
                
                foreach (var page in pages)
                {
                    try
                    {
                        using var img = Pix.LoadFromMemory(page.ImageBytes);
                        using var processedPage = engine.Process(img);
                        var text = processedPage.GetText();
                        pagesWithText.Add((page, text));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error procesando página para OCR, continuando sin texto");
                        pagesWithText.Add((page, string.Empty));
                    }
                }
                
                // Generar PDF con texto embebido (QuestPDF no soporta capa de texto invisible,
                // así que agregamos el texto al final de cada página de forma visible pero pequeña)
                using var ms = new MemoryStream();
                
                QuestPDF.Fluent.Document.Create(container =>
                {
                    foreach (var (page, text) in pagesWithText)
                    {
                        container.Page(pageDescriptor =>
                        {
                            pageDescriptor.Size(PageSizes.Letter);
                            pageDescriptor.Margin(0);
                            
                            pageDescriptor.Content().Column(column =>
                            {
                                // Imagen principal
                                column.Item().Image(page.ImageBytes).FitArea();
                                
                                // Texto OCR oculto (fuente muy pequeña, color blanco sobre blanco)
                                // Esto permite búsqueda en el PDF
                                if (!string.IsNullOrWhiteSpace(text))
                                {
                                    column.Item()
                                        .Height(1) // Altura mínima
                                        .Text(text)
                                        .FontSize(1)
                                        .FontColor(Colors.White);
                                }
                            });
                        });
                    }
                }).GeneratePdf(ms);
                
                _logger.LogInformation("PDF buscable generado: {Size} bytes", ms.Length);
                return Result<byte[]>.Ok(ms.ToArray());
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando PDF con OCR");
            return Result<byte[]>.Fail($"Error generando PDF: {ex.Message}");
        }
    }
    
    /// <inheritdoc />
    public Task<List<OcrLanguageDto>> GetAvailableLanguagesAsync()
    {
        var languages = new List<OcrLanguageDto>
        {
            new() { Code = "spa", Name = "Español", IsInstalled = IsLanguageInstalled("spa") },
            new() { Code = "eng", Name = "Inglés", IsInstalled = IsLanguageInstalled("eng") },
            new() { Code = "por", Name = "Portugués", IsInstalled = IsLanguageInstalled("por") },
            new() { Code = "fra", Name = "Francés", IsInstalled = IsLanguageInstalled("fra") },
            new() { Code = "deu", Name = "Alemán", IsInstalled = IsLanguageInstalled("deu") }
        };
        
        return Task.FromResult(languages);
    }
    
    private bool IsLanguageInstalled(string langCode)
    {
        if (!Directory.Exists(_tessDataPath))
            return false;
        
        var filePath = Path.Combine(_tessDataPath, $"{langCode}.traineddata");
        return File.Exists(filePath);
    }
    
    /// <summary>
    /// Genera un PDF simple sin OCR (solo imágenes)
    /// </summary>
    private async Task<Result<byte[]>> GenerateSimplePdfAsync(List<ScannedPageDto> pages)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var ms = new MemoryStream();
                
                QuestPDF.Fluent.Document.Create(container =>
                {
                    foreach (var page in pages)
                    {
                        container.Page(pageDescriptor =>
                        {
                            pageDescriptor.Size(PageSizes.Letter);
                            pageDescriptor.Margin(0);
                            
                            pageDescriptor.Content().Image(page.ImageBytes)
                                .FitArea();
                        });
                    }
                }).GeneratePdf(ms);
                
                return Result<byte[]>.Ok(ms.ToArray());
            }
            catch (Exception ex)
            {
                return Result<byte[]>.Fail($"Error generando PDF: {ex.Message}");
            }
        });
    }
}
