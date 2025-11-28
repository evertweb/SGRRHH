using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;
using SGRRHH.WPF.Messages;
using SGRRHH.WPF.Views;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// Representa un elemento del menú de navegación
/// </summary>
public partial class MenuItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string _icon = string.Empty;
    
    [ObservableProperty]
    private string _title = string.Empty;
    
    [ObservableProperty]
    private string _viewName = string.Empty;
    
    [ObservableProperty]
    private bool _isSelected;
    
    [ObservableProperty]
    private bool _isVisible = true;
    
    /// <summary>
    /// Roles que pueden ver este elemento
    /// </summary>
    public RolUsuario[] AllowedRoles { get; set; } = Array.Empty<RolUsuario>();
}

/// <summary>
/// ViewModel principal de la aplicación
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly Usuario _currentUser;
    private readonly IServiceProvider _serviceProvider;
    
    // Scope actual para mantener vivo el DbContext de la vista activa
    private IServiceScope? _currentViewScope;
    
    [ObservableProperty]
    private string _windowTitle = "SGRRHH - Sistema de Gestión de Recursos Humanos";
    
    [ObservableProperty]
    private string _currentUserName = string.Empty;
    
    [ObservableProperty]
    private string _currentUserRole = string.Empty;
    
    [ObservableProperty]
    private ObservableCollection<MenuItemViewModel> _menuItems = new();
    
    [ObservableProperty]
    private MenuItemViewModel? _selectedMenuItem;
    
    [ObservableProperty]
    private string _currentViewTitle = "Dashboard";
    
    [ObservableProperty]
    private object? _currentView;
    
    [ObservableProperty]
    private int _pendingAlerts;
    
    [ObservableProperty]
    private int _totalEmpleados;
    
    /// <summary>
    /// Evento para cerrar sesión
    /// </summary>
    public event EventHandler? LogoutRequested;
    
    public MainViewModel(Usuario currentUser, IServiceProvider serviceProvider)
    {
        _currentUser = currentUser;
        _serviceProvider = serviceProvider;
        CurrentUserName = currentUser.NombreCompleto;
        CurrentUserRole = GetRoleName(currentUser.Rol);
        
        // Suscribirse a mensajes de navegación
        WeakReferenceMessenger.Default.Register<NavigationMessage>(this, (r, m) =>
        {
            var menuItem = MenuItems.FirstOrDefault(x => x.ViewName == m.Value);
            if (menuItem != null)
            {
                NavigateTo(menuItem);
            }
            else if (m.Value == "CreateEmpleado")
            {
                // Navegación especial que no es un menú principal
                ShowEmpleadoForm(null);
            }
        });

        InitializeMenu();
        _ = LoadDashboardDataAsync();
    }
    
    private void InitializeMenu()
    {
        var allMenuItems = new List<MenuItemViewModel>
        {
            new MenuItemViewModel
            {
                Icon = "📊",
                Title = "Dashboard",
                ViewName = "Dashboard",
                AllowedRoles = new[] { RolUsuario.Administrador, RolUsuario.Aprobador, RolUsuario.Operador }
            },
            new MenuItemViewModel
            {
                Icon = "👥",
                Title = "Empleados",
                ViewName = "Empleados",
                AllowedRoles = new[] { RolUsuario.Administrador, RolUsuario.Aprobador, RolUsuario.Operador }
            },
            new MenuItemViewModel
            {
                Icon = "📅",
                Title = "Control Diario",
                ViewName = "ControlDiario",
                AllowedRoles = new[] { RolUsuario.Administrador, RolUsuario.Aprobador, RolUsuario.Operador }
            },
            new MenuItemViewModel
            {
                Icon = "�",
                Title = "Chat",
                ViewName = "Chat",
                AllowedRoles = new[] { RolUsuario.Administrador, RolUsuario.Aprobador, RolUsuario.Operador }
            },
            new MenuItemViewModel
            {
                Icon = "�📝",
                Title = "Permisos",
                ViewName = "Permisos",
                AllowedRoles = new[] { RolUsuario.Administrador, RolUsuario.Aprobador, RolUsuario.Operador }
            },
            new MenuItemViewModel
            {
                Icon = "🏖️",
                Title = "Vacaciones",
                ViewName = "Vacaciones",
                AllowedRoles = new[] { RolUsuario.Administrador, RolUsuario.Aprobador, RolUsuario.Operador }
            },
            new MenuItemViewModel
            {
                Icon = "📄",
                Title = "Contratos",
                ViewName = "Contratos",
                AllowedRoles = new[] { RolUsuario.Administrador, RolUsuario.Aprobador }
            },
            new MenuItemViewModel
            {
                Icon = "📁",
                Title = "Catálogos",
                ViewName = "Catalogos",
                AllowedRoles = new[] { RolUsuario.Administrador, RolUsuario.Aprobador, RolUsuario.Operador }
            },
            new MenuItemViewModel
            {
                Icon = "🏢",
                Title = "Departamentos",
                ViewName = "Departamentos",
                AllowedRoles = new[] { RolUsuario.Administrador }
            },
            new MenuItemViewModel
            {
                Icon = "💼",
                Title = "Cargos",
                ViewName = "Cargos",
                AllowedRoles = new[] { RolUsuario.Administrador }
            },
            new MenuItemViewModel
            {
                Icon = "🚀",
                Title = "Proyectos",
                ViewName = "Proyectos",
                AllowedRoles = new[] { RolUsuario.Administrador, RolUsuario.Aprobador }
            },
            new MenuItemViewModel
            {
                Icon = "📝",
                Title = "Actividades",
                ViewName = "Actividades",
                AllowedRoles = new[] { RolUsuario.Administrador }
            },
            new MenuItemViewModel
            {
                Icon = "📋",
                Title = "Tipos de Permiso",
                ViewName = "TiposPermiso",
                AllowedRoles = new[] { RolUsuario.Administrador }
            },
            new MenuItemViewModel
            {
                Icon = "📈",
                Title = "Reportes",
                ViewName = "Reportes",
                AllowedRoles = new[] { RolUsuario.Administrador, RolUsuario.Aprobador, RolUsuario.Operador }
            },
            new MenuItemViewModel
            {
                Icon = "📄",
                Title = "Documentos",
                ViewName = "Documentos",
                AllowedRoles = new[] { RolUsuario.Administrador, RolUsuario.Aprobador, RolUsuario.Operador }
            },
            new MenuItemViewModel
            {
                Icon = "⚙️",
                Title = "Configuración",
                ViewName = "Configuracion",
                AllowedRoles = new[] { RolUsuario.Administrador }
            },
            new MenuItemViewModel
            {
                Icon = "👤",
                Title = "Usuarios",
                ViewName = "Usuarios",
                AllowedRoles = new[] { RolUsuario.Administrador }
            }
        };
        
        // Filtrar por rol
        foreach (var item in allMenuItems)
        {
            if (item.AllowedRoles.Contains(_currentUser.Rol))
            {
                MenuItems.Add(item);
            }
        }
        
        // Seleccionar Dashboard por defecto
        if (MenuItems.Any())
        {
            SelectedMenuItem = MenuItems.First();
            // La navegación se ejecutará después de que la ventana se cargue
        }
    }
    
    private async Task LoadDashboardDataAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var empleadoService = scope.ServiceProvider.GetRequiredService<IEmpleadoService>();
            TotalEmpleados = await empleadoService.CountActiveAsync();
        }
        catch
        {
            TotalEmpleados = 0;
        }
    }
    
    [RelayCommand]
    private void NavigateTo(MenuItemViewModel? menuItem)
    {
        if (menuItem == null) return;
        
        // Deseleccionar anterior
        foreach (var item in MenuItems)
        {
            item.IsSelected = false;
        }
        
        // Seleccionar nuevo
        menuItem.IsSelected = true;
        SelectedMenuItem = menuItem;
        CurrentViewTitle = menuItem.Title;
        
        // Cargar la vista correspondiente
        LoadView(menuItem.ViewName);
    }
    
    private void LoadView(string viewName)
    {
        // Disponer el scope anterior si existe
        _currentViewScope?.Dispose();
        
        // Crear un nuevo scope que se mantendrá activo mientras la vista esté cargada
        _currentViewScope = _serviceProvider.CreateScope();
        var scope = _currentViewScope;
        
        switch (viewName)
        {
            case "Empleados":
                var empleadosViewModel = scope.ServiceProvider.GetRequiredService<EmpleadosListViewModel>();
                var empleadosView = new EmpleadosListView(empleadosViewModel);
                
                // Suscribirse a eventos
                empleadosViewModel.CreateEmpleadoRequested += OnCreateEmpleadoRequested;
                empleadosViewModel.EditEmpleadoRequested += OnEditEmpleadoRequested;
                empleadosViewModel.ViewEmpleadoRequested += OnViewEmpleadoRequested;
                
                // Cargar datos
                _ = empleadosViewModel.LoadDataAsync();
                
                CurrentView = empleadosView;
                break;
                
            case "Departamentos":
                var deptViewModel = scope.ServiceProvider.GetRequiredService<DepartamentosListViewModel>();
                var deptView = new DepartamentosListView(deptViewModel);
                CurrentView = deptView;
                break;
                
            case "Cargos":
                var cargosViewModel = scope.ServiceProvider.GetRequiredService<CargosListViewModel>();
                var cargosView = new CargosListView(cargosViewModel);
                CurrentView = cargosView;
                break;
                
            case "ControlDiario":
                var controlDiarioViewModel = scope.ServiceProvider.GetRequiredService<ControlDiarioViewModel>();
                var controlDiarioView = new ControlDiarioView(controlDiarioViewModel);
                _ = controlDiarioViewModel.LoadDataAsync();
                CurrentView = controlDiarioView;
                break;
            
            case "Chat":
                var chatViewModel = scope.ServiceProvider.GetRequiredService<ChatViewModel>();
                var chatView = new ChatView(chatViewModel);
                CurrentView = chatView;
                break;
                
            case "Proyectos":
                var proyectosViewModel = scope.ServiceProvider.GetRequiredService<ProyectosListViewModel>();
                var proyectosView = new ProyectosListView(proyectosViewModel);
                _ = proyectosViewModel.LoadDataAsync();
                CurrentView = proyectosView;
                break;
                
            case "Actividades":
                var actividadesViewModel = scope.ServiceProvider.GetRequiredService<ActividadesListViewModel>();
                var actividadesView = new ActividadesListView(actividadesViewModel);
                _ = actividadesViewModel.LoadDataAsync();
                CurrentView = actividadesView;
                break;
            
            case "Permisos":
                // Determinar qué vista mostrar según el rol
                if (_currentUser.Rol == RolUsuario.Aprobador || _currentUser.Rol == RolUsuario.Administrador)
                {
                    // Aprobadores ven la bandeja de aprobación
                    var bandejaViewModel = scope.ServiceProvider.GetRequiredService<BandejaAprobacionViewModel>();
                    var bandejaView = new BandejaAprobacionView(bandejaViewModel);
                    _ = bandejaViewModel.LoadDataAsync();
                    CurrentView = bandejaView;
                }
                else
                {
                    // Operadores ven la lista de permisos para solicitar
                    var permisosViewModel = scope.ServiceProvider.GetRequiredService<PermisosListViewModel>();
                    
                    // Suscribirse a eventos
                    permisosViewModel.CreatePermisoRequested += OnCreatePermisoRequested;
                    permisosViewModel.EditPermisoRequested += OnEditPermisoRequested;
                    
                    var permisosView = new PermisosListView(permisosViewModel);
                    _ = permisosViewModel.LoadDataAsync();
                    CurrentView = permisosView;
                }
                break;
            
            case "HistorialPermisos":
                var historialPermisosViewModel = scope.ServiceProvider.GetRequiredService<PermisosListViewModel>();
                historialPermisosViewModel.CreatePermisoRequested += OnCreatePermisoRequested;
                historialPermisosViewModel.EditPermisoRequested += OnEditPermisoRequested;
                var historialPermisosView = new PermisosListView(historialPermisosViewModel);
                _ = historialPermisosViewModel.LoadDataAsync();
                CurrentView = historialPermisosView;
                break;
            
            case "TiposPermiso":
                var tiposPermisoViewModel = scope.ServiceProvider.GetRequiredService<TiposPermisoListViewModel>();
                var tiposPermisoView = new TiposPermisoListView(tiposPermisoViewModel);
                _ = tiposPermisoViewModel.LoadDataAsync();
                CurrentView = tiposPermisoView;
                break;

            case "Reportes":
                var reportsViewModel = scope.ServiceProvider.GetRequiredService<ReportsViewModel>();
                var reportsView = new ReportsView(reportsViewModel);
                _ = reportsViewModel.LoadDataAsync();
                CurrentView = reportsView;
                break;
            
            case "Vacaciones":
                var vacacionesViewModel = scope.ServiceProvider.GetRequiredService<VacacionesViewModel>();
                var vacacionesView = new VacacionesView(vacacionesViewModel);
                _ = vacacionesViewModel.LoadDataAsync();
                CurrentView = vacacionesView;
                break;
            
            case "Contratos":
                var contratosViewModel = scope.ServiceProvider.GetRequiredService<ContratosViewModel>();
                var contratosView = new ContratosView(contratosViewModel);
                _ = contratosViewModel.LoadDataAsync();
                CurrentView = contratosView;
                break;
                
            case "Dashboard":
                var dashboardViewModel = scope.ServiceProvider.GetRequiredService<DashboardViewModel>();
                var dashboardView = new DashboardView(dashboardViewModel);
                _ = dashboardViewModel.LoadDataAsync();
                CurrentView = dashboardView;
                break;
            
            case "Catalogos":
                var catalogosViewModel = scope.ServiceProvider.GetRequiredService<CatalogosViewModel>();
                var catalogosView = new CatalogosView(catalogosViewModel);
                CurrentView = catalogosView;
                break;
            
            case "Documentos":
                var documentsViewModel = scope.ServiceProvider.GetRequiredService<DocumentsViewModel>();
                var documentsView = new DocumentsView(documentsViewModel);
                CurrentView = documentsView;
                break;
            
            case "Configuracion":
                var configuracionViewModel = scope.ServiceProvider.GetRequiredService<ConfiguracionViewModel>();
                var configuracionView = new ConfiguracionView(configuracionViewModel);
                _ = configuracionViewModel.LoadDataAsync();
                CurrentView = configuracionView;
                break;
            
            case "Usuarios":
                var usuariosViewModel = scope.ServiceProvider.GetRequiredService<UsuariosListViewModel>();
                var usuariosView = new UsuariosListView(usuariosViewModel);
                _ = usuariosViewModel.LoadDataAsync();
                CurrentView = usuariosView;
                break;
                
            default:
                CurrentView = null;
                break;
        }
    }
    
    private void OnCreateEmpleadoRequested(object? sender, EventArgs e)
    {
        ShowEmpleadoForm(null);
    }
    
    private void OnEditEmpleadoRequested(object? sender, Empleado empleado)
    {
        ShowEmpleadoForm(empleado.Id);
    }
    
    private void OnViewEmpleadoRequested(object? sender, Empleado empleado)
    {
        ShowEmpleadoDetail(empleado.Id);
    }
    
    private void OnCreatePermisoRequested(object? sender, EventArgs e)
    {
        ShowPermisoForm(null);
    }
    
    private void OnEditPermisoRequested(object? sender, Permiso permiso)
    {
        ShowPermisoForm(permiso.Id);
    }
    
    private void ShowPermisoForm(int? permisoId)
    {
        using var scope = _serviceProvider.CreateScope();
        var viewModel = scope.ServiceProvider.GetRequiredService<PermisoFormViewModel>();
        var window = new PermisoFormWindow(viewModel);
        
        if (permisoId.HasValue)
        {
            _ = viewModel.InitializeForEditAsync(permisoId.Value);
        }
        else
        {
            _ = viewModel.InitializeForCreateAsync();
        }
        
        var result = window.ShowDialog();
        
        if (result == true)
        {
            // Recargar lista de permisos
            LoadView("Permisos");
        }
    }
    
    private void ShowEmpleadoForm(int? empleadoId)
    {
        using var scope = _serviceProvider.CreateScope();
        var viewModel = scope.ServiceProvider.GetRequiredService<EmpleadoFormViewModel>();
        var window = new EmpleadoFormWindow(viewModel);
        
        if (empleadoId.HasValue)
        {
            _ = viewModel.InitializeForEditAsync(empleadoId.Value);
        }
        else
        {
            _ = viewModel.InitializeForCreateAsync();
        }
        
        var result = window.ShowDialog();
        
        if (result == true)
        {
            // Recargar lista de empleados
            LoadView("Empleados");
        }
    }
    
    private void ShowEmpleadoDetail(int empleadoId)
    {
        using var scope = _serviceProvider.CreateScope();
        var viewModel = scope.ServiceProvider.GetRequiredService<EmpleadoDetailViewModel>();
        var window = new EmpleadoDetailWindow(viewModel);
        
        // Suscribirse a evento de edición
        viewModel.EditRequested += (s, emp) =>
        {
            window.Close();
            ShowEmpleadoForm(emp.Id);
        };
        
        _ = viewModel.LoadEmpleadoAsync(empleadoId);
        
        window.ShowDialog();
    }
    
    [RelayCommand]
    private void Logout()
    {
        var result = MessageBox.Show(
            "¿Está seguro de cerrar sesión?",
            "Cerrar Sesión",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
            
        if (result == MessageBoxResult.Yes)
        {
            // Limpiar scope antes de cerrar
            _currentViewScope?.Dispose();
            _currentViewScope = null;
            LogoutRequested?.Invoke(this, EventArgs.Empty);
        }
    }
    
    [RelayCommand]
    private void ChangePassword()
    {
        using var scope = _serviceProvider.CreateScope();
        var viewModel = scope.ServiceProvider.GetRequiredService<CambiarPasswordViewModel>();
        var window = new CambiarPasswordWindow(viewModel);
        window.Owner = Application.Current.MainWindow;
        window.ShowDialog();
    }
    
    /// <summary>
    /// Limpia los recursos cuando se cierra la ventana principal
    /// </summary>
    public void Cleanup()
    {
        _currentViewScope?.Dispose();
        _currentViewScope = null;
    }
    
    private static string GetRoleName(RolUsuario rol) => rol switch
    {
        RolUsuario.Administrador => "Administrador",
        RolUsuario.Aprobador => "Aprobador (Ingeniera)",
        RolUsuario.Operador => "Operador (Secretaria)",
        _ => "Desconocido"
    };
}
