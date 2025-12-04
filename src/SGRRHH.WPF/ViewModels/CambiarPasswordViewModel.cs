using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGRRHH.Core.Interfaces;
using System.Windows;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// ViewModel para cambiar contraseña del usuario actual
/// </summary>
public partial class CambiarPasswordViewModel : ViewModelBase
{
    private readonly IUsuarioService _usuarioService;
    private readonly IDialogService _dialogService;
    
    [ObservableProperty]
    private string _passwordActual = string.Empty;
    
    [ObservableProperty]
    private string _passwordNueva = string.Empty;
    
    [ObservableProperty]
    private string _passwordConfirmar = string.Empty;
    
    [ObservableProperty]
    private string? _mensaje;
    
    /// <summary>
    /// Evento que se dispara cuando el cambio de contraseña es exitoso
    /// </summary>
    public event EventHandler? PasswordChanged;
    
    public CambiarPasswordViewModel(IUsuarioService usuarioService, IDialogService dialogService)
    {
        _usuarioService = usuarioService;
        _dialogService = dialogService;
    }
    
    [RelayCommand]
    private async Task CambiarPasswordAsync()
    {
        Mensaje = null;
        
        // Validaciones
        if (string.IsNullOrWhiteSpace(PasswordActual))
        {
            Mensaje = "Ingrese la contraseña actual";
            return;
        }
        
        if (string.IsNullOrWhiteSpace(PasswordNueva))
        {
            Mensaje = "Ingrese la nueva contraseña";
            return;
        }
        
        if (PasswordNueva.Length < 6)
        {
            Mensaje = "La nueva contraseña debe tener al menos 6 caracteres";
            return;
        }
        
        if (PasswordNueva != PasswordConfirmar)
        {
            Mensaje = "Las contraseñas no coinciden";
            return;
        }
        
        if (App.CurrentUser == null)
        {
            Mensaje = "No hay usuario autenticado";
            return;
        }
        
        IsLoading = true;
        
        try
        {
            var result = await _usuarioService.ChangePasswordAsync(
                App.CurrentUser.Id,
                PasswordActual,
                PasswordNueva);
            
            if (result.Success)
            {
                _dialogService.ShowSuccess("Contraseña cambiada exitosamente");
                PasswordChanged?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                Mensaje = result.Message;
            }
        }
        catch (Exception ex)
        {
            Mensaje = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private void Limpiar()
    {
        PasswordActual = string.Empty;
        PasswordNueva = string.Empty;
        PasswordConfirmar = string.Empty;
        Mensaje = null;
    }
}
