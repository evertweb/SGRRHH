namespace SGRRHH.Local.Domain.DTOs;

/// <summary>
/// Resultado del procesamiento de un PDF de hoja de vida.
/// </summary>
public class ResultadoParseo
{
    /// <summary>
    /// Indica si el parseo fue exitoso.
    /// </summary>
    public bool Exitoso { get; set; }
    
    /// <summary>
    /// Mensaje descriptivo del resultado.
    /// </summary>
    public string? Mensaje { get; set; }
    
    /// <summary>
    /// Indica si el PDF es formato Forestech (generado por el sistema).
    /// </summary>
    public bool EsFormatoForestec { get; set; }
    
    /// <summary>
    /// Indica si el PDF tiene firma digital.
    /// </summary>
    public bool TieneFirma { get; set; }
    
    /// <summary>
    /// Datos extraídos del PDF (null si no fue exitoso).
    /// </summary>
    public DatosHojaVida? Datos { get; set; }
    
    /// <summary>
    /// Lista de errores encontrados durante el parseo.
    /// </summary>
    public List<string> Errores { get; set; } = new();
    
    /// <summary>
    /// Advertencias no críticas encontradas.
    /// </summary>
    public List<string> Advertencias { get; set; } = new();
    
    /// <summary>
    /// Hash del contenido del PDF para detección de duplicados.
    /// </summary>
    public string? HashContenido { get; set; }
    
    // ========== MÉTODOS ESTÁTICOS DE FÁBRICA ==========
    
    /// <summary>
    /// Crea un resultado exitoso.
    /// </summary>
    public static ResultadoParseo Exito(DatosHojaVida datos, bool esForestec, bool tieneFirma)
    {
        return new ResultadoParseo
        {
            Exitoso = true,
            Mensaje = "PDF procesado correctamente",
            EsFormatoForestec = esForestec,
            TieneFirma = tieneFirma,
            Datos = datos
        };
    }
    
    /// <summary>
    /// Crea un resultado de error.
    /// </summary>
    public static ResultadoParseo Error(string mensaje, params string[] errores)
    {
        return new ResultadoParseo
        {
            Exitoso = false,
            Mensaje = mensaje,
            Errores = errores.ToList()
        };
    }
}
