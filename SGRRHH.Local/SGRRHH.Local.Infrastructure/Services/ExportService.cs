using ClosedXML.Excel;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Shared;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Services;

public class ExportService : IExportService
{
    private readonly IEmpleadoRepository _empleadoRepository;
    private readonly IPermisoRepository _permisoRepository;
    private readonly IVacacionRepository _vacacionRepository;
    private readonly ILogger<ExportService> _logger;
    
    public ExportService(
        IEmpleadoRepository empleadoRepository,
        IPermisoRepository permisoRepository,
        IVacacionRepository vacacionRepository,
        ILogger<ExportService> logger)
    {
        _empleadoRepository = empleadoRepository;
        _permisoRepository = permisoRepository;
        _vacacionRepository = vacacionRepository;
        _logger = logger;
    }
    
    public async Task<Result<byte[]>> ExportToExcelAsync<T>(
        IEnumerable<T> data, 
        string sheetName,
        Dictionary<string, Func<T, object>>? columns = null)
    {
        try
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(sheetName);
            
            var properties = typeof(T).GetProperties();
            var colIndex = 1;
            
            // Headers
            if (columns != null)
            {
                foreach (var col in columns.Keys)
                {
                    var cell = worksheet.Cell(1, colIndex);
                    cell.Value = col;
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    colIndex++;
                }
            }
            else
            {
                foreach (var prop in properties)
                {
                    var cell = worksheet.Cell(1, colIndex);
                    cell.Value = prop.Name;
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    colIndex++;
                }
            }
            
            // Data
            var rowIndex = 2;
            foreach (var item in data)
            {
                colIndex = 1;
                
                if (columns != null)
                {
                    foreach (var col in columns.Values)
                    {
                        var value = col(item);
                        var cell = worksheet.Cell(rowIndex, colIndex);
                        
                        if (value is DateTime dateValue)
                        {
                            cell.Value = dateValue.ToString("dd/MM/yyyy");
                        }
                        else if (value is decimal || value is double || value is float)
                        {
                            if (double.TryParse(value.ToString(), out var numValue))
                            {
                                cell.Value = numValue;
                                cell.Style.NumberFormat.Format = "#,##0.00";
                            }
                            else
                            {
                                cell.Value = value?.ToString() ?? "";
                            }
                        }
                        else
                        {
                            cell.Value = value?.ToString() ?? "";
                        }
                        
                        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        colIndex++;
                    }
                }
                else
                {
                    foreach (var prop in properties)
                    {
                        var value = prop.GetValue(item);
                        var cell = worksheet.Cell(rowIndex, colIndex);
                        cell.Value = value?.ToString() ?? "";
                        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        colIndex++;
                    }
                }
                
                rowIndex++;
            }
            
            // Autofit columns
            worksheet.Columns().AdjustToContents();
            
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            
            _logger.LogInformation("Excel exportado exitosamente: {SheetName}, {Rows} filas", sheetName, rowIndex - 2);
            return Result<byte[]>.Ok(stream.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exportando a Excel");
            return Result<byte[]>.Fail($"Error exportando: {ex.Message}");
        }
    }
    
    public async Task<Result<byte[]>> ExportEmpleadosToExcelAsync(ExportEmpleadosOptions? options = null)
    {
        try
        {
            options ??= new ExportEmpleadosOptions();
            
            // Obtener empleados
            var empleados = await _empleadoRepository.GetAllAsync();
            
            // Aplicar filtros
            if (options.SoloActivos)
            {
                empleados = empleados.Where(e => e.Estado == Domain.Enums.EstadoEmpleado.Activo);
            }
            
            if (options.DepartamentoId.HasValue)
            {
                empleados = empleados.Where(e => e.DepartamentoId == options.DepartamentoId.Value);
            }
            
            if (options.CargoId.HasValue)
            {
                empleados = empleados.Where(e => e.CargoId == options.CargoId.Value);
            }
            
            if (options.FechaIngresoDesde.HasValue)
            {
                empleados = empleados.Where(e => e.FechaIngreso >= options.FechaIngresoDesde.Value);
            }
            
            if (options.FechaIngresoHasta.HasValue)
            {
                empleados = empleados.Where(e => e.FechaIngreso <= options.FechaIngresoHasta.Value);
            }
            
            // Definir columnas específicas
            var columns = new Dictionary<string, Func<Empleado, object>>
            {
                { "Código", e => e.Codigo },
                { "Cédula", e => e.Cedula },
                { "Nombres", e => e.Nombres },
                { "Apellidos", e => e.Apellidos },
                { "Cargo", e => e.Cargo?.Nombre ?? "" },
                { "Departamento", e => e.Departamento?.Nombre ?? "" },
                { "Fecha Ingreso", e => e.FechaIngreso },
                { "Estado", e => e.Estado.ToString() },
                { "Teléfono", e => e.Telefono ?? "" },
                { "Email", e => e.Email ?? "" },
                { "Salario", e => e.SalarioBase ?? 0m },
                { "Antigüedad (años)", e => e.Antiguedad }
            };
            
            return await ExportToExcelAsync(empleados, "Empleados", columns);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exportando empleados");
            return Result<byte[]>.Fail($"Error: {ex.Message}");
        }
    }
    
    public async Task<Result<byte[]>> ExportPermisosToExcelAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        try
        {
            var permisos = await _permisoRepository.GetAllAsync();
            
            // Filtrar por rango de fechas
            permisos = permisos.Where(p => 
                p.FechaInicio >= fechaInicio && p.FechaInicio <= fechaFin);
            
            var columns = new Dictionary<string, Func<Permiso, object>>
            {
                { "Empleado", p => p.Empleado?.NombreCompleto ?? "" },
                { "Cédula", p => p.Empleado?.Cedula ?? "" },
                { "Tipo", p => p.TipoPermiso?.Nombre ?? "" },
                { "Fecha Inicio", p => p.FechaInicio },
                { "Fecha Fin", p => p.FechaFin },
                { "Días", p => (p.FechaFin - p.FechaInicio).Days + 1 },
                { "Estado", p => p.Estado.ToString() },
                { "Motivo", p => p.Motivo ?? "" },
                { "Fecha Solicitud", p => p.FechaSolicitud },
                { "Aprobado Por", p => p.AprobadoPor?.NombreCompleto ?? "" }
            };
            
            return await ExportToExcelAsync(permisos, "Permisos", columns);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exportando permisos");
            return Result<byte[]>.Fail($"Error: {ex.Message}");
        }
    }
    
    public async Task<Result<byte[]>> ExportVacacionesToExcelAsync(int año)
    {
        try
        {
            var vacaciones = await _vacacionRepository.GetAllAsync();
            
            // Filtrar por año
            vacaciones = vacaciones.Where(v => v.PeriodoCorrespondiente == año);
            
            var columns = new Dictionary<string, Func<Vacacion, object>>
            {
                { "Empleado", v => v.Empleado?.NombreCompleto ?? "" },
                { "Cédula", v => v.Empleado?.Cedula ?? "" },
                { "Período", v => v.PeriodoCorrespondiente },
                { "Días Tomados", v => v.DiasTomados },
                { "Fecha Inicio", v => v.FechaInicio },
                { "Fecha Fin", v => v.FechaFin },
                { "Estado", v => v.Estado.ToString() },
                { "Observaciones", v => v.Observaciones ?? "" }
            };
            
            return await ExportToExcelAsync(vacaciones, $"Vacaciones {año}", columns);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exportando vacaciones");
            return Result<byte[]>.Fail($"Error: {ex.Message}");
        }
    }
}
