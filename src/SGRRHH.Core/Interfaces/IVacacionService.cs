using SGRRHH.Core.Common;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Models;

namespace SGRRHH.Core.Interfaces;

public interface IVacacionService
{
    Task<ServiceResult<IEnumerable<Vacacion>>> GetByEmpleadoIdAsync(int empleadoId);
    Task<ServiceResult<Vacacion>> GetByIdAsync(int id);
    Task<ServiceResult<Vacacion>> CreateAsync(Vacacion vacacion);
    Task<ServiceResult<Vacacion>> UpdateAsync(Vacacion vacacion);
    Task<ServiceResult<bool>> DeleteAsync(int id);
    
    /// <summary>
    /// Calcula días de vacaciones disponibles para un empleado.
    /// En Colombia: 15 días hábiles por año completo trabajado.
    /// </summary>
    Task<ServiceResult<int>> CalcularDiasDisponiblesAsync(int empleadoId, int periodo);
    
    /// <summary>
    /// Obtiene el total de días tomados por un empleado en un periodo
    /// </summary>
    Task<ServiceResult<int>> GetDiasTomadosEnPeriodoAsync(int empleadoId, int periodo);
    
    /// <summary>
    /// Obtiene el resumen de vacaciones de un empleado
    /// </summary>
    Task<ServiceResult<ResumenVacaciones>> GetResumenVacacionesAsync(int empleadoId);
    
    /// <summary>
    /// Marca vacaciones como disfrutadas (cuando pasa la fecha)
    /// </summary>
    Task<ServiceResult<bool>> MarcarComoDisfrutadaAsync(int id);
    
    /// <summary>
    /// Cancela una vacación programada
    /// </summary>
    Task<ServiceResult<bool>> CancelarVacacionAsync(int id, string motivo);
    
    /// <summary>
    /// Obtiene vacaciones programadas (futuras)
    /// </summary>
    Task<ServiceResult<IEnumerable<Vacacion>>> GetVacacionesProgramadasAsync(int? empleadoId = null);
}
