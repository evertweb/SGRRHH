namespace SGRRHH.Core.Enums;

/// <summary>
/// Roles de usuario en el sistema
/// </summary>
public enum RolUsuario
{
    /// <summary>
    /// Administrador - Acceso total al sistema
    /// </summary>
    Administrador = 1,
    
    /// <summary>
    /// Aprobador (Ingeniera) - Puede aprobar permisos y consultar
    /// </summary>
    Aprobador = 2,
    
    /// <summary>
    /// Operador (Secretaria) - Puede registrar y solicitar
    /// </summary>
    Operador = 3
}
