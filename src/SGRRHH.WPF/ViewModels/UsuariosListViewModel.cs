using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// ViewModel para la gestión de usuarios
/// </summary>
public partial class UsuariosListViewModel : ViewModelBase
{
    private readonly IUsuarioService _usuarioService;
    private readonly IDialogService _dialogService;
    
    [ObservableProperty]
    private ObservableCollection<Usuario> _usuarios = new();
    
    [ObservableProperty]
    private Usuario? _selectedUsuario;
    
    [ObservableProperty]
    private string? _mensaje;
    
    [ObservableProperty]
    private string _searchText = string.Empty;
    
    // Para el formulario de edición/creación
    [ObservableProperty]
    private bool _isEditing;
    
    [ObservableProperty]
    private bool _isCreating;
    
    [ObservableProperty]
    private string _editUsername = string.Empty;
    
    [ObservableProperty]
    private string _editPassword = string.Empty;
    
    [ObservableProperty]
    private string _editNombreCompleto = string.Empty;
    
    [ObservableProperty]
    private string? _editEmail;
    
    [ObservableProperty]
    private RolUsuario _editRol = RolUsuario.Operador;
    
    [ObservableProperty]
    private bool _editActivo = true;
    
    [ObservableProperty]
    private int _editUsuarioId;
    
    // Lista de roles para el ComboBox
    public List<RolUsuario> Roles { get; } = Enum.GetValues<RolUsuario>().ToList();
    
    public UsuariosListViewModel(IUsuarioService usuarioService, IDialogService dialogService)
    {
        _usuarioService = usuarioService;
        _dialogService = dialogService;
    }
    
    public async Task LoadDataAsync()
    {
        IsLoading = true;
        Mensaje = null;
        
        try
        {
            var usuarios = await _usuarioService.GetAllAsync();
            
            Usuarios.Clear();
            foreach (var usuario in usuarios)
            {
                Usuarios.Add(usuario);
            }
        }
        catch (Exception ex)
        {
            Mensaje = $"Error al cargar usuarios: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private void NuevoUsuario()
    {
        IsCreating = true;
        IsEditing = true;
        EditUsuarioId = 0;
        EditUsername = string.Empty;
        EditPassword = string.Empty;
        EditNombreCompleto = string.Empty;
        EditEmail = null;
        EditRol = RolUsuario.Operador;
        EditActivo = true;
    }
    
    [RelayCommand]
    private void EditarUsuario()
    {
        if (SelectedUsuario == null)
        {
            _dialogService.ShowInfo("Seleccione un usuario para editar");
            return;
        }
        
        IsCreating = false;
        IsEditing = true;
        EditUsuarioId = SelectedUsuario.Id;
        EditUsername = SelectedUsuario.Username;
        EditPassword = string.Empty; // No mostramos la contraseña
        EditNombreCompleto = SelectedUsuario.NombreCompleto;
        EditEmail = SelectedUsuario.Email;
        EditRol = SelectedUsuario.Rol;
        EditActivo = SelectedUsuario.Activo;
    }
    
    [RelayCommand]
    private void CancelarEdicion()
    {
        IsEditing = false;
        IsCreating = false;
    }
    
    [RelayCommand]
    private async Task GuardarUsuarioAsync()
    {
        IsLoading = true;
        Mensaje = null;
        
        try
        {
            if (IsCreating)
            {
                // Validar campos requeridos para creación
                if (string.IsNullOrWhiteSpace(EditUsername))
                {
                    _dialogService.ShowError("El nombre de usuario es requerido");
                    return;
                }
                
                if (string.IsNullOrWhiteSpace(EditPassword))
                {
                    _dialogService.ShowError("La contraseña es requerida");
                    return;
                }
                
                var result = await _usuarioService.CreateAsync(
                    EditUsername,
                    EditPassword,
                    EditNombreCompleto,
                    EditRol,
                    EditEmail);
                
                if (result.Success)
                {
                    _dialogService.ShowSuccess("Usuario creado exitosamente");
                    IsEditing = false;
                    IsCreating = false;
                    await LoadDataAsync();
                }
                else
                {
                    _dialogService.ShowError(result.Message);
                }
            }
            else
            {
                // Actualizar usuario existente
                var result = await _usuarioService.UpdateAsync(
                    EditUsuarioId,
                    EditNombreCompleto,
                    EditRol,
                    EditEmail,
                    null,
                    EditActivo);
                
                if (result.Success)
                {
                    _dialogService.ShowSuccess("Usuario actualizado exitosamente");
                    IsEditing = false;
                    await LoadDataAsync();
                }
                else
                {
                    _dialogService.ShowError(result.Message);
                }
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Error: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private async Task ResetPasswordAsync()
    {
        if (SelectedUsuario == null)
        {
            _dialogService.ShowInfo("Seleccione un usuario");
            return;
        }
        
        // Solicitar nueva contraseña
        var dialog = new ResetPasswordDialog();
        if (dialog.ShowDialog() == true)
        {
            var newPassword = dialog.NewPassword;
            
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                _dialogService.ShowError("La contraseña no puede estar vacía");
                return;
            }
            
            IsLoading = true;
            
            try
            {
                var result = await _usuarioService.ResetPasswordAsync(SelectedUsuario.Id, newPassword);
                
                if (result.Success)
                {
                    _dialogService.ShowSuccess($"Contraseña restablecida para {SelectedUsuario.Username}");
                }
                else
                {
                    _dialogService.ShowError(result.Message);
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
    
    [RelayCommand]
    private async Task ToggleActivoAsync()
    {
        if (SelectedUsuario == null)
        {
            _dialogService.ShowInfo("Seleccione un usuario");
            return;
        }
        
        var nuevoEstado = !SelectedUsuario.Activo;
        var accion = nuevoEstado ? "activar" : "desactivar";
        
        var confirmado = _dialogService.Confirm(
            $"¿Está seguro de {accion} al usuario '{SelectedUsuario.Username}'?",
            "Confirmar");
        
        if (!confirmado)
            return;
        
        IsLoading = true;
        
        try
        {
            var result = await _usuarioService.SetActivoAsync(SelectedUsuario.Id, nuevoEstado);
            
            if (result.Success)
            {
                await LoadDataAsync();
            }
            else
            {
                _dialogService.ShowError(result.Message);
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Error: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private async Task EliminarUsuarioAsync()
    {
        if (SelectedUsuario == null)
        {
            _dialogService.ShowInfo("Seleccione un usuario para eliminar");
            return;
        }
        
        // Verificar que no sea el usuario actual
        if (App.CurrentUser?.Id == SelectedUsuario.Id)
        {
            _dialogService.ShowError("No puede eliminar su propio usuario");
            return;
        }
        
        var confirmado = _dialogService.ConfirmWarning(
            $"¿Está seguro de eliminar al usuario '{SelectedUsuario.Username}'?\n\nEsta acción no se puede deshacer.",
            "Confirmar Eliminación");
        
        if (!confirmado)
            return;
        
        IsLoading = true;
        
        try
        {
            var result = await _usuarioService.DeleteAsync(SelectedUsuario.Id);
            
            if (result.Success)
            {
                _dialogService.ShowSuccess("Usuario eliminado exitosamente");
                await LoadDataAsync();
            }
            else
            {
                _dialogService.ShowError(result.Message);
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Error: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    public static string GetRoleName(RolUsuario rol) => rol switch
    {
        RolUsuario.Administrador => "Administrador",
        RolUsuario.Aprobador => "Aprobador",
        RolUsuario.Operador => "Operador",
        _ => "Desconocido"
    };
}

/// <summary>
/// Diálogo simple para resetear contraseña
/// </summary>
public class ResetPasswordDialog : Window
{
    private readonly System.Windows.Controls.PasswordBox _passwordBox;
    
    public string NewPassword => _passwordBox.Password;
    
    public ResetPasswordDialog()
    {
        Title = "Restablecer Contraseña";
        Width = 350;
        Height = 150;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.NoResize;
        
        var stackPanel = new System.Windows.Controls.StackPanel { Margin = new Thickness(15) };
        
        stackPanel.Children.Add(new System.Windows.Controls.TextBlock
        {
            Text = "Nueva contraseña:",
            Margin = new Thickness(0, 0, 0, 5)
        });
        
        _passwordBox = new System.Windows.Controls.PasswordBox
        {
            Margin = new Thickness(0, 0, 0, 15)
        };
        stackPanel.Children.Add(_passwordBox);
        
        var buttonPanel = new System.Windows.Controls.StackPanel
        {
            Orientation = System.Windows.Controls.Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        
        var okButton = new System.Windows.Controls.Button
        {
            Content = "Aceptar",
            Width = 80,
            Margin = new Thickness(0, 0, 10, 0),
            IsDefault = true
        };
        okButton.Click += (s, e) => { DialogResult = true; };
        buttonPanel.Children.Add(okButton);
        
        var cancelButton = new System.Windows.Controls.Button
        {
            Content = "Cancelar",
            Width = 80,
            IsCancel = true
        };
        buttonPanel.Children.Add(cancelButton);
        
        stackPanel.Children.Add(buttonPanel);
        
        Content = stackPanel;
    }
}
