using SGRRHH.Core.Common;
using SGRRHH.Core.Entities;

namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Interfaz para el servicio de proyectos
/// </summary>
public interface IProyectoService
{
    /// <summary>
    /// Obtiene todos los proyectos activos
    /// </summary>
    Task<IEnumerable<Proyecto>> GetAllAsync();
    
    /// <summary>
    /// Obtiene un proyecto por ID
    /// </summary>
    Task<Proyecto?> GetByIdAsync(int id);
    
    /// <summary>
    /// Busca proyectos por término
    /// </summary>
    Task<IEnumerable<Proyecto>> SearchAsync(string searchTerm);
    
    /// <summary>
    /// Obtiene proyectos por estado
    /// </summary>
    Task<IEnumerable<Proyecto>> GetByEstadoAsync(EstadoProyecto estado);
    
    /// <summary>
    /// Crea un nuevo proyecto
    /// </summary>
    Task<ServiceResult<Proyecto>> CreateAsync(Proyecto proyecto);
    
    /// <summary>
    /// Actualiza un proyecto existente
    /// </summary>
    Task<ServiceResult> UpdateAsync(Proyecto proyecto);
    
    /// <summary>
    /// Elimina (desactiva) un proyecto
    /// </summary>
    Task<ServiceResult> DeleteAsync(int id);
    
    /// <summary>
    /// Obtiene el siguiente código disponible
    /// </summary>
    Task<string> GetNextCodigoAsync();
    
    /// <summary>
    /// Cuenta proyectos activos
    /// </summary>
    Task<int> CountActiveAsync();
}
