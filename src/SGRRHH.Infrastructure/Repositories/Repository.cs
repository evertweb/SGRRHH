using Microsoft.EntityFrameworkCore;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Infrastructure.Repositories;

/// <summary>
/// Implementación genérica del repositorio base
/// </summary>
/// <typeparam name="T">Tipo de entidad</typeparam>
public class Repository<T> : IRepository<T> where T : EntidadBase
{
    protected readonly DbContext _context;
    protected readonly DbSet<T> _dbSet;
    
    public Repository(DbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }
    
    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }
    
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }
    
    public virtual async Task<IEnumerable<T>> GetAllActiveAsync()
    {
        return await _dbSet.Where(e => e.Activo).ToListAsync();
    }
    
    public virtual async Task<T> AddAsync(T entity)
    {
        entity.FechaCreacion = DateTime.Now;
        await _dbSet.AddAsync(entity);
        return entity;
    }
    
    public virtual Task UpdateAsync(T entity)
    {
        entity.FechaModificacion = DateTime.Now;
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }
    
    public virtual async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            entity.Activo = false;
            entity.FechaModificacion = DateTime.Now;
            _dbSet.Update(entity);
        }
    }
    
    public virtual async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
