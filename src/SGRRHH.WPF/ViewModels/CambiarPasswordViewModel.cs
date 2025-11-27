using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGRRHH.Core.Interfaces;
using System.Windows;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// ViewModel para cambiar contraseña del usuario actual
/// </summary>
public partial class CambiarPasswordViewModel : ObservableObject
{
    private readonly IUsuarioService _usuarioService;
    
    [ObservableProperty]
    private string _passwordActual = string.Empty;
    
    [ObservableProperty]
    private string _passwordNueva = string.Empty;
    
    [ObservableProperty]
    private string _passwordConfirmar = string.Empty;
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private string? _mensaje;
    
    /// <summary>
    /// Evento que se dispara cuando el cambio de contraseña es exitoso
    /// </summary>
    public event EventHandler? PasswordChanged;
    
    public CambiarPasswordViewModel(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
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
                MessageBox.Show("Contraseña cambiada exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
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
