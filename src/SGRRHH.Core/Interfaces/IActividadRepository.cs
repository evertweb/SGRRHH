using SGRRHH.Core.Entities;

namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Interfaz para el repositorio de actividades
/// </summary>
public interface IActividadRepository : IRepository<Actividad>
{
    /// <summary>
    /// Obtiene todas las actividades activas ordenadas
    /// </summary>
    new Task<IEnumerable<Actividad>> GetAllActiveAsync();
    
    /// <summary>
    /// Obtiene actividades por categoría
    /// </summary>
    Task<IEnumerable<Actividad>> GetByCategoriaAsync(string categoria);
    
    /// <summary>
    /// Obtiene las categorías disponibles
    /// </summary>
    Task<IEnumerable<string>> GetCategoriasAsync();
    
    /// <summary>
    /// Busca actividades por término
    /// </summary>
    Task<IEnumerable<Actividad>> SearchAsync(string searchTerm);
    
    /// <summary>
    /// Verifica si existe una actividad con el código especificado
    /// </summary>
    Task<bool> ExistsCodigoAsync(string codigo, int? excludeId = null);
    
    /// <summary>
    /// Obtiene el siguiente código disponible
    /// </summary>
    Task<string> GetNextCodigoAsync();
}
