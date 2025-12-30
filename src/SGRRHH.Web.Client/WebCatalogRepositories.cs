using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Web.Client;

public class WebCargoRepository : WebFirestoreRepository<Cargo>, ICargoRepository
{
    public WebCargoRepository(FirebaseJsInterop firebase) : base(firebase, "cargos") { }

    public async Task<Cargo?> GetByCodigoAsync(string codigo)
    {
        var results = await _firebase.QueryCollectionAsync<object>(_collectionName, "codigo", "==", codigo);
        return results.Select(MapToEntity).FirstOrDefault();
    }

    public Task<Cargo?> GetByIdWithDepartamentoAsync(int id) => GetByIdAsync(id);
    public Task<Cargo?> GetByIdWithEmpleadosAsync(int id) => GetByIdAsync(id);
    public async Task<IEnumerable<Cargo>> GetAllWithDepartamentoAsync() => await GetAllAsync();
    public async Task<IEnumerable<Cargo>> GetAllActiveWithDepartamentoAsync() => await GetAllActiveAsync();

    public async Task<IEnumerable<Cargo>> GetByDepartamentoAsync(int departamentoId)
    {
        var results = await _firebase.QueryCollectionAsync<object>(_collectionName, "departamentoId", "==", departamentoId);
        return results.Select(MapToEntity).ToList();
    }

    public async Task<bool> ExistsCodigoAsync(string codigo, int? excludeId = null)
    {
        var cargo = await GetByCodigoAsync(codigo);
        return cargo != null && (excludeId == null || cargo.Id != excludeId);
    }

    public async Task<string> GetNextCodigoAsync()
    {
        var nextId = await GetNextIdAsync();
        return $"CAR{nextId:D3}";
    }

    public Task<bool> HasEmpleadosAsync(int id) => Task.FromResult(false);

    public async Task<int> CountActiveAsync()
    {
        var active = await GetAllActiveAsync();
        return active.Count();
    }

    public async Task<bool> ExistsNombreInDepartamentoAsync(string nombre, int? departamentoId, int? excludeId = null)
    {
        var results = await GetAllAsync();
        return results.Any(c => c.Nombre == nombre && c.DepartamentoId == departamentoId && (excludeId == null || c.Id != excludeId));
    }

    public void InvalidateCache() { }
}

public class WebDepartamentoRepository : WebFirestoreRepository<Departamento>, IDepartamentoRepository
{
    public WebDepartamentoRepository(FirebaseJsInterop firebase) : base(firebase, "departamentos") { }

    public async Task<Departamento?> GetByCodigoAsync(string codigo)
    {
        var results = await _firebase.QueryCollectionAsync<object>(_collectionName, "codigo", "==", codigo);
        return results.Select(MapToEntity).FirstOrDefault();
    }

    public Task<Departamento?> GetByIdWithEmpleadosAsync(int id) => GetByIdAsync(id);
    public Task<Departamento?> GetByIdWithCargosAsync(int id) => GetByIdAsync(id);

    public async Task<bool> ExistsCodigoAsync(string codigo, int? excludeId = null)
    {
        var dep = await GetByCodigoAsync(codigo);
        return dep != null && (excludeId == null || dep.Id != excludeId);
    }

    public async Task<string> GetNextCodigoAsync()
    {
        var nextId = await GetNextIdAsync();
        return $"DEP{nextId:D3}";
    }

    public Task<bool> HasEmpleadosAsync(int id) => Task.FromResult(false);
    public Task<bool> HasCargosAsync(int id) => Task.FromResult(false);

    public async Task<int> CountActiveAsync()
    {
        var active = await GetAllActiveAsync();
        return active.Count();
    }

    public async Task<bool> ExistsNombreAsync(string nombre, int? excludeId = null)
    {
        var results = await GetAllAsync();
        return results.Any(d => d.Nombre == nombre && (excludeId == null || d.Id != excludeId));
    }

    public async Task<IEnumerable<Departamento>> GetAllWithEmpleadosCountAsync()
    {
        // Simplificación para la web: retornar los departamentos normales
        return await GetAllAsync();
    }

    public Task<bool> ExistsByNameAsync(string nombre, int? excludeId = null) => ExistsNombreAsync(nombre, excludeId);

    public void InvalidateCache() { }

    public async Task<(IEnumerable<Departamento> Items, int TotalCount)> GetAllActivePagedAsync(int pageNumber, int pageSize)
    {
        var all = await GetAllActiveAsync();
        var items = all.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        return (items, all.Count());
    }
}

public class WebTipoPermisoRepository : WebFirestoreRepository<TipoPermiso>, ITipoPermisoRepository
{
    public WebTipoPermisoRepository(FirebaseJsInterop firebase) : base(firebase, "tipos-permiso") { }

    public async Task<TipoPermiso?> GetByCodigoAsync(string codigo)
    {
        var results = await _firebase.QueryCollectionAsync<object>(_collectionName, "codigo", "==", codigo);
        return results.Select(MapToEntity).FirstOrDefault();
    }

    public async Task<bool> ExistsCodigoAsync(string codigo, int? excludeId = null)
    {
        var tp = await GetByCodigoAsync(codigo);
        return tp != null && (excludeId == null || tp.Id != excludeId);
    }

    public async Task<string> GetNextCodigoAsync()
    {
        var nextId = await GetNextIdAsync();
        return $"TP{nextId:D3}";
    }

    public Task<IEnumerable<TipoPermiso>> GetActivosAsync() => GetAllActiveAsync();

    public async Task<bool> ExisteNombreAsync(string nombre, int? excludeId = null)
    {
        var results = await GetAllAsync();
        return results.Any(tp => tp.Nombre == nombre && (excludeId == null || tp.Id != excludeId));
    }

    public void InvalidateCache() { }
}
