using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;
using SGRRHH.WPF.Helpers;
using SGRRHH.WPF.Messages;
using SGRRHH.WPF.Services;
using SGRRHH.WPF.Views;
using System.Collections.ObjectModel;
using System.Windows;

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
    /// Categoría del menú para agrupación visual
    /// </summary>
    [ObservableProperty]
    private string _category = string.Empty;
    
    /// <summary>
    /// Indica si es un separador de categoría (no clickeable)
    /// </summary>
    [ObservableProperty]
    private bool _isSeparator = false;
    
    /// <summary>
    /// Roles que pueden ver este elemento
    /// </summary>
    public RolUsuario[] AllowedRoles { get; set; } = Array.Empty<RolUsuario>();
}

/// <summary>
/// ViewModel principal de la aplicación.
/// Refactorizado para usar ViewFactory y MenuConfiguration.
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    private readonly Usuario _currentUser;
    private readonly IServiceProvider _serviceProvider;
    private readonly IDialogService _dialogService;
    private readonly ViewFactory _viewFactory;
    
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
    
    [ObservableProperty]
    private string _currentDateTime = string.Empty;
    
    [ObservableProperty]
    private string _appVersion = string.Empty;
    
    private System.Windows.Threading.DispatcherTimer? _dateTimeTimer;
    
    /// <summary>
    /// Evento para cerrar sesión
    /// </summary>
    public event EventHandler? LogoutRequested;
    
    public MainViewModel(Usuario currentUser, IServiceProvider serviceProvider, IDialogService dialogService)
    {
        _currentUser = currentUser;
        _serviceProvider = serviceProvider;
        _dialogService = dialogService;
        CurrentUserName = currentUser.NombreCompleto;
        CurrentUserRole = GetRoleName(currentUser.Rol);
        
        // Crear ViewFactory y suscribirse a eventos
        _viewFactory = new ViewFactory(currentUser);
        SetupViewFactoryEvents();
        
        // Inicializar fecha/hora y versión
        InitializeDateTimeAndVersion();
        
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
                ShowEmpleadoForm(null);
            }
        });

        InitializeMenu();
        LoadDashboardDataAsync().SafeFireAndForget(showErrorMessage: false);
    }
    
    /// <summary>
    /// Configura los eventos de ViewFactory para manejar formularios modales
    /// </summary>
    private void SetupViewFactoryEvents()
    {
        _viewFactory.CreateEmpleadoRequested += (s, e) => ShowEmpleadoForm(null);
        _viewFactory.EditEmpleadoRequested += (s, emp) => ShowEmpleadoForm(emp.Id);
        _viewFactory.ViewEmpleadoRequested += (s, emp) => ShowEmpleadoDetail(emp.Id);
        _viewFactory.CreatePermisoRequested += (s, e) => ShowPermisoForm(null);
        _viewFactory.EditPermisoRequested += (s, p) => ShowPermisoForm(p.Id);
        _viewFactory.CreateActividadRequested += (s, e) => ShowActividadForm(null);
        _viewFactory.EditActividadRequested += (s, act) => ShowActividadForm(act.Id);
    }
    
    /// <summary>
    /// Inicializa el menú usando MenuConfiguration
    /// </summary>
    private void InitializeMenu()
    {
        MenuItems.Clear();
        foreach (var item in MenuConfiguration.GetMenuItems(_currentUser.Rol))
        {
            MenuItems.Add(item);
        }
        
        // Seleccionar Dashboard por defecto
        var dashboard = MenuItems.FirstOrDefault(m => m.ViewName == "Dashboard");
        if (dashboard != null)
        {
            SelectedMenuItem = dashboard;
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
    
    /// <summary>
    /// Carga una vista usando ViewFactory
    /// </summary>
    private void LoadView(string viewName)
    {
        // Disponer el scope anterior si existe
        _currentViewScope?.Dispose();
        
        // Crear un nuevo scope que se mantendrá activo mientras la vista esté cargada
        _currentViewScope = _serviceProvider.CreateScope();
        
        var result = _viewFactory.CreateView(viewName, _currentViewScope);
        
        if (result == null)
        {
            CurrentView = null;
            return;
        }
        
        // Configurar eventos si es necesario
        result.SetupEvents?.Invoke();
        
        // Cargar datos automáticamente si corresponde
        if (result.AutoLoadData && result.ViewModel is ViewModelBase vmBase)
        {
            // Usar reflection para llamar LoadDataAsync si existe
            var loadMethod = result.ViewModel.GetType().GetMethod("LoadDataAsync");
            if (loadMethod != null)
            {
                var task = loadMethod.Invoke(result.ViewModel, null) as Task;
                task?.SafeFireAndForget(showErrorMessage: false);
            }
        }
        
        CurrentView = result.View;
    }
    
    #region Modal Forms
    
    private void ShowActividadForm(int? actividadId)
    {
        using var scope = _serviceProvider.CreateScope();
        var viewModel = scope.ServiceProvider.GetRequiredService<ActividadFormViewModel>();
        var window = new ActividadFormWindow(viewModel);
        window.Owner = Application.Current.MainWindow;
        
        if (actividadId.HasValue)
        {
            viewModel.InitializeForEditAsync(actividadId.Value).SafeFireAndForget();
        }
        else
        {
            viewModel.InitializeForCreateAsync().SafeFireAndForget();
        }
        
        var result = window.ShowDialog();
        
        if (result == true)
        {
            LoadView("Actividades");
        }
    }
    
    private void ShowPermisoForm(int? permisoId)
    {
        using var scope = _serviceProvider.CreateScope();
        var viewModel = scope.ServiceProvider.GetRequiredService<PermisoFormViewModel>();
        var window = new PermisoFormWindow(viewModel);
        
        if (permisoId.HasValue)
        {
            viewModel.InitializeForEditAsync(permisoId.Value).SafeFireAndForget();
        }
        else
        {
            viewModel.InitializeForCreateAsync().SafeFireAndForget();
        }
        
        var result = window.ShowDialog();
        
        if (result == true)
        {
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
            viewModel.InitializeForEditAsync(empleadoId.Value).SafeFireAndForget();
        }
        else
        {
            viewModel.InitializeForCreateAsync().SafeFireAndForget();
        }
        
        var result = window.ShowDialog();
        
        if (result == true)
        {
            LoadView("Empleados");
        }
    }
    
    private void ShowEmpleadoDetail(int empleadoId)
    {
        using var scope = _serviceProvider.CreateScope();
        var viewModel = scope.ServiceProvider.GetRequiredService<EmpleadoDetailViewModel>();
        var window = new EmpleadoDetailWindow(viewModel);
        
        viewModel.EditRequested += (s, emp) =>
        {
            window.Close();
            ShowEmpleadoForm(emp.Id);
        };
        
        viewModel.LoadEmpleadoAsync(empleadoId).SafeFireAndForget();
        
        window.ShowDialog();
    }
    
    #endregion
    
    #region Commands
    
    [RelayCommand]
    private void Logout()
    {
        if (!_dialogService.Confirm("¿Está seguro de cerrar sesión?", "Cerrar Sesión"))
            return;
            
        Cleanup();
        LogoutRequested?.Invoke(this, EventArgs.Empty);
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
    
    #endregion
    
    #region Cleanup & Utilities
    
    /// <summary>
    /// Limpia los recursos cuando se cierra la ventana principal
    /// </summary>
    public void Cleanup()
    {
        StopDateTimeTimer();
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
    
    /// <summary>
    /// Inicializa el timer para actualizar fecha/hora y carga la versión
    /// </summary>
    private void InitializeDateTimeAndVersion()
    {
        UpdateDateTime();
        
        _dateTimeTimer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _dateTimeTimer.Tick += (s, e) => UpdateDateTime();
        _dateTimeTimer.Start();
        
        LoadAppVersion();
    }
    
    private void UpdateDateTime()
    {
        CurrentDateTime = DateTime.Now.ToString("dddd, dd 'de' MMMM yyyy  HH:mm:ss", 
            new System.Globalization.CultureInfo("es-CO"));
    }
    
    private void LoadAppVersion()
    {
        AppVersion = $"v{AppSettings.GetAppVersion()}";
    }
    
    /// <summary>
    /// Detiene el timer al limpiar recursos
    /// </summary>
    private void StopDateTimeTimer()
    {
        _dateTimeTimer?.Stop();
        _dateTimeTimer = null;
    }
    
    #endregion
}
