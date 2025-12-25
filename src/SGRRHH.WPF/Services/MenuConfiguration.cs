using SGRRHH.Core.Enums;
using SGRRHH.WPF.ViewModels;

namespace SGRRHH.WPF.Services;

/// <summary>
/// Configuración centralizada del menú de navegación.
/// Extrae la lógica de construcción del menú desde MainViewModel.
/// </summary>
public static class MenuConfiguration
{
    /// <summary>
    /// Obtiene los elementos del menú filtrados por rol de usuario.
    /// </summary>
    /// <param name="userRole">Rol del usuario actual</param>
    /// <returns>Lista de elementos de menú visibles para el rol</returns>
    public static IEnumerable<MenuItemViewModel> GetMenuItems(RolUsuario userRole)
    {
        var allMenuItems = GetAllMenuItems();
        
        // Filtrar por rol y añadir separadores solo si hay items visibles
        MenuItemViewModel? pendingSeparator = null;
        
        foreach (var item in allMenuItems)
        {
            if (item.IsSeparator)
            {
                // Guardar separador para añadirlo solo si hay items después
                pendingSeparator = item;
                continue;
            }
            
            if (item.AllowedRoles.Contains(userRole))
            {
                // Añadir separador pendiente si existe
                if (pendingSeparator != null)
                {
                    yield return pendingSeparator;
                    pendingSeparator = null;
                }
                yield return item;
            }
        }
    }
    
    /// <summary>
    /// Define todos los elementos del menú con sus roles permitidos.
    /// </summary>
    private static IEnumerable<MenuItemViewModel> GetAllMenuItems()
    {
        // ============================================
        // INICIO
        // ============================================
        yield return new MenuItemViewModel { IsSeparator = true, Title = "INICIO", Category = "header" };
        yield return new MenuItemViewModel
        {
            Icon = "📊",
            Title = "Dashboard",
            ViewName = "Dashboard",
            Category = "Inicio",
            AllowedRoles = new[] { RolUsuario.Administrador, RolUsuario.Aprobador, RolUsuario.Operador }
        };
        
        // ============================================
        // CATÁLOGOS (Estructura Base - se configura primero)
        // ============================================
        yield return new MenuItemViewModel { IsSeparator = true, Title = "CATÁLOGOS", Category = "header" };
        yield return new MenuItemViewModel
        {
            Icon = "🏢",
            Title = "Departamentos",
            ViewName = "Departamentos",
            Category = "Catálogos",
            AllowedRoles = new[] { RolUsuario.Administrador, RolUsuario.Aprobador, RolUsuario.Operador }
        };
        yield return new MenuItemViewModel
        {
            Icon = "💼",
            Title = "Cargos",
            ViewName = "Cargos",
            Category = "Catálogos",
            AllowedRoles = new[] { RolUsuario.Administrador, RolUsuario.Aprobador, RolUsuario.Operador }
        };
        yield return new MenuItemViewModel
        {
            Icon = "📋",
            Title = "Tipos de Permiso",
            ViewName = "TiposPermiso",
            Category = "Catálogos",
            AllowedRoles = new[] { RolUsuario.Administrador }
        };
        yield return new MenuItemViewModel
        {
            Icon = "🚀",
            Title = "Proyectos",
            ViewName = "Proyectos",
            Category = "Catálogos",
            AllowedRoles = new[] { RolUsuario.Administrador, RolUsuario.Aprobador }
        };
        yield return new MenuItemViewModel
        {
            Icon = "📝",
            Title = "Actividades",
            ViewName = "Actividades",
            Category = "Catálogos",
            AllowedRoles = new[] { RolUsuario.Administrador }
        };
        
        // ============================================
        // PERSONAL (requiere catálogos configurados)
        // ============================================
        yield return new MenuItemViewModel { IsSeparator = true, Title = "PERSONAL", Category = "header" };
        yield return new MenuItemViewModel
        {
            Icon = "👥",
            Title = "Empleados",
            ViewName = "Empleados",
            Category = "Personal",
            AllowedRoles = new[] { RolUsuario.Administrador, RolUsuario.Aprobador, RolUsuario.Operador }
        };
        yield return new MenuItemViewModel
        {
            Icon = "📄",
            Title = "Contratos",
            ViewName = "Contratos",
            Category = "Personal",
            AllowedRoles = new[] { RolUsuario.Administrador, RolUsuario.Aprobador, RolUsuario.Operador }
        };
        yield return new MenuItemViewModel
        {
            Icon = "📁",
            Title = "Documentos",
            ViewName = "Documentos",
            Category = "Personal",
            AllowedRoles = new[] { RolUsuario.Administrador, RolUsuario.Aprobador, RolUsuario.Operador }
        };
        
        // ============================================
        // OPERACIONES DIARIAS (requiere empleados)
        // ============================================
        yield return new MenuItemViewModel { IsSeparator = true, Title = "OPERACIONES", Category = "header" };
        yield return new MenuItemViewModel
        {
            Icon = "📅",
            Title = "Control Diario",
            ViewName = "ControlDiario",
            Category = "Operaciones",
            AllowedRoles = new[] { RolUsuario.Administrador, RolUsuario.Aprobador, RolUsuario.Operador }
        };
        yield return new MenuItemViewModel
        {
            Icon = "📝",
            Title = "Permisos",
            ViewName = "Permisos",
            Category = "Operaciones",
            AllowedRoles = new[] { RolUsuario.Administrador, RolUsuario.Aprobador, RolUsuario.Operador }
        };
        yield return new MenuItemViewModel
        {
            Icon = "🏖️",
            Title = "Vacaciones",
            ViewName = "Vacaciones",
            Category = "Operaciones",
            AllowedRoles = new[] { RolUsuario.Administrador, RolUsuario.Aprobador, RolUsuario.Operador }
        };
        yield return new MenuItemViewModel
        {
            Icon = "✅",
            Title = "Aprobar Vacaciones",
            ViewName = "BandejaVacaciones",
            Category = "Operaciones",
            AllowedRoles = new[] { RolUsuario.Administrador, RolUsuario.Aprobador }
        };
        
        // ============================================
        // SISTEMA (Administración y Reportes)
        // ============================================
        yield return new MenuItemViewModel { IsSeparator = true, Title = "SISTEMA", Category = "header" };
        yield return new MenuItemViewModel
        {
            Icon = "📈",
            Title = "Reportes",
            ViewName = "Reportes",
            Category = "Sistema",
            AllowedRoles = new[] { RolUsuario.Administrador, RolUsuario.Aprobador, RolUsuario.Operador }
        };
        yield return new MenuItemViewModel
        {
            Icon = "👤",
            Title = "Usuarios",
            ViewName = "Usuarios",
            Category = "Sistema",
            AllowedRoles = new[] { RolUsuario.Administrador }
        };
        yield return new MenuItemViewModel
        {
            Icon = "⚙️",
            Title = "Configuración",
            ViewName = "Configuracion",
            Category = "Sistema",
            AllowedRoles = new[] { RolUsuario.Administrador }
        };
    }
}
