using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// ViewModel para la gestión de departamentos
/// </summary>
public partial class DepartamentosListViewModel : ObservableObject
{
    private readonly IDepartamentoService _departamentoService;
    
    [ObservableProperty]
    private ObservableCollection<Departamento> _departamentos = new();
    
    [ObservableProperty]
    private Departamento? _selectedDepartamento;
    
    [ObservableProperty]
    private bool _isLoading;
    
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
    private bool _isEditing;
    
    [ObservableProperty]
    private int? _editingId;
    
    public DepartamentosListViewModel(IDepartamentoService departamentoService)
    {
        _departamentoService = departamentoService;
    }
    
    /// <summary>
    /// Carga inicial de datos
    /// </summary>
    public async Task LoadAsync()
    {
        await LoadDepartamentosAsync();
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
    
    /// <summary>
    /// Prepara el formulario para nuevo departamento
    /// </summary>
    [RelayCommand]
    private async Task NuevoAsync()
    {
        IsEditing = true;
        EditingId = null;
        Codigo = await _departamentoService.GetNextCodigoAsync();
        Nombre = string.Empty;
        Descripcion = string.Empty;
        SelectedDepartamento = null;
    }
    
    /// <summary>
    /// Prepara el formulario para editar
    /// </summary>
    [RelayCommand]
    private void Editar()
    {
        if (SelectedDepartamento == null)
        {
            MessageBox.Show("Seleccione un departamento para editar", "Aviso",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        IsEditing = true;
        EditingId = SelectedDepartamento.Id;
        Codigo = SelectedDepartamento.Codigo;
        Nombre = SelectedDepartamento.Nombre;
        Descripcion = SelectedDepartamento.Descripcion ?? string.Empty;
    }
    
    /// <summary>
    /// Guarda el departamento (nuevo o editado)
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
                var departamento = new Departamento
                {
                    Id = EditingId.Value,
                    Codigo = Codigo,
                    Nombre = Nombre,
                    Descripcion = Descripcion
                };
                
                var result = await _departamentoService.UpdateAsync(departamento);
                
                if (result.Success)
                {
                    MessageBox.Show("Departamento actualizado correctamente", "Éxito",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    Cancelar();
                    await LoadDepartamentosAsync();
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
                var departamento = new Departamento
                {
                    Codigo = Codigo,
                    Nombre = Nombre,
                    Descripcion = Descripcion
                };
                
                var result = await _departamentoService.CreateAsync(departamento);
                
                if (result.Success)
                {
                    MessageBox.Show("Departamento creado correctamente", "Éxito",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    Cancelar();
                    await LoadDepartamentosAsync();
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
    }
    
    /// <summary>
    /// Elimina un departamento
    /// </summary>
    [RelayCommand]
    private async Task EliminarAsync()
    {
        if (SelectedDepartamento == null)
        {
            MessageBox.Show("Seleccione un departamento para eliminar", "Aviso",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        var confirmResult = MessageBox.Show(
            $"¿Está seguro de eliminar el departamento '{SelectedDepartamento.Nombre}'?\n\nEsta acción no se puede deshacer.",
            "Confirmar eliminación",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        
        if (confirmResult != MessageBoxResult.Yes)
            return;
        
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            
            var deleteResult = await _departamentoService.DeleteAsync(SelectedDepartamento.Id);
            
            if (deleteResult.Success)
            {
                MessageBox.Show("Departamento eliminado correctamente", "Éxito",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadDepartamentosAsync();
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
