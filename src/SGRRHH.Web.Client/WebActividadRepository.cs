using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Web.Client;

public class WebActividadRepository : WebFirestoreRepository<Actividad>, IActividadRepository
{
    public WebActividadRepository(FirebaseJsInterop firebase) 
        : base(firebase, "actividades")
    {
    }
    
    public new async Task<IEnumerable<Actividad>> GetAllActiveAsync()
    {
        var all = await GetAllAsync();
        return all.Where(a => a.Activo).OrderBy(a => a.Nombre);
    }
    
    public async Task<IEnumerable<Actividad>> GetByCategoriaAsync(string categoria)
    {
        var all = await GetAllAsync();
        return all.Where(a => a.Categoria == categoria && a.Activo).OrderBy(a => a.Nombre);
    }
    
    public async Task<IEnumerable<string>> GetCategoriasAsync()
    {
        var all = await GetAllAsync();
        return all.Where(a => !string.IsNullOrEmpty(a.Categoria))
                  .Select(a => a.Categoria!)
                  .Distinct()
                  .OrderBy(c => c);
    }
    
    public async Task<IEnumerable<Actividad>> SearchAsync(string searchTerm)
    {
        var all = await GetAllAsync();
        var term = searchTerm.ToLower();
        return all.Where(a => a.Nombre.ToLower().Contains(term) || 
                              (a.Codigo?.ToLower().Contains(term) ?? false));
    }
    
    public async Task<bool> ExistsCodigoAsync(string codigo, int? excludeId = null)
    {
        var all = await GetAllAsync();
        return all.Any(a => a.Codigo == codigo && a.Id != excludeId);
    }
    
    public async Task<string> GetNextCodigoAsync()
    {
        var all = await GetAllAsync();
        var maxCode = all.Where(a => !string.IsNullOrEmpty(a.Codigo) && a.Codigo.StartsWith("ACT"))
                         .Select(a => int.TryParse(a.Codigo.Substring(3), out var n) ? n : 0)
                         .DefaultIfEmpty(0)
                         .Max();
        return $"ACT{(maxCode + 1):D3}";
    }
}
