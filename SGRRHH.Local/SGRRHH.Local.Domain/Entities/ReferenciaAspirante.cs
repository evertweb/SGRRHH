namespace SGRRHH.Local.Domain.Entities;

/// <summary>
/// Representa una referencia personal o laboral de un aspirante.
/// </summary>
public class ReferenciaAspirante : EntidadBase
{
    public int AspiranteId { get; set; }
    
    public Aspirante? Aspirante { get; set; }
    
    /// <summary>
    /// Tipo de referencia: Personal o Laboral.
    /// </summary>
    public string Tipo { get; set; } = string.Empty;
    
    /// <summary>
    /// Nombre completo de la persona que da la referencia.
    /// </summary>
    public string NombreCompleto { get; set; } = string.Empty;
    
    /// <summary>
    /// Teléfono de contacto.
    /// </summary>
    public string Telefono { get; set; } = string.Empty;
    
    /// <summary>
    /// Relación con el aspirante (jefe, compañero, familiar, amigo, etc.)
    /// </summary>
    public string Relacion { get; set; } = string.Empty;
    
    /// <summary>
    /// Empresa donde trabajaron juntos (solo para referencias laborales).
    /// </summary>
    public string? Empresa { get; set; }
    
    /// <summary>
    /// Cargo de la persona que da la referencia.
    /// </summary>
    public string? Cargo { get; set; }
    
    // ========== PROPIEDADES CALCULADAS ==========
    
    /// <summary>
    /// Indica si es referencia laboral.
    /// </summary>
    public bool EsLaboral => Tipo?.ToLowerInvariant() == "laboral";
    
    /// <summary>
    /// Descripción corta para mostrar en listas.
    /// </summary>
    public string DescripcionCorta => EsLaboral 
        ? $"{NombreCompleto} ({Cargo} - {Empresa})" 
        : $"{NombreCompleto} ({Relacion})";
}
