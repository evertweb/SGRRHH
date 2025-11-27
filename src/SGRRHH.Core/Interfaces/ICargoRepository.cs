using SGRRHH.Core.Entities;

namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Interfaz específica para el repositorio de cargos
/// </summary>
public interface ICargoRepository : IRepository<Cargo>
{
    /// <summary>
    /// Obtiene un cargo por su código
    /// </summary>
    Task<Cargo?> GetByCodigoAsync(string codigo);
    
    /// <summary>
    /// Obtiene un cargo con su departamento
    /// </summary>
    Task<Cargo?> GetByIdWithDepartamentoAsync(int id);
    
    /// <summary>
    /// Obtiene un cargo con todos sus empleados
    /// </summary>
    Task<Cargo?> GetByIdWithEmpleadosAsync(int id);
    
    /// <summary>
    /// Obtiene todos los cargos con su departamento
    /// </summary>
    Task<IEnumerable<Cargo>> GetAllWithDepartamentoAsync();
    
    /// <summary>
    /// Obtiene cargos activos con su departamento
    /// </summary>
    Task<IEnumerable<Cargo>> GetAllActiveWithDepartamentoAsync();
    
    /// <summary>
    /// Obtiene cargos por departamento
    /// </summary>
    Task<IEnumerable<Cargo>> GetByDepartamentoAsync(int departamentoId);
    
    /// <summary>
    /// Verifica si existe un cargo con el código dado
    /// </summary>
    Task<bool> ExistsCodigoAsync(string codigo, int? excludeId = null);
    
    /// <summary>
    /// Obtiene el siguiente código de cargo disponible
    /// </summary>
    Task<string> GetNextCodigoAsync();
    
    /// <summary>
    /// Verifica si el cargo tiene empleados asignados
    /// </summary>
    Task<bool> HasEmpleadosAsync(int id);
    
    /// <summary>
    /// Cuenta el total de cargos activos
    /// </summary>
    Task<int> CountActiveAsync();
}
