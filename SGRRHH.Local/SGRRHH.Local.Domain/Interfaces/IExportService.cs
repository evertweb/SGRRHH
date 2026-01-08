using SGRRHH.Local.Domain.DTOs;
using SGRRHH.Local.Shared;

namespace SGRRHH.Local.Domain.Interfaces;

public interface IExportService
{
    /// <summary>
    /// Exporta una colección genérica a Excel
    /// </summary>
    Task<Result<byte[]>> ExportToExcelAsync<T>(
        IEnumerable<T> data, 
        string sheetName, 
        Dictionary<string, Func<T, object>>? columns = null);
    
    /// <summary>
    /// Exporta empleados a Excel con filtros opcionales
    /// </summary>
    Task<Result<byte[]>> ExportEmpleadosToExcelAsync(ExportEmpleadosOptions? options = null);
    
    /// <summary>
    /// Exporta permisos de un período a Excel
    /// </summary>
    Task<Result<byte[]>> ExportPermisosToExcelAsync(DateTime fechaInicio, DateTime fechaFin);
    
    /// <summary>
    /// Exporta vacaciones de un año a Excel
    /// </summary>
    Task<Result<byte[]>> ExportVacacionesToExcelAsync(int año);
}
