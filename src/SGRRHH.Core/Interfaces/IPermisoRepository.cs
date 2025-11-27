using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;

namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Repositorio para la gestión de permisos
/// </summary>
public interface IPermisoRepository : IRepository<Permiso>
{
    /// <summary>
    /// Obtiene todos los permisos pendientes de aprobación
    /// </summary>
    Task<IEnumerable<Permiso>> GetPendientesAsync();
    
    /// <summary>
    /// Obtiene los permisos de un empleado específico
    /// </summary>
    Task<IEnumerable<Permiso>> GetByEmpleadoIdAsync(int empleadoId);
    
    /// <summary>
    /// Obtiene los permisos en un rango de fechas
    /// </summary>
    Task<IEnumerable<Permiso>> GetByRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin);
    
    /// <summary>
    /// Obtiene los permisos por estado
    /// </summary>
    Task<IEnumerable<Permiso>> GetByEstadoAsync(EstadoPermiso estado);
    
    /// <summary>
    /// Obtiene el próximo número de acta disponible
    /// </summary>
    Task<string> GetProximoNumeroActaAsync();
    
    /// <summary>
    /// Verifica si existe solapamiento de fechas para un empleado
    /// </summary>
    Task<bool> ExisteSolapamientoAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin, int? excludePermisoId = null);
    
    /// <summary>
    /// Obtiene todos los permisos con filtros opcionales
    /// </summary>
    Task<IEnumerable<Permiso>> GetAllWithFiltersAsync(int? empleadoId = null, EstadoPermiso? estado = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null);
}
