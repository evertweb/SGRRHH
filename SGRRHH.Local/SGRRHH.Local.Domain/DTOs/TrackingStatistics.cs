using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Domain.DTOs;

/// <summary>
/// Estadísticas del dashboard de seguimiento de permisos
/// </summary>
public class TrackingStatistics
{
    /// <summary>Total de permisos en estado Pendiente</summary>
    public int TotalPendingLeaveRequests { get; set; }
    
    /// <summary>Total de permisos aprobados pendientes de documento</summary>
    public int TotalPendingDocument { get; set; }
    
    /// <summary>Total de permisos en proceso de compensación</summary>
    public int TotalInCompensation { get; set; }
    
    /// <summary>Total de permisos marcados para descuento</summary>
    public int TotalForDeduction { get; set; }
    
    /// <summary>Total de documentos vencidos sin entregar</summary>
    public int TotalExpiredDocuments { get; set; }
    
    /// <summary>Total de compensaciones vencidas sin completar</summary>
    public int TotalExpiredCompensations { get; set; }
    
    /// <summary>Monto total a descontar en el período actual</summary>
    public decimal TotalDeductionAmount { get; set; }
    
    /// <summary>Lista de permisos que requieren atención urgente</summary>
    public List<LeaveRequestWithTracking> UrgentLeaveRequests { get; set; } = new();
}

/// <summary>
/// DTO para mostrar permisos con información de seguimiento
/// </summary>
public class LeaveRequestWithTracking
{
    public int Id { get; set; }
    
    public string ActNumber { get; set; } = string.Empty;
    
    public string EmployeeName { get; set; } = string.Empty;
    
    public int EmployeeId { get; set; }
    
    public string LeaveRequestType { get; set; } = string.Empty;
    
    public int LeaveRequestTypeId { get; set; }
    
    public DateTime StartDate { get; set; }
    
    public DateTime EndDate { get; set; }
    
    public int TotalDays { get; set; }
    
    public EstadoPermiso Status { get; set; }
    
    public TipoResolucionPermiso ResolutionType { get; set; }
    
    public DateTime? Deadline { get; set; }
    
    public int? RemainingDays { get; set; }
    
    public string UrgencyReason { get; set; } = string.Empty;
    
    /// <summary>Para compensaciones: horas totales a compensar</summary>
    public int? HoursToCompensate { get; set; }
    
    /// <summary>Para compensaciones: horas ya compensadas</summary>
    public int CompensatedHours { get; set; }
    
    /// <summary>Monto a descontar (si aplica)</summary>
    public decimal? DeductionAmount { get; set; }
    
    /// <summary>Período de nómina para descuento</summary>
    public string? DeductionPeriod { get; set; }
    
    /// <summary>Indica si está vencido</summary>
    public bool IsExpired { get; set; }
    
    /// <summary>Indica si vence pronto (próximos 3 días)</summary>
    public bool ExpiresSoon { get; set; }
}

/// <summary>
/// DTO para alertas de permisos
/// </summary>
public class LeaveRequestAlert
{
    public int LeaveRequestId { get; set; }
    
    public string ActNumber { get; set; } = string.Empty;
    
    public string EmployeeName { get; set; } = string.Empty;
    
    public LeaveRequestAlertType AlertType { get; set; }
    
    public string Message { get; set; } = string.Empty;
    
    public DateTime AlertDate { get; set; }
    
    public DateTime? Deadline { get; set; }
    
    public int? RemainingDays { get; set; }
    
    public bool IsUrgent { get; set; }
}

/// <summary>
/// Tipos de alertas para permisos
/// </summary>
public enum LeaveRequestAlertType
{
    /// <summary>Documento próximo a vencer (3 días)</summary>
    DocumentExpiringSoon = 1,
    
    /// <summary>Documento ya vencido</summary>
    DocumentExpired = 2,
    
    /// <summary>Compensación próxima a vencer (3 días)</summary>
    CompensationExpiringSoon = 3,
    
    /// <summary>Compensación ya vencida</summary>
    CompensationExpired = 4,
    
    /// <summary>Permiso pendiente de aprobación por más de 3 días</summary>
    ApprovalPending = 5
}
