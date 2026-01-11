namespace SGRRHH.Local.Domain.Enums;

/// <summary>
/// Sistema de roles temporalmente deshabilitado - Todos los usuarios funcionan como Administrador.
/// Este enum se mantiene por compatibilidad con la BD pero será rediseñado.
/// </summary>
public enum RolUsuario
{
    /// <summary>
    /// Único rol activo actualmente - Acceso completo al sistema
    /// </summary>
    Administrador = 1,
    
    /// <summary>
    /// [DESHABILITADO] Será rediseñado
    /// </summary>
    [Obsolete("Rol deshabilitado temporalmente. Todos los usuarios funcionan como Administrador.")]
    Aprobador = 2,
    
    /// <summary>
    /// [DESHABILITADO] Será rediseñado
    /// </summary>
    [Obsolete("Rol deshabilitado temporalmente. Todos los usuarios funcionan como Administrador.")]
    Operador = 3
}


