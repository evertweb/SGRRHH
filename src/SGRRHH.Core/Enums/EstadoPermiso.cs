namespace SGRRHH.Core.Enums;

/// <summary>
/// Estados de un permiso en el flujo de aprobación
/// </summary>
public enum EstadoPermiso
{
    /// <summary>
    /// Permiso pendiente de aprobación
    /// </summary>
    Pendiente = 1,
    
    /// <summary>
    /// Permiso aprobado
    /// </summary>
    Aprobado = 2,
    
    /// <summary>
    /// Permiso rechazado
    /// </summary>
    Rechazado = 3,
    
    /// <summary>
    /// Permiso cancelado por el solicitante
    /// </summary>
    Cancelado = 4
}
