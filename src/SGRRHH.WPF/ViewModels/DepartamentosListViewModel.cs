using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;
using SGRRHH.WPF.Helpers;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// ViewModel para la gestión de departamentos
/// </summary>
public partial class DepartamentosListViewModel : ViewModelBase
{
    private readonly IDepartamentoService _departamentoService;
    private readonly IEmpleadoService _empleadoService;
    private readonly IDialogService _dialogService;
    private readonly RolUsuario _currentUserRole;

    [ObservableProperty]
    private ObservableCollection<Departamento> _departamentos = new();

    [ObservableProperty]
    private ObservableCollection<Empleado> _empleados = new();
    
    [ObservableProperty]
    private Departamento? _selectedDepartamento;
    
    [ObservableProperty]
    private string _errorMessage = string.Empty;
    
    // Campos para nuevo/editar departamento
    [ObservableProperty]
    private string _codigo = string.Empty;

    [ObservableProperty]
    private string _nombre = string.Empty;

    [ObservableProperty]
    private string _descripcion = string.Empty;

    [ObservableProperty]
    private int? _jefeId;

    [ObservableProperty]
    private bool _activo = true;
    
    [ObservableProperty]
    private bool _isEditing;
    
    [ObservableProperty]
    private int? _editingId;
    
    // ======== Permisos de UI ========
    
    /// <summary>
    /// Indica si el usuario puede crear departamentos
    /// </summary>
    public bool CanCreate => PermissionHelper.CanCreate(_currentUserRole, PermissionHelper.Modulos.Departamentos);
    
    /// <summary>
    /// Indica si el usuario puede editar departamentos
    /// </summary>
    public bool CanEdit => PermissionHelper.CanEdit(_currentUserRole, PermissionHelper.Modulos.Departamentos);
    
    /// <summary>
    /// Indica si el usuario puede eliminar departamentos
    /// </summary>
    public bool CanDelete => PermissionHelper.CanDelete(_currentUserRole, PermissionHelper.Modulos.Departamentos);
    
    // ======== Control de vistas ========
    
    /// <summary>
    /// Vista Home visible
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFormViewVisible))]
    [NotifyPropertyChangedFor(nameof(IsListViewVisible))]
    [NotifyPropertyChangedFor(nameof(IsEditViewVisible))]
    private bool _isHomeVisible = true;
    
    /// <summary>
    /// Vista de formulario (crear nuevo) visible
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsHomeVisible))]
    [NotifyPropertyChangedFor(nameof(IsListViewVisible))]
    [NotifyPropertyChangedFor(nameof(IsEditViewVisible))]
    private bool _isFormViewVisible;
    
    /// <summary>
    /// Vista de lista visible
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsHomeVisible))]
    [NotifyPropertyChangedFor(nameof(IsFormViewVisible))]
    [NotifyPropertyChangedFor(nameof(IsEditViewVisible))]
    private bool _isListViewVisible;
    
    /// <summary>
    /// Vista de edición visible
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsHomeVisible))]
    [NotifyPropertyChangedFor(nameof(IsFormViewVisible))]
    [NotifyPropertyChangedFor(nameof(IsListViewVisible))]
    private bool _isEditViewVisible;
    
    /// <summary>
    /// Texto con la fecha actual para el footer
    /// </summary>
    public string CurrentDateText => DateTime.Now.ToString("dddd, dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("es-ES"));
    
    public DepartamentosListViewModel(
        IDepartamentoService departamentoService,
        IEmpleadoService empleadoService,
        IDialogService dialogService)
    {
        _departamentoService = departamentoService;
        _empleadoService = empleadoService;
        _dialogService = dialogService;

        // Obtener rol del usuario actual desde App.CurrentUser
        _currentUserRole = App.CurrentUser?.Rol ?? RolUsuario.Operador;
    }
    
    /// <summary>
    /// Carga inicial de datos
    /// </summary>
    public async Task LoadAsync()
    {
        await Task.WhenAll(LoadDepartamentosAsync(), LoadEmpleadosAsync());
    }

    /// <summary>
    /// Carga la lista de empleados (para selector de jefe)
    /// </summary>
    private async Task LoadEmpleadosAsync()
    {
        try
        {
            var empleados = await _empleadoService.GetAllAsync();
            Empleados.Clear();
            foreach (var emp in empleados)
            {
                Empleados.Add(emp);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error cargando empleados: {ex.Message}";
        }
    }
    
    /// <summary>
    /// Carga la lista de departamentos
    /// </summary>
    [RelayCommand]
    private async Task LoadDepartamentosAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            
            var result = await _departamentoService.GetAllAsync();
            
            Departamentos.Clear();
            foreach (var dept in result)
            {
                Departamentos.Add(dept);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    // ======== Comandos de navegación ========
    
    /// <summary>
    /// Muestra la pantalla de inicio
    /// </summary>
    [RelayCommand]
    private void ShowHome()
    {
        IsHomeVisible = true;
        IsFormViewVisible = false;
        IsListViewVisible = false;
        IsEditViewVisible = false;
        LimpiarFormulario();
    }
    
    /// <summary>
    /// Muestra el formulario para crear nuevo
    /// </summary>
    [RelayCommand]
    private async Task ShowFormAsync()
    {
        IsHomeVisible = false;
        IsFormViewVisible = true;
        IsListViewVisible = false;
        IsEditViewVisible = false;
        
        // Preparar formulario vacío
        LimpiarFormulario();
        Codigo = await _departamentoService.GetNextCodigoAsync();
        Activo = true;
    }
    
    /// <summary>
    /// Muestra la lista de departamentos
    /// </summary>
    [RelayCommand]
    private async Task ShowListAsync()
    {
        IsHomeVisible = false;
        IsFormViewVisible = false;
        IsListViewVisible = true;
        IsEditViewVisible = false;
        
        await LoadDepartamentosAsync();
    }
    
    /// <summary>
    /// Muestra la vista de edición
    /// </summary>
    private void ShowEdit()
    {
        IsHomeVisible = false;
        IsFormViewVisible = false;
        IsListViewVisible = false;
        IsEditViewVisible = true;
    }
    
    // ======== Comandos CRUD ========
    
    /// <summary>
    /// Prepara el formulario para nuevo departamento (legacy - mantener compatibilidad)
    /// </summary>
    [RelayCommand]
    private async Task NuevoAsync()
    {
        await ShowFormAsync();
    }
    
    /// <summary>
    /// Prepara el formulario para editar
    /// </summary>
    [RelayCommand]
    private void Editar()
    {
        if (SelectedDepartamento == null)
        {
            _dialogService.ShowInfo("Seleccione un departamento para editar");
            return;
        }

        IsEditing = true;
        EditingId = SelectedDepartamento.Id;
        Codigo = SelectedDepartamento.Codigo;
        Nombre = SelectedDepartamento.Nombre;
        Descripcion = SelectedDepartamento.Descripcion ?? string.Empty;
        JefeId = SelectedDepartamento.JefeId;  // FIX 2.4: Cargar JefeId actual
        Activo = SelectedDepartamento.Activo;

        ShowEdit();
    }
    
    /// <summary>
    /// Guarda el departamento (nuevo o editado)
    /// </summary>
    [RelayCommand]
    private async Task GuardarAsync()
    {
        if (string.IsNullOrWhiteSpace(Nombre))
        {
            _dialogService.ShowWarning("El nombre es obligatorio", "Validación");
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            if (IsEditing && EditingId.HasValue)
            {
                // Editar existente
                var departamento = new Departamento
                {
                    Id = EditingId.Value,
                    Codigo = Codigo,
                    Nombre = Nombre,
                    Descripcion = Descripcion,
                    JefeId = JefeId,  // FIX 2.4: Guardar JefeId seleccionado
                    Activo = Activo
                };

                var result = await _departamentoService.UpdateAsync(departamento);

                if (result.Success)
                {
                    _dialogService.ShowSuccess("Departamento actualizado correctamente");
                    await ShowListAsync();
                }
                else
                {
                    _dialogService.ShowError(result.Message ?? "Error al actualizar");
                }
            }
            else if (!IsEditing)
            {
                // Crear nuevo
                var departamento = new Departamento
                {
                    Codigo = Codigo,
                    Nombre = Nombre,
                    Descripcion = Descripcion,
                    JefeId = JefeId,  // FIX 2.4: Guardar JefeId seleccionado
                    Activo = Activo
                };

                var result = await _departamentoService.CreateAsync(departamento);

                if (result.Success)
                {
                    _dialogService.ShowSuccess("Departamento creado correctamente");
                    await ShowListAsync();  // FIX 2.3: Mostrar lista actualizada, no home (navegación consistente)
                }
                else
                {
                    _dialogService.ShowError(result.Message ?? "Error al crear");
                }
            }
            else
            {
                // Estado inconsistente: IsEditing=true pero EditingId=null
                _dialogService.ShowError("Error: Estado inconsistente en el formulario");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    /// <summary>
    /// Cancela la creación y vuelve al home
    /// </summary>
    [RelayCommand]
    private void Cancelar()
    {
        LimpiarFormulario();
        ShowHome();
    }
    
    /// <summary>
    /// Cancela la edición y vuelve a la lista
    /// </summary>
    [RelayCommand]
    private async Task CancelarEditAsync()
    {
        LimpiarFormulario();
        await ShowListAsync();
    }
    
    /// <summary>
    /// Limpia los campos del formulario
    /// </summary>
    private void LimpiarFormulario()
    {
        IsEditing = false;
        EditingId = null;
        Codigo = string.Empty;
        Nombre = string.Empty;
        Descripcion = string.Empty;
        JefeId = null;  // Resetear JefeId
        Activo = true;
    }
    
    /// <summary>
    /// Elimina un departamento
    /// </summary>
    [RelayCommand]
    private async Task EliminarAsync()
    {
        if (SelectedDepartamento == null)
        {
            _dialogService.ShowInfo("Seleccione un departamento para eliminar");
            return;
        }
        
        if (!_dialogService.ConfirmWarning($"¿Está seguro de eliminar el departamento '{SelectedDepartamento.Nombre}'?\n\nEsta acción no se puede deshacer.", "Confirmar eliminación"))
            return;
        
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            
            var deleteResult = await _departamentoService.DeleteAsync(SelectedDepartamento.Id);
            
            if (deleteResult.Success)
            {
                _dialogService.ShowSuccess("Departamento eliminado correctamente");
                await LoadDepartamentosAsync();
            }
            else
            {
                _dialogService.ShowError(deleteResult.Message ?? "Error al eliminar");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
