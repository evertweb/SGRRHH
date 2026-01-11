namespace SGRRHH.Local.Domain.Entities;

/// <summary>
/// Entidad para gestionar festivos colombianos según Ley 51 de 1983 (Ley Emiliani)
/// </summary>
public class FestivoColombia : EntidadBase
{
    /// <summary>
    /// Fecha del festivo
    /// </summary>
    public DateTime Fecha { get; set; }
    
    /// <summary>
    /// Nombre del festivo (ej: "Día del Trabajo", "Navidad")
    /// </summary>
    public string Nombre { get; set; } = string.Empty;
    
    /// <summary>
    /// Descripción adicional del festivo
    /// </summary>
    public string? Descripcion { get; set; }
    
    /// <summary>
    /// Indica si es un festivo que se traslada al lunes siguiente según Ley Emiliani (Ley 51/1983)
    /// Los festivos religiosos (excepto Navidad y Viernes Santo) se trasladan al lunes siguiente
    /// </summary>
    public bool EsLeyEmiliani { get; set; }
    
    /// <summary>
    /// Fecha original del festivo (antes de traslado por Ley Emiliani)
    /// </summary>
    public DateTime? FechaOriginal { get; set; }
    
    /// <summary>
    /// Tipo de festivo (Religioso, Civil, Nacional)
    /// </summary>
    public TipoFestivo Tipo { get; set; }
    
    /// <summary>
    /// Indica si es un festivo de fecha fija (como Navidad) o variable (como Semana Santa)
    /// </summary>
    public bool EsFechaFija { get; set; }
    
    /// <summary>
    /// Año al que corresponde el festivo
    /// </summary>
    public int Año { get; set; }
}

/// <summary>
/// Tipos de festivos en Colombia
/// </summary>
public enum TipoFestivo
{
    /// <summary>
    /// Festivo de carácter religioso (ej: Semana Santa, Navidad)
    /// </summary>
    Religioso = 1,
    
    /// <summary>
    /// Festivo de carácter civil (ej: Día del Trabajo)
    /// </summary>
    Civil = 2,
    
    /// <summary>
    /// Festivo nacional (ej: Independencia)
    /// </summary>
    Nacional = 3
}
