using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Domain.Entities;

public class Permiso : EntidadBase
{
    // ===== CAMPOS EXISTENTES =====
    
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
    
    // ===== NUEVOS CAMPOS PARA SEGUIMIENTO =====
    
    /// <summary>Tipo de resolución del permiso (Remunerado/Descontado/Compensado)</summary>
    public TipoResolucionPermiso TipoResolucion { get; set; } = TipoResolucionPermiso.PendienteDefinir;
    
    /// <summary>Indica si se aprobó con documento pendiente de entrega</summary>
    public bool RequiereDocumentoPosterior { get; set; }
    
    /// <summary>Fecha límite para entregar el documento</summary>
    public DateTime? FechaLimiteDocumento { get; set; }
    
    /// <summary>Fecha en que se entregó el documento</summary>
    public DateTime? FechaEntregaDocumento { get; set; }
    
    /// <summary>Total de horas a compensar (si aplica)</summary>
    public int? HorasCompensar { get; set; }
    
    /// <summary>Horas ya compensadas</summary>
    public int HorasCompensadas { get; set; }
    
    /// <summary>Fecha límite para completar la compensación</summary>
    public DateTime? FechaLimiteCompensacion { get; set; }
    
    /// <summary>Monto a descontar en nómina</summary>
    public decimal? MontoDescuento { get; set; }
    
    /// <summary>Período de nómina en que se descontará (ej: "2026-01")</summary>
    public string? PeriodoDescuento { get; set; }
    
    /// <summary>Indica si el permiso está completamente cerrado</summary>
    public bool Completado { get; set; }
    
    // ===== INTEGRACIÓN CON INCAPACIDADES =====
    
    /// <summary>ID de incapacidad si el permiso se convirtió a incapacidad</summary>
    public int? IncapacidadId { get; set; }
    
    /// <summary>Incapacidad relacionada</summary>
    public Incapacidad? Incapacidad { get; set; }
    
    /// <summary>Indica si este permiso fue convertido a incapacidad</summary>
    public bool ConvertidoAIncapacidad { get; set; }
    
    // ===== RELACIONES =====
    
    /// <summary>Historial de seguimiento del permiso</summary>
    public ICollection<SeguimientoPermiso> Seguimientos { get; set; } = new List<SeguimientoPermiso>();
    
    /// <summary>Compensaciones de horas registradas</summary>
    public ICollection<CompensacionHoras> Compensaciones { get; set; } = new List<CompensacionHoras>();
}


