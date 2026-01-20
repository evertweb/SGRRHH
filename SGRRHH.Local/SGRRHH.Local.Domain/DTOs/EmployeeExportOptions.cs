using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Domain.DTOs;

public class ExportEmployeesOptions
{
    public int? DepartmentId { get; set; }
    public int? PositionId { get; set; }
    public EstadoEmpleado? Status { get; set; }
    public bool OnlyActive { get; set; } = true;
    public DateTime? FechaIngresoDesde { get; set; }
    public DateTime? FechaIngresoHasta { get; set; }
}
