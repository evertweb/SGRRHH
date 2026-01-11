namespace SGRRHH.Local.Domain.Entities;

/// <summary>
/// Categorías para agrupar actividades silviculturales
/// </summary>
public class CategoriaActividad : EntidadBase
{
    /// <summary>
    /// Código único de la categoría (ej: PREP, SIEM, MANT)
    /// </summary>
    public string Codigo { get; set; } = string.Empty;
    
    /// <summary>
    /// Nombre de la categoría (ej: Preparación de Terreno)
    /// </summary>
    public string Nombre { get; set; } = string.Empty;
    
    /// <summary>
    /// Descripción de la categoría
    /// </summary>
    public string? Descripcion { get; set; }
    
    /// <summary>
    /// Icono o emoji representativo (opcional)
    /// </summary>
    public string? Icono { get; set; }
    
    /// <summary>
    /// Color para identificación visual (hex)
    /// </summary>
    public string? ColorHex { get; set; }
    
    /// <summary>
    /// Orden de visualización
    /// </summary>
    public int Orden { get; set; }
    
    /// <summary>
    /// Actividades pertenecientes a esta categoría
    /// </summary>
    public ICollection<Actividad>? Actividades { get; set; }
}
