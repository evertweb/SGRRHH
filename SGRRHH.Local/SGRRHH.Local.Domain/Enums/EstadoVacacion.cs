namespace SGRRHH.Local.Domain.Enums;

public enum EstadoVacacion
{
    Pendiente,    // Solicitud creada, esperando aprobacia³n
    Aprobada,     // Aprobada por supervisor/admin
    Rechazada,    // Rechazada con motivo
    Programada,   // Fechas confirmadas
    Disfrutada,   // Vacaciones tomadas
    Cancelada     // Cancelada antes de disfrutar
}


