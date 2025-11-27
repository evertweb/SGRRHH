using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// ViewModel para la gestión de cargos
/// </summary>
public partial class CargosListViewModel : ObservableObject
{
    private readonly ICargoService _cargoService;
    private readonly IDepartamentoService _departamentoService;
    
    [ObservableProperty]
    private ObservableCollection<Cargo> _cargos = new();
    
    [ObservableProperty]
    private ObservableCollection<Departamento> _departamentos = new();
    
    [ObservableProperty]
    private Cargo? _selectedCargo;
    
    [ObservableProperty]
    private bool _isLoading;
    
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
    private bool _isEditing;
    
    [ObservableProperty]
    private int? _editingId;
    
    // Filtro
    [ObservableProperty]
    private int? _filterDepartamentoId;
    
    private List<Cargo> _allCargos = new();
    
    public CargosListViewModel(ICargoService cargoService, IDepartamentoService departamentoService)
    {
        _cargoService = cargoService;
        _departamentoService = departamentoService;
    }
    
    /// <summary>
    /// Carga inicial de datos
    /// </summary>
    public async Task LoadAsync()
    {
        await LoadDepartamentosAsync();
        await LoadCargosAsync();
    }
    
    /// <summary>
    /// Carga la lista de departamentos
    /// </summary>
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
    
    /// <summary>
    /// Carga la lista de cargos
    /// </summary>
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
    
    /// <summary>
    /// Aplica el filtro de departamento
    /// </summary>
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
    
    /// <summary>
    /// Limpia el filtro
    /// </summary>
    [RelayCommand]
    private void LimpiarFiltro()
    {
        FilterDepartamentoId = null;
    }
    
    /// <summary>
    /// Prepara el formulario para nuevo cargo
    /// </summary>
    [RelayCommand]
    private async Task NuevoAsync()
    {
        IsEditing = true;
        EditingId = null;
        Codigo = await _cargoService.GetNextCodigoAsync();
        Nombre = string.Empty;
        Descripcion = string.Empty;
        DepartamentoId = null;
        Nivel = 1;
        SelectedCargo = null;
    }
    
    /// <summary>
    /// Prepara el formulario para editar
    /// </summary>
    [RelayCommand]
    private void Editar()
    {
        if (SelectedCargo == null)
        {
            MessageBox.Show("Seleccione un cargo para editar", "Aviso",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        IsEditing = true;
        EditingId = SelectedCargo.Id;
        Codigo = SelectedCargo.Codigo;
        Nombre = SelectedCargo.Nombre;
        Descripcion = SelectedCargo.Descripcion ?? string.Empty;
        DepartamentoId = SelectedCargo.DepartamentoId;
        Nivel = SelectedCargo.Nivel;
    }
    
    /// <summary>
    /// Guarda el cargo (nuevo o editado)
    /// </summary>
    [RelayCommand]
    private async Task GuardarAsync()
    {
        if (string.IsNullOrWhiteSpace(Nombre))
        {
            MessageBox.Show("El nombre es obligatorio", "Validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            
            if (EditingId.HasValue)
            {
                // Editar existente
                var cargo = new Cargo
                {
                    Id = EditingId.Value,
                    Codigo = Codigo,
                    Nombre = Nombre,
                    Descripcion = Descripcion,
                    DepartamentoId = DepartamentoId,
                    Nivel = Nivel
                };
                
                var result = await _cargoService.UpdateAsync(cargo);
                
                if (result.Success)
                {
                    MessageBox.Show("Cargo actualizado correctamente", "Éxito",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    Cancelar();
                    await LoadCargosAsync();
                }
                else
                {
                    MessageBox.Show(result.Message ?? "Error al actualizar", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                // Crear nuevo
                var cargo = new Cargo
                {
                    Codigo = Codigo,
                    Nombre = Nombre,
                    Descripcion = Descripcion,
                    DepartamentoId = DepartamentoId,
                    Nivel = Nivel
                };
                
                var result = await _cargoService.CreateAsync(cargo);
                
                if (result.Success)
                {
                    MessageBox.Show("Cargo creado correctamente", "Éxito",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    Cancelar();
                    await LoadCargosAsync();
                }
                else
                {
                    MessageBox.Show(result.Message ?? "Error al crear", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
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
    
    /// <summary>
    /// Cancela la edición
    /// </summary>
    [RelayCommand]
    private void Cancelar()
    {
        IsEditing = false;
        EditingId = null;
        Codigo = string.Empty;
        Nombre = string.Empty;
        Descripcion = string.Empty;
        DepartamentoId = null;
        Nivel = 1;
    }
    
    /// <summary>
    /// Elimina un cargo
    /// </summary>
    [RelayCommand]
    private async Task EliminarAsync()
    {
        if (SelectedCargo == null)
        {
            MessageBox.Show("Seleccione un cargo para eliminar", "Aviso",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        var confirmResult = MessageBox.Show(
            $"¿Está seguro de eliminar el cargo '{SelectedCargo.Nombre}'?\n\nEsta acción no se puede deshacer.",
            "Confirmar eliminación",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        
        if (confirmResult != MessageBoxResult.Yes)
            return;
        
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            
            var deleteResult = await _cargoService.DeleteAsync(SelectedCargo.Id);
            
            if (deleteResult.Success)
            {
                MessageBox.Show("Cargo eliminado correctamente", "Éxito",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadCargosAsync();
            }
            else
            {
                MessageBox.Show(deleteResult.Message ?? "Error al eliminar", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
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
