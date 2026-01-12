using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Domain.Services;

/// <summary>
/// Servicio que centraliza la lógica de estados de empleados.
/// Define reglas de transición y permisos según rol.
/// </summary>
public static class EstadoEmpleadoService
{
    /// <summary>
    /// Determina el estado inicial de un empleado según el rol del creador.
    /// - Operador: siempre PendienteAprobacion
    /// - Aprobador/Admin: siempre Activo
    /// </summary>
    public static EstadoEmpleado ObtenerEstadoInicialSegunRol(RolUsuario rol)
    {
        return rol switch
        {
            RolUsuario.Operador => EstadoEmpleado.PendienteAprobacion,
            RolUsuario.Aprobador => EstadoEmpleado.Activo,
            RolUsuario.Administrador => EstadoEmpleado.Activo,
            _ => EstadoEmpleado.PendienteAprobacion // Por seguridad, default a pendiente
        };
    }
    
    /// <summary>
    /// Verifica si un rol puede crear empleados con un estado específico.
    /// - Operador: solo puede crear con PendienteAprobacion
    /// - Aprobador/Admin: solo pueden crear con Activo
    /// </summary>
    public static bool PuedeCrearConEstado(RolUsuario rol, EstadoEmpleado estado)
    {
        return rol switch
        {
            RolUsuario.Operador => estado == EstadoEmpleado.PendienteAprobacion,
            RolUsuario.Aprobador => estado == EstadoEmpleado.Activo,
            RolUsuario.Administrador => estado == EstadoEmpleado.Activo,
            _ => false
        };
    }
    
    /// <summary>
    /// Verifica si una transición de estado es válida según las reglas del negocio.
    /// </summary>
    public static bool EsTransicionValida(EstadoEmpleado desde, EstadoEmpleado hacia)
    {
        // Desde Retirado no hay retorno (estado final)
        if (desde == EstadoEmpleado.Retirado)
            return false;
        
        // Desde Rechazado no hay retorno (estado final)
        if (desde == EstadoEmpleado.Rechazado)
            return false;
        
        return (desde, hacia) switch
        {
            // Desde PendienteAprobacion
            (EstadoEmpleado.PendienteAprobacion, EstadoEmpleado.Activo) => true,
            (EstadoEmpleado.PendienteAprobacion, EstadoEmpleado.Rechazado) => true,
            
            // Desde Activo - puede ir a cualquier estado temporal o final
            (EstadoEmpleado.Activo, EstadoEmpleado.EnVacaciones) => true,
            (EstadoEmpleado.Activo, EstadoEmpleado.EnLicencia) => true,
            (EstadoEmpleado.Activo, EstadoEmpleado.EnIncapacidad) => true,
            (EstadoEmpleado.Activo, EstadoEmpleado.Suspendido) => true,
            (EstadoEmpleado.Activo, EstadoEmpleado.Retirado) => true,
            
            // Desde estados temporales - vuelven a Activo
            (EstadoEmpleado.EnVacaciones, EstadoEmpleado.Activo) => true,
            (EstadoEmpleado.EnLicencia, EstadoEmpleado.Activo) => true,
            (EstadoEmpleado.EnIncapacidad, EstadoEmpleado.Activo) => true,
            
            // Desde Suspendido - puede volver a Activo o ser Retirado
            (EstadoEmpleado.Suspendido, EstadoEmpleado.Activo) => true,
            (EstadoEmpleado.Suspendido, EstadoEmpleado.Retirado) => true,
            
            // Cualquier otra transición no está permitida
            _ => false
        };
    }
    
    /// <summary>
    /// Verifica si un rol tiene permiso para realizar una transición específica.
    /// </summary>
    public static bool TienePermisoParaTransicion(RolUsuario rol, EstadoEmpleado desde, EstadoEmpleado hacia)
    {
        // Primero verificar si la transición es válida
        if (!EsTransicionValida(desde, hacia))
            return false;
        
        // Transiciones que requieren Aprobador o Admin
        var requiereAprobador = (desde, hacia) switch
        {
            (EstadoEmpleado.PendienteAprobacion, EstadoEmpleado.Activo) => true,
            (EstadoEmpleado.PendienteAprobacion, EstadoEmpleado.Rechazado) => true,
            (EstadoEmpleado.Activo, EstadoEmpleado.Suspendido) => true,
            (EstadoEmpleado.Activo, EstadoEmpleado.Retirado) => true,
            (EstadoEmpleado.Suspendido, EstadoEmpleado.Retirado) => true,
            _ => false
        };
        
        // Transiciones que requieren SOLO Admin
        var requiereAdmin = (desde, hacia) switch
        {
            (EstadoEmpleado.Suspendido, EstadoEmpleado.Activo) => true, // Solo Admin puede levantar suspensión
            _ => false
        };
        
        if (requiereAdmin)
            return rol == RolUsuario.Administrador;
        
        if (requiereAprobador)
            return rol == RolUsuario.Aprobador || rol == RolUsuario.Administrador;
        
        // Transiciones que cualquier rol puede hacer (estados temporales)
        return true;
    }
    
    /// <summary>
    /// Obtiene las transiciones permitidas desde un estado para un rol específico.
    /// Útil para mostrar solo opciones válidas en la UI.
    /// </summary>
    public static IEnumerable<EstadoEmpleado> ObtenerTransicionesPermitidas(EstadoEmpleado estadoActual, RolUsuario rol)
    {
        var todosLosEstados = Enum.GetValues<EstadoEmpleado>();
        
        foreach (var estado in todosLosEstados)
        {
            if (estado != estadoActual && TienePermisoParaTransicion(rol, estadoActual, estado))
            {
                yield return estado;
            }
        }
    }
    
    /// <summary>
    /// Obtiene una descripción amigable del estado para mostrar en la UI.
    /// </summary>
    public static string ObtenerDescripcion(EstadoEmpleado estado)
    {
        return estado switch
        {
            EstadoEmpleado.PendienteAprobacion => "PENDIENTE DE APROBACIÓN",
            EstadoEmpleado.Activo => "ACTIVO",
            EstadoEmpleado.EnVacaciones => "EN VACACIONES",
            EstadoEmpleado.EnLicencia => "EN LICENCIA",
            EstadoEmpleado.EnIncapacidad => "EN INCAPACIDAD",
            EstadoEmpleado.Suspendido => "SUSPENDIDO",
            EstadoEmpleado.Retirado => "RETIRADO",
            EstadoEmpleado.Rechazado => "RECHAZADO",
            _ => estado.ToString().ToUpperInvariant()
        };
    }
    
    /// <summary>
    /// Obtiene el color CSS para mostrar el estado en la UI.
    /// Usa los colores definidos en hospital.css.
    /// </summary>
    public static string ObtenerColorCss(EstadoEmpleado estado)
    {
        return estado switch
        {
            EstadoEmpleado.Activo => "var(--color-success, #006600)",
            EstadoEmpleado.PendienteAprobacion => "var(--color-warning, #FF9900)",
            EstadoEmpleado.EnVacaciones => "#0066CC",
            EstadoEmpleado.EnLicencia => "#0066CC",
            EstadoEmpleado.EnIncapacidad => "#CC6600",
            EstadoEmpleado.Suspendido => "var(--color-error, #CC0000)",
            EstadoEmpleado.Retirado => "#666666",
            EstadoEmpleado.Rechazado => "var(--color-error, #CC0000)",
            _ => "inherit"
        };
    }
    
    /// <summary>
    /// Indica si un estado es considerado "activo" para fines de nómina y asistencia.
    /// </summary>
    public static bool EsEstadoActivo(EstadoEmpleado estado)
    {
        return estado switch
        {
            EstadoEmpleado.Activo => true,
            EstadoEmpleado.EnVacaciones => true,
            EstadoEmpleado.EnLicencia => true,
            EstadoEmpleado.EnIncapacidad => true,
            _ => false
        };
    }
    
    /// <summary>
    /// Indica si un estado es temporal (el empleado regresará a Activo).
    /// </summary>
    public static bool EsEstadoTemporal(EstadoEmpleado estado)
    {
        return estado switch
        {
            EstadoEmpleado.EnVacaciones => true,
            EstadoEmpleado.EnLicencia => true,
            EstadoEmpleado.EnIncapacidad => true,
            EstadoEmpleado.Suspendido => true,
            _ => false
        };
    }
    
    /// <summary>
    /// Indica si un estado es final (no hay retorno).
    /// </summary>
    public static bool EsEstadoFinal(EstadoEmpleado estado)
    {
        return estado switch
        {
            EstadoEmpleado.Retirado => true,
            EstadoEmpleado.Rechazado => true,
            _ => false
        };
    }
}
