using Microsoft.Extensions.DependencyInjection;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.WPF.Helpers;
using SGRRHH.WPF.ViewModels;
using SGRRHH.WPF.Views;
using System.Windows;

namespace SGRRHH.WPF.Services;

/// <summary>
/// Resultado de la creación de una vista.
/// Contiene la vista, el ViewModel y callbacks opcionales para eventos.
/// </summary>
public class ViewCreationResult
{
    public required object View { get; init; }
    public required object ViewModel { get; init; }
    
    /// <summary>
    /// Acción para configurar eventos del ViewModel.
    /// Se ejecuta después de crear la vista.
    /// </summary>
    public Action? SetupEvents { get; init; }
    
    /// <summary>
    /// Indica si se debe llamar LoadDataAsync automáticamente.
    /// </summary>
    public bool AutoLoadData { get; init; } = true;
}

/// <summary>
/// Interface para la fábrica de vistas.
/// </summary>
public interface IViewFactory
{
    /// <summary>
    /// Crea una vista y su ViewModel correspondiente.
    /// </summary>
    /// <param name="viewName">Nombre de la vista a crear</param>
    /// <param name="scope">Scope de DI para resolver servicios</param>
    /// <returns>Resultado con la vista y ViewModel, o null si no existe</returns>
    ViewCreationResult? CreateView(string viewName, IServiceScope scope);
}

/// <summary>
/// Fábrica de vistas que centraliza la creación de Views y ViewModels.
/// Extrae la lógica del switch gigante de MainViewModel.LoadView().
/// </summary>
public class ViewFactory : IViewFactory
{
    private readonly Usuario _currentUser;
    
    // Callbacks para eventos que MainViewModel necesita manejar
    public event EventHandler? CreateEmpleadoRequested;
    public event EventHandler<Empleado>? EditEmpleadoRequested;
    public event EventHandler<Empleado>? ViewEmpleadoRequested;
    public event EventHandler? CreatePermisoRequested;
    public event EventHandler<Permiso>? EditPermisoRequested;
    public event EventHandler? CreateActividadRequested;
    public event EventHandler<Actividad>? EditActividadRequested;
    
    public ViewFactory(Usuario currentUser)
    {
        _currentUser = currentUser;
    }
    
    public ViewCreationResult? CreateView(string viewName, IServiceScope scope)
    {
        return viewName switch
        {
            "Dashboard" => CreateDashboardView(scope),
            "Empleados" => CreateEmpleadosView(scope),
            "Departamentos" => CreateDepartamentosView(scope),
            "Cargos" => CreateCargosView(scope),
            "ControlDiario" => CreateControlDiarioView(scope),
            "Proyectos" => CreateProyectosView(scope),
            "Actividades" => CreateActividadesView(scope),
            "Permisos" => CreatePermisosView(scope),
            "HistorialPermisos" => CreateHistorialPermisosView(scope),
            "TiposPermiso" => CreateTiposPermisoView(scope),
            "Reportes" => CreateReportesView(scope),
            "Vacaciones" => CreateVacacionesView(scope),
            "BandejaVacaciones" => CreateBandejaVacacionesView(scope),
            "Contratos" => CreateContratosView(scope),
            "Catalogos" => CreateCatalogosView(scope),
            "Documentos" => CreateDocumentosView(scope),
            "Configuracion" => CreateConfiguracionView(scope),
            "Usuarios" => CreateUsuariosView(scope),
            _ => null
        };
    }
    
    private ViewCreationResult CreateDashboardView(IServiceScope scope)
    {
        var vm = scope.ServiceProvider.GetRequiredService<DashboardViewModel>();
        var view = new DashboardView(vm);
        return new ViewCreationResult { View = view, ViewModel = vm };
    }
    
    private ViewCreationResult CreateEmpleadosView(IServiceScope scope)
    {
        var vm = scope.ServiceProvider.GetRequiredService<EmpleadosListViewModel>();
        var view = new EmpleadosListView(vm);
        
        return new ViewCreationResult
        {
            View = view,
            ViewModel = vm,
            SetupEvents = () =>
            {
                vm.CreateEmpleadoRequested += (s, e) => CreateEmpleadoRequested?.Invoke(s, e);
                vm.EditEmpleadoRequested += (s, emp) => EditEmpleadoRequested?.Invoke(s, emp);
                vm.ViewEmpleadoRequested += (s, emp) => ViewEmpleadoRequested?.Invoke(s, emp);
            }
        };
    }
    
    private ViewCreationResult CreateDepartamentosView(IServiceScope scope)
    {
        var vm = scope.ServiceProvider.GetRequiredService<DepartamentosListViewModel>();
        var view = new DepartamentosListView(vm);
        return new ViewCreationResult { View = view, ViewModel = vm, AutoLoadData = false };
    }
    
    private ViewCreationResult CreateCargosView(IServiceScope scope)
    {
        var vm = scope.ServiceProvider.GetRequiredService<CargosListViewModel>();
        var view = new CargosListView(vm);
        return new ViewCreationResult { View = view, ViewModel = vm, AutoLoadData = false };
    }
    
    private ViewCreationResult CreateControlDiarioView(IServiceScope scope)
    {
        var vm = scope.ServiceProvider.GetRequiredService<DailyActivityWizardViewModel>();
        var view = new DailyActivityWizardView(vm);
        return new ViewCreationResult { View = view, ViewModel = vm, AutoLoadData = false };
    }
    
    private ViewCreationResult CreateProyectosView(IServiceScope scope)
    {
        var vm = scope.ServiceProvider.GetRequiredService<ProyectosListViewModel>();
        var view = new ProyectosListView(vm);
        return new ViewCreationResult { View = view, ViewModel = vm };
    }
    
    private ViewCreationResult CreateActividadesView(IServiceScope scope)
    {
        var vm = scope.ServiceProvider.GetRequiredService<ActividadesListViewModel>();
        var view = new ActividadesListView(vm);
        
        return new ViewCreationResult
        {
            View = view,
            ViewModel = vm,
            SetupEvents = () =>
            {
                vm.CreateActividadRequested += (s, e) => CreateActividadRequested?.Invoke(s, e);
                vm.EditActividadRequested += (s, act) => EditActividadRequested?.Invoke(s, act);
            }
        };
    }
    
    private ViewCreationResult CreatePermisosView(IServiceScope scope)
    {
        // Determinar qué vista mostrar según el rol
        if (_currentUser.Rol == RolUsuario.Aprobador || _currentUser.Rol == RolUsuario.Administrador)
        {
            var vm = scope.ServiceProvider.GetRequiredService<BandejaAprobacionViewModel>();
            var view = new BandejaAprobacionView(vm);
            return new ViewCreationResult { View = view, ViewModel = vm };
        }
        else
        {
            var vm = scope.ServiceProvider.GetRequiredService<PermisosListViewModel>();
            var view = new PermisosListView(vm);
            
            return new ViewCreationResult
            {
                View = view,
                ViewModel = vm,
                SetupEvents = () =>
                {
                    vm.CreatePermisoRequested += (s, e) => CreatePermisoRequested?.Invoke(s, e);
                    vm.EditPermisoRequested += (s, p) => EditPermisoRequested?.Invoke(s, p);
                }
            };
        }
    }
    
    private ViewCreationResult CreateHistorialPermisosView(IServiceScope scope)
    {
        var vm = scope.ServiceProvider.GetRequiredService<PermisosListViewModel>();
        var view = new PermisosListView(vm);
        
        return new ViewCreationResult
        {
            View = view,
            ViewModel = vm,
            SetupEvents = () =>
            {
                vm.CreatePermisoRequested += (s, e) => CreatePermisoRequested?.Invoke(s, e);
                vm.EditPermisoRequested += (s, p) => EditPermisoRequested?.Invoke(s, p);
            }
        };
    }
    
    private ViewCreationResult CreateTiposPermisoView(IServiceScope scope)
    {
        var vm = scope.ServiceProvider.GetRequiredService<TiposPermisoListViewModel>();
        var view = new TiposPermisoListView(vm);
        return new ViewCreationResult { View = view, ViewModel = vm };
    }
    
    private ViewCreationResult CreateReportesView(IServiceScope scope)
    {
        var vm = scope.ServiceProvider.GetRequiredService<ReportsViewModel>();
        var view = new ReportsView(vm);
        return new ViewCreationResult { View = view, ViewModel = vm };
    }
    
    private ViewCreationResult CreateVacacionesView(IServiceScope scope)
    {
        var vm = scope.ServiceProvider.GetRequiredService<VacacionesViewModel>();
        var view = new VacacionesView(vm);
        return new ViewCreationResult { View = view, ViewModel = vm };
    }
    
    private ViewCreationResult CreateBandejaVacacionesView(IServiceScope scope)
    {
        var vm = scope.ServiceProvider.GetRequiredService<BandejaVacacionesViewModel>();
        var view = new BandejaVacacionesView { DataContext = vm };
        return new ViewCreationResult { View = view, ViewModel = vm };
    }
    
    private ViewCreationResult CreateContratosView(IServiceScope scope)
    {
        var vm = scope.ServiceProvider.GetRequiredService<ContratosViewModel>();
        var view = new ContratosView(vm);
        return new ViewCreationResult { View = view, ViewModel = vm };
    }
    
    private ViewCreationResult CreateCatalogosView(IServiceScope scope)
    {
        var vm = scope.ServiceProvider.GetRequiredService<CatalogosViewModel>();
        var view = new CatalogosView(vm);
        return new ViewCreationResult { View = view, ViewModel = vm, AutoLoadData = false };
    }
    
    private ViewCreationResult CreateDocumentosView(IServiceScope scope)
    {
        var vm = scope.ServiceProvider.GetRequiredService<DocumentsViewModel>();
        var view = new DocumentsView(vm);
        return new ViewCreationResult { View = view, ViewModel = vm, AutoLoadData = false };
    }
    
    private ViewCreationResult CreateConfiguracionView(IServiceScope scope)
    {
        var vm = scope.ServiceProvider.GetRequiredService<ConfiguracionViewModel>();
        var view = new ConfiguracionView(vm);
        return new ViewCreationResult { View = view, ViewModel = vm };
    }
    
    private ViewCreationResult CreateUsuariosView(IServiceScope scope)
    {
        var vm = scope.ServiceProvider.GetRequiredService<UsuariosListViewModel>();
        var view = new UsuariosListView(vm);
        return new ViewCreationResult { View = view, ViewModel = vm };
    }
}
