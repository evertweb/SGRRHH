using SGRRHH.Core.Entities;

namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Interfaz para el repositorio de registros diarios
/// </summary>
public interface IRegistroDiarioRepository : IRepository<RegistroDiario>
{
    /// <summary>
    /// Obtiene un registro por fecha y empleado
    /// </summary>
    Task<RegistroDiario?> GetByFechaEmpleadoAsync(DateTime fecha, int empleadoId);
    
    /// <summary>
    /// Obtiene un registro con sus detalles
    /// </summary>
    Task<RegistroDiario?> GetByIdWithDetallesAsync(int id);
    
    /// <summary>
    /// Obtiene registros de un empleado en un rango de fechas
    /// </summary>
    Task<IEnumerable<RegistroDiario>> GetByEmpleadoRangoFechasAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin);
    
    /// <summary>
    /// Obtiene registros de una fecha específica (todos los empleados)
    /// </summary>
    Task<IEnumerable<RegistroDiario>> GetByFechaAsync(DateTime fecha);
    
    /// <summary>
    /// Obtiene los registros de un empleado con sus detalles
    /// </summary>
    Task<IEnumerable<RegistroDiario>> GetByEmpleadoWithDetallesAsync(int empleadoId, int? cantidad = null);
    
    /// <summary>
    /// Obtiene registros del mes actual de un empleado
    /// </summary>
    Task<IEnumerable<RegistroDiario>> GetByEmpleadoMesActualAsync(int empleadoId);
    
    /// <summary>
    /// Verifica si existe un registro para la fecha y empleado
    /// </summary>
    Task<bool> ExistsByFechaEmpleadoAsync(DateTime fecha, int empleadoId);
    
    /// <summary>
    /// Calcula el total de horas de un empleado en un rango de fechas
    /// </summary>
    Task<decimal> GetTotalHorasByEmpleadoRangoAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin);
}
