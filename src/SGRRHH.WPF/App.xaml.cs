using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;
using SGRRHH.Core.Models;
using SGRRHH.Infrastructure.Firebase;
using SGRRHH.Infrastructure.Services;
using SGRRHH.WPF.Helpers;
using SGRRHH.WPF.ViewModels;
using SGRRHH.WPF.Views;

namespace SGRRHH.WPF;

/// <summary>
/// Lógica principal de la aplicación
/// </summary>
public partial class App : Application
{
    private IServiceProvider? _serviceProvider;
    private Usuario? _currentUser;
    private IUpdateService? _updateService;
    private bool _shouldRestartForUpdate;
    private FirebaseInitializer? _firebaseInitializer;
    
    /// <summary>
    /// Proveedor de servicios para acceso global
    /// </summary>
    public static IServiceProvider? Services { get; private set; }
    
    /// <summary>
    /// Usuario actual logueado
    /// </summary>
    public static Usuario? CurrentUser { get; private set; }
    
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Configurar manejo global de excepciones
        SetupGlobalExceptionHandling();
        
        // Configurar servicios Firebase
        var services = new ServiceCollection();
        ConfigureFirebaseServices(services);
        
        _serviceProvider = services.BuildServiceProvider();
        Services = _serviceProvider;
        
        // Configurar proveedor de usuario actual para el servicio de auditoría
        AuditService.ConfigureCurrentUserProvider(() => CurrentUser);
        
        // Inicializar Firebase
        await InitializeFirebaseAsync();
        
        // Verificar actualizaciones si está habilitado
        await CheckForUpdatesAsync();
        
        // Mostrar login
        await ShowLoginAsync();
    }
    
    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);
        
        // Liberar recursos de Firebase
        _firebaseInitializer?.Dispose();
        
        // Si hay actualización pendiente, ejecutar el script de actualización
        if (_shouldRestartForUpdate)
        {
            FirebaseUpdateService.ExecutePendingUpdate();
        }
    }
    
    /// <summary>
    /// Configura los servicios para Firebase
    /// </summary>
    private void ConfigureFirebaseServices(IServiceCollection services)
    {
        // Obtener configuración de Firebase
        var firebaseConfig = AppSettings.GetFirebaseConfig();
        var currentVersion = AppSettings.GetAppVersion();
        
        // Crear e inicializar Firebase
        _firebaseInitializer = new FirebaseInitializer(firebaseConfig);
        
        // ========== Registrar todos los servicios de Firebase ==========
        // Esto incluye:
        // - Autenticación Firebase (IAuthService, IFirebaseAuthService)
        // - Repositorios de catálogos (Departamento, Cargo, Actividad, Proyecto, TipoPermiso, Configuracion)
        // - Repositorios principales (Empleado, Usuario, Permiso, Vacacion, Contrato)
        // - Repositorios de registros (RegistroDiario, AuditLog)
        // - Firebase Storage (IFirebaseStorageService)
        // - Sistema de actualizaciones (IUpdateService, IFirebaseUpdateService)
        services.AddFullFirebaseSupport(firebaseConfig, _firebaseInitializer, currentVersion);
        
        // Registrar ViewModels
        RegisterViewModels(services);
    }
    
    /// <summary>
    /// Inicializa Firebase
    /// </summary>
    private async Task InitializeFirebaseAsync()
    {
        if (_firebaseInitializer == null) return;
        
        try
        {
            // Inicializar Firebase
            var initialized = await _firebaseInitializer.InitializeAsync();
            
            if (!initialized)
            {
                MessageBox.Show(
                    "No se pudo conectar con Firebase. Verifique la configuración y las credenciales.",
                    "Error de Conexión",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                
                Shutdown();
                return;
            }
            
            // Verificar conexión
            var connected = await _firebaseInitializer.TestConnectionAsync();
            if (!connected)
            {
                MessageBox.Show(
                    "No se pudo verificar la conexión con Firebase. Verifique su conexión a internet.",
                    "Error de Conexión",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                
                Shutdown();
                return;
            }
        }
        catch (Exception ex)
        {
            LogException("InitializeFirebase", ex);
            MessageBox.Show(
                $"Error al inicializar Firebase: {ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            
            Shutdown();
        }
    }
    
    /// <summary>
    /// Registra los ViewModels
    /// </summary>
    private void RegisterViewModels(IServiceCollection services)
    {
        services.AddTransient<LoginViewModel>();
        services.AddTransient<EmpleadosListViewModel>();
        services.AddTransient<EmpleadoFormViewModel>();
        services.AddTransient<EmpleadoDetailViewModel>();
        services.AddTransient<DepartamentosListViewModel>();
        services.AddTransient<CargosListViewModel>();
        services.AddTransient<ControlDiarioViewModel>();
        services.AddTransient<ProyectosListViewModel>();
        services.AddTransient<ActividadesListViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<ReportsViewModel>();
        services.AddTransient<DocumentsViewModel>();
        services.AddTransient<PermisosListViewModel>();
        services.AddTransient<PermisoFormViewModel>();
        services.AddTransient<BandejaAprobacionViewModel>();
        services.AddTransient<TiposPermisoListViewModel>();
        services.AddTransient<VacacionesViewModel>();
        services.AddTransient<ContratosViewModel>();
        services.AddTransient<ConfiguracionViewModel>();
        services.AddTransient<ConfiguracionEmpresaViewModel>();
        services.AddTransient<BackupViewModel>();
        services.AddTransient<AuditLogViewModel>();
        services.AddTransient<UsuariosListViewModel>();
        services.AddTransient<CambiarPasswordViewModel>();
        services.AddTransient<CatalogosViewModel>();
        services.AddTransient<DocumentosEmpleadoViewModel>();
        services.AddTransient<ChatViewModel>();
    }
    
    /// <summary>
    /// Verifica si hay actualizaciones disponibles
    /// </summary>
    private async Task CheckForUpdatesAsync()
    {
        // Verificar si las actualizaciones están habilitadas
        if (!AppSettings.UpdatesEnabled() || !AppSettings.CheckUpdatesOnStartup())
        {
            return;
        }
        
        try
        {
            _updateService = _serviceProvider!.GetService<IUpdateService>();
            if (_updateService == null) return;
            
            var result = await _updateService.CheckForUpdatesAsync();
            
            if (result.Success && result.UpdateAvailable && result.NewVersion != null)
            {
                // Mostrar diálogo de actualización
                var viewModel = new UpdateDialogViewModel(_updateService, result.NewVersion);
                var dialog = new UpdateDialog(viewModel);
                
                var dialogResult = dialog.ShowDialog();
                
                if (dialogResult == true && viewModel.DownloadCompleted)
                {
                    // Marcar para reinicio con actualización
                    _shouldRestartForUpdate = true;
                    
                    // Cerrar la aplicación para aplicar la actualización
                    Shutdown();
                }
            }
        }
        catch (Exception ex)
        {
            // No bloquear el inicio si falla la verificación de actualizaciones
            LogException("CheckForUpdates", ex);
        }
    }
    
    private Task ShowLoginAsync()
    {
        bool authenticated = false;
        
        while (!authenticated)
        {
            using var scope = _serviceProvider!.CreateScope();
            var loginViewModel = scope.ServiceProvider.GetRequiredService<LoginViewModel>();
            var loginWindow = new LoginWindow(loginViewModel);
            
            var result = loginWindow.ShowDialog();
            
            if (result == true && loginViewModel.AuthenticatedUser != null)
            {
                _currentUser = loginViewModel.AuthenticatedUser;
                CurrentUser = _currentUser;
                authenticated = true;
                
                // Mostrar ventana principal
                var shouldContinue = ShowMainWindow();
                
                if (!shouldContinue)
                {
                    // El usuario cerró sesión, volver a mostrar login
                    authenticated = false;
                    _currentUser = null;
                    CurrentUser = null;
                }
            }
            else
            {
                // Usuario cerró la ventana de login
                Shutdown();
                return Task.CompletedTask;
            }
        }
        
        return Task.CompletedTask;
    }
    
    private bool ShowMainWindow()
    {
        try
        {
            var mainViewModel = new MainViewModel(_currentUser!, _serviceProvider!);
            var mainWindow = new MainWindow(mainViewModel);
            
            // Establecer la ventana como la ventana principal de la aplicación
            Application.Current.MainWindow = mainWindow;
            
            var result = mainWindow.ShowDialog();
            
            // Si result es false, el usuario pidió cerrar sesión
            return result == true;
        }
        catch (Exception ex)
        {
            LogException("ShowMainWindow", ex);
            MessageBox.Show(
                $"Error al abrir la ventana principal:\n\n{ex.Message}\n\nDetalles: {ex.InnerException?.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }
    }
    
    /// <summary>
    /// Configura el manejo global de excepciones no controladas
    /// </summary>
    private void SetupGlobalExceptionHandling()
    {
        // Excepciones no controladas en el hilo principal
        DispatcherUnhandledException += (sender, e) =>
        {
            LogException("DispatcherUnhandledException", e.Exception);
            ShowErrorMessage(e.Exception);
            e.Handled = true; // Evita que la aplicación se cierre
        };
        
        // Excepciones no controladas en otros hilos
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            if (e.ExceptionObject is Exception ex)
            {
                LogException("AppDomain.UnhandledException", ex);
                Current.Dispatcher.Invoke(() => ShowErrorMessage(ex));
            }
        };
        
        // Excepciones en tareas asíncronas
        TaskScheduler.UnobservedTaskException += (sender, e) =>
        {
            LogException("TaskScheduler.UnobservedTaskException", e.Exception);
            e.SetObserved(); // Marca la excepción como observada
        };
    }
    
    /// <summary>
    /// Muestra un mensaje de error amigable al usuario
    /// </summary>
    private void ShowErrorMessage(Exception ex)
    {
        string userMessage = GetUserFriendlyMessage(ex);
        
        MessageBox.Show(
            userMessage,
            "Error en la aplicación",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }
    
    /// <summary>
    /// Convierte la excepción en un mensaje amigable para el usuario
    /// </summary>
    private string GetUserFriendlyMessage(Exception ex)
    {
        return ex switch
        {
            IOException ioEx =>
                "Error al acceder al sistema de archivos. Verifique permisos y espacio disponible.",
            
            UnauthorizedAccessException =>
                "No tiene permisos para realizar esta operación.",
            
            InvalidOperationException when ex.Message.Contains("disposed") =>
                "Error interno: componente no disponible. Intente nuevamente.",
            
            Grpc.Core.RpcException rpcEx =>
                $"Error de conexión con Firebase: {rpcEx.Status.Detail}. Verifique su conexión a internet.",
            
            _ => $"Ha ocurrido un error inesperado.\n\nDetalles técnicos: {ex.Message}\n\nSi el problema persiste, contacte al administrador del sistema."
        };
    }
    
    /// <summary>
    /// Registra la excepción en un archivo de log
    /// </summary>
    private void LogException(string source, Exception ex)
    {
        try
        {
            var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "logs");
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }
            
            var logFile = Path.Combine(logPath, $"error_{DateTime.Now:yyyy-MM-dd}.log");
            var logEntry = $"""
                ================================================================================
                [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {source}
                --------------------------------------------------------------------------------
                Mensaje: {ex.Message}
                Tipo: {ex.GetType().FullName}
                StackTrace:
                {ex.StackTrace}
                
                """;
            
            if (ex.InnerException != null)
            {
                logEntry += $"""
                Inner Exception:
                Mensaje: {ex.InnerException.Message}
                Tipo: {ex.InnerException.GetType().FullName}
                StackTrace:
                {ex.InnerException.StackTrace}
                
                """;
            }
            
            File.AppendAllText(logFile, logEntry);
        }
        catch
        {
            // No hacer nada si falla el logging
        }
    }
}

