using SGRRHH.Local.Domain.Entities;

namespace SGRRHH.Local.Shared.Interfaces;

public interface IActividadRepository : IRepository<Actividad>
{
    new Task<IEnumerable<Actividad>> GetAllActiveAsync();
    
    Task<IEnumerable<Actividad>> GetByCategoriaAsync(string categoria);
    
    Task<IEnumerable<string>> GetCategoriasAsync();
    
    Task<IEnumerable<Actividad>> SearchAsync(string searchTerm);
    
    Task<bool> ExistsCodigoAsync(string codigo, int? excludeId = null);
    
    Task<string> GetNextCodigoAsync();
}


