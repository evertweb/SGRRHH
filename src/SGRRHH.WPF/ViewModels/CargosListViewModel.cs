using System;
using System.Collections.ObjectModel;
using System.Linq;
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
/// ViewModel para la gestión de cargos
/// </summary>
public partial class CargosListViewModel : ViewModelBase
{
    private readonly ICargoService _cargoService;
    private readonly IDepartamentoService _departamentoService;
    private readonly IDialogService _dialogService;
    private readonly RolUsuario _currentUserRole;
    
    [ObservableProperty]
    private ObservableCollection<Cargo> _cargos = new();
    
    [ObservableProperty]
    private ObservableCollection<Departamento> _departamentos = new();
    
    [ObservableProperty]
    private Cargo? _selectedCargo;
    
    [ObservableProperty]
    private string _errorMessage = string.Empty;
    
    // Campos para nuevo/editar cargo
    [ObservableProperty]
    private string _codigo = string.Empty;
    
    [ObservableProperty]
    private string _nombre = string.Empty;
    
    [ObservableProperty]
    private string _descripcion = string.Empty;
    
    [ObservableProperty]
    private int? _departamentoId;
    
    [ObservableProperty]
    private int _nivel = 1;
    
    [ObservableProperty]
    private bool _activo = true;
    
    [ObservableProperty]
    private bool _isEditing;
    
    [ObservableProperty]
    private int? _editingId;
    
    // Filtro
    [ObservableProperty]
    private int? _filterDepartamentoId;
    
    private List<Cargo> _allCargos = new();
    
    // ======== Permisos de UI ========
    
    /// <summary>
    /// Indica si el usuario puede crear cargos
    /// </summary>
    public bool CanCreate => PermissionHelper.CanCreate(_currentUserRole, PermissionHelper.Modulos.Cargos);
    
    /// <summary>
    /// Indica si el usuario puede editar cargos
    /// </summary>
    public bool CanEdit => PermissionHelper.CanEdit(_currentUserRole, PermissionHelper.Modulos.Cargos);
    
    /// <summary>
    /// Indica si el usuario puede eliminar cargos
    /// </summary>
    public bool CanDelete => PermissionHelper.CanDelete(_currentUserRole, PermissionHelper.Modulos.Cargos);
    
    // ======== Control de vistas ========
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFormViewVisible))]
    [NotifyPropertyChangedFor(nameof(IsListViewVisible))]
    [NotifyPropertyChangedFor(nameof(IsEditViewVisible))]
    private bool _isHomeVisible = true;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsHomeVisible))]
    [NotifyPropertyChangedFor(nameof(IsListViewVisible))]
    [NotifyPropertyChangedFor(nameof(IsEditViewVisible))]
    private bool _isFormViewVisible;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsHomeVisible))]
    [NotifyPropertyChangedFor(nameof(IsFormViewVisible))]
    [NotifyPropertyChangedFor(nameof(IsEditViewVisible))]
    private bool _isListViewVisible;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsHomeVisible))]
    [NotifyPropertyChangedFor(nameof(IsFormViewVisible))]
    [NotifyPropertyChangedFor(nameof(IsListViewVisible))]
    private bool _isEditViewVisible;
    
    public string CurrentDateText => DateTime.Now.ToString("dddd, dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("es-ES"));
    
    public CargosListViewModel(ICargoService cargoService, IDepartamentoService departamentoService, IDialogService dialogService)
    {
        _cargoService = cargoService;
        _departamentoService = departamentoService;
        _dialogService = dialogService;
        
        // Obtener rol del usuario actual desde App.CurrentUser
        _currentUserRole = App.CurrentUser?.Rol ?? RolUsuario.Operador;
    }
    
    public async Task LoadAsync()
    {
        await LoadDepartamentosAsync();
        await LoadCargosAsync();
    }
    
    private async Task LoadDepartamentosAsync()
    {
        try
        {
            var result = await _departamentoService.GetAllAsync();
            
            Departamentos.Clear();
            foreach (var dept in result)
            {
                Departamentos.Add(dept);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error cargando departamentos: {ex.Message}";
        }
    }
    
    [RelayCommand]
    private async Task LoadCargosAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            
            var result = await _cargoService.GetAllAsync();
            
            _allCargos = result.ToList();
            ApplyFilter();
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
    
    partial void OnFilterDepartamentoIdChanged(int? value)
    {
        ApplyFilter();
    }
    
    private void ApplyFilter()
    {
        Cargos.Clear();
        
        var filtered = _allCargos.AsEnumerable();
        
        if (FilterDepartamentoId.HasValue)
        {
            filtered = filtered.Where(c => c.DepartamentoId == FilterDepartamentoId.Value);
        }
        
        foreach (var cargo in filtered)
        {
            Cargos.Add(cargo);
        }
    }
    
    [RelayCommand]
    private void LimpiarFiltro()
    {
        FilterDepartamentoId = null;
    }
    
    // ======== Comandos de navegación ========
    
    [RelayCommand]
    private void ShowHome()
    {
        IsHomeVisible = true;
        IsFormViewVisible = false;
        IsListViewVisible = false;
        IsEditViewVisible = false;
        LimpiarFormulario();
    }
    
    [RelayCommand]
    private async Task ShowFormAsync()
    {
        IsHomeVisible = false;
        IsFormViewVisible = true;
        IsListViewVisible = false;
        IsEditViewVisible = false;
        
        LimpiarFormulario();
        Codigo = await _cargoService.GetNextCodigoAsync();
        Activo = true;
        await LoadDepartamentosAsync();
    }
    
    [RelayCommand]
    private async Task ShowListAsync()
    {
        IsHomeVisible = false;
        IsFormViewVisible = false;
        IsListViewVisible = true;
        IsEditViewVisible = false;
        
        await LoadDepartamentosAsync();
        await LoadCargosAsync();
    }
    
    private void ShowEdit()
    {
        IsHomeVisible = false;
        IsFormViewVisible = false;
        IsListViewVisible = false;
        IsEditViewVisible = true;
    }
    
    // ======== Comandos CRUD ========
    
    [RelayCommand]
    private async Task NuevoAsync()
    {
        await ShowFormAsync();
    }
    
    [RelayCommand]
    private void Editar()
    {
        if (SelectedCargo == null)
        {
            _dialogService.ShowInfo("Seleccione un cargo para editar");
            return;
        }
        
        IsEditing = true;
        EditingId = SelectedCargo.Id;
        Codigo = SelectedCargo.Codigo;
        Nombre = SelectedCargo.Nombre;
        Descripcion = SelectedCargo.Descripcion ?? string.Empty;
        DepartamentoId = SelectedCargo.DepartamentoId;
        Nivel = SelectedCargo.Nivel;
        Activo = SelectedCargo.Activo;
        
        ShowEdit();
    }
    
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
            
            if (EditingId.HasValue)
            {
                var cargo = new Cargo
                {
                    Id = EditingId.Value,
                    Codigo = Codigo,
                    Nombre = Nombre,
                    Descripcion = Descripcion,
                    DepartamentoId = DepartamentoId,
                    Nivel = Nivel,
                    Activo = Activo
                };
                
                var result = await _cargoService.UpdateAsync(cargo);
                
                if (result.Success)
                {
                    _dialogService.ShowSuccess("Cargo actualizado correctamente");
                    await ShowListAsync();
                }
                else
                {
                    _dialogService.ShowError(result.Message ?? "Error al actualizar");
                }
            }
            else
            {
                var cargo = new Cargo
                {
                    Codigo = Codigo,
                    Nombre = Nombre,
                    Descripcion = Descripcion,
                    DepartamentoId = DepartamentoId,
                    Nivel = Nivel,
                    Activo = Activo
                };
                
                var result = await _cargoService.CreateAsync(cargo);
                
                if (result.Success)
                {
                    _dialogService.ShowSuccess("Cargo creado correctamente");
                    ShowHome();
                }
                else
                {
                    _dialogService.ShowError(result.Message ?? "Error al crear");
                }
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
    
    [RelayCommand]
    private void Cancelar()
    {
        LimpiarFormulario();
        ShowHome();
    }
    
    [RelayCommand]
    private async Task CancelarEditAsync()
    {
        LimpiarFormulario();
        await ShowListAsync();
    }
    
    private void LimpiarFormulario()
    {
        IsEditing = false;
        EditingId = null;
        Codigo = string.Empty;
        Nombre = string.Empty;
        Descripcion = string.Empty;
        DepartamentoId = null;
        Nivel = 1;
        Activo = true;
    }
    
    [RelayCommand]
    private async Task EliminarAsync()
    {
        if (SelectedCargo == null)
        {
            _dialogService.ShowInfo("Seleccione un cargo para eliminar");
            return;
        }
        
        if (!_dialogService.ConfirmWarning($"¿Está seguro de eliminar el cargo '{SelectedCargo.Nombre}'?\n\nEsta acción no se puede deshacer.", "Confirmar eliminación"))
            return;
        
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            
            var deleteResult = await _cargoService.DeleteAsync(SelectedCargo.Id);
            
            if (deleteResult.Success)
            {
                _dialogService.ShowSuccess("Cargo eliminado correctamente");
                await LoadCargosAsync();
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
