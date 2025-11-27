using SGRRHH.Core.Enums;

namespace SGRRHH.Core.Entities;

/// <summary>
/// Entidad que representa una solicitud de permiso o licencia
/// </summary>
public class Permiso : EntidadBase
{
    /// <summary>
    /// Número de acta único autogenerado (formato: PERM-YYYY-NNNN)
    /// </summary>
    public string NumeroActa { get; set; } = string.Empty;
    
    /// <summary>
    /// ID del empleado que solicita el permiso
    /// </summary>
    public int EmpleadoId { get; set; }
    
    /// <summary>
    /// Empleado que solicita el permiso
    /// </summary>
    public Empleado Empleado { get; set; } = null!;
    
    /// <summary>
    /// ID del tipo de permiso
    /// </summary>
    public int TipoPermisoId { get; set; }
    
    /// <summary>
    /// Tipo de permiso
    /// </summary>
    public TipoPermiso TipoPermiso { get; set; } = null!;
    
    /// <summary>
    /// Motivo de la solicitud del permiso
    /// </summary>
    public string Motivo { get; set; } = string.Empty;
    
    /// <summary>
    /// Fecha en que se realizó la solicitud
    /// </summary>
    public DateTime FechaSolicitud { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Fecha de inicio del permiso
    /// </summary>
    public DateTime FechaInicio { get; set; }
    
    /// <summary>
    /// Fecha de fin del permiso
    /// </summary>
    public DateTime FechaFin { get; set; }
    
    /// <summary>
    /// Total de días del permiso (calculado)
    /// </summary>
    public int TotalDias { get; set; }
    
    /// <summary>
    /// Estado actual del permiso
    /// </summary>
    public EstadoPermiso Estado { get; set; } = EstadoPermiso.Pendiente;
    
    /// <summary>
    /// Observaciones adicionales sobre el permiso
    /// </summary>
    public string? Observaciones { get; set; }
    
    /// <summary>
    /// Ruta del documento soporte adjunto (si aplica)
    /// </summary>
    public string? DocumentoSoportePath { get; set; }
    
    /// <summary>
    /// Días pendientes de compensación (si es compensable)
    /// </summary>
    public int? DiasPendientesCompensacion { get; set; }
    
    /// <summary>
    /// Fecha programada para compensar (si es compensable)
    /// </summary>
    public DateTime? FechaCompensacion { get; set; }
    
    /// <summary>
    /// ID del usuario que solicitó el permiso
    /// </summary>
    public int SolicitadoPorId { get; set; }
    
    /// <summary>
    /// Usuario que solicitó el permiso
    /// </summary>
    public Usuario SolicitadoPor { get; set; } = null!;
    
    /// <summary>
    /// ID del usuario que aprobó/rechazó el permiso
    /// </summary>
    public int? AprobadoPorId { get; set; }
    
    /// <summary>
    /// Usuario que aprobó/rechazó el permiso
    /// </summary>
    public Usuario? AprobadoPor { get; set; }
    
    /// <summary>
    /// Fecha de aprobación o rechazo
    /// </summary>
    public DateTime? FechaAprobacion { get; set; }
    
    /// <summary>
    /// Motivo del rechazo (si fue rechazado)
    /// </summary>
    public string? MotivoRechazo { get; set; }
}
