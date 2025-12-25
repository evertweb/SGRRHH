using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Google.Cloud.Firestore;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;
using SGRRHH.Core.Models;
using SGRRHH.Infrastructure.Firebase.Repositories;
using SGRRHH.WPF.Messages;
using System.Collections.ObjectModel;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// ViewModel del Dashboard con soporte para actualizaciones en tiempo real.
/// </summary>
public partial class DashboardViewModel : ViewModelBase, IDisposable
{
    private readonly IEmpleadoService _empleadoService;
    private readonly IProyectoService _proyectoService;
    private readonly IControlDiarioService _controlDiarioService;
    private readonly IPermisoService _permisoService;
    private readonly IContratoService _contratoService;
    private readonly IFirestoreListenerService? _listenerService;
    private readonly PermisoFirestoreRepository? _permisoRepository;
    private string? _permisosSubscriptionId;
    private bool _disposed;

    [ObservableProperty]
    private int _totalEmpleados;

    [ObservableProperty]
    private int _proyectosActivos;

    [ObservableProperty]
    private int _empleadosPresentesHoy;

    [ObservableProperty]
    private int _permisosPendientes;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HayAlertasContratos))]
    private int _contratosPorVencer;
    
    /// <summary>
    /// Indica si hay contratos próximos a vencer (para alerta visual)
    /// </summary>
    public bool HayAlertasContratos => ContratosPorVencer > 0;

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

    [ObservableProperty]
    private bool _isRealTimeEnabled;

    public DashboardViewModel(
        IEmpleadoService empleadoService,
        IProyectoService proyectoService,
        IControlDiarioService controlDiarioService,
        IPermisoService permisoService,
        IContratoService contratoService,
        IFirestoreListenerService? listenerService = null,
        PermisoFirestoreRepository? permisoRepository = null)
    {
        _empleadoService = empleadoService;
        _proyectoService = proyectoService;
        _controlDiarioService = controlDiarioService;
        _permisoService = permisoService;
        _contratoService = contratoService;
        _listenerService = listenerService;
        _permisoRepository = permisoRepository;

        IsRealTimeEnabled = _listenerService != null && _permisoRepository != null;

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
            // Iniciar listener de permisos pendientes si está disponible
            if (IsRealTimeEnabled && _permisosSubscriptionId == null)
            {
                StartPermisosRealTimeListener();
            }

            // Cargar estadísticas en paralelo para mejor rendimiento
            var totalEmpleadosTask = _empleadoService.CountActiveAsync();
            var proyectosActivosTask = _proyectoService.CountActiveAsync();
            var registrosHoyTask = _controlDiarioService.GetByFechaAsync(DateTime.Today);
            var permisosPendientesTask = IsRealTimeEnabled ? Task.FromResult(Core.Common.ServiceResult<IEnumerable<Permiso>>.Ok(Enumerable.Empty<Permiso>())) : _permisoService.GetPendientesAsync();
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

            // Obtener permisos pendientes (solo si no hay listener activo)
            if (!IsRealTimeEnabled)
            {
                var permisosResult = await permisosPendientesTask;
                if (permisosResult.Success && permisosResult.Data != null)
                {
                    PermisosPendientes = permisosResult.Data.Count();
                }
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

    /// <summary>
    /// Inicia el listener en tiempo real para permisos pendientes.
    /// </summary>
    private void StartPermisosRealTimeListener()
    {
        if (_listenerService == null || _permisoRepository == null) return;

        _permisosSubscriptionId = _listenerService.Subscribe<Permiso>(
            collectionName: "permisos",
            onSnapshot: OnPermisosPendientesUpdated,
            documentToEntity: _permisoRepository.ConvertFromSnapshot,
            onError: _ => { /* Silenciar errores del listener */ },
            queryBuilder: query => query
                .WhereEqualTo("estado", EstadoPermiso.Pendiente.ToString())
                .WhereEqualTo("activo", true)
        );
    }

    /// <summary>
    /// Callback cuando se actualizan los permisos pendientes en tiempo real.
    /// </summary>
    private void OnPermisosPendientesUpdated(IEnumerable<Permiso> permisos)
    {
        // Solo actualizar el contador
        PermisosPendientes = permisos.Count();
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
    
    /// <summary>
    /// Navega a la vista de Contratos
    /// </summary>
    [RelayCommand]
    private void NavigateToContratos()
    {
        WeakReferenceMessenger.Default.Send(new NavigationMessage("Contratos"));
    }
    
    [RelayCommand]
    private async Task RefreshDataAsync()
    {
        await LoadDataAsync();
    }

    /// <summary>
    /// Limpia los recursos y cancela la suscripción al listener.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_permisosSubscriptionId != null && _listenerService != null)
        {
            _listenerService.Unsubscribe(_permisosSubscriptionId);
            _permisosSubscriptionId = null;
        }

        GC.SuppressFinalize(this);
    }
}
