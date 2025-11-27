using System.Collections.Generic;

namespace SGRRHH.Core.Entities;

/// <summary>
/// Entidad que representa una actividad o tarea que pueden realizar los empleados
/// </summary>
public class Actividad : EntidadBase
{
    /// <summary>
    /// Código único de la actividad
    /// </summary>
    public string Codigo { get; set; } = string.Empty;
    
    /// <summary>
    /// Nombre de la actividad
    /// </summary>
    public string Nombre { get; set; } = string.Empty;
    
    /// <summary>
    /// Descripción detallada de la actividad
    /// </summary>
    public string? Descripcion { get; set; }
    
    /// <summary>
    /// Categoría de la actividad para agrupar
    /// </summary>
    public string? Categoria { get; set; }
    
    /// <summary>
    /// Indica si la actividad requiere especificar un proyecto
    /// </summary>
    public bool RequiereProyecto { get; set; }
    
    /// <summary>
    /// Orden para mostrar en listas
    /// </summary>
    public int Orden { get; set; }
    
    /// <summary>
    /// Detalles de actividad que usan esta actividad
    /// </summary>
    public ICollection<DetalleActividad>? DetallesActividades { get; set; }
}
