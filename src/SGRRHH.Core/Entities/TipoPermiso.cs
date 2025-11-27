namespace SGRRHH.Core.Entities;

/// <summary>
/// Catálogo de tipos de permiso configurables
/// </summary>
public class TipoPermiso : EntidadBase
{
    /// <summary>
    /// Nombre del tipo de permiso
    /// </summary>
    public string Nombre { get; set; } = string.Empty;
    
    /// <summary>
    /// Descripción del tipo de permiso
    /// </summary>
    public string? Descripcion { get; set; }
    
    /// <summary>
    /// Color para identificación visual en la UI (formato #RRGGBB)
    /// </summary>
    public string Color { get; set; } = "#1E88E5";
    
    /// <summary>
    /// Indica si este tipo de permiso requiere aprobación
    /// </summary>
    public bool RequiereAprobacion { get; set; } = true;
    
    /// <summary>
    /// Indica si este tipo de permiso requiere adjuntar documento soporte
    /// </summary>
    public bool RequiereDocumento { get; set; } = false;
    
    /// <summary>
    /// Días por defecto para este tipo de permiso
    /// </summary>
    public int DiasPorDefecto { get; set; } = 1;
    
    /// <summary>
    /// Indica si el permiso es compensable (requiere reposición de horas)
    /// </summary>
    public bool EsCompensable { get; set; } = false;
    
    /// <summary>
    /// Indica si el tipo de permiso está activo
    /// </summary>
    public new bool Activo { get; set; } = true;
}
