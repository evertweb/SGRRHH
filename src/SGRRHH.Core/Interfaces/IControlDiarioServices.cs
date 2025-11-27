using SGRRHH.Core.Common;
using SGRRHH.Core.Entities;

namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Interfaz para el servicio de proyectos
/// </summary>
public interface IProyectoService
{
    /// <summary>
    /// Obtiene todos los proyectos activos
    /// </summary>
    Task<IEnumerable<Proyecto>> GetAllAsync();
    
    /// <summary>
    /// Obtiene un proyecto por ID
    /// </summary>
    Task<Proyecto?> GetByIdAsync(int id);
    
    /// <summary>
    /// Busca proyectos por término
    /// </summary>
    Task<IEnumerable<Proyecto>> SearchAsync(string searchTerm);
    
    /// <summary>
    /// Obtiene proyectos por estado
    /// </summary>
    Task<IEnumerable<Proyecto>> GetByEstadoAsync(EstadoProyecto estado);
    
    /// <summary>
    /// Crea un nuevo proyecto
    /// </summary>
    Task<ServiceResult<Proyecto>> CreateAsync(Proyecto proyecto);
    
    /// <summary>
    /// Actualiza un proyecto existente
    /// </summary>
    Task<ServiceResult> UpdateAsync(Proyecto proyecto);
    
    /// <summary>
    /// Elimina (desactiva) un proyecto
    /// </summary>
    Task<ServiceResult> DeleteAsync(int id);
    
    /// <summary>
    /// Obtiene el siguiente código disponible
    /// </summary>
    Task<string> GetNextCodigoAsync();
    
    /// <summary>
    /// Cuenta proyectos activos
    /// </summary>
    Task<int> CountActiveAsync();
}

/// <summary>
/// Interfaz para el servicio de actividades
/// </summary>
public interface IActividadService
{
    /// <summary>
    /// Obtiene todas las actividades activas
    /// </summary>
    Task<IEnumerable<Actividad>> GetAllAsync();
    
    /// <summary>
    /// Obtiene una actividad por ID
    /// </summary>
    Task<Actividad?> GetByIdAsync(int id);
    
    /// <summary>
    /// Obtiene actividades por categoría
    /// </summary>
    Task<IEnumerable<Actividad>> GetByCategoriaAsync(string categoria);
    
    /// <summary>
    /// Obtiene las categorías disponibles
    /// </summary>
    Task<IEnumerable<string>> GetCategoriasAsync();
    
    /// <summary>
    /// Busca actividades por término
    /// </summary>
    Task<IEnumerable<Actividad>> SearchAsync(string searchTerm);
    
    /// <summary>
    /// Crea una nueva actividad
    /// </summary>
    Task<ServiceResult<Actividad>> CreateAsync(Actividad actividad);
    
    /// <summary>
    /// Actualiza una actividad existente
    /// </summary>
    Task<ServiceResult> UpdateAsync(Actividad actividad);
    
    /// <summary>
    /// Elimina (desactiva) una actividad
    /// </summary>
    Task<ServiceResult> DeleteAsync(int id);
    
    /// <summary>
    /// Obtiene el siguiente código disponible
    /// </summary>
    Task<string> GetNextCodigoAsync();
    
    /// <summary>
    /// Cuenta actividades activas
    /// </summary>
    Task<int> CountActiveAsync();
}

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
}
