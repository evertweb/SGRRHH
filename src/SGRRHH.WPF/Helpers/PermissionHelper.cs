using SGRRHH.Core.Enums;

namespace SGRRHH.WPF.Helpers;

/// <summary>
/// Helper centralizado para determinar permisos de UI según el rol del usuario.
/// Controla la visibilidad de botones de acción (Crear/Editar/Eliminar).
/// 
/// NOTA: Todos los roles pueden LEER todos los catálogos porque se necesitan 
/// para formularios de otros módulos. Solo se restringe la ESCRITURA.
/// </summary>
public static class PermissionHelper
{
    /// <summary>
    /// Módulos del sistema para control de permisos
    /// </summary>
    public static class Modulos
    {
        public const string Departamentos = "Departamentos";
        public const string Cargos = "Cargos";
        public const string TiposPermiso = "TiposPermiso";
        public const string Proyectos = "Proyectos";
        public const string Actividades = "Actividades";
        public const string Empleados = "Empleados";
        public const string Contratos = "Contratos";
        public const string Permisos = "Permisos";
        public const string Vacaciones = "Vacaciones";
        public const string Usuarios = "Usuarios";
        public const string Configuracion = "Configuracion";
    }

    /// <summary>
    /// Determina si el usuario puede crear en el módulo especificado
    /// </summary>
    public static bool CanCreate(RolUsuario rol, string modulo)
    {
        return modulo switch
        {
            // Solo Admin puede crear catálogos base
            Modulos.Departamentos => rol == RolUsuario.Administrador,
            Modulos.Cargos => rol == RolUsuario.Administrador,
            Modulos.TiposPermiso => rol == RolUsuario.Administrador,
            Modulos.Actividades => rol == RolUsuario.Administrador,
            
            // Admin y Aprobador pueden crear proyectos
            Modulos.Proyectos => rol == RolUsuario.Administrador || rol == RolUsuario.Aprobador,
            
            // Admin y Operador pueden crear empleados, contratos, vacaciones
            Modulos.Empleados => rol == RolUsuario.Administrador || rol == RolUsuario.Operador,
            Modulos.Contratos => rol == RolUsuario.Administrador || rol == RolUsuario.Operador,
            Modulos.Vacaciones => rol == RolUsuario.Administrador || rol == RolUsuario.Operador,
            
            // Todos pueden crear permisos
            Modulos.Permisos => true,
            
            // Solo Admin puede crear usuarios y configuración
            Modulos.Usuarios => rol == RolUsuario.Administrador,
            Modulos.Configuracion => rol == RolUsuario.Administrador,
            
            _ => false
        };
    }

    /// <summary>
    /// Determina si el usuario puede editar en el módulo especificado
    /// </summary>
    public static bool CanEdit(RolUsuario rol, string modulo)
    {
        return modulo switch
        {
            // Solo Admin puede editar catálogos base
            Modulos.Departamentos => rol == RolUsuario.Administrador,
            Modulos.Cargos => rol == RolUsuario.Administrador,
            Modulos.TiposPermiso => rol == RolUsuario.Administrador,
            Modulos.Actividades => rol == RolUsuario.Administrador,
            
            // Admin y Aprobador pueden editar proyectos
            Modulos.Proyectos => rol == RolUsuario.Administrador || rol == RolUsuario.Aprobador,
            
            // Admin y Operador pueden editar empleados, contratos, vacaciones
            Modulos.Empleados => rol == RolUsuario.Administrador || rol == RolUsuario.Operador,
            Modulos.Contratos => rol == RolUsuario.Administrador || rol == RolUsuario.Operador,
            Modulos.Vacaciones => rol == RolUsuario.Administrador || rol == RolUsuario.Operador,
            
            // Admin, Aprobador y Operador pueden editar permisos
            Modulos.Permisos => true,
            
            // Solo Admin puede editar usuarios y configuración
            Modulos.Usuarios => rol == RolUsuario.Administrador,
            Modulos.Configuracion => rol == RolUsuario.Administrador,
            
            _ => false
        };
    }

    /// <summary>
    /// Determina si el usuario puede eliminar en el módulo especificado
    /// </summary>
    public static bool CanDelete(RolUsuario rol, string modulo)
    {
        // Solo Admin puede eliminar en todos los módulos
        return rol == RolUsuario.Administrador;
    }

    /// <summary>
    /// Determina si el usuario tiene acceso al módulo (solo lectura)
    /// </summary>
    public static bool CanRead(RolUsuario rol, string modulo)
    {
        // Todos los usuarios pueden leer todos los módulos excepto Usuarios y Configuración
        return modulo switch
        {
            Modulos.Usuarios => rol == RolUsuario.Administrador,
            Modulos.Configuracion => rol == RolUsuario.Administrador,
            _ => true
        };
    }
}
