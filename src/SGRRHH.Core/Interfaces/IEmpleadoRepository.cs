using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;

namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Interfaz específica para el repositorio de empleados
/// </summary>
public interface IEmpleadoRepository : IRepository<Empleado>
{
    /// <summary>
    /// Obtiene un empleado por su código
    /// </summary>
    Task<Empleado?> GetByCodigoAsync(string codigo);
    
    /// <summary>
    /// Obtiene un empleado por su cédula
    /// </summary>
    Task<Empleado?> GetByCedulaAsync(string cedula);
    
    /// <summary>
    /// Obtiene un empleado con todas sus relaciones cargadas
    /// </summary>
    Task<Empleado?> GetByIdWithRelationsAsync(int id);
    
    /// <summary>
    /// Obtiene todos los empleados con sus relaciones
    /// </summary>
    Task<IEnumerable<Empleado>> GetAllWithRelationsAsync();
    
    /// <summary>
    /// Obtiene empleados activos con sus relaciones
    /// </summary>
    Task<IEnumerable<Empleado>> GetAllActiveWithRelationsAsync();
    
    /// <summary>
    /// Obtiene empleados por departamento
    /// </summary>
    Task<IEnumerable<Empleado>> GetByDepartamentoAsync(int departamentoId);
    
    /// <summary>
    /// Obtiene empleados por cargo
    /// </summary>
    Task<IEnumerable<Empleado>> GetByCargoAsync(int cargoId);
    
    /// <summary>
    /// Obtiene empleados por estado
    /// </summary>
    Task<IEnumerable<Empleado>> GetByEstadoAsync(EstadoEmpleado estado);
    
    /// <summary>
    /// Busca empleados por nombre, apellido o cédula
    /// </summary>
    Task<IEnumerable<Empleado>> SearchAsync(string searchTerm);
    
    /// <summary>
    /// Verifica si existe un empleado con el código dado
    /// </summary>
    Task<bool> ExistsCodigoAsync(string codigo, int? excludeId = null);
    
    /// <summary>
    /// Verifica si existe un empleado con la cédula dada
    /// </summary>
    Task<bool> ExistsCedulaAsync(string cedula, int? excludeId = null);
    
    /// <summary>
    /// Obtiene el siguiente código de empleado disponible
    /// </summary>
    Task<string> GetNextCodigoAsync();
    
    /// <summary>
    /// Cuenta el total de empleados activos
    /// </summary>
    Task<int> CountActiveAsync();
    
    /// <summary>
    /// Verifica si existe un empleado con el email dado
    /// </summary>
    Task<bool> ExistsEmailAsync(string email, int? excludeId = null);
    
    /// <summary>
    /// Invalida el cache de empleados. Llamar después de Add/Update/Delete.
    /// </summary>
    void InvalidateCache();
}
