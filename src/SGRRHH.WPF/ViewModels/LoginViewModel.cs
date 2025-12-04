using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;
using System.Windows;
using System.Windows.Interop;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// ViewModel para la ventana de Login
/// </summary>
public partial class LoginViewModel : ViewModelBase
{
    private readonly IAuthService _authService;
    private readonly IWindowsHelloService? _windowsHelloService;

    [ObservableProperty]
    private string _username = string.Empty;
    
    [ObservableProperty]
    private string _password = string.Empty;
    
    [ObservableProperty]
    private string _errorMessage = string.Empty;
    
    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isWindowsHelloAvailable;

    [ObservableProperty]
    private string _windowsHelloStatusMessage = string.Empty;

    /// <summary>
    /// Indica si hay una passkey detectada localmente (login automático posible)
    /// </summary>
    [ObservableProperty]
    private bool _hasDetectedPasskey;

    /// <summary>
    /// Usuario detectado con passkey local
    /// </summary>
    [ObservableProperty]
    private string _detectedPasskeyUsername = string.Empty;

    /// <summary>
    /// CredentialId de la passkey detectada
    /// </summary>
    private string _detectedCredentialId = string.Empty;

    /// <summary>
    /// Indica si el usuario quiere mostrar el formulario de email/contraseña
    /// </summary>
    [ObservableProperty]
    private bool _showEmailLogin;

    /// <summary>
    /// Usuario autenticado después de un login exitoso
    /// </summary>
    public Usuario? AuthenticatedUser { get; private set; }

    /// <summary>
    /// Método de autenticación usado en el último login exitoso
    /// </summary>
    public string LastAuthMethod { get; private set; } = "password";

    /// <summary>
    /// Evento que se dispara cuando el login es exitoso
    /// </summary>
    public event EventHandler? LoginSuccessful;

    public LoginViewModel(IAuthService authService, IWindowsHelloService? windowsHelloService = null)
    {
        _authService = authService;
        _windowsHelloService = windowsHelloService;

        // Verificar disponibilidad de Windows Hello al iniciar
        _ = CheckWindowsHelloAvailabilityAsync();
    }
    
    [RelayCommand]
    private async Task LoginAsync()
    {
        // Limpiar errores previos
        HasError = false;
        ErrorMessage = string.Empty;
        
        // Validaciones básicas
        if (string.IsNullOrWhiteSpace(Username))
        {
            HasError = true;
            ErrorMessage = "Ingrese el nombre de usuario";
            return;
        }
        
        if (string.IsNullOrWhiteSpace(Password))
        {
            HasError = true;
            ErrorMessage = "Ingrese la contraseña";
            return;
        }
        
        try
        {
            IsLoading = true;
            
            // Usar username con dominio auto-completado
            var fullUsername = GetFullUsername();
            var result = await _authService.AuthenticateAsync(fullUsername, Password);
            
            if (result.Success)
            {
                AuthenticatedUser = result.Usuario;
                LastAuthMethod = "password";
                LoginSuccessful?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                HasError = true;
                ErrorMessage = result.Message;
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Error al conectar: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    /// <summary>
    /// Obtiene el username completo con dominio @sgrrhh.local si no está incluido
    /// </summary>
    private string GetFullUsername()
    {
        if (string.IsNullOrWhiteSpace(Username))
            return string.Empty;

        // Si ya contiene @, usarlo tal cual (permite dominios personalizados)
        if (Username.Contains("@"))
            return Username.Trim();

        // Auto-completar con dominio predeterminado
        return $"{Username.Trim()}@sgrrhh.local";
    }

    /// <summary>
    /// Verifica si Windows Hello está disponible en el dispositivo
    /// </summary>
    private async Task CheckWindowsHelloAvailabilityAsync()
    {
        if (_windowsHelloService == null)
        {
            IsWindowsHelloAvailable = false;
            WindowsHelloStatusMessage = "Windows Hello no está configurado";
            ShowEmailLogin = true;
            return;
        }

        try
        {
            IsWindowsHelloAvailable = await _windowsHelloService.IsAvailableAsync();
            WindowsHelloStatusMessage = await _windowsHelloService.GetAvailabilityMessageAsync();

            if (IsWindowsHelloAvailable)
            {
                // Buscar passkeys registradas localmente
                var localPasskeys = await _windowsHelloService.GetAllLocalPasskeysAsync();
                
                if (localPasskeys.Count > 0)
                {
                    // Usar la primera passkey encontrada (o la más reciente si tenemos múltiples)
                    var passkey = localPasskeys[0];
                    HasDetectedPasskey = true;
                    DetectedPasskeyUsername = passkey.Username;
                    _detectedCredentialId = passkey.CredentialId;
                    ShowEmailLogin = false; // Ocultar login de email por defecto
                    
                    // También establecer el username para compatibilidad
                    Username = passkey.Username;
                }
                else
                {
                    // No hay passkey registrada, mostrar login normal
                    HasDetectedPasskey = false;
                    ShowEmailLogin = true;
                    
                    // Intentar cargar el último usuario
                    var lastUsername = await _windowsHelloService.GetLastPasskeyUsernameAsync();
                    if (!string.IsNullOrEmpty(lastUsername))
                    {
                        Username = lastUsername;
                    }
                }
            }
            else
            {
                HasDetectedPasskey = false;
                ShowEmailLogin = true;
            }
        }
        catch (Exception ex)
        {
            IsWindowsHelloAvailable = false;
            HasDetectedPasskey = false;
            ShowEmailLogin = true;
            WindowsHelloStatusMessage = $"Error al verificar Windows Hello: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task LoginWithWindowsHelloAsync()
    {
        // Limpiar errores previos
        HasError = false;
        ErrorMessage = string.Empty;

        // Validar que el servicio esté disponible
        if (_windowsHelloService == null)
        {
            HasError = true;
            ErrorMessage = "El servicio de Windows Hello no está disponible";
            return;
        }

        try
        {
            IsLoading = true;

            string fullUsername;
            string storedCredentialId;

            // Si tenemos una passkey detectada automáticamente, usarla
            if (HasDetectedPasskey && !string.IsNullOrEmpty(DetectedPasskeyUsername) && !string.IsNullOrEmpty(_detectedCredentialId))
            {
                fullUsername = DetectedPasskeyUsername;
                storedCredentialId = _detectedCredentialId;
            }
            else
            {
                // Fallback: usar el username ingresado manualmente
                if (string.IsNullOrWhiteSpace(Username))
                {
                    HasError = true;
                    ErrorMessage = "Ingrese el nombre de usuario para continuar";
                    return;
                }

                fullUsername = GetFullUsername();

                // Verificar si el usuario tiene una passkey registrada
                storedCredentialId = await _windowsHelloService.GetStoredCredentialIdAsync(fullUsername) ?? "";
                if (string.IsNullOrEmpty(storedCredentialId))
                {
                    HasError = true;
                    ErrorMessage = "No tiene Windows Hello configurado para este usuario. Inicie sesión con contraseña primero.";
                    return;
                }
            }

            // Obtener el handle de la ventana actual
            var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.DataContext == this);
            IntPtr hwnd = IntPtr.Zero;
            if (window != null)
            {
                var helper = new WindowInteropHelper(window);
                hwnd = helper.Handle;
            }

            // Solicitar verificación de Windows Hello
            var verifyResult = await _windowsHelloService.VerifyAsync(hwnd, fullUsername);

            if (!verifyResult.Success)
            {
                HasError = true;
                ErrorMessage = verifyResult.Message ?? "Verificación de Windows Hello fallida";
                return;
            }

            // Autenticar con Firebase usando la passkey
            if (_authService is IFirebaseAuthService firebaseAuthService)
            {
                var authResult = await firebaseAuthService.AuthenticateWithPasskeyAsync(fullUsername, storedCredentialId);

                if (authResult.Success && authResult.Usuario != null)
                {
                    AuthenticatedUser = authResult.Usuario;
                    LastAuthMethod = "windows_hello";
                    LoginSuccessful?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    HasError = true;
                    ErrorMessage = authResult.Message ?? "Error al autenticar con Windows Hello";
                }
            }
            else
            {
                HasError = true;
                ErrorMessage = "El servicio de autenticación no soporta Windows Hello";
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Error al iniciar sesión: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Exit()
    {
        Application.Current.Shutdown();
    }

    [RelayCommand]
    private void ToggleEmailLogin()
    {
        ShowEmailLogin = !ShowEmailLogin;
    }

    [RelayCommand]
    private void UsePasskeyLogin()
    {
        ShowEmailLogin = false;
        HasError = false;
        ErrorMessage = string.Empty;
    }
}
