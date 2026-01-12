namespace SGRRHH.Local.Domain.Enums;

/// <summary>
/// Roles de usuario del sistema.
/// Los permisos de cada rol se aplican SOLO cuando el Modo Corporativo está activado.
/// Cuando el Modo Corporativo está desactivado, todos los usuarios tienen acceso completo.
/// </summary>
public enum RolUsuario
{
    /// <summary>
    /// Acceso completo al sistema. Puede activar/desactivar Modo Corporativo.
    /// </summary>
    Administrador = 1,
    
    /// <summary>
    /// Puede aprobar/rechazar solicitudes. Acceso a reportes y configuraciones básicas.
    /// Típicamente: Ingeniera, Jefe de área, Supervisor.
    /// </summary>
    Aprobador = 2,
    
    /// <summary>
    /// Puede crear y editar registros, pero requiere aprobación para cambios críticos.
    /// Típicamente: Secretaria, Auxiliar administrativo.
    /// </summary>
    Operador = 3
}


