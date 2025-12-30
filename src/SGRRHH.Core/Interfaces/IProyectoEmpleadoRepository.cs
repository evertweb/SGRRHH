using SGRRHH.Core.Entities;

namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Interfaz para el repositorio de asignaciones proyecto-empleado
/// </summary>
public interface IProyectoEmpleadoRepository : IRepository<ProyectoEmpleado>
{
    /// <summary>
    /// Obtiene todas las asignaciones de un proyecto
    /// </summary>
    Task<IEnumerable<ProyectoEmpleado>> GetByProyectoAsync(int proyectoId);

    /// <summary>
    /// Obtiene todas las asignaciones activas de un proyecto
    /// </summary>
    Task<IEnumerable<ProyectoEmpleado>> GetActiveByProyectoAsync(int proyectoId);

    /// <summary>
    /// Obtiene todos los proyectos de un empleado
    /// </summary>
    Task<IEnumerable<ProyectoEmpleado>> GetByEmpleadoAsync(int empleadoId);

    /// <summary>
    /// Obtiene todos los proyectos activos de un empleado
    /// </summary>
    Task<IEnumerable<ProyectoEmpleado>> GetActiveByEmpleadoAsync(int empleadoId);

    /// <summary>
    /// Verifica si un empleado está asignado a un proyecto
    /// </summary>
    Task<bool> ExistsAsignacionAsync(int proyectoId, int empleadoId);

    /// <summary>
    /// Obtiene la asignación de un empleado a un proyecto
    /// </summary>
    Task<ProyectoEmpleado?> GetAsignacionAsync(int proyectoId, int empleadoId);

    /// <summary>
    /// Desasigna un empleado de un proyecto (soft delete)
    /// </summary>
    Task DesasignarAsync(int proyectoId, int empleadoId);

    /// <summary>
    /// Obtiene la cantidad de empleados asignados a un proyecto
    /// </summary>
    Task<int> GetCountByProyectoAsync(int proyectoId);
}
