using SGRRHH.Local.Domain.Entities;

namespace SGRRHH.Local.Shared.Interfaces;

public interface IRepository<T> where T : EntidadBase
{
    Task<T?> GetByIdAsync(int id);
    
    Task<IEnumerable<T>> GetAllAsync();
    
    Task<IEnumerable<T>> GetAllActiveAsync();
    
    Task<T> AddAsync(T entity);
    
    Task UpdateAsync(T entity);
    
    Task DeleteAsync(int id);
    
    Task<int> SaveChangesAsync();
}


