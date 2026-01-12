namespace SGRRHH.Local.Domain.Enums;

/// <summary>
/// Estados posibles de un empleado en el sistema.
/// </summary>
public enum EstadoEmpleado
{
    /// <summary>
    /// Empleado creado por Operador, pendiente de aprobación por Aprobador/Admin.
    /// </summary>
    PendienteAprobacion = 0,
    
    /// <summary>
    /// Empleado activo y trabajando normalmente.
    /// </summary>
    Activo = 1,
    
    /// <summary>
    /// Empleado en período de vacaciones.
    /// </summary>
    EnVacaciones = 2,
    
    /// <summary>
    /// Empleado en licencia (maternidad, paternidad, luto, etc.).
    /// </summary>
    EnLicencia = 3,
    
    /// <summary>
    /// Empleado suspendido temporalmente (sanción disciplinaria).
    /// </summary>
    Suspendido = 4,
    
    /// <summary>
    /// Empleado retirado definitivamente de la empresa.
    /// </summary>
    Retirado = 5,
    
    /// <summary>
    /// Solicitud de empleado rechazada (solo aplica si venía de PendienteAprobacion).
    /// </summary>
    Rechazado = 6,
    
    /// <summary>
    /// Empleado con incapacidad médica activa.
    /// </summary>
    EnIncapacidad = 7
}


