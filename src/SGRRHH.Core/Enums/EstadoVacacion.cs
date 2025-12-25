namespace SGRRHH.Core.Enums;

/// <summary>
/// Estados de una solicitud de vacaciones.
/// Flujo: Pendiente → Aprobada → Programada → Disfrutada
///        Pendiente → Rechazada
///        Programada → Cancelada
/// </summary>
public enum EstadoVacacion
{
    Pendiente,    // Solicitud creada, esperando aprobación
    Aprobada,     // Aprobada por supervisor/admin
    Rechazada,    // Rechazada con motivo
    Programada,   // Fechas confirmadas
    Disfrutada,   // Vacaciones tomadas
    Cancelada     // Cancelada antes de disfrutar
}
