using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Domain.Entities;

public class Vacacion : EntidadBase
{
    public int EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;
    
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    
    public int DiasTomados { get; set; }
    
    public int PeriodoCorrespondiente { get; set; }
    
    /// <summary>
    /// Periodo de vacaciones en formato "YYYY"
    /// </summary>
    public string Periodo => PeriodoCorrespondiente.ToString();
    
    /// <summary>
    /// Días disponibles para el período (15 días por año por defecto)
    /// </summary>
    public int DiasDisponibles { get; set; } = 15;
    
    public EstadoVacacion Estado { get; set; }
    
    public string? Observaciones { get; set; }
    
    // ===== Campos de trazabilidad (agregados para consistencia con Permiso) =====
    
    public DateTime FechaSolicitud { get; set; } = DateTime.Now;
    
    public int? SolicitadoPorId { get; set; }
    
    public Usuario? SolicitadoPor { get; set; }
    
    public int? AprobadoPorId { get; set; }
    
    public Usuario? AprobadoPor { get; set; }
    
    public DateTime? FechaAprobacion { get; set; }
    
    public string? MotivoRechazo { get; set; }
}


