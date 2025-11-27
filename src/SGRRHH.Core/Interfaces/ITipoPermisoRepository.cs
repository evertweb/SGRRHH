using SGRRHH.Core.Entities;

namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Repositorio para la gestión de tipos de permiso
/// </summary>
public interface ITipoPermisoRepository : IRepository<TipoPermiso>
{
    /// <summary>
    /// Obtiene todos los tipos de permiso activos
    /// </summary>
    Task<IEnumerable<TipoPermiso>> GetActivosAsync();
    
    /// <summary>
    /// Verifica si existe un tipo de permiso con el nombre especificado
    /// </summary>
    Task<bool> ExisteNombreAsync(string nombre, int? excludeId = null);
}
