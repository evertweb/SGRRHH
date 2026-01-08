namespace SGRRHH.Local.Domain.Entities;

public class ProyectoEmpleado : EntidadBase
{
    public int ProyectoId { get; set; }

    public Proyecto? Proyecto { get; set; }

    public int EmpleadoId { get; set; }

    public Empleado? Empleado { get; set; }

    public DateTime FechaAsignacion { get; set; } = DateTime.Now;

    public DateTime? FechaDesasignacion { get; set; }

    public string? Rol { get; set; }

    public string? Observaciones { get; set; }

    public bool EstaActivo => !FechaDesasignacion.HasValue;
}


