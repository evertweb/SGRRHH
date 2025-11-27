namespace SGRRHH.Core.Enums;

/// <summary>
/// Estados de un empleado en el sistema
/// </summary>
public enum EstadoEmpleado
{
    /// <summary>
    /// Pendiente de aprobación por la Ingeniera
    /// </summary>
    PendienteAprobacion = 0,
    
    /// <summary>
    /// Empleado activo laborando
    /// </summary>
    Activo = 1,
    
    /// <summary>
    /// Empleado en vacaciones
    /// </summary>
    EnVacaciones = 2,
    
    /// <summary>
    /// Empleado en licencia temporal
    /// </summary>
    EnLicencia = 3,
    
    /// <summary>
    /// Empleado suspendido temporalmente
    /// </summary>
    Suspendido = 4,
    
    /// <summary>
    /// Empleado retirado (despedido, renuncia, etc.)
    /// </summary>
    Retirado = 5,
    
    /// <summary>
    /// Solicitud rechazada por la Ingeniera
    /// </summary>
    Rechazado = 6
}
