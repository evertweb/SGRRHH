using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Google.Cloud.Firestore;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;
using System.Windows;
using System.Windows.Interop;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// ViewModel para el Wizard de configuración de Windows Hello
/// </summary>
public partial class WindowsHelloWizardViewModel : ViewModelBase
{
    private readonly IWindowsHelloService _windowsHelloService;
    private readonly IFirebaseAuthService _firebaseAuthService;
    private readonly FirestoreDb _firestore;

    // ===== Propiedades de Navegación =====
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsStep1))]
    [NotifyPropertyChangedFor(nameof(IsStep2))]
    [NotifyPropertyChangedFor(nameof(IsStep3))]
    [NotifyPropertyChangedFor(nameof(CanGoBack))]
    [NotifyPropertyChangedFor(nameof(ShowNextButton))]
    [NotifyPropertyChangedFor(nameof(ShowFinishButton))]
    private int _currentStep = 1;

    public bool IsStep1 => CurrentStep == 1;
    public bool IsStep2 => CurrentStep == 2;
    public bool IsStep3 => CurrentStep == 3;
    public bool CanGoBack => CurrentStep > 1 && !IsRegistering && !RegistrationSuccess;
    public bool ShowNextButton => CurrentStep < 3 && !IsNotCompatible;
    public bool ShowFinishButton => CurrentStep == 3 && !RegistrationSuccess;

    // ===== Propiedades de Verificación (Paso 2) =====

    [ObservableProperty]
    private bool _isVerifying;

    [ObservableProperty]
    private bool _isWindowsHelloAvailable;

    [ObservableProperty]
    private string _windowsHelloStatus = "Verificando...";

    [ObservableProperty]
    private string _windowsHelloCheckIcon = "⏳";

    [ObservableProperty]
    private string _deviceName = Environment.MachineName;

    [ObservableProperty]
    private string _detectedMethod = "Verificando...";

    [ObservableProperty]
    private string _methodCheckIcon = "⏳";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowNextButton))]
    private bool _isNotCompatible;

    [ObservableProperty]
    private string _compatibilityErrorMessage = string.Empty;

    // ===== Propiedades de Registro (Paso 3) =====

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsRegistrationPending))]
    [NotifyPropertyChangedFor(nameof(CanFinish))]
    [NotifyPropertyChangedFor(nameof(CanGoBack))]
    private bool _isRegistering;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsRegistrationPending))]
    [NotifyPropertyChangedFor(nameof(CanFinish))]
    [NotifyPropertyChangedFor(nameof(CanGoBack))]
    [NotifyPropertyChangedFor(nameof(ShowFinishButton))]
    private bool _registrationSuccess;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsRegistrationPending))]
    private bool _hasRegistrationError;

    [ObservableProperty]
    private string _registrationErrorMessage = string.Empty;

    public bool IsRegistrationPending => !IsRegistering && !RegistrationSuccess && !HasRegistrationError;
    public bool CanFinish => !IsRegistering && !RegistrationSuccess;

    /// <summary>
    /// Usuario actual para el cual configurar Windows Hello
    /// </summary>
    public Usuario? CurrentUser { get; set; }

    /// <summary>
    /// Evento para solicitar el cierre del wizard
    /// </summary>
    public event EventHandler<bool>? RequestClose;

    public WindowsHelloWizardViewModel(
        IWindowsHelloService windowsHelloService,
        IFirebaseAuthService firebaseAuthService,
        FirestoreDb firestore)
    {
        _windowsHelloService = windowsHelloService;
        _firebaseAuthService = firebaseAuthService;
        _firestore = firestore;
    }

    /// <summary>
    /// Inicializa el wizard
    /// </summary>
    public Task InitializeAsync()
    {
        CurrentStep = 1;
        RegistrationSuccess = false;
        HasRegistrationError = false;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Avanza al siguiente paso
    /// </summary>
    [RelayCommand]
    private async Task NextAsync()
    {
        if (CurrentStep >= 3) return;

        CurrentStep++;

        // Si llegamos al paso 2, verificar compatibilidad
        if (CurrentStep == 2)
        {
            await VerifyCompatibilityAsync();
        }
    }

    /// <summary>
    /// Retrocede al paso anterior
    /// </summary>
    [RelayCommand]
    private void Previous()
    {
        if (CurrentStep > 1 && !IsRegistering)
        {
            CurrentStep--;
        }
    }

    /// <summary>
    /// Finaliza el wizard realizando el registro
    /// </summary>
    [RelayCommand]
    private async Task FinishAsync()
    {
        await PerformRegistrationAsync();
    }

    /// <summary>
    /// Reintenta el registro después de un error
    /// </summary>
    [RelayCommand]
    private async Task RetryRegistrationAsync()
    {
        HasRegistrationError = false;
        RegistrationErrorMessage = string.Empty;
        await PerformRegistrationAsync();
    }

    /// <summary>
    /// Cierra el wizard después del éxito
    /// </summary>
    [RelayCommand]
    private void Close()
    {
        RequestClose?.Invoke(this, RegistrationSuccess);
    }

    /// <summary>
    /// Cancela el wizard
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        RequestClose?.Invoke(this, false);
    }

    /// <summary>
    /// Verifica la compatibilidad del dispositivo con Windows Hello
    /// </summary>
    private async Task VerifyCompatibilityAsync()
    {
        IsVerifying = true;
        IsNotCompatible = false;

        try
        {
            // Simular pequeño delay para UX
            await Task.Delay(500);

            // Verificar Windows Hello
            IsWindowsHelloAvailable = await _windowsHelloService.IsAvailableAsync();
            var statusMessage = await _windowsHelloService.GetAvailabilityMessageAsync();

            if (IsWindowsHelloAvailable)
            {
                WindowsHelloCheckIcon = "✓";
                WindowsHelloStatus = "Disponible";
                MethodCheckIcon = "✓";
                DetectedMethod = "Biometría o PIN detectado";
            }
            else
            {
                WindowsHelloCheckIcon = "✗";
                WindowsHelloStatus = "No disponible";
                MethodCheckIcon = "✗";
                DetectedMethod = statusMessage;

                IsNotCompatible = true;
                CompatibilityErrorMessage = $"Windows Hello no está disponible en este dispositivo.\n\n{statusMessage}\n\nPara usar esta función, configure Windows Hello desde la Configuración de Windows.";
            }
        }
        catch (Exception ex)
        {
            WindowsHelloCheckIcon = "✗";
            WindowsHelloStatus = "Error al verificar";
            MethodCheckIcon = "✗";
            DetectedMethod = "No se pudo detectar";

            IsNotCompatible = true;
            CompatibilityErrorMessage = $"Error al verificar compatibilidad: {ex.Message}";
        }
        finally
        {
            IsVerifying = false;
        }
    }

    /// <summary>
    /// Realiza el registro de Windows Hello
    /// </summary>
    private async Task PerformRegistrationAsync()
    {
        if (CurrentUser == null)
        {
            HasRegistrationError = true;
            RegistrationErrorMessage = "No hay usuario autenticado";
            return;
        }

        try
        {
            IsRegistering = true;
            HasRegistrationError = false;
            RegistrationErrorMessage = string.Empty;

            // Obtener el handle de la ventana
            var window = Application.Current.Windows.OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this);

            IntPtr hwnd = IntPtr.Zero;
            if (window != null)
            {
                var helper = new WindowInteropHelper(window);
                hwnd = helper.Handle;
            }

            var fullUsername = CurrentUser.Email ?? CurrentUser.Username;

            // Registrar la passkey
            var result = await _windowsHelloService.RegisterPasskeyAsync(
                hwnd,
                fullUsername,
                CurrentUser.FirebaseUid ?? string.Empty);

            if (result.Success)
            {
                // Guardar preferencia de que se mostró el prompt
                await SaveWindowsHelloPreferenceAsync(CurrentUser.FirebaseUid!, "promptShown", true);

                RegistrationSuccess = true;
            }
            else
            {
                HasRegistrationError = true;
                RegistrationErrorMessage = result.Message ?? "Error desconocido al registrar Windows Hello";
            }
        }
        catch (Exception ex)
        {
            HasRegistrationError = true;
            RegistrationErrorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsRegistering = false;
        }
    }

    /// <summary>
    /// Guarda una preferencia de Windows Hello en Firestore
    /// </summary>
    private async Task SaveWindowsHelloPreferenceAsync(string firebaseUid, string key, bool value)
    {
        try
        {
            var docRef = _firestore
                .Collection("users")
                .Document(firebaseUid)
                .Collection("preferences")
                .Document("windowsHello");

            await docRef.SetAsync(new Dictionary<string, object>
            {
                [key] = value,
                ["updatedAt"] = FieldValue.ServerTimestamp
            }, SetOptions.MergeAll);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al guardar preferencia en Firestore: {ex.Message}");
        }
    }
}
