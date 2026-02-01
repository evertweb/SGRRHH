using SGRRHH.Local.Domain.DTOs;

namespace SGRRHH.Local.Domain.Interfaces;

/// <summary>
/// Servicio para generación y procesamiento de PDFs de hoja de vida.
/// </summary>
public interface IPdfHojaVidaService
{
    /// <summary>
    /// Genera un PDF vacío con el formulario de hoja de vida para aspirantes nuevos.
    /// </summary>
    /// <returns>Bytes del PDF generado.</returns>
    Task<byte[]> GenerarPdfVacioAsync();
    
    /// <summary>
    /// Genera un PDF prellenado con los datos del empleado existente.
    /// </summary>
    /// <param name="empleadoId">ID del empleado.</param>
    /// <returns>Bytes del PDF generado.</returns>
    Task<byte[]> GenerarPdfEmpleadoAsync(int empleadoId);
    
    /// <summary>
    /// Genera un PDF prellenado con los datos del aspirante.
    /// </summary>
    /// <param name="aspiranteId">ID del aspirante.</param>
    /// <returns>Bytes del PDF generado.</returns>
    Task<byte[]> GenerarPdfAspiranteAsync(int aspiranteId);
    
    /// <summary>
    /// Procesa un PDF subido y extrae los datos del formulario.
    /// </summary>
    /// <param name="pdfStream">Stream del PDF subido.</param>
    /// <param name="nombreArchivo">Nombre original del archivo.</param>
    /// <returns>Resultado del parseo con datos extraídos.</returns>
    Task<ResultadoParseo> ProcesarPdfAsync(Stream pdfStream, string nombreArchivo);
    
    /// <summary>
    /// Verifica si un PDF es formato Forestech (generado por el sistema).
    /// </summary>
    /// <param name="pdfStream">Stream del PDF a verificar.</param>
    /// <returns>True si es formato Forestech.</returns>
    Task<bool> EsFormatoForestechAsync(Stream pdfStream);
}
