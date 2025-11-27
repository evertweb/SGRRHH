using SGRRHH.Core.Entities;

namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Interfaz para el repositorio de configuraciones del sistema
/// </summary>
public interface IConfiguracionRepository : IRepository<ConfiguracionSistema>
{
    /// <summary>
    /// Obtiene una configuración por su clave
    /// </summary>
    Task<ConfiguracionSistema?> GetByClaveAsync(string clave);
    
    /// <summary>
    /// Obtiene todas las configuraciones de una categoría
    /// </summary>
    Task<List<ConfiguracionSistema>> GetByCategoriaAsync(string categoria);
    
    /// <summary>
    /// Verifica si existe una configuración con la clave especificada
    /// </summary>
    Task<bool> ExistsClaveAsync(string clave);
}
