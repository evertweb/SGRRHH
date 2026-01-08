using SGRRHH.Local.Domain.Entities;

namespace SGRRHH.Local.Shared.Interfaces;

public interface ICargoRepository : IRepository<Cargo>
{
    Task<Cargo?> GetByCodigoAsync(string codigo);
    
    Task<Cargo?> GetByIdWithDepartamentoAsync(int id);
    
    Task<Cargo?> GetByIdWithEmpleadosAsync(int id);
    
    Task<IEnumerable<Cargo>> GetAllWithDepartamentoAsync();
    
    Task<IEnumerable<Cargo>> GetAllActiveWithDepartamentoAsync();
    
    Task<IEnumerable<Cargo>> GetByDepartamentoAsync(int departamentoId);
    
    Task<bool> ExistsCodigoAsync(string codigo, int? excludeId = null);
    
    Task<string> GetNextCodigoAsync();
    
    Task<bool> HasEmpleadosAsync(int id);
    
    Task<int> CountActiveAsync();
    
    Task<bool> ExistsNombreInDepartamentoAsync(string nombre, int? departamentoId, int? excludeId = null);
}


