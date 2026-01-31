namespace SGRRHH.Local.Domain.Entities;

/// <summary>
/// Representa un registro de formación académica de un aspirante.
/// </summary>
public class FormacionAspirante : EntidadBase
{
    public int AspiranteId { get; set; }
    
    public Aspirante? Aspirante { get; set; }
    
    /// <summary>
    /// Nivel de formación (Bachillerato, Técnico, Tecnológico, Profesional, Posgrado, etc.)
    /// </summary>
    public string Nivel { get; set; } = string.Empty;
    
    /// <summary>
    /// Título obtenido o en curso.
    /// </summary>
    public string Titulo { get; set; } = string.Empty;
    
    /// <summary>
    /// Nombre de la institución educativa.
    /// </summary>
    public string Institucion { get; set; } = string.Empty;
    
    public DateTime FechaInicio { get; set; }
    
    public DateTime? FechaFin { get; set; }
    
    /// <summary>
    /// Indica si el programa está en curso.
    /// </summary>
    public bool EnCurso { get; set; }
    
    // ========== PROPIEDADES CALCULADAS ==========
    
    /// <summary>
    /// Descripción corta para mostrar en listas.
    /// </summary>
    public string DescripcionCorta => EnCurso 
        ? $"{Titulo} (En curso) - {Institucion}" 
        : $"{Titulo} - {Institucion}";
}
