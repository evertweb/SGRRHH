using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Domain.DTOs;

public class ExportEmpleadosOptions
{
    public int? DepartamentoId { get; set; }
    public int? CargoId { get; set; }
    public EstadoEmpleado? Estado { get; set; }
    public DateTime? FechaIngresoDesde { get; set; }
    public DateTime? FechaIngresoHasta { get; set; }
}
