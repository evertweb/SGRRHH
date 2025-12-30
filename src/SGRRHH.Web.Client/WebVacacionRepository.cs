using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Web.Client;

public class WebVacacionRepository : WebFirestoreRepository<Vacacion>, IVacacionRepository
{
    public WebVacacionRepository(FirebaseJsInterop firebase) 
        : base(firebase, "vacaciones")
    {
    }

    public async Task<IEnumerable<Vacacion>> GetByEmpleadoIdAsync(int empleadoId)
    {
        var results = await _firebase.QueryCollectionAsync<object>(_collectionName, "empleadoId", "==", empleadoId);
        return results.Select(MapToEntity).ToList();
    }

    public async Task<IEnumerable<Vacacion>> GetByEmpleadoYPeriodoAsync(int empleadoId, int periodo)
    {
        var results = await _firebase.QueryCollectionAsync<object>(_collectionName, "empleadoId", "==", empleadoId);
        return results.Select(MapToEntity).Where(v => v.PeriodoCorrespondiente == periodo).ToList();
    }

    public async Task<IEnumerable<Vacacion>> GetByRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        // Simplificación: filtrar en memoria para evitar índices compuestos complejos en esta fase
        var all = await GetAllAsync();
        return all.Where(v => v.FechaInicio >= fechaInicio && v.FechaFin <= fechaFin).ToList();
    }

    public async Task<bool> ExisteTraslapeAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin, int? vacacionIdExcluir = null)
    {
        var vacaciones = await GetByEmpleadoIdAsync(empleadoId);
        return vacaciones.Any(v => 
            (vacacionIdExcluir == null || v.Id != vacacionIdExcluir) &&
            v.Activo &&
            (v.Estado != EstadoVacacion.Rechazada && v.Estado != EstadoVacacion.Cancelada) &&
            ((fechaInicio >= v.FechaInicio && fechaInicio <= v.FechaFin) ||
             (fechaFin >= v.FechaInicio && fechaFin <= v.FechaFin) ||
             (v.FechaInicio >= fechaInicio && v.FechaInicio <= fechaFin)));
    }

    public async Task<IEnumerable<Vacacion>> GetByEstadoAsync(EstadoVacacion estado)
    {
        var results = await _firebase.QueryCollectionAsync<object>(_collectionName, "estado", "==", estado.ToString());
        return results.Select(MapToEntity).ToList();
    }
}
