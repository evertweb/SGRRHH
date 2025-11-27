using SGRRHH.Core.Common;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;

namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Servicio para la gestión de permisos y su flujo de aprobación
/// </summary>
public interface IPermisoService
{
    /// <summary>
    /// Obtiene todos los permisos con filtros opcionales
    /// </summary>
    Task<ServiceResult<IEnumerable<Permiso>>> GetAllAsync(int? empleadoId = null, EstadoPermiso? estado = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null);
    
    /// <summary>
    /// Obtiene un permiso por su ID
    /// </summary>
    Task<ServiceResult<Permiso>> GetByIdAsync(int id);
    
    /// <summary>
    /// Obtiene los permisos pendientes de aprobación
    /// </summary>
    Task<ServiceResult<IEnumerable<Permiso>>> GetPendientesAsync();
    
    /// <summary>
    /// Obtiene los permisos de un empleado específico
    /// </summary>
    Task<ServiceResult<IEnumerable<Permiso>>> GetByEmpleadoIdAsync(int empleadoId);
    
    /// <summary>
    /// Solicita un nuevo permiso
    /// </summary>
    Task<ServiceResult<Permiso>> SolicitarPermisoAsync(Permiso permiso, int usuarioSolicitanteId);
    
    /// <summary>
    /// Aprueba un permiso pendiente
    /// </summary>
    Task<ServiceResult<Permiso>> AprobarPermisoAsync(int permisoId, int usuarioAprobadorId, string? observaciones = null);
    
    /// <summary>
    /// Rechaza un permiso pendiente
    /// </summary>
    Task<ServiceResult<Permiso>> RechazarPermisoAsync(int permisoId, int usuarioAprobadorId, string motivoRechazo);
    
    /// <summary>
    /// Cancela un permiso
    /// </summary>
    Task<ServiceResult<Permiso>> CancelarPermisoAsync(int permisoId, int usuarioId);
    
    /// <summary>
    /// Actualiza un permiso existente (solo si está pendiente)
    /// </summary>
    Task<ServiceResult<Permiso>> UpdateAsync(Permiso permiso);
    
    /// <summary>
    /// Elimina un permiso (solo si está pendiente)
    /// </summary>
    Task<ServiceResult<bool>> DeleteAsync(int id);
}
