using SGRRHH.Core.Entities;

namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Interfaz para el repositorio de proyectos
/// </summary>
public interface IProyectoRepository : IRepository<Proyecto>
{
    /// <summary>
    /// Obtiene todos los proyectos activos
    /// </summary>
    new Task<IEnumerable<Proyecto>> GetAllActiveAsync();
    
    /// <summary>
    /// Obtiene proyectos por estado
    /// </summary>
    Task<IEnumerable<Proyecto>> GetByEstadoAsync(EstadoProyecto estado);
    
    /// <summary>
    /// Busca proyectos por término
    /// </summary>
    Task<IEnumerable<Proyecto>> SearchAsync(string searchTerm);
    
    /// <summary>
    /// Verifica si existe un proyecto con el código especificado
    /// </summary>
    Task<bool> ExistsCodigoAsync(string codigo, int? excludeId = null);
    
    /// <summary>
    /// Obtiene el siguiente código disponible
    /// </summary>
    Task<string> GetNextCodigoAsync();
}
