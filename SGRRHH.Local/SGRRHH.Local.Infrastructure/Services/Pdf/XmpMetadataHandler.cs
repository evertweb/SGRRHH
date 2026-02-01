using iText.Kernel.Pdf;
using iText.Kernel.XMP;
using iText.Kernel.XMP.Properties;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Interfaces;

namespace SGRRHH.Local.Infrastructure.Services.Pdf;

/// <summary>
/// Manejador de metadatos XMP para identificar PDFs generados por Forestech.
/// </summary>
public class XmpMetadataHandler : IXmpMetadataHandler
{
    private readonly ILogger<XmpMetadataHandler> _logger;
    
    // Namespace personalizado de Forestech
    private const string NAMESPACE_FORESTECH = "http://forestech.co/hojavida/";
    private const string PREFIX_FORESTECH = "forestech";
    
    // Claves de metadatos
    private const string CLAVE_ORIGEN = "origen";
    private const string CLAVE_VERSION = "version";
    private const string CLAVE_FECHA_GENERACION = "fechaGeneracion";
    private const string CLAVE_HASH = "hash";
    
    // Valor que identifica PDFs del sistema
    private const string VALOR_ORIGEN_SISTEMA = "FORESTECH-SGRRHH";
    private const string VERSION_ACTUAL = "1.0";
    
    public XmpMetadataHandler(ILogger<XmpMetadataHandler> logger)
    {
        _logger = logger;
    }
    
    /// <summary>
    /// Escribe metadatos XMP personalizados al documento PDF.
    /// </summary>
    public void EscribirMetadatos(object pdfDocument, Dictionary<string, string> datos)
    {
        if (pdfDocument is not PdfDocument doc)
        {
            throw new ArgumentException("El parámetro debe ser de tipo PdfDocument", nameof(pdfDocument));
        }
        
        try
        {
            // Obtener o crear XMP metadata
            var xmpMeta = doc.GetXmpMetadata(true);
            
            if (xmpMeta == null)
            {
                xmpMeta = XMPMetaFactory.Create();
            }
            
            // Registrar namespace personalizado
            XMPMetaFactory.GetSchemaRegistry().RegisterNamespace(NAMESPACE_FORESTECH, PREFIX_FORESTECH);
            
            // Agregar metadatos de identificación Forestech
            xmpMeta.SetProperty(NAMESPACE_FORESTECH, CLAVE_ORIGEN, VALOR_ORIGEN_SISTEMA);
            xmpMeta.SetProperty(NAMESPACE_FORESTECH, CLAVE_VERSION, VERSION_ACTUAL);
            xmpMeta.SetProperty(NAMESPACE_FORESTECH, CLAVE_FECHA_GENERACION, DateTime.Now.ToString("O"));
            
            // Agregar metadatos personalizados adicionales
            foreach (var kvp in datos)
            {
                xmpMeta.SetProperty(NAMESPACE_FORESTECH, kvp.Key, kvp.Value);
            }
            
            // Guardar metadatos en el documento
            doc.SetXmpMetadata(xmpMeta);
            
            _logger.LogDebug("Metadatos XMP escritos exitosamente en el PDF");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al escribir metadatos XMP en el PDF");
            throw;
        }
    }
    
    /// <summary>
    /// Lee los metadatos XMP del documento PDF.
    /// </summary>
    public Dictionary<string, string> LeerMetadatos(object pdfDocument)
    {
        if (pdfDocument is not PdfDocument doc)
        {
            throw new ArgumentException("El parámetro debe ser de tipo PdfDocument", nameof(pdfDocument));
        }
        
        var resultado = new Dictionary<string, string>();
        
        try
        {
            var xmpMeta = doc.GetXmpMetadata(false);
            
            if (xmpMeta == null)
            {
                _logger.LogDebug("El PDF no contiene metadatos XMP");
                return resultado;
            }
            
            // Intentar leer metadatos Forestech
            var claves = new[] { CLAVE_ORIGEN, CLAVE_VERSION, CLAVE_FECHA_GENERACION, CLAVE_HASH };
            
            foreach (var clave in claves)
            {
                try
                {
                    var prop = xmpMeta.GetProperty(NAMESPACE_FORESTECH, clave);
                    if (prop != null && !string.IsNullOrEmpty(prop.GetValue()))
                    {
                        resultado[clave] = prop.GetValue();
                    }
                }
                catch
                {
                    // Ignorar si la propiedad no existe
                }
            }
            
            _logger.LogDebug("Se leyeron {Count} metadatos XMP del PDF", resultado.Count);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al leer metadatos XMP del PDF");
        }
        
        return resultado;
    }
    
    /// <summary>
    /// Verifica si el documento tiene metadatos de Forestech.
    /// </summary>
    public bool EsFormatoForestec(object pdfDocument)
    {
        if (pdfDocument is not PdfDocument doc)
        {
            return false;
        }
        
        try
        {
            var xmpMeta = doc.GetXmpMetadata(false);
            
            if (xmpMeta == null)
            {
                return false;
            }
            
            var propOrigen = xmpMeta.GetProperty(NAMESPACE_FORESTECH, CLAVE_ORIGEN);
            
            if (propOrigen != null && propOrigen.GetValue() == VALOR_ORIGEN_SISTEMA)
            {
                _logger.LogDebug("PDF identificado como formato Forestech");
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al verificar si el PDF es formato Forestech");
            return false;
        }
    }
}
