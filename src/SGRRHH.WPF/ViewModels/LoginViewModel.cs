using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;
using System.Windows;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// ViewModel para la ventana de Login
/// </summary>
public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    
    [ObservableProperty]
    private string _username = string.Empty;
    
    [ObservableProperty]
    private string _password = string.Empty;
    
    [ObservableProperty]
    private string _errorMessage = string.Empty;
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private bool _hasError;
    
    /// <summary>
    /// Usuario autenticado después de un login exitoso
    /// </summary>
    public Usuario? AuthenticatedUser { get; private set; }
    
    /// <summary>
    /// Evento que se dispara cuando el login es exitoso
    /// </summary>
    public event EventHandler? LoginSuccessful;
    
    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
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
    
    [RelayCommand]
    private void Exit()
    {
        Application.Current.Shutdown();
    }
}
