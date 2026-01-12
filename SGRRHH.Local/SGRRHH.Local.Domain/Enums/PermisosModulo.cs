namespace SGRRHH.Local.Domain.Enums;

/// <summary>
/// Define los tipos de acciones que se pueden realizar en cada módulo.
/// Usado para verificar permisos cuando el Modo Corporativo está activo.
/// </summary>
[Flags]
public enum PermisosModulo
{
    Ninguno = 0,
    
    // Permisos básicos
    Ver = 1,
    Crear = 2,
    Editar = 4,
    Eliminar = 8,
    
    // Permisos de flujo de aprobación
    Aprobar = 16,
    Rechazar = 32,
    
    // Permisos especiales
    EditarDatosCriticos = 64,   // Salario, cargo, etc.
    Retirar = 128,              // Dar de baja empleados
    Exportar = 256,
    Importar = 512,
    
    // Combinaciones comunes
    Lectura = Ver,
    Basico = Ver | Crear | Editar,
    Completo = Ver | Crear | Editar | Eliminar | Aprobar | Rechazar | EditarDatosCriticos | Retirar | Exportar | Importar
}
