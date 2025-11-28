using SGRRHH.Core.Enums;

namespace SGRRHH.Core.Entities;

public class Vacacion : EntidadBase
{
    public int EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;
    
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    
    public int DiasTomados { get; set; }
    
    /// <summary>
    /// Año al que corresponden estas vacaciones (ej: 2025)
    /// </summary>
    public int PeriodoCorrespondiente { get; set; }
    
    public EstadoVacacion Estado { get; set; }
    
    public string? Observaciones { get; set; }
    
    // ===== Campos de trazabilidad (agregados para consistencia con Permiso) =====
    
    /// <summary>
    /// Fecha en que se realizó la solicitud
    /// </summary>
    public DateTime FechaSolicitud { get; set; } = DateTime.Now;
    
    /// <summary>
    /// ID del usuario que solicitó la vacación
    /// </summary>
    public int? SolicitadoPorId { get; set; }
    
    /// <summary>
    /// Usuario que solicitó la vacación
    /// </summary>
    public Usuario? SolicitadoPor { get; set; }
    
    /// <summary>
    /// ID del usuario que aprobó/rechazó la vacación
    /// </summary>
    public int? AprobadoPorId { get; set; }
    
    /// <summary>
    /// Usuario que aprobó/rechazó la vacación
    /// </summary>
    public Usuario? AprobadoPor { get; set; }
    
    /// <summary>
    /// Fecha de aprobación o rechazo
    /// </summary>
    public DateTime? FechaAprobacion { get; set; }
    
    /// <summary>
    /// Motivo del rechazo (si fue rechazada)
    /// </summary>
    public string? MotivoRechazo { get; set; }
}
