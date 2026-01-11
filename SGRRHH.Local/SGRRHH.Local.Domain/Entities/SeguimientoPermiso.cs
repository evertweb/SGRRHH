using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Domain.Entities;

/// <summary>
/// Registra el historial de acciones sobre un permiso
/// </summary>
public class SeguimientoPermiso : EntidadBase
{
    /// <summary>ID del permiso al que pertenece este seguimiento</summary>
    public int PermisoId { get; set; }
    
    /// <summary>Permiso relacionado</summary>
    public Permiso Permiso { get; set; } = null!;
    
    /// <summary>Fecha y hora en que se realizó la acción</summary>
    public DateTime FechaAccion { get; set; } = DateTime.Now;
    
    /// <summary>Tipo de acción realizada</summary>
    public TipoAccionSeguimiento TipoAccion { get; set; }
    
    /// <summary>Descripción de la acción realizada</summary>
    public string? Descripcion { get; set; }
    
    /// <summary>ID del usuario que realizó la acción</summary>
    public int RealizadoPorId { get; set; }
    
    /// <summary>Usuario que realizó la acción</summary>
    public Usuario RealizadoPor { get; set; } = null!;
    
    /// <summary>JSON para datos adicionales específicos de cada tipo de acción</summary>
    public string? DatosAdicionales { get; set; }
}
