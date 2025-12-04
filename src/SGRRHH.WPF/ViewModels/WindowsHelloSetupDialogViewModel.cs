using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Google.Cloud.Firestore;
using Microsoft.Extensions.DependencyInjection;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;
using SGRRHH.WPF.Views;
using System.Windows;
using System.Windows.Interop;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// ViewModel para el diálogo de configuración de Windows Hello post-login
/// </summary>
public partial class WindowsHelloSetupDialogViewModel : ViewModelBase
{
    private readonly IWindowsHelloService _windowsHelloService;
    private readonly IFirebaseAuthService _firebaseAuthService;
    private readonly FirestoreDb _firestore;

    [ObservableProperty]
    private bool _isWindowsHelloAvailable;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    /// <summary>
    /// Usuario actual para el cual configurar Windows Hello
    /// </summary>
    public Usuario? CurrentUser { get; set; }

    /// <summary>
    /// Evento para solicitar el cierre del diálogo
    /// </summary>
    public event EventHandler<bool>? RequestClose;

    public WindowsHelloSetupDialogViewModel(
        IWindowsHelloService windowsHelloService,
        IFirebaseAuthService firebaseAuthService,
        FirestoreDb firestore)
    {
        _windowsHelloService = windowsHelloService;
        _firebaseAuthService = firebaseAuthService;
        _firestore = firestore;

        // Verificar disponibilidad al iniciar
        _ = CheckAvailabilityAsync();
    }

    private async Task CheckAvailabilityAsync()
    {
        try
        {
            IsWindowsHelloAvailable = await _windowsHelloService.IsAvailableAsync();
        }
        catch
        {
            IsWindowsHelloAvailable = false;
        }
    }

    /// <summary>
    /// Configura Windows Hello ahora - Abre el Wizard
    /// </summary>
    [RelayCommand]
    private async Task ConfigureNowAsync()
    {
        if (CurrentUser == null)
        {
            HasError = true;
            ErrorMessage = "No hay usuario autenticado";
            return;
        }

        if (!IsWindowsHelloAvailable)
        {
            HasError = true;
            ErrorMessage = "Windows Hello no está disponible en este dispositivo";
            return;
        }

        try
        {
            // Cerrar el diálogo actual primero
            RequestClose?.Invoke(this, false);

            // Obtener el ViewModel del wizard desde DI
            var wizardViewModel = App.Services?.GetService<WindowsHelloWizardViewModel>();
            if (wizardViewModel == null) return;

            wizardViewModel.CurrentUser = CurrentUser;
            await wizardViewModel.InitializeAsync();

            var wizard = new WindowsHelloWizard
            {
                DataContext = wizardViewModel
            };

            // Mostrar el wizard
            wizard.ShowDialog();
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Recuerda más tarde (no guarda preferencia)
    /// </summary>
    [RelayCommand]
    private void RemindLater()
    {
        // No guardar nada, volverá a aparecer en el próximo login
        RequestClose?.Invoke(this, false);
    }

    /// <summary>
    /// No volver a preguntar
    /// </summary>
    [RelayCommand]
    private async Task NeverAskAsync()
    {
        if (CurrentUser == null || string.IsNullOrEmpty(CurrentUser.FirebaseUid))
        {
            RequestClose?.Invoke(this, false);
            return;
        }

        try
        {
            IsLoading = true;

            // Guardar preferencia de que no quiere que se le pregunte
            await SaveWindowsHelloPreferenceAsync(CurrentUser.FirebaseUid, "dismissed", true);

            RequestClose?.Invoke(this, false);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al guardar preferencia: {ex.Message}");
            RequestClose?.Invoke(this, false);
        }
        finally
        {
            IsLoading = false;
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
            throw;
        }
    }
}
