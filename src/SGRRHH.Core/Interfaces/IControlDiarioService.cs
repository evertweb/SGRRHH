using SGRRHH.Core.Common;
using SGRRHH.Core.Entities;

namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Interfaz para el servicio de control diario
/// </summary>
public interface IControlDiarioService
{
    /// <summary>
    /// Obtiene el registro de un empleado para una fecha
    /// </summary>
    Task<RegistroDiario?> GetRegistroByFechaEmpleadoAsync(DateTime fecha, int empleadoId);
    
    /// <summary>
    /// Obtiene un registro por ID con sus detalles
    /// </summary>
    Task<RegistroDiario?> GetByIdAsync(int id);
    
    /// <summary>
    /// Obtiene los registros de un empleado en un rango de fechas
    /// </summary>
    Task<IEnumerable<RegistroDiario>> GetByEmpleadoRangoAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin);
    
    /// <summary>
    /// Obtiene los registros del mes actual de un empleado
    /// </summary>
    Task<IEnumerable<RegistroDiario>> GetByEmpleadoMesActualAsync(int empleadoId);
    
    /// <summary>
    /// Obtiene registros de una fecha específica (todos los empleados)
    /// </summary>
    Task<IEnumerable<RegistroDiario>> GetByFechaAsync(DateTime fecha);
    
    /// <summary>
    /// Crea o actualiza un registro diario
    /// </summary>
    Task<ServiceResult<RegistroDiario>> SaveRegistroAsync(RegistroDiario registro);
    
    /// <summary>
    /// Agrega una actividad a un registro diario
    /// </summary>
    Task<ServiceResult<DetalleActividad>> AddActividadAsync(int registroId, DetalleActividad detalle);
    
    /// <summary>
    /// Actualiza una actividad de un registro diario
    /// </summary>
    Task<ServiceResult> UpdateActividadAsync(DetalleActividad detalle);
    
    /// <summary>
    /// Elimina una actividad de un registro diario
    /// </summary>
    Task<ServiceResult> DeleteActividadAsync(int detalleId);
    
    /// <summary>
    /// Marca un registro como completado
    /// </summary>
    Task<ServiceResult> CompletarRegistroAsync(int registroId);
    
    /// <summary>
    /// Calcula el total de horas de un empleado en un rango
    /// </summary>
    Task<decimal> GetTotalHorasAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin);
    
    /// <summary>
    /// Obtiene las horas trabajadas en el mes actual
    /// </summary>
    Task<decimal> GetHorasMesActualAsync(int empleadoId);
    
    /// <summary>
    /// Obtiene registros de todos los empleados para un mes/año específico
    /// </summary>
    Task<IEnumerable<RegistroDiario>> GetByMesAnioAsync(int mes, int anio);
    
    /// <summary>
    /// Obtiene registros de un empleado para un mes/año específico
    /// </summary>
    Task<IEnumerable<RegistroDiario>> GetByEmpleadoMesAnioAsync(int empleadoId, int mes, int anio);
    
    /// <summary>
    /// Elimina un registro diario completo
    /// </summary>
    Task<ServiceResult> DeleteRegistroAsync(int registroId);
    
    /// <summary>
    /// FIX #6: Aprueba un registro completado
    /// </summary>
    Task<ServiceResult> AprobarRegistroAsync(int registroId);
    
    /// <summary>
    /// FIX #6: Rechaza un registro (vuelve a Borrador)
    /// </summary>
    Task<ServiceResult> RechazarRegistroAsync(int registroId, string? motivo = null);
}
