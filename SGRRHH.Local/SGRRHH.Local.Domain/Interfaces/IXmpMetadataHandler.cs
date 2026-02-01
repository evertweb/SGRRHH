namespace SGRRHH.Local.Domain.Interfaces;

/// <summary>
/// Manejador de metadatos XMP para PDFs de Forestech.
/// Nota: Esta interfaz usa object para evitar dependencia de iText en Domain.
/// La implementación en Infrastructure hace el cast a PdfDocument.
/// </summary>
public interface IXmpMetadataHandler
{
    /// <summary>
    /// Escribe metadatos XMP personalizados al documento PDF.
    /// </summary>
    /// <param name="pdfDocument">Documento PDF (iText.Kernel.Pdf.PdfDocument).</param>
    /// <param name="datos">Diccionario de metadatos a escribir.</param>
    void EscribirMetadatos(object pdfDocument, Dictionary<string, string> datos);
    
    /// <summary>
    /// Lee los metadatos XMP del documento PDF.
    /// </summary>
    /// <param name="pdfDocument">Documento PDF (iText.Kernel.Pdf.PdfDocument).</param>
    /// <returns>Diccionario con los metadatos leídos.</returns>
    Dictionary<string, string> LeerMetadatos(object pdfDocument);
    
    /// <summary>
    /// Verifica si el documento tiene metadatos de Forestech.
    /// </summary>
    /// <param name="pdfDocument">Documento PDF (iText.Kernel.Pdf.PdfDocument).</param>
    /// <returns>True si es formato Forestech.</returns>
    bool EsFormatoForestec(object pdfDocument);
}
