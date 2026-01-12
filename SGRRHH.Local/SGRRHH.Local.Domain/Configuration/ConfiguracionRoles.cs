namespace SGRRHH.Local.Domain.Configuration;

using SGRRHH.Local.Domain.Enums;

/// <summary>
/// Configuración centralizada de permisos por rol y módulo.
/// Esta es la ÚNICA fuente de verdad para los permisos del sistema.
/// Solo se aplica cuando el Modo Corporativo está ACTIVADO.
/// </summary>
public static class ConfiguracionRoles
{
    /// <summary>
    /// Matriz de permisos: Define qué puede hacer cada rol en cada módulo.
    /// Solo se aplica cuando Modo Corporativo está ACTIVADO.
    /// </summary>
    private static readonly Dictionary<(RolUsuario Rol, string Modulo), PermisosModulo> _permisos = new()
    {
        // ========== MÓDULO: EMPLEADOS ==========
        
        // Administrador - Acceso completo
        { (RolUsuario.Administrador, "Empleados"), PermisosModulo.Completo },
        
        // Aprobador (Ingeniera) - Puede aprobar, editar todo, no eliminar
        { (RolUsuario.Aprobador, "Empleados"), 
            PermisosModulo.Ver | 
            PermisosModulo.Crear | 
            PermisosModulo.Editar | 
            PermisosModulo.Aprobar | 
            PermisosModulo.Rechazar | 
            PermisosModulo.EditarDatosCriticos | 
            PermisosModulo.Retirar |
            PermisosModulo.Exportar },
        
        // Operador (Secretaria) - Solo crear y editar básico, sin aprobar
        { (RolUsuario.Operador, "Empleados"), 
            PermisosModulo.Ver | 
            PermisosModulo.Crear | 
            PermisosModulo.Editar |
            PermisosModulo.Exportar },
        
        // ========== MÓDULOS FUTUROS (plantilla) ==========
        // Descomentar y ajustar cuando se implementen
        
        // { (RolUsuario.Administrador, "Permisos"), PermisosModulo.Completo },
        // { (RolUsuario.Aprobador, "Permisos"), PermisosModulo.Ver | PermisosModulo.Aprobar | PermisosModulo.Rechazar },
        // { (RolUsuario.Operador, "Permisos"), PermisosModulo.Ver | PermisosModulo.Crear },
        
        // { (RolUsuario.Administrador, "Vacaciones"), PermisosModulo.Completo },
        // { (RolUsuario.Aprobador, "Vacaciones"), PermisosModulo.Ver | PermisosModulo.Aprobar | PermisosModulo.Rechazar },
        // { (RolUsuario.Operador, "Vacaciones"), PermisosModulo.Ver | PermisosModulo.Crear },
    };
    
    /// <summary>
    /// Obtiene los permisos de un rol para un módulo específico.
    /// </summary>
    public static PermisosModulo ObtenerPermisos(RolUsuario rol, string modulo)
    {
        if (_permisos.TryGetValue((rol, modulo), out var permisos))
            return permisos;
        
        // Por defecto: solo lectura si no está definido
        return PermisosModulo.Ver;
    }
    
    /// <summary>
    /// Verifica si un rol tiene un permiso específico en un módulo.
    /// </summary>
    public static bool TienePermiso(RolUsuario rol, string modulo, PermisosModulo permiso)
    {
        var permisos = ObtenerPermisos(rol, modulo);
        return permisos.HasFlag(permiso);
    }
    
    /// <summary>
    /// Lista de todos los módulos configurados.
    /// </summary>
    public static IEnumerable<string> ModulosConfigurados => 
        _permisos.Keys.Select(k => k.Modulo).Distinct();
}
