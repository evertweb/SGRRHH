using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Shared.Interfaces;

public interface IEmpleadoRepository : IRepository<Empleado>
{
    Task<Empleado?> GetByCodigoAsync(string codigo);
    
    Task<Empleado?> GetByCedulaAsync(string cedula);
    
    Task<Empleado?> GetByIdWithRelationsAsync(int id);
    
    Task<IEnumerable<Empleado>> GetAllWithRelationsAsync();
    
    Task<IEnumerable<Empleado>> GetAllActiveWithRelationsAsync();
    
    Task<IEnumerable<Empleado>> GetByDepartamentoAsync(int departamentoId);
    
    Task<IEnumerable<Empleado>> GetByCargoAsync(int cargoId);
    
    Task<IEnumerable<Empleado>> GetByEstadoAsync(EstadoEmpleado estado);
    
    Task<IEnumerable<Empleado>> SearchAsync(string searchTerm);
    
    Task<bool> ExistsCodigoAsync(string codigo, int? excludeId = null);
    
    Task<bool> ExistsCedulaAsync(string cedula, int? excludeId = null);
    
    Task<string> GetNextCodigoAsync();
    
    Task<int> CountActiveAsync();
    
    Task<bool> ExistsEmailAsync(string email, int? excludeId = null);
    
    void InvalidateCache();
}


