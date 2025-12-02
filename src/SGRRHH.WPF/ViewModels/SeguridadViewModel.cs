using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Google.Cloud.Firestore;
using Microsoft.Extensions.DependencyInjection;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;
using SGRRHH.Core.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Interop;
using SGRRHH.WPF.Views;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// ViewModel para gestión de seguridad y passkeys
/// </summary>
public partial class SeguridadViewModel : ObservableObject
{
    private readonly IWindowsHelloService _windowsHelloService;
    private readonly IFirebaseAuthService _firebaseAuthService;
    private readonly Usuario _currentUser;

    [ObservableProperty]
    private bool _isWindowsHelloAvailable;

    [ObservableProperty]
    private string _windowsHelloStatusMessage = string.Empty;

    [ObservableProperty]
    private ObservableCollection<PasskeyInfo> _passkeys = new();

    [ObservableProperty]
    private PasskeyInfo? _selectedPasskey;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _mensaje = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _hasThisDevicePasskey;

    [ObservableProperty]
    private string _deviceName = Environment.MachineName;

    public SeguridadViewModel(
        IWindowsHelloService windowsHelloService,
        IFirebaseAuthService firebaseAuthService)
    {
        _windowsHelloService = windowsHelloService;
        _firebaseAuthService = firebaseAuthService;

        // Obtener usuario actual de App
        _currentUser = App.CurrentUser ?? throw new InvalidOperationException("No hay usuario autenticado");
    }

    public async Task LoadDataAsync()
    {
        await CheckWindowsHelloAvailabilityAsync();
        await LoadPasskeysAsync();
    }

    /// <summary>
    /// Verifica si Windows Hello está disponible
    /// </summary>
    private async Task CheckWindowsHelloAvailabilityAsync()
    {
        try
        {
            IsWindowsHelloAvailable = await _windowsHelloService.IsAvailableAsync();
            WindowsHelloStatusMessage = await _windowsHelloService.GetAvailabilityMessageAsync();
        }
        catch (Exception ex)
        {
            IsWindowsHelloAvailable = false;
            WindowsHelloStatusMessage = $"Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Carga las passkeys registradas del usuario
    /// </summary>
    private async Task LoadPasskeysAsync()
    {
        try
        {
            IsLoading = true;

            if (string.IsNullOrEmpty(_currentUser.FirebaseUid))
            {
                Mensaje = "Usuario sin UID de Firebase";
                return;
            }

            var passkeys = await _firebaseAuthService.GetUserPasskeysAsync(_currentUser.FirebaseUid);

            Passkeys.Clear();
            foreach (var passkey in passkeys)
            {
                // Marcar si es el dispositivo actual
                passkey.IsCurrent = passkey.DeviceName.Equals(DeviceName, StringComparison.OrdinalIgnoreCase);
                Passkeys.Add(passkey);
            }

            // Verificar si este dispositivo tiene passkey
            HasThisDevicePasskey = Passkeys.Any(p => p.IsCurrent);
        }
        catch (Exception ex)
        {
            HasError = true;
            Mensaje = $"Error al cargar passkeys: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Registra una nueva passkey para este dispositivo
    /// </summary>
    [RelayCommand]
    private async Task RegisterPasskeyAsync()
    {
        if (!IsWindowsHelloAvailable)
        {
            HasError = true;
            Mensaje = "Windows Hello no está disponible en este dispositivo";
            return;
        }

        if (HasThisDevicePasskey)
        {
            HasError = true;
            Mensaje = "Este dispositivo ya tiene una passkey registrada";
            return;
        }

        try
        {
            IsLoading = true;
            HasError = false;
            Mensaje = string.Empty;

            // Obtener el handle de la ventana principal
            var mainWindow = Application.Current.MainWindow;
            IntPtr hwnd = IntPtr.Zero;
            if (mainWindow != null)
            {
                var helper = new WindowInteropHelper(mainWindow);
                hwnd = helper.Handle;
            }

            var fullUsername = _currentUser.Email ?? _currentUser.Username;

            // Registrar la passkey con Windows Hello
            var result = await _windowsHelloService.RegisterPasskeyAsync(
                hwnd,
                fullUsername,
                _currentUser.FirebaseUid ?? string.Empty);

            if (!result.Success)
            {
                HasError = true;
                Mensaje = result.Message ?? "Error al registrar la passkey";
                return;
            }

            // Guardar en Firestore
            var credentialId = result.CredentialId ?? string.Empty;
            bool saved = await _firebaseAuthService.RegisterPasskeyAsync(
                _currentUser.FirebaseUid ?? string.Empty,
                credentialId,
                DeviceName);

            if (saved)
            {
                HasError = false;
                Mensaje = "✅ Passkey registrada exitosamente para este dispositivo";
                await LoadPasskeysAsync();
            }
            else
            {
                HasError = true;
                Mensaje = "Error al guardar la passkey en el servidor";
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            Mensaje = $"Error al registrar passkey: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Elimina una passkey
    /// </summary>
    [RelayCommand]
    private async Task RemovePasskeyAsync(PasskeyInfo? passkey)
    {
        if (passkey == null)
        {
            HasError = true;
            Mensaje = "Seleccione una passkey para eliminar";
            return;
        }

        // Confirmar
        var result = MessageBox.Show(
            $"¿Está seguro de eliminar la passkey del dispositivo '{passkey.DeviceName}'?\n\n" +
            $"Esta acción no se puede deshacer.",
            "Confirmar eliminación",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            IsLoading = true;
            HasError = false;
            Mensaje = string.Empty;

            // Revocar en Firestore
            bool revoked = await _firebaseAuthService.RevokePasskeyAsync(
                _currentUser.FirebaseUid ?? string.Empty,
                passkey.CredentialId);

            if (revoked)
            {
                // Si es el dispositivo actual, también eliminar del Credential Manager local
                if (passkey.IsCurrent)
                {
                    var fullUsername = _currentUser.Email ?? _currentUser.Username;
                    await _windowsHelloService.RemovePasskeyMappingAsync(fullUsername);
                }

                HasError = false;
                Mensaje = "✅ Passkey eliminada exitosamente";
                await LoadPasskeysAsync();
            }
            else
            {
                HasError = true;
                Mensaje = "Error al eliminar la passkey";
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            Mensaje = $"Error al eliminar passkey: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Abre el Wizard de configuración de Windows Hello
    /// </summary>
    [RelayCommand]
    private async Task OpenWizardAsync()
    {
        try
        {
            // Obtener el ViewModel del wizard desde DI
            var wizardViewModel = App.Services?.GetService<WindowsHelloWizardViewModel>();
            if (wizardViewModel == null)
            {
                HasError = true;
                Mensaje = "Error al iniciar el asistente";
                return;
            }

            wizardViewModel.CurrentUser = _currentUser;
            await wizardViewModel.InitializeAsync();

            var wizard = new WindowsHelloWizard
            {
                Owner = Application.Current.MainWindow,
                DataContext = wizardViewModel
            };

            var result = wizard.ShowDialog();

            // Si el wizard completó exitosamente, recargar los datos
            if (result == true)
            {
                HasError = false;
                Mensaje = "✅ Windows Hello configurado exitosamente";
                await LoadPasskeysAsync();
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            Mensaje = $"Error al abrir el asistente: {ex.Message}";
        }
    }
}
