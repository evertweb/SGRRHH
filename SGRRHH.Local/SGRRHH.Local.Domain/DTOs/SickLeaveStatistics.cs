using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Domain.DTOs;

/// <summary>
/// Estadísticas generales de incapacidades para el dashboard
/// </summary>
public class SickLeaveStatistics
{
    public int TotalActive { get; set; }
    public int TotalPendingTranscription { get; set; }
    public int TotalPendingPayment { get; set; }
    public int TotalCompletedThisMonth { get; set; }
    public decimal TotalToCollect { get; set; }
    public decimal TotalCollectedThisMonth { get; set; }
    public int TotalSickLeaveDaysThisMonth { get; set; }
    public List<SickLeaveSummary> ActiveSickLeaves { get; set; } = new();
    public List<SickLeaveSummary> ExpiringSoon { get; set; } = new();
}

/// <summary>
/// Resumen de una incapacidad para listados
/// </summary>
public class SickLeaveSummary
{
    public int Id { get; set; }
    public string NumeroIncapacidad { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeNationalId { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalDays { get; set; }
    public int RemainingDays { get; set; }
    public TipoIncapacidad Type { get; set; }
    public EstadoIncapacidad Status { get; set; }
    public bool Transcribed { get; set; }
    public bool Collected { get; set; }
    public decimal? AmountToCollect { get; set; }
    public string? DiagnosisDescription { get; set; }
    public string? IssuingEntity { get; set; }
    public string? PayingEntity { get; set; }
    public bool IsExtension { get; set; }
    
    /// <summary>Nombre descriptivo del tipo de incapacidad</summary>
    public string TypeName => Type switch
    {
        TipoIncapacidad.EnfermedadGeneral => "Enfermedad General",
        TipoIncapacidad.AccidenteTrabajo => "Accidente de Trabajo",
        TipoIncapacidad.EnfermedadLaboral => "Enfermedad Laboral",
        TipoIncapacidad.LicenciaMaternidad => "Licencia Maternidad",
        TipoIncapacidad.LicenciaPaternidad => "Licencia Paternidad",
        _ => "Desconocido"
    };
    
    /// <summary>Nombre descriptivo del estado</summary>
    public string StatusName => Status switch
    {
        EstadoIncapacidad.Activa => "Activa",
        EstadoIncapacidad.Finalizada => "Finalizada",
        EstadoIncapacidad.Transcrita => "Transcrita",
        EstadoIncapacidad.Cobrada => "Cobrada",
        EstadoIncapacidad.Cancelada => "Cancelada",
        _ => "Desconocido"
    };
    
    /// <summary>Indica si la incapacidad está vencida</summary>
    public bool IsExpired => DateTime.Today > EndDate.Date;
    
    /// <summary>Indica si vence pronto (en los próximos 3 días)</summary>
    public bool ExpiresSoon => !IsExpired && RemainingDays <= 3;
}

