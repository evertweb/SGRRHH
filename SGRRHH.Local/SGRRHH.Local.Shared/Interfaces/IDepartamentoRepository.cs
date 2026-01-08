using SGRRHH.Local.Domain.Entities;

namespace SGRRHH.Local.Shared.Interfaces;

public interface IDepartamentoRepository : IRepository<Departamento>
{
    Task<Departamento?> GetByCodigoAsync(string codigo);
    
    Task<Departamento?> GetByIdWithEmpleadosAsync(int id);
    
    Task<Departamento?> GetByIdWithCargosAsync(int id);
    
    Task<IEnumerable<Departamento>> GetAllWithEmpleadosCountAsync();
    
    Task<bool> ExistsCodigoAsync(string codigo, int? excludeId = null);

    Task<bool> ExistsByNameAsync(string nombre, int? excludeId = null);
    
    Task<string> GetNextCodigoAsync();
    
    Task<bool> HasEmpleadosAsync(int id);
    
    Task<int> CountActiveAsync();

    void InvalidateCache();

    Task<(IEnumerable<Departamento> Items, int TotalCount)> GetAllActivePagedAsync(int pageNumber = 1, int pageSize = 50);
}


