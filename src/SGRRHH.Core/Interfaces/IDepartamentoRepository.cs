using SGRRHH.Core.Entities;

namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Interfaz específica para el repositorio de departamentos
/// </summary>
public interface IDepartamentoRepository : IRepository<Departamento>
{
    /// <summary>
    /// Obtiene un departamento por su código
    /// </summary>
    Task<Departamento?> GetByCodigoAsync(string codigo);
    
    /// <summary>
    /// Obtiene un departamento con todos sus empleados
    /// </summary>
    Task<Departamento?> GetByIdWithEmpleadosAsync(int id);
    
    /// <summary>
    /// Obtiene un departamento con todos sus cargos
    /// </summary>
    Task<Departamento?> GetByIdWithCargosAsync(int id);
    
    /// <summary>
    /// Obtiene todos los departamentos con cantidad de empleados
    /// </summary>
    Task<IEnumerable<Departamento>> GetAllWithEmpleadosCountAsync();
    
    /// <summary>
    /// Verifica si existe un departamento con el código dado
    /// </summary>
    Task<bool> ExistsCodigoAsync(string codigo, int? excludeId = null);

    /// <summary>
    /// Verifica si existe un departamento con el nombre dado
    /// </summary>
    Task<bool> ExistsByNameAsync(string nombre, int? excludeId = null);
    
    /// <summary>
    /// Obtiene el siguiente código de departamento disponible
    /// </summary>
    Task<string> GetNextCodigoAsync();
    
    /// <summary>
    /// Verifica si el departamento tiene empleados asignados
    /// </summary>
    Task<bool> HasEmpleadosAsync(int id);
    
    /// <summary>
    /// Cuenta el total de departamentos activos
    /// </summary>
    Task<int> CountActiveAsync();

    /// <summary>
    /// Invalida el cache de departamentos después de cambios
    /// </summary>
    void InvalidateCache();

    /// <summary>
    /// Obtiene departamentos activos con paginación
    /// </summary>
    Task<(IEnumerable<Departamento> Items, int TotalCount)> GetAllActivePagedAsync(int pageNumber = 1, int pageSize = 50);
}
