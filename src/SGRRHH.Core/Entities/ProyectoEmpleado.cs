namespace SGRRHH.Core.Entities;

/// <summary>
/// Entidad de relación muchos a muchos entre Proyecto y Empleado.
/// Representa la asignación de un empleado a un proyecto.
/// </summary>
public class ProyectoEmpleado : EntidadBase
{
    /// <summary>
    /// ID del proyecto
    /// </summary>
    public int ProyectoId { get; set; }

    /// <summary>
    /// Proyecto asociado
    /// </summary>
    public Proyecto? Proyecto { get; set; }

    /// <summary>
    /// ID del empleado asignado
    /// </summary>
    public int EmpleadoId { get; set; }

    /// <summary>
    /// Empleado asignado
    /// </summary>
    public Empleado? Empleado { get; set; }

    /// <summary>
    /// Fecha en que el empleado fue asignado al proyecto
    /// </summary>
    public DateTime FechaAsignacion { get; set; } = DateTime.Now;

    /// <summary>
    /// Fecha en que el empleado dejó el proyecto (si aplica)
    /// </summary>
    public DateTime? FechaDesasignacion { get; set; }

    /// <summary>
    /// Rol del empleado en el proyecto (ej: "Trabajador", "Supervisor", "Apoyo")
    /// </summary>
    public string? Rol { get; set; }

    /// <summary>
    /// Observaciones sobre la asignación
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// Indica si el empleado está actualmente asignado (no ha sido desasignado)
    /// </summary>
    public bool EstaActivo => !FechaDesasignacion.HasValue;
}
