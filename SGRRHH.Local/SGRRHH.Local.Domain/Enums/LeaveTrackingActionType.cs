namespace SGRRHH.Local.Domain.Enums;

/// <summary>
/// Tipos de acciones registradas en el seguimiento de permisos
/// </summary>
public enum LeaveTrackingActionType
{
    /// <summary>Solicitud inicial del permiso</summary>
    Request = 1,
    
    /// <summary>Aprobación del permiso</summary>
    Approval = 2,
    
    /// <summary>Rechazo del permiso</summary>
    Rejection = 3,
    
    /// <summary>Documento soporte entregado</summary>
    DocumentDelivered = 4,
    
    /// <summary>Se inició el proceso de compensación de horas</summary>
    CompensationStarted = 5,
    
    /// <summary>Se registraron horas compensadas</summary>
    HoursCompensated = 6,
    
    /// <summary>Permiso completado y cerrado</summary>
    Completed = 7,
    
    /// <summary>Observación o nota adicional</summary>
    Observation = 8,
    
    /// <summary>Documento rechazado (no válido)</summary>
    DocumentRejected = 9,
    
    /// <summary>Compensación vencida sin completar</summary>
    CompensationExpired = 10,
    
    /// <summary>Cambio de resolución (ej: de Remunerado a Descontado)</summary>
    ResolutionChange = 11,
    
    /// <summary>Documento vencido sin entregar</summary>
    DocumentExpired = 12
}
