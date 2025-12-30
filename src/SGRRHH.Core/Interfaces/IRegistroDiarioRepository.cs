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
    /// Obtiene registros de todos los empleados en un rango de fechas
    /// </summary>
    Task<IEnumerable<RegistroDiario>> GetByRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin);
    
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
    
    /// <summary>
    /// Agrega un detalle de actividad a un registro diario
    /// </summary>
    Task<DetalleActividad> AddDetalleAsync(int registroId, DetalleActividad detalle);
    
    /// <summary>
    /// Actualiza un detalle de actividad (requiere registroId)
    /// </summary>
    Task UpdateDetalleAsync(int registroId, DetalleActividad detalle);
    
    /// <summary>
    /// Actualiza un detalle de actividad (usa RegistroDiarioId del detalle)
    /// </summary>
    Task UpdateDetalleAsync(DetalleActividad detalle);
    
    /// <summary>
    /// Elimina un detalle de actividad
    /// </summary>
    Task DeleteDetalleAsync(int registroId, int detalleId);
    
    /// <summary>
    /// Obtiene un detalle de actividad con su registro padre (por registroId y detalleId)
    /// </summary>
    Task<DetalleActividad?> GetDetalleByIdAsync(int registroId, int detalleId);
    
    /// <summary>
    /// Obtiene un detalle de actividad solo por su ID (busca en todos los registros)
    /// </summary>
    Task<DetalleActividad?> GetDetalleByIdAsync(int detalleId);

    /// <summary>
    /// Obtiene las actividades (detalles) asociadas a un proyecto
    /// </summary>
    Task<IEnumerable<DetalleActividad>> GetDetallesByProyectoAsync(int proyectoId);

    /// <summary>
    /// Obtiene las actividades de un proyecto en un rango de fechas
    /// </summary>
    Task<IEnumerable<DetalleActividad>> GetDetallesByProyectoRangoFechasAsync(int proyectoId, DateTime fechaInicio, DateTime fechaFin);

    /// <summary>
    /// Calcula el total de horas trabajadas en un proyecto
    /// </summary>
    Task<decimal> GetTotalHorasByProyectoAsync(int proyectoId);

    /// <summary>
    /// Obtiene un resumen de horas por empleado en un proyecto
    /// </summary>
    Task<IEnumerable<ProyectoHorasEmpleado>> GetHorasPorEmpleadoProyectoAsync(int proyectoId);
}

/// <summary>
/// Resumen de horas trabajadas por empleado en un proyecto
/// </summary>
public class ProyectoHorasEmpleado
{
    public int EmpleadoId { get; set; }
    public string EmpleadoNombre { get; set; } = string.Empty;
    public decimal TotalHoras { get; set; }
    public int CantidadActividades { get; set; }
    public DateTime? UltimaActividad { get; set; }
}
