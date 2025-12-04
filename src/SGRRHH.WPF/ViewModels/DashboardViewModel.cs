using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SGRRHH.Core.Interfaces;
using SGRRHH.Core.Models;
using SGRRHH.WPF.Messages;
using System.Collections.ObjectModel;

namespace SGRRHH.WPF.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly IEmpleadoService _empleadoService;
    private readonly IProyectoService _proyectoService;
    private readonly IControlDiarioService _controlDiarioService;
    private readonly IPermisoService _permisoService;
    private readonly IContratoService _contratoService;

    [ObservableProperty]
    private int _totalEmpleados;

    [ObservableProperty]
    private int _proyectosActivos;

    [ObservableProperty]
    private int _empleadosPresentesHoy;

    [ObservableProperty]
    private int _permisosPendientes;

    [ObservableProperty]
    private int _contratosPorVencer;
    
    [ObservableProperty]
    private string _welcomeMessage = string.Empty;

    [ObservableProperty]
    private string _currentDate = DateTime.Now.ToString("dddd, dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("es-CO"));

    [ObservableProperty]
    private ObservableCollection<CumpleaniosDTO> _cumpleaniosProximos = new();
    
    [ObservableProperty]
    private ObservableCollection<AniversarioDTO> _aniversariosProximos = new();
    
    [ObservableProperty]
    private bool _hayCumpleanios;
    
    [ObservableProperty]
    private bool _hayAniversarios;
    
    [ObservableProperty]
    private ObservableCollection<EstadisticaItemDTO> _empleadosPorDepartamento = new();
    
    [ObservableProperty]
    private bool _hayEstadisticasDepartamento;

    public DashboardViewModel(
        IEmpleadoService empleadoService,
        IProyectoService proyectoService,
        IControlDiarioService controlDiarioService,
        IPermisoService permisoService,
        IContratoService contratoService)
    {
        _empleadoService = empleadoService;
        _proyectoService = proyectoService;
        _controlDiarioService = controlDiarioService;
        _permisoService = permisoService;
        _contratoService = contratoService;
        
        // Mensaje de bienvenida según la hora
        var hora = DateTime.Now.Hour;
        var saludo = hora switch
        {
            < 12 => "Buenos días",
            < 18 => "Buenas tardes",
            _ => "Buenas noches"
        };
        WelcomeMessage = $"{saludo}, {App.CurrentUser?.NombreCompleto ?? "Usuario"}";
    }

    public async Task LoadDataAsync()
    {
        IsLoading = true;
        
        try
        {
            // Cargar estadísticas en paralelo para mejor rendimiento
            var totalEmpleadosTask = _empleadoService.CountActiveAsync();
            var proyectosActivosTask = _proyectoService.CountActiveAsync();
            var registrosHoyTask = _controlDiarioService.GetByFechaAsync(DateTime.Today);
            var permisosPendientesTask = _permisoService.GetPendientesAsync();
            var contratosPorVencerTask = _contratoService.GetContratosProximosAVencerAsync(30);
            var cumpleaniosTask = _empleadoService.GetCumpleaniosProximosAsync(7);
            var aniversariosTask = _empleadoService.GetAniversariosProximosAsync(7);
            var estadisticasDeptoTask = _empleadoService.GetEmpleadosPorDepartamentoAsync();
            
            await Task.WhenAll(
                totalEmpleadosTask,
                proyectosActivosTask,
                registrosHoyTask,
                permisosPendientesTask,
                contratosPorVencerTask,
                cumpleaniosTask,
                aniversariosTask,
                estadisticasDeptoTask
            );
            
            TotalEmpleados = await totalEmpleadosTask;
            ProyectosActivos = await proyectosActivosTask;
            
            var registrosHoy = await registrosHoyTask;
            EmpleadosPresentesHoy = registrosHoy.Count();

            // Obtener permisos pendientes
            var permisosResult = await permisosPendientesTask;
            if (permisosResult.Success && permisosResult.Data != null)
            {
                PermisosPendientes = permisosResult.Data.Count();
            }
            
            // Obtener contratos por vencer
            var contratosResult = await contratosPorVencerTask;
            if (contratosResult.Success && contratosResult.Data != null)
            {
                ContratosPorVencer = contratosResult.Data.Count();
            }
            
            // Procesar cumpleaños
            var cumpleanios = await cumpleaniosTask;
            CumpleaniosProximos.Clear();
            var hoy = DateTime.Today;
            
            foreach (var emp in cumpleanios)
            {
                if (emp.FechaNacimiento.HasValue)
                {
                    var proximoCumple = GetNextBirthday(emp.FechaNacimiento.Value);
                    var diasRestantes = (proximoCumple - hoy).Days;
                    var edadCumplir = proximoCumple.Year - emp.FechaNacimiento.Value.Year;
                    
                    CumpleaniosProximos.Add(new CumpleaniosDTO
                    {
                        EmpleadoId = emp.Id,
                        NombreCompleto = emp.NombreCompleto,
                        Cargo = emp.Cargo?.Nombre ?? "Sin cargo",
                        FechaCumple = proximoCumple,
                        DiasRestantes = diasRestantes,
                        EdadCumplir = edadCumplir
                    });
                }
            }
            HayCumpleanios = CumpleaniosProximos.Any();
            
            // Procesar aniversarios
            var aniversarios = await aniversariosTask;
            AniversariosProximos.Clear();
            
            foreach (var emp in aniversarios)
            {
                var proximoAniversario = GetNextAnniversary(emp.FechaIngreso);
                var diasRestantes = (proximoAniversario - hoy).Days;
                var anosServicio = proximoAniversario.Year - emp.FechaIngreso.Year;
                
                AniversariosProximos.Add(new AniversarioDTO
                {
                    EmpleadoId = emp.Id,
                    NombreCompleto = emp.NombreCompleto,
                    Cargo = emp.Cargo?.Nombre ?? "Sin cargo",
                    FechaAniversario = proximoAniversario,
                    DiasRestantes = diasRestantes,
                    AnosCumplir = anosServicio
                });
            }
            HayAniversarios = AniversariosProximos.Any();
            
            // Procesar estadísticas por departamento
            var estadisticasDepto = await estadisticasDeptoTask;
            EmpleadosPorDepartamento.Clear();
            foreach (var stat in estadisticasDepto)
            {
                EmpleadosPorDepartamento.Add(stat);
            }
            HayEstadisticasDepartamento = EmpleadosPorDepartamento.Any();
        }
        catch (Exception)
        {
            // En un caso real, registraríamos el error
            TotalEmpleados = 0;
            ProyectosActivos = 0;
            EmpleadosPresentesHoy = 0;
            PermisosPendientes = 0;
            ContratosPorVencer = 0;
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    private static DateTime GetNextBirthday(DateTime fechaNacimiento)
    {
        var hoy = DateTime.Today;
        try
        {
            var cumpleEsteAnio = new DateTime(hoy.Year, fechaNacimiento.Month, fechaNacimiento.Day);
            return cumpleEsteAnio < hoy ? cumpleEsteAnio.AddYears(1) : cumpleEsteAnio;
        }
        catch (ArgumentOutOfRangeException)
        {
            var cumpleEsteAnio = new DateTime(hoy.Year, fechaNacimiento.Month, 28);
            return cumpleEsteAnio < hoy ? cumpleEsteAnio.AddYears(1) : cumpleEsteAnio;
        }
    }
    
    private static DateTime GetNextAnniversary(DateTime fechaIngreso)
    {
        var hoy = DateTime.Today;
        try
        {
            var aniversarioEsteAnio = new DateTime(hoy.Year, fechaIngreso.Month, fechaIngreso.Day);
            return aniversarioEsteAnio < hoy ? aniversarioEsteAnio.AddYears(1) : aniversarioEsteAnio;
        }
        catch (ArgumentOutOfRangeException)
        {
            var aniversarioEsteAnio = new DateTime(hoy.Year, fechaIngreso.Month, 28);
            return aniversarioEsteAnio < hoy ? aniversarioEsteAnio.AddYears(1) : aniversarioEsteAnio;
        }
    }

    [RelayCommand]
    private void NavigateToControlDiario()
    {
        WeakReferenceMessenger.Default.Send(new NavigationMessage("ControlDiario"));
    }

    [RelayCommand]
    private void NavigateToNewEmpleado()
    {
        WeakReferenceMessenger.Default.Send(new NavigationMessage("CreateEmpleado"));
    }

    [RelayCommand]
    private void NavigateToPermisos()
    {
        WeakReferenceMessenger.Default.Send(new NavigationMessage("Permisos"));
    }
    
    [RelayCommand]
    private async Task RefreshDataAsync()
    {
        await LoadDataAsync();
    }
}
