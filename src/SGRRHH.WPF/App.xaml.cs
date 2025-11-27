using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;
using SGRRHH.Infrastructure.Data;
using SGRRHH.Infrastructure.Repositories;
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
        
        // Configurar servicios
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
        Services = _serviceProvider;
        
        // Configurar proveedor de usuario actual para el servicio de auditoría
        AuditService.ConfigureCurrentUserProvider(() => CurrentUser);
        
        // Inicializar base de datos
        await InitializeDatabaseAsync();
        
        // Mostrar login
        await ShowLoginAsync();
    }
    
    private void ConfigureServices(IServiceCollection services)
    {
        // Obtener ruta de la base de datos desde configuración
        var dbPath = AppSettings.GetDatabasePath();
        var dbDirectory = Path.GetDirectoryName(dbPath);
        
        // Crear directorio si no existe
        if (!string.IsNullOrEmpty(dbDirectory) && !Directory.Exists(dbDirectory))
        {
            Directory.CreateDirectory(dbDirectory);
        }
        
        // Obtener cadena de conexión optimizada
        var connectionString = AppSettings.GetConnectionString();
        
        // Configurar DbContext con opciones optimizadas para concurrencia
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlite(connectionString);
        });
        
        // Registrar repositorios
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IEmpleadoRepository, EmpleadoRepository>();
        services.AddScoped<IDepartamentoRepository, DepartamentoRepository>();
        services.AddScoped<ICargoRepository, CargoRepository>();
        services.AddScoped<IProyectoRepository, ProyectoRepository>();
        services.AddScoped<IActividadRepository, ActividadRepository>();
        services.AddScoped<IRegistroDiarioRepository, RegistroDiarioRepository>();
        services.AddScoped<IPermisoRepository, PermisoRepository>();
        services.AddScoped<ITipoPermisoRepository, TipoPermisoRepository>();
        services.AddScoped<IVacacionRepository, VacacionRepository>();
        services.AddScoped<IContratoRepository, ContratoRepository>();
        services.AddScoped<IConfiguracionRepository, ConfiguracionRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        
        // Registrar servicios
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmpleadoService, EmpleadoService>();
        services.AddScoped<IDepartamentoService, DepartamentoService>();
        services.AddScoped<ICargoService, CargoService>();
        services.AddScoped<IProyectoService, ProyectoService>();
        services.AddScoped<IActividadService, ActividadService>();
        services.AddScoped<IControlDiarioService, ControlDiarioService>();
        services.AddScoped<IPermisoService, PermisoService>();
        services.AddScoped<ITipoPermisoService, TipoPermisoService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IVacacionService, VacacionService>();
        services.AddScoped<IContratoService, ContratoService>();
        services.AddScoped<IConfiguracionService, ConfiguracionService>();
        services.AddScoped<IBackupService, BackupService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        
        // Registrar ViewModels
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
    }
    
    private async Task InitializeDatabaseAsync()
    {
        using var scope = _serviceProvider!.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        // Inicializar base de datos
        await DatabaseInitializer.InitializeAsync(context);
        
        // Habilitar modo WAL para mejor concurrencia en red
        if (AppSettings.EnableWalMode())
        {
            try
            {
                var busyTimeout = AppSettings.GetBusyTimeout();
                await context.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;");
                // Usar parámetro numérico directamente (no es SQL injection porque es un int)
                #pragma warning disable EF1002
                await context.Database.ExecuteSqlRawAsync($"PRAGMA busy_timeout={busyTimeout};");
                #pragma warning restore EF1002
                // Sincronización normal para balance entre seguridad y rendimiento
                await context.Database.ExecuteSqlRawAsync("PRAGMA synchronous=NORMAL;");
                // Cache compartido para mejor concurrencia
                await context.Database.ExecuteSqlRawAsync("PRAGMA cache_size=-64000;"); // 64MB cache
            }
            catch (Exception ex)
            {
                LogException("WAL Mode Configuration", ex);
            }
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
            Microsoft.Data.Sqlite.SqliteException sqliteEx =>
                $"Error de base de datos: {GetSqliteErrorMessage(sqliteEx)}",
            
            DbUpdateException dbEx =>
                "Error al guardar en la base de datos. Verifique los datos ingresados.",
            
            IOException ioEx =>
                "Error al acceder al sistema de archivos. Verifique permisos y espacio disponible.",
            
            UnauthorizedAccessException =>
                "No tiene permisos para realizar esta operación.",
            
            InvalidOperationException when ex.Message.Contains("disposed") =>
                "Error interno: componente no disponible. Intente nuevamente.",
            
            _ => $"Ha ocurrido un error inesperado.\n\nDetalles técnicos: {ex.Message}\n\nSi el problema persiste, contacte al administrador del sistema."
        };
    }
    
    /// <summary>
    /// Obtiene mensaje amigable para errores de SQLite
    /// </summary>
    private string GetSqliteErrorMessage(Microsoft.Data.Sqlite.SqliteException ex)
    {
        return ex.SqliteErrorCode switch
        {
            5 => "La base de datos está bloqueada por otro usuario. Espere un momento e intente nuevamente.",
            6 => "La tabla está bloqueada. Otro usuario está realizando una operación. Intente de nuevo.",
            19 => "El registro no puede guardarse porque viola una restricción de unicidad.",
            1 => "Error de sintaxis en la base de datos.",
            11 => "La base de datos está corrupta. Considere restaurar un backup.",
            14 => "No se puede abrir la base de datos. Verifique que el archivo existe y tiene permisos de red.",
            _ => ex.Message
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

