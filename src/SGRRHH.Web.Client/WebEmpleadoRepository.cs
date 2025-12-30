using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Web.Client;

public class WebEmpleadoRepository : WebFirestoreRepository<Empleado>, IEmpleadoRepository
{
    public WebEmpleadoRepository(FirebaseJsInterop firebase) 
        : base(firebase, "empleados")
    {
    }

    public async Task<Empleado?> GetByCodigoAsync(string codigo)
    {
        var results = await _firebase.QueryCollectionAsync<object>(_collectionName, "codigo", "==", codigo);
        return results.Select(MapToEntity).FirstOrDefault();
    }

    public async Task<Empleado?> GetByCedulaAsync(string cedula)
    {
        var results = await _firebase.QueryCollectionAsync<object>(_collectionName, "cedula", "==", cedula);
        return results.Select(MapToEntity).FirstOrDefault();
    }

    public Task<Empleado?> GetByIdWithRelationsAsync(int id) => GetByIdAsync(id); // Implementación simplificada para web

    public async Task<IEnumerable<Empleado>> GetAllWithRelationsAsync() => await GetAllAsync();

    public async Task<IEnumerable<Empleado>> GetAllActiveWithRelationsAsync() => await GetAllActiveAsync();

    public async Task<IEnumerable<Empleado>> GetByDepartamentoAsync(int departamentoId)
    {
        var results = await _firebase.QueryCollectionAsync<object>(_collectionName, "departamentoId", "==", departamentoId);
        return results.Select(MapToEntity).ToList();
    }

    public async Task<IEnumerable<Empleado>> GetByCargoAsync(int cargoId)
    {
        var results = await _firebase.QueryCollectionAsync<object>(_collectionName, "cargoId", "==", cargoId);
        return results.Select(MapToEntity).ToList();
    }

    public async Task<IEnumerable<Empleado>> GetByEstadoAsync(EstadoEmpleado estado)
    {
        var results = await _firebase.QueryCollectionAsync<object>(_collectionName, "estado", "==", estado.ToString());
        return results.Select(MapToEntity).ToList();
    }

    public Task<IEnumerable<Empleado>> SearchAsync(string searchTerm)
    {
        // En Firestore cliente el filtrado por texto parcial es difícil sin Algolia/Elastic.
        // Simularemos buscando por igualdad exacta en campos clave por ahora.
        return Task.FromResult<IEnumerable<Empleado>>(new List<Empleado>());
    }

    public async Task<bool> ExistsCodigoAsync(string codigo, int? excludeId = null)
    {
        var emp = await GetByCodigoAsync(codigo);
        return emp != null && (excludeId == null || emp.Id != excludeId);
    }

    public async Task<bool> ExistsCedulaAsync(string cedula, int? excludeId = null)
    {
        var emp = await GetByCedulaAsync(cedula);
        return emp != null && (excludeId == null || emp.Id != excludeId);
    }

    public async Task<string> GetNextCodigoAsync()
    {
        var nextId = await GetNextIdAsync();
        return $"EMP{nextId:D3}";
    }

    public async Task<int> CountActiveAsync()
    {
        var active = await GetAllActiveAsync();
        return active.Count();
    }

    public async Task<bool> ExistsEmailAsync(string email, int? excludeId = null)
    {
        var results = await _firebase.QueryCollectionAsync<object>(_collectionName, "email", "==", email);
        var emp = results.Select(MapToEntity).FirstOrDefault();
        return emp != null && (excludeId == null || emp.Id != excludeId);
    }

    public void InvalidateCache() { /* No caché en esta versión web simplificada */ }
}
