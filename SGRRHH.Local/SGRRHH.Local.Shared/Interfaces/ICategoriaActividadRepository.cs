using SGRRHH.Local.Domain.Entities;

namespace SGRRHH.Local.Shared.Interfaces;

/// <summary>
/// Repositorio para gestionar las categorías de actividades silviculturales
/// </summary>
public interface ICategoriaActividadRepository : IRepository<CategoriaActividad>
{
    /// <summary>
    /// Obtiene todas las categorías activas
    /// </summary>
    new Task<IEnumerable<CategoriaActividad>> GetAllActiveAsync();
    
    /// <summary>
    /// Obtiene una categoría por su código
    /// </summary>
    Task<CategoriaActividad?> GetByCodigoAsync(string codigo);
    
    /// <summary>
    /// Verifica si existe una categoría con el código especificado
    /// </summary>
    Task<bool> ExistsCodigoAsync(string codigo, int? excludeId = null);
    
    /// <summary>
    /// Obtiene las categorías ordenadas por el campo Orden
    /// </summary>
    Task<IEnumerable<CategoriaActividad>> GetAllOrderedAsync();
}
