namespace SGRRHH.Local.Domain.Enums;

/// <summary>
/// Define los tipos de acciones que se pueden realizar en cada módulo.
/// Usado para verificar permisos cuando el Modo Corporativo está activo.
/// </summary>
[Flags]
public enum ModulePermissions
{
    None = 0,
    
    // Permisos básicos
    View = 1,
    Create = 2,
    Edit = 4,
    Delete = 8,
    
    // Permisos de flujo de aprobación
    Approve = 16,
    Reject = 32,
    
    // Permisos especiales
    EditCriticalData = 64,   // Salario, cargo, etc.
    Terminate = 128,              // Dar de baja empleados
    Export = 256,
    Import = 512,
    
    // Combinaciones comunes
    ReadOnly = View,
    Basic = View | Create | Edit,
    Full = View | Create | Edit | Delete | Approve | Reject | EditCriticalData | Terminate | Export | Import
}
