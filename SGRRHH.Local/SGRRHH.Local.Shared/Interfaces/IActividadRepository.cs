using SGRRHH.Local.Domain.Entities;

namespace SGRRHH.Local.Shared.Interfaces;

public interface IActividadRepository : IRepository<Actividad>
{
    new Task<IEnumerable<Actividad>> GetAllActiveAsync();
    
    /// <summary>
    /// Obtiene actividades por categoría de texto (legacy)
    /// </summary>
    Task<IEnumerable<Actividad>> GetByCategoriaAsync(string categoria);
    
    /// <summary>
    /// Obtiene actividades por ID de categoría
    /// </summary>
    Task<IEnumerable<Actividad>> GetByCategoriaIdAsync(int categoriaId);
    
    /// <summary>
    /// Obtiene las categorías de texto únicas (legacy)
    /// </summary>
    Task<IEnumerable<string>> GetCategoriasAsync();
    
    Task<IEnumerable<Actividad>> SearchAsync(string searchTerm);
    
    Task<bool> ExistsCodigoAsync(string codigo, int? excludeId = null);
    
    Task<string> GetNextCodigoAsync();
    
    /// <summary>
    /// Obtiene todas las actividades con su categoría incluida
    /// </summary>
    Task<IEnumerable<Actividad>> GetAllWithCategoriaAsync();
    
    /// <summary>
    /// Obtiene todas las actividades activas con su categoría incluida
    /// </summary>
    Task<IEnumerable<Actividad>> GetAllActiveWithCategoriaAsync();
    
    /// <summary>
    /// Obtiene solo las actividades destacadas
    /// </summary>
    Task<IEnumerable<Actividad>> GetDestacadasAsync();
}


