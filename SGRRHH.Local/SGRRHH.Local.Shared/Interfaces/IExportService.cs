using SGRRHH.Local.Shared;

namespace SGRRHH.Local.Shared.Interfaces;

public interface IExportService
{
    Task<Result<byte[]>> ExportToExcelAsync<T>(
        IEnumerable<T> data, 
        string sheetName, 
        Dictionary<string, Func<T, object>>? columns = null);
    
    Task<Result<byte[]>> ExportEmpleadosToExcelAsync(ExportEmpleadosOptions? options = null);
    Task<Result<byte[]>> ExportPermisosToExcelAsync(DateTime fechaInicio, DateTime fechaFin);
    Task<Result<byte[]>> ExportVacacionesToExcelAsync(int año);
}

public class ExportEmpleadosOptions
{
    public int? DepartamentoId { get; set; }
    public int? CargoId { get; set; }
    public bool SoloActivos { get; set; } = true;
    public DateTime? FechaIngresoDesde { get; set; }
    public DateTime? FechaIngresoHasta { get; set; }
}
