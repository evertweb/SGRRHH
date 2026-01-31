namespace SGRRHH.Local.Domain.Entities;

/// <summary>
/// Representa un registro de experiencia laboral de un aspirante.
/// </summary>
public class ExperienciaAspirante : EntidadBase
{
    public int AspiranteId { get; set; }
    
    public Aspirante? Aspirante { get; set; }
    
    /// <summary>
    /// Nombre de la empresa donde laboró.
    /// </summary>
    public string Empresa { get; set; } = string.Empty;
    
    /// <summary>
    /// Cargo desempeñado.
    /// </summary>
    public string Cargo { get; set; } = string.Empty;
    
    public DateTime FechaInicio { get; set; }
    
    public DateTime? FechaFin { get; set; }
    
    /// <summary>
    /// Indica si es el trabajo actual del aspirante.
    /// </summary>
    public bool TrabajoActual { get; set; }
    
    /// <summary>
    /// Descripción de funciones desempeñadas.
    /// </summary>
    public string? Funciones { get; set; }
    
    /// <summary>
    /// Motivo de retiro de la empresa.
    /// </summary>
    public string? MotivoRetiro { get; set; }
    
    // ========== PROPIEDADES CALCULADAS ==========
    
    /// <summary>
    /// Duración de la experiencia en meses.
    /// </summary>
    public int DuracionMeses
    {
        get
        {
            var fechaFin = FechaFin ?? DateTime.Today;
            return ((fechaFin.Year - FechaInicio.Year) * 12) + fechaFin.Month - FechaInicio.Month;
        }
    }
    
    /// <summary>
    /// Descripción corta para mostrar en listas.
    /// </summary>
    public string DescripcionCorta => TrabajoActual 
        ? $"{Cargo} en {Empresa} (Actual)" 
        : $"{Cargo} en {Empresa}";
}
