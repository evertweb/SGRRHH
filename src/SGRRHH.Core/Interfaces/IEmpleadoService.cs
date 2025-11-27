using SGRRHH.Core.Common;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Models;

namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Interfaz para el servicio de empleados
/// </summary>
public interface IEmpleadoService
{
    /// <summary>
    /// Obtiene todos los empleados activos con sus relaciones
    /// </summary>
    Task<IEnumerable<Empleado>> GetAllAsync();
    
    /// <summary>
    /// Obtiene un empleado por ID
    /// </summary>
    Task<Empleado?> GetByIdAsync(int id);
    
    /// <summary>
    /// Busca empleados por término
    /// </summary>
    Task<IEnumerable<Empleado>> SearchAsync(string searchTerm);
    
    /// <summary>
    /// Filtra empleados por departamento
    /// </summary>
    Task<IEnumerable<Empleado>> GetByDepartamentoAsync(int departamentoId);
    
    /// <summary>
    /// Filtra empleados por estado
    /// </summary>
    Task<IEnumerable<Empleado>> GetByEstadoAsync(EstadoEmpleado estado);
    
    /// <summary>
    /// Crea un nuevo empleado
    /// </summary>
    Task<ServiceResult<Empleado>> CreateAsync(Empleado empleado);
    
    /// <summary>
    /// Actualiza un empleado existente
    /// </summary>
    Task<ServiceResult> UpdateAsync(Empleado empleado);
    
    /// <summary>
    /// Desactiva un empleado (eliminación lógica)
    /// </summary>
    Task<ServiceResult> DeactivateAsync(int id);
    
    /// <summary>
    /// Elimina un empleado permanentemente de la base de datos
    /// </summary>
    Task<ServiceResult> DeletePermanentlyAsync(int id);
    
    /// <summary>
    /// Reactiva un empleado
    /// </summary>
    Task<ServiceResult> ReactivateAsync(int id);
    
    /// <summary>
    /// Obtiene el siguiente código disponible
    /// </summary>
    Task<string> GetNextCodigoAsync();
    
    /// <summary>
    /// Guarda la foto del empleado
    /// </summary>
    Task<ServiceResult<string>> SaveFotoAsync(int empleadoId, byte[] fotoData, string extension);
    
    /// <summary>
    /// Elimina la foto del empleado
    /// </summary>
    Task<ServiceResult> DeleteFotoAsync(int empleadoId);
    
    /// <summary>
    /// Cuenta empleados activos
    /// </summary>
    Task<int> CountActiveAsync();
    
    /// <summary>
    /// Obtiene empleados con cumpleaños próximos
    /// </summary>
    /// <param name="diasAnticipacion">Días de anticipación para buscar (default: 7)</param>
    Task<IEnumerable<Empleado>> GetCumpleaniosProximosAsync(int diasAnticipacion = 7);
    
    /// <summary>
    /// Obtiene empleados con aniversarios laborales próximos
    /// </summary>
    /// <param name="diasAnticipacion">Días de anticipación para buscar (default: 7)</param>
    Task<IEnumerable<Empleado>> GetAniversariosProximosAsync(int diasAnticipacion = 7);
    
    /// <summary>
    /// Obtiene estadísticas de empleados por departamento para gráficos
    /// </summary>
    Task<IEnumerable<EstadisticaItemDTO>> GetEmpleadosPorDepartamentoAsync();
    
    /// <summary>
    /// Obtiene empleados pendientes de aprobación
    /// </summary>
    Task<IEnumerable<Empleado>> GetPendientesAprobacionAsync();
    
    /// <summary>
    /// Aprueba un empleado (solo Aprobador/Admin)
    /// </summary>
    Task<ServiceResult> AprobarAsync(int id, int aprobadorId);
    
    /// <summary>
    /// Rechaza un empleado (solo Aprobador/Admin)
    /// </summary>
    Task<ServiceResult> RechazarAsync(int id, int aprobadorId, string motivo);
    
    /// <summary>
    /// Cuenta empleados pendientes de aprobación
    /// </summary>
    Task<int> CountPendientesAsync();
}

/// <summary>
/// Interfaz para el servicio de departamentos
/// </summary>
public interface IDepartamentoService
{
    Task<IEnumerable<Departamento>> GetAllAsync();
    Task<Departamento?> GetByIdAsync(int id);
    Task<ServiceResult<Departamento>> CreateAsync(Departamento departamento);
    Task<ServiceResult> UpdateAsync(Departamento departamento);
    Task<ServiceResult> DeleteAsync(int id);
    Task<string> GetNextCodigoAsync();
    Task<int> CountActiveAsync();
}

/// <summary>
/// Interfaz para el servicio de cargos
/// </summary>
public interface ICargoService
{
    Task<IEnumerable<Cargo>> GetAllAsync();
    Task<IEnumerable<Cargo>> GetByDepartamentoAsync(int departamentoId);
    Task<Cargo?> GetByIdAsync(int id);
    Task<ServiceResult<Cargo>> CreateAsync(Cargo cargo);
    Task<ServiceResult> UpdateAsync(Cargo cargo);
    Task<ServiceResult> DeleteAsync(int id);
    Task<string> GetNextCodigoAsync();
    Task<int> CountActiveAsync();
}
