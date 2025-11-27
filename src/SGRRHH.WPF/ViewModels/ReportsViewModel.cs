using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;
using System.Collections.ObjectModel;
using System.Data;

namespace SGRRHH.WPF.ViewModels;

public class ReportDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool RequiresDateRange { get; set; }
    public bool RequiresEmpleado { get; set; }
}

public partial class ReportsViewModel : ObservableObject
{
    private readonly IEmpleadoService _empleadoService;
    private readonly IControlDiarioService _controlDiarioService;
    private readonly IProyectoService _proyectoService;
    private readonly IActividadService _actividadService;

    [ObservableProperty]
    private DateTime _fechaInicio = DateTime.Today.AddDays(-30);

    [ObservableProperty]
    private DateTime _fechaFin = DateTime.Today;

    [ObservableProperty]
    private ObservableCollection<Empleado> _empleados = new();

    [ObservableProperty]
    private Empleado? _selectedEmpleado;

    [ObservableProperty]
    private ObservableCollection<ReportDefinition> _reportTypes = new();

    [ObservableProperty]
    private ReportDefinition? _selectedReport;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasResults))]
    private object? _reportResults;

    public bool HasResults => ReportResults != null;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ReportsViewModel(
        IEmpleadoService empleadoService,
        IControlDiarioService controlDiarioService,
        IProyectoService proyectoService,
        IActividadService actividadService)
    {
        _empleadoService = empleadoService;
        _controlDiarioService = controlDiarioService;
        _proyectoService = proyectoService;
        _actividadService = actividadService;

        InitializeReports();
    }

    private void InitializeReports()
    {
        ReportTypes = new ObservableCollection<ReportDefinition>
        {
            new ReportDefinition { Name = "Lista de Empleados", Code = "EMP_LIST", RequiresDateRange = false, RequiresEmpleado = false },
            new ReportDefinition { Name = "Actividades por Empleado", Code = "ACT_EMP", RequiresDateRange = true, RequiresEmpleado = true },
            new ReportDefinition { Name = "Resumen de Horas por Proyecto", Code = "PROJ_HOURS", RequiresDateRange = true, RequiresEmpleado = false },
            // Placeholder reports for future phases
            new ReportDefinition { Name = "Estado de Vacaciones", Code = "VAC_STATUS", RequiresDateRange = false, RequiresEmpleado = false },
            new ReportDefinition { Name = "Contratos por Vencer", Code = "CONT_EXP", RequiresDateRange = true, RequiresEmpleado = false }
        };

        SelectedReport = ReportTypes.First();
    }

    public async Task LoadDataAsync()
    {
        try
        {
            var empleados = await _empleadoService.GetAllAsync();
            Empleados = new ObservableCollection<Empleado>(empleados);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error cargando datos: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task GenerateReport()
    {
        if (SelectedReport == null) return;

        IsLoading = true;
        StatusMessage = "Generando reporte...";
        ReportResults = null;

        try
        {
            switch (SelectedReport.Code)
            {
                case "EMP_LIST":
                    await GenerateEmpleadoListReport();
                    break;
                case "ACT_EMP":
                    await GenerateActividadesEmpleadoReport();
                    break;
                case "PROJ_HOURS":
                    await GenerateProyectoHorasReport();
                    break;
                default:
                    StatusMessage = "Reporte no implementado aún.";
                    break;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error generando reporte: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            if (string.IsNullOrEmpty(StatusMessage) || StatusMessage.StartsWith("Generando"))
            {
                StatusMessage = "Reporte generado exitosamente.";
            }
        }
    }

    private async Task GenerateEmpleadoListReport()
    {
        var empleados = await _empleadoService.GetAllAsync();
        // Proyectar a una forma simple para el grid
        ReportResults = empleados.Select(e => new 
        {
            e.Cedula,
            Nombre = $"{e.Nombres} {e.Apellidos}",
            Departamento = e.Departamento?.Nombre ?? "N/A",
            Cargo = e.Cargo?.Nombre ?? "N/A",
            e.FechaIngreso,
            Estado = e.Estado.ToString()
        }).ToList();
    }

    private async Task GenerateActividadesEmpleadoReport()
    {
        if (SelectedEmpleado == null)
        {
            StatusMessage = "Seleccione un empleado.";
            return;
        }

        var registros = await _controlDiarioService.GetByEmpleadoRangoAsync(SelectedEmpleado.Id, FechaInicio, FechaFin);
        
        // Aplanar los detalles
        var reporte = new List<object>();
        
        foreach (var reg in registros)
        {
            foreach (var det in reg.DetallesActividades ?? Enumerable.Empty<DetalleActividad>())
            {
                reporte.Add(new
                {
                    Fecha = reg.Fecha,
                    Actividad = det.Actividad?.Nombre ?? "N/A",
                    Proyecto = det.Proyecto?.Nombre ?? "N/A",
                    Horas = det.Horas,
                    Descripcion = det.Descripcion
                });
            }
        }

        ReportResults = reporte.OrderBy(r => ((dynamic)r).Fecha).ToList();
    }

    private async Task GenerateProyectoHorasReport()
    {
        // Este reporte requiere lógica de agregación que quizás no esté directa en el servicio.
        // Obtendremos registros por fecha (iterando o si hubiera un método de rango global)
        // Como no hay método de rango global en IControlDiarioService, iteraremos por días (ineficiente pero funcional para MVP)
        // O mejor, añadimos un método al servicio si fuera necesario, pero por ahora usaremos lo que hay.
        
        // Workaround: Get all active projects and calculate hours? No, that's hard.
        // Let's assume we can get all records for the range. 
        // Actually IControlDiarioService doesn't have GetByRangoAsync(start, end). It has GetByEmpleadoRangoAsync.
        // I'll iterate over all employees for now.
        
        var empleados = await _empleadoService.GetAllAsync();
        var reporte = new List<object>();

        foreach (var emp in empleados)
        {
            var registros = await _controlDiarioService.GetByEmpleadoRangoAsync(emp.Id, FechaInicio, FechaFin);
            foreach (var reg in registros)
            {
                foreach (var det in reg.DetallesActividades ?? Enumerable.Empty<DetalleActividad>())
                {
                    if (det.Proyecto != null)
                    {
                        reporte.Add(new
                        {
                            Proyecto = det.Proyecto.Nombre,
                            Empleado = $"{emp.Nombres} {emp.Apellidos}",
                            Fecha = reg.Fecha,
                            Horas = det.Horas
                        });
                    }
                }
            }
        }

        // Group by Project
        var grouped = reporte.GroupBy(r => ((dynamic)r).Proyecto)
            .Select(g => new
            {
                Proyecto = g.Key,
                TotalHoras = g.Sum(x => (decimal)((dynamic)x).Horas),
                Empleados = g.Select(x => (string)((dynamic)x).Empleado).Distinct().Count()
            })
            .ToList();

        ReportResults = grouped;
    }
}
