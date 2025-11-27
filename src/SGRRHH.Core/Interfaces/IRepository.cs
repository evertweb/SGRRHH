using SGRRHH.Core.Entities;

namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Interfaz genérica para repositorios
/// </summary>
/// <typeparam name="T">Tipo de entidad</typeparam>
public interface IRepository<T> where T : EntidadBase
{
    /// <summary>
    /// Obtiene una entidad por su ID
    /// </summary>
    Task<T?> GetByIdAsync(int id);
    
    /// <summary>
    /// Obtiene todas las entidades
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync();
    
    /// <summary>
    /// Obtiene todas las entidades activas
    /// </summary>
    Task<IEnumerable<T>> GetAllActiveAsync();
    
    /// <summary>
    /// Agrega una nueva entidad
    /// </summary>
    Task<T> AddAsync(T entity);
    
    /// <summary>
    /// Actualiza una entidad existente
    /// </summary>
    Task UpdateAsync(T entity);
    
    /// <summary>
    /// Elimina (desactiva) una entidad
    /// </summary>
    Task DeleteAsync(int id);
    
    /// <summary>
    /// Guarda los cambios en la base de datos
    /// </summary>
    Task<int> SaveChangesAsync();
}
