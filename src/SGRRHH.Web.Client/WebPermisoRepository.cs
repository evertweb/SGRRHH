using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Web.Client;

public class WebPermisoRepository : WebFirestoreRepository<Permiso>, IPermisoRepository
{
    public WebPermisoRepository(FirebaseJsInterop firebase) : base(firebase, "permisos") { }

    public async Task<IEnumerable<Permiso>> GetPendientesAsync()
    {
        var results = await _firebase.QueryCollectionAsync<object>(_collectionName, "estado", "==", EstadoPermiso.Pendiente.ToString());
        return results.Select(MapToEntity).ToList();
    }

    public async Task<IEnumerable<Permiso>> GetByEmpleadoIdAsync(int empleadoId)
    {
        var results = await _firebase.QueryCollectionAsync<object>(_collectionName, "empleadoId", "==", empleadoId);
        return results.Select(MapToEntity).ToList();
    }

    public async Task<IEnumerable<Permiso>> GetByRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        var all = await GetAllAsync();
        return all.Where(p => p.FechaInicio >= fechaInicio && p.FechaFin <= fechaFin).ToList();
    }

    public async Task<IEnumerable<Permiso>> GetByEstadoAsync(EstadoPermiso estado)
    {
        var results = await _firebase.QueryCollectionAsync<object>(_collectionName, "estado", "==", estado.ToString());
        return results.Select(MapToEntity).ToList();
    }

    public async Task<string> GetProximoNumeroActaAsync()
    {
        var nextId = await GetNextIdAsync();
        return $"ACT-{nextId:D4}";
    }

    public async Task<bool> ExisteSolapamientoAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin, int? excludePermisoId = null)
    {
        var permisos = await GetByEmpleadoIdAsync(empleadoId);
        return permisos.Any(p => 
            (excludePermisoId == null || p.Id != excludePermisoId) &&
            p.Activo &&
            p.Estado != EstadoPermiso.Rechazado &&
            ((fechaInicio >= p.FechaInicio && fechaInicio <= p.FechaFin) ||
             (fechaFin >= p.FechaInicio && fechaFin <= p.FechaFin) ||
             (p.FechaInicio >= fechaInicio && p.FechaInicio <= fechaFin)));
    }

    public async Task<IEnumerable<Permiso>> GetAllWithFiltersAsync(int? empleadoId = null, EstadoPermiso? estado = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null)
    {
        var all = await GetAllAsync();
        var query = all.AsQueryable();

        if (empleadoId.HasValue) query = query.Where(p => p.EmpleadoId == empleadoId.Value);
        if (estado.HasValue) query = query.Where(p => p.Estado == estado.Value);
        if (fechaDesde.HasValue) query = query.Where(p => p.FechaInicio >= fechaDesde.Value);
        if (fechaHasta.HasValue) query = query.Where(p => p.FechaFin <= fechaHasta.Value);

        return query.ToList();
    }
}
