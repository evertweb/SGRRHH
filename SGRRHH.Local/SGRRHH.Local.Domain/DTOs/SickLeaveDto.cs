using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Domain.DTOs;

/// <summary>
/// DTO para crear una nueva incapacidad
/// </summary>
public class CreateSickLeaveDto
{
    public int EmployeeId { get; set; }
    public int? LeaveRequestOriginId { get; set; }
    public int? PreviousSickLeaveId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime IssueDate { get; set; }
    public string? DiagnosisCIE10 { get; set; }
    public string DiagnosisDescription { get; set; } = string.Empty;
    public TipoIncapacidad SickLeaveType { get; set; }
    public string IssuingEntity { get; set; } = string.Empty;
    public string? PayingEntity { get; set; }
    public string? Notes { get; set; }
    public string? DocumentPath { get; set; }
}

/// <summary>
/// DTO para registrar la transcripción de una incapacidad
/// </summary>
public class RegisterTranscriptionDto
{
    public int SickLeaveId { get; set; }
    public DateTime TranscriptionDate { get; set; }
    public string? FileNumber { get; set; }
    public string? DocumentPath { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO para registrar el cobro de una incapacidad
/// </summary>
public class RegisterCollectionDto
{
    public int SickLeaveId { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal AmountPaid { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO para crear una prórroga de incapacidad
/// </summary>
public class CreateExtensionDto
{
    public int PreviousSickLeaveId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime IssueDate { get; set; }
    public string? DiagnosisCIE10 { get; set; }
    public string DiagnosisDescription { get; set; } = string.Empty;
    public string IssuingEntity { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? DocumentPath { get; set; }
}

/// <summary>
/// DTO con información completa de una incapacidad para vista detallada
/// </summary>
public class SickLeaveDetailDto
{
    // Datos básicos
    public int Id { get; set; }
    public string SickLeaveNumber { get; set; } = string.Empty;
    
    // Empleado
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeNationalId { get; set; } = string.Empty;
    public string EmployeePosition { get; set; } = string.Empty;
    public string EmployeeDepartment { get; set; } = string.Empty;
    public decimal? EmployeeSalary { get; set; }
    
    // Origen
    public int? LeaveRequestOriginId { get; set; }
    public string? LeaveRequestOriginNumber { get; set; }
    public int? PreviousSickLeaveId { get; set; }
    public string? PreviousSickLeaveNumber { get; set; }
    public bool IsExtension { get; set; }
    
    // Fechas
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalDays { get; set; }
    public DateTime IssueDate { get; set; }
    public int RemainingDays { get; set; }
    
    // Diagnóstico
    public string? DiagnosisCIE10 { get; set; }
    public string DiagnosisDescription { get; set; } = string.Empty;
    
    // Tipo y Entidad
    public TipoIncapacidad SickLeaveType { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string IssuingEntity { get; set; } = string.Empty;
    public string? PayingEntity { get; set; }
    
    // Cálculos
    public int CompanyDays { get; set; }
    public int EpsArlDays { get; set; }
    public decimal PaymentPercentage { get; set; }
    public decimal? BaseDayValue { get; set; }
    public decimal? TotalToCollect { get; set; }
    
    // Estado
    public EstadoIncapacidad Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public bool Transcribed { get; set; }
    public DateTime? TranscriptionDate { get; set; }
    public string? EpsFileNumber { get; set; }
    public bool Collected { get; set; }
    public DateTime? PaymentDate { get; set; }
    public decimal? AmountPaid { get; set; }
    
    // Documentos
    public string? SickLeaveDocumentPath { get; set; }
    public string? TranscriptionDocumentPath { get; set; }
    
    // Otros
    public string? Notes { get; set; }
    public int RegisteredById { get; set; }
    public string RegisteredByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    // Prórrogas
    public List<SickLeaveSummary> Extensions { get; set; } = new();
    public int TotalAccumulatedDays { get; set; }
    
    // Seguimiento
    public List<SickLeaveTrackingDto> Tracking { get; set; } = new();
}

/// <summary>
/// DTO para items de seguimiento de incapacidad
/// </summary>
public class SickLeaveTrackingDto
{
    public int Id { get; set; }
    public DateTime ActionDate { get; set; }
    public SickLeaveTrackingActionType ActionType { get; set; }
    public string ActionTypeName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string PerformedByName { get; set; } = string.Empty;
    public string? AdditionalData { get; set; }
}

/// <summary>
/// DTO para el reporte de cobro a EPS/ARL
/// </summary>
public class EpsPaymentReportDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string Period { get; set; } = string.Empty;
    public decimal TotalToCollect { get; set; }
    public int TotalSickLeaves { get; set; }
    public int TotalDays { get; set; }
    public List<PaymentReportItemDto> Items { get; set; } = new();
    public Dictionary<string, decimal> TotalsByEntity { get; set; } = new();
}

/// <summary>
/// Item individual del reporte de cobro
/// </summary>
public class PaymentReportItemDto
{
    public string SickLeaveNumber { get; set; } = string.Empty;
    public string EmployeeNationalId { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalDays { get; set; }
    public int EpsArlDays { get; set; }
    public string SickLeaveType { get; set; } = string.Empty;
    public string DiagnosisDescription { get; set; } = string.Empty;
    public string PayingEntity { get; set; } = string.Empty;
    public decimal BaseDayValue { get; set; }
    public decimal PaymentPercentage { get; set; }
    public decimal AmountToCollect { get; set; }
    public bool Transcribed { get; set; }
    public string? FileNumber { get; set; }
}
