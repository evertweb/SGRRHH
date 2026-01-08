using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Domain.Entities;

public class Permiso : EntidadBase
{
    public string NumeroActa { get; set; } = string.Empty;
    
    public int EmpleadoId { get; set; }
    
    public Empleado Empleado { get; set; } = null!;
    
    public int TipoPermisoId { get; set; }
    
    public TipoPermiso TipoPermiso { get; set; } = null!;
    
    public string Motivo { get; set; } = string.Empty;
    
    public DateTime FechaSolicitud { get; set; } = DateTime.Now;
    
    public DateTime FechaInicio { get; set; }
    
    public DateTime FechaFin { get; set; }
    
    public int TotalDias { get; set; }
    
    public EstadoPermiso Estado { get; set; } = EstadoPermiso.Pendiente;
    
    public string? Observaciones { get; set; }
    
    public string? DocumentoSoportePath { get; set; }
    
    public int? DiasPendientesCompensacion { get; set; }
    
    public DateTime? FechaCompensacion { get; set; }
    
    public int SolicitadoPorId { get; set; }
    
    public Usuario SolicitadoPor { get; set; } = null!;
    
    public int? AprobadoPorId { get; set; }
    
    public Usuario? AprobadoPor { get; set; }
    
    public DateTime? FechaAprobacion { get; set; }
    
    public string? MotivoRechazo { get; set; }
}


