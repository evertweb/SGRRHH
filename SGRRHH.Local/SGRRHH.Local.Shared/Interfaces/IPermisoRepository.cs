using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Shared.Interfaces;

public interface IPermisoRepository : IRepository<Permiso>
{
    Task<IEnumerable<Permiso>> GetPendientesAsync();
    
    Task<IEnumerable<Permiso>> GetByEmpleadoIdAsync(int empleadoId);
    
    Task<IEnumerable<Permiso>> GetByRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin);
    
    Task<IEnumerable<Permiso>> GetByEstadoAsync(EstadoPermiso estado);
    
    Task<string> GetProximoNumeroActaAsync();
    
    Task<bool> ExisteSolapamientoAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin, int? excludePermisoId = null);
    
    Task<IEnumerable<Permiso>> GetAllWithFiltersAsync(int? empleadoId = null, EstadoPermiso? estado = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null);
}


