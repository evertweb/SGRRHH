using System.IO;
using System.Windows;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;
using SGRRHH.Core.Models;
using SGRRHH.Infrastructure.Firebase;
using SGRRHH.Infrastructure.Services;
using SGRRHH.WPF.Helpers;
using SGRRHH.WPF.Services;
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
        
        // ========== INICIALIZAR RUTAS DE DATOS ==========
        // Esto crea todos los directorios necesarios en %LOCALAPPDATA%\SGRRHH
        // ANTES de que cualquier servicio intente usarlos
        SGRRHH.Core.Common.AppDataPaths.Initialize();
        
        // Configurar manejo global de excepciones
        SetupGlobalExceptionHandling();
        
        // Configurar servicios
        var services = new ServiceCollection();
        ConfigureFirebaseServices(services);
        ConfigureSendbirdServices(services);

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

        // Dar tiempo para que los recursos se liberen completamente
        System.Threading.Thread.Sleep(500);
    }
    
    /// <summary>
    /// Configura los servicios para Firebase
    /// </summary>
    private void ConfigureFirebaseServices(IServiceCollection services)
    {
        // Registrar IConfiguration para acceder a appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        services.AddSingleton<IConfiguration>(configuration);
        
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
        
        // Sobrescribir el servicio de actualización con la implementación de GitHub
        services.AddSingleton<IUpdateService>(sp =>
            new GithubUpdateService(currentVersion, sp.GetService<Microsoft.Extensions.Logging.ILogger<GithubUpdateService>>()));

        // Registrar servicio de Windows Hello
        services.AddSingleton<IWindowsHelloService, WindowsHelloService>();

        // Registrar ViewModels
        RegisterViewModels(services);
        
        // Registrar servicio de SMS/MFA
        ConfigureSmsService(services);
    }

    /// <summary>
    /// Configura los servicios para Sendbird
    /// </summary>
    private void ConfigureSendbirdServices(IServiceCollection services)
    {
        // Leer configuración de Sendbird desde appsettings.json
        var sendbirdSettings = AppSettings.GetSendbirdSettings();

        // Configurar options para SendbirdSettings
        services.Configure<SendbirdSettings>(options =>
        {
            options.Enabled = sendbirdSettings.Enabled;
            options.ApplicationId = sendbirdSettings.ApplicationId;
            options.ApiToken = sendbirdSettings.ApiToken;
            options.Region = sendbirdSettings.Region;
        });

        // Registrar el servicio de Sendbird
        services.AddSingleton<ISendbirdChatService, SendbirdChatService>();

        // Nota: El registro del ViewModel se hace en RegisterViewModels
    }
    
    /// <summary>
    /// Configura el servicio de verificación SMS para MFA
    /// </summary>
    private void ConfigureSmsService(IServiceCollection services)
    {
        var twilioSettings = AppSettings.GetTwilioSettings();
        var mfaSettings = AppSettings.GetMfaSettings();
        
        // Solo registrar si MFA está habilitado y Twilio está configurado
        if (mfaSettings.Enabled && twilioSettings.Enabled && 
            !string.IsNullOrEmpty(twilioSettings.AccountSid) &&
            !twilioSettings.AccountSid.StartsWith("YOUR_"))
        {
            services.AddSingleton<ISmsVerificationService>(sp => 
                new TwilioSmsVerificationService(
                    twilioSettings.AccountSid,
                    twilioSettings.AuthToken,
                    twilioSettings.VerifyServiceSid,
                    sp.GetService<Microsoft.Extensions.Logging.ILogger<TwilioSmsVerificationService>>()));
            
            System.Diagnostics.Debug.WriteLine("Servicio SMS (Twilio) configurado para MFA");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("MFA deshabilitado o Twilio no configurado");
        }
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
        // Registrar DialogService como singleton
        services.AddSingleton<IDialogService, DialogService>();
        
        services.AddTransient<LoginViewModel>();
        services.AddTransient<EmpleadosListViewModel>();
        services.AddTransient<EmpleadoFormViewModel>();
        services.AddTransient<EmpleadoDetailViewModel>();
        services.AddTransient<DepartamentosListViewModel>();
        services.AddTransient<CargosListViewModel>();
        services.AddTransient<ControlDiarioViewModel>();
        services.AddTransient<DailyActivityWizardViewModel>();
        services.AddTransient<ProyectosListViewModel>();
        services.AddTransient<ActividadesListViewModel>();
        services.AddTransient<ActividadFormViewModel>();
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
        services.AddTransient<SeguridadViewModel>();
        services.AddTransient<UsuariosListViewModel>();
        services.AddTransient<CambiarPasswordViewModel>();
        services.AddTransient<CatalogosViewModel>();
        services.AddTransient<DocumentosEmpleadoViewModel>();
        services.AddTransient<ChatViewModel>();
        services.AddTransient<WindowsHelloSetupDialogViewModel>();
        services.AddTransient<WindowsHelloWizardViewModel>();
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

                // Si el usuario completó la descarga y eligió instalar
                if (dialogResult == true && viewModel.DownloadCompleted)
                {
                    // El updater ya fue lanzado desde InstallAndRestartCommand
                    // Solo cerramos la aplicación
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
    
    private async Task ShowLoginAsync()
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
                var user = loginViewModel.AuthenticatedUser;
                
                // ===== VERIFICACIÓN MFA =====
                var mfaSettings = AppSettings.GetMfaSettings();
                
                if (mfaSettings.Enabled && !string.IsNullOrEmpty(user.PhoneNumber))
                {
                    var mfaPassed = await PerformMfaVerificationAsync(user);
                    
                    if (!mfaPassed)
                    {
                        // MFA falló o fue cancelado, volver a login
                        continue;
                    }
                }
                
                _currentUser = user;
                CurrentUser = _currentUser;
                authenticated = true;
                
                // ===== DIÁLOGO WINDOWS HELLO POST-LOGIN =====
                // Solo mostrar si el login fue con contraseña (no con Windows Hello)
                if (loginViewModel.LastAuthMethod == "password")
                {
                    await ShowWindowsHelloPromptIfNeededAsync(user);
                }
                
                // Conectar a Sendbird automáticamente después del login
                await ConnectToSendbirdAsync();
                
                // Mostrar ventana principal
                var shouldContinue = ShowMainWindow();
                
                if (!shouldContinue)
                {
                    // El usuario cerró sesión, desconectar de Sendbird y volver a mostrar login
                    await DisconnectFromSendbirdAsync();
                    authenticated = false;
                    _currentUser = null;
                    CurrentUser = null;
                }
            }
            else
            {
                // Usuario cerró la ventana de login
                Shutdown();
                return;
            }
        }
    }

    /// <summary>
    /// Muestra el diálogo de configuración de Windows Hello si corresponde
    /// </summary>
    private async Task ShowWindowsHelloPromptIfNeededAsync(Usuario usuario)
    {
        try
        {
            var shouldShowPrompt = await ShouldShowWindowsHelloPromptAsync(usuario);

            if (!shouldShowPrompt) return;

            // Crear y mostrar el diálogo
            var dialogViewModel = _serviceProvider?.GetService<WindowsHelloSetupDialogViewModel>();
            if (dialogViewModel == null) return;

            dialogViewModel.CurrentUser = usuario;

            var setupDialog = new WindowsHelloSetupDialog
            {
                DataContext = dialogViewModel
            };

            // Mostrar como diálogo modal
            setupDialog.ShowDialog();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al mostrar prompt de Windows Hello: {ex.Message}");
            // No bloquear el flujo si falla el prompt
        }
    }

    /// <summary>
    /// Determina si se debe mostrar el prompt de configuración de Windows Hello
    /// </summary>
    private async Task<bool> ShouldShowWindowsHelloPromptAsync(Usuario usuario)
    {
        try
        {
            // 1. Verificar si Windows Hello está disponible
            var windowsHelloService = Services?.GetService<IWindowsHelloService>();
            if (windowsHelloService == null || !await windowsHelloService.IsAvailableAsync())
                return false;

            // 2. Verificar si el usuario ya tiene passkey registrada
            var firebaseAuthService = Services?.GetService<IFirebaseAuthService>();
            if (firebaseAuthService == null || string.IsNullOrEmpty(usuario.FirebaseUid))
                return false;

            var passkeys = await firebaseAuthService.GetUserPasskeysAsync(usuario.FirebaseUid);
            if (passkeys.Any())
                return false;

            // 3. Verificar si el usuario ya dijo "No Preguntar"
            var dismissed = await CheckWindowsHelloPromptDismissedAsync(usuario.FirebaseUid);
            return !dismissed;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al verificar prompt de Windows Hello: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Verifica si el usuario ya descartó el prompt de Windows Hello
    /// </summary>
    private async Task<bool> CheckWindowsHelloPromptDismissedAsync(string firebaseUid)
    {
        try
        {
            var firestore = Services?.GetService<FirestoreDb>();
            if (firestore == null) return false;

            var docRef = firestore
                .Collection("users")
                .Document(firebaseUid)
                .Collection("preferences")
                .Document("windowsHello");

            var snapshot = await docRef.GetSnapshotAsync();
            if (!snapshot.Exists) return false;

            // Verificar si la propiedad "dismissed" existe y es true
            if (snapshot.TryGetValue<bool>("dismissed", out var dismissed))
            {
                return dismissed;
            }

            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al verificar preferencia de Windows Hello: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Conecta al usuario actual a Sendbird para mantener el estado online
    /// </summary>
    private async Task ConnectToSendbirdAsync()
    {
        if (_currentUser == null || _serviceProvider == null) return;
        
        try
        {
            var chatProvider = Helpers.AppSettings.GetChatProvider();
            if (chatProvider != "Sendbird") return;
            
            var sendbirdService = _serviceProvider.GetService<ISendbirdChatService>();
            if (sendbirdService != null)
            {
                await sendbirdService.ConnectAsync(_currentUser);
                System.Diagnostics.Debug.WriteLine($"Usuario {_currentUser.Username} conectado a Sendbird al iniciar sesión");
            }
        }
        catch (Exception ex)
        {
            // No bloquear el inicio si falla la conexión a Sendbird
            System.Diagnostics.Debug.WriteLine($"Error al conectar a Sendbird: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Realiza la verificación MFA por SMS
    /// </summary>
    private async Task<bool> PerformMfaVerificationAsync(Usuario user)
    {
        if (_serviceProvider == null || string.IsNullOrEmpty(user.PhoneNumber))
            return true; // Si no hay teléfono, permitir sin MFA
        
        try
        {
            var smsService = _serviceProvider.GetService<ISmsVerificationService>();
            
            if (smsService == null)
            {
                System.Diagnostics.Debug.WriteLine("Servicio SMS no configurado, saltando MFA");
                return true;
            }
            
            // Verificar si el dispositivo es confiable
            var userId = user.FirebaseUid ?? user.Id.ToString();
            var isTrusted = await smsService.IsDeviceTrustedAsync(userId);
            
            if (isTrusted)
            {
                System.Diagnostics.Debug.WriteLine($"Dispositivo confiable para {user.Username}, saltando MFA");
                return true;
            }
            
            // Mostrar ventana de verificación SMS
            var smsWindow = new SmsVerificationWindow(smsService, user.PhoneNumber, userId)
            {
                Owner = null
            };
            
            // Enviar código SMS
            var sent = await smsWindow.SendCodeAsync();
            
            if (!sent)
            {
                MessageBox.Show(
                    "No se pudo enviar el código de verificación. Intente nuevamente.",
                    "Error SMS",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
            
            // Mostrar diálogo y esperar verificación
            var dialogResult = smsWindow.ShowDialog();
            
            return dialogResult == true && smsWindow.IsVerified;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en MFA: {ex.Message}");
            
            // En caso de error, mostrar mensaje pero permitir continuar
            var result = MessageBox.Show(
                "Error al verificar SMS. ¿Desea continuar sin verificación?",
                "Error MFA",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            
            return result == MessageBoxResult.Yes;
        }
    }
    
    /// <summary>
    /// Desconecta al usuario de Sendbird al cerrar sesión
    /// </summary>
    private async Task DisconnectFromSendbirdAsync()
    {
        if (_serviceProvider == null) return;
        
        try
        {
            var sendbirdService = _serviceProvider.GetService<ISendbirdChatService>();
            if (sendbirdService != null)
            {
                await sendbirdService.DisconnectAsync();
                System.Diagnostics.Debug.WriteLine("Usuario desconectado de Sendbird");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al desconectar de Sendbird: {ex.Message}");
        }
    }
    
    private bool ShowMainWindow()
    {
        try
        {
            var dialogService = _serviceProvider!.GetRequiredService<IDialogService>();
            var mainViewModel = new MainViewModel(_currentUser!, _serviceProvider!, dialogService);
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
            var logFile = Helpers.DataPaths.GetLogFilePath("error");
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

