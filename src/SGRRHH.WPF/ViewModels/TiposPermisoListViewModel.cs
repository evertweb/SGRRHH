using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGRRHH.Core.Common;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;
using SGRRHH.WPF.Helpers;
using System.Collections.ObjectModel;
using System.Windows;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// ViewModel para el catálogo de tipos de permiso
/// </summary>
public partial class TiposPermisoListViewModel : ObservableObject
{
    private readonly ITipoPermisoService _tipoPermisoService;
    
    [ObservableProperty]
    private ObservableCollection<TipoPermiso> _tiposPermiso = new();
    
    [ObservableProperty]
    private TipoPermiso? _selectedTipoPermiso;
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private bool _showInactivos;
    
    // Propiedades del formulario
    [ObservableProperty]
    private bool _isFormVisible;
    
    [ObservableProperty]
    private bool _isEditing;
    
    [ObservableProperty]
    private string _formTitle = "Nuevo Tipo de Permiso";
    
    [ObservableProperty]
    private string _nombre = string.Empty;
    
    [ObservableProperty]
    private string? _descripcion;
    
    [ObservableProperty]
    private string _color = "#1E88E5";
    
    [ObservableProperty]
    private bool _requiereAprobacion = true;
    
    [ObservableProperty]
    private bool _requiereDocumento;
    
    [ObservableProperty]
    private int _diasPorDefecto = 1;
    
    [ObservableProperty]
    private bool _esCompensable;
    
    [ObservableProperty]
    private bool _activo = true;
    
    private int? _editingId;
    
    /// <summary>
    /// Colores disponibles para selección
    /// </summary>
    public ObservableCollection<string> ColoresDisponibles { get; } = new()
    {
        "#E91E63", // Pink
        "#F44336", // Red
        "#FF5722", // Deep Orange
        "#FF9800", // Orange
        "#FFC107", // Amber
        "#8BC34A", // Light Green
        "#4CAF50", // Green
        "#009688", // Teal
        "#00BCD4", // Cyan
        "#2196F3", // Blue
        "#3F51B5", // Indigo
        "#9C27B0", // Purple
        "#607D8B", // Blue Grey
        "#795548", // Brown
        "#424242"  // Grey
    };
    
    public TiposPermisoListViewModel(ITipoPermisoService tipoPermisoService)
    {
        _tipoPermisoService = tipoPermisoService;
    }
    
    public async Task LoadDataAsync()
    {
        IsLoading = true;
        
        try
        {
            var result = ShowInactivos 
                ? await _tipoPermisoService.GetAllAsync()
                : await _tipoPermisoService.GetActivosAsync();
                
            if (result.Success && result.Data != null)
            {
                TiposPermiso.Clear();
                foreach (var tipo in result.Data.OrderBy(t => t.Nombre))
                {
                    TiposPermiso.Add(tipo);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar tipos de permiso: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private void Create()
    {
        _editingId = null;
        IsEditing = false;
        FormTitle = "Nuevo Tipo de Permiso";
        
        // Limpiar formulario
        Nombre = string.Empty;
        Descripcion = null;
        Color = "#1E88E5";
        RequiereAprobacion = true;
        RequiereDocumento = false;
        DiasPorDefecto = 1;
        EsCompensable = false;
        Activo = true;
        
        IsFormVisible = true;
    }
    
    [RelayCommand]
    private void Edit(TipoPermiso? tipoPermiso)
    {
        if (tipoPermiso == null) return;
        
        _editingId = tipoPermiso.Id;
        IsEditing = true;
        FormTitle = "Editar Tipo de Permiso";
        
        // Cargar datos
        Nombre = tipoPermiso.Nombre;
        Descripcion = tipoPermiso.Descripcion;
        Color = tipoPermiso.Color;
        RequiereAprobacion = tipoPermiso.RequiereAprobacion;
        RequiereDocumento = tipoPermiso.RequiereDocumento;
        DiasPorDefecto = tipoPermiso.DiasPorDefecto;
        EsCompensable = tipoPermiso.EsCompensable;
        Activo = tipoPermiso.Activo;
        
        IsFormVisible = true;
    }
    
    [RelayCommand]
    private async Task Save()
    {
        // Validaciones
        if (string.IsNullOrWhiteSpace(Nombre))
        {
            MessageBox.Show("El nombre es requerido", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        if (DiasPorDefecto < 0)
        {
            MessageBox.Show("Los días por defecto no pueden ser negativos", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        IsLoading = true;
        
        try
        {
            var tipoPermiso = new TipoPermiso
            {
                Nombre = Nombre.Trim(),
                Descripcion = Descripcion?.Trim(),
                Color = Color,
                RequiereAprobacion = RequiereAprobacion,
                RequiereDocumento = RequiereDocumento,
                DiasPorDefecto = DiasPorDefecto,
                EsCompensable = EsCompensable,
                Activo = Activo
            };
            
            if (IsEditing && _editingId.HasValue)
            {
                tipoPermiso.Id = _editingId.Value;
                var result = await _tipoPermisoService.UpdateAsync(tipoPermiso);
                
                if (result.Success)
                {
                    MessageBox.Show("Tipo de permiso actualizado exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    IsFormVisible = false;
                    await LoadDataAsync();
                }
                else
                {
                    MessageBox.Show($"Error: {string.Join(", ", result.Errors)}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                var result = await _tipoPermisoService.CreateAsync(tipoPermiso);
                
                if (result.Success)
                {
                    MessageBox.Show("Tipo de permiso creado exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    IsFormVisible = false;
                    await LoadDataAsync();
                }
                else
                {
                    MessageBox.Show($"Error: {string.Join(", ", result.Errors)}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private void CancelForm()
    {
        IsFormVisible = false;
    }
    
    [RelayCommand]
    private async Task Delete(TipoPermiso? tipoPermiso)
    {
        if (tipoPermiso == null) return;
        
        var result = MessageBox.Show(
            $"¿Está seguro de desactivar el tipo de permiso '{tipoPermiso.Nombre}'?",
            "Confirmar desactivación",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
            
        if (result == MessageBoxResult.Yes)
        {
            IsLoading = true;
            
            try
            {
                var deleteResult = await _tipoPermisoService.DeleteAsync(tipoPermiso.Id);
                
                if (deleteResult.Success)
                {
                    MessageBox.Show("Tipo de permiso desactivado exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadDataAsync();
                }
                else
                {
                    MessageBox.Show($"Error: {string.Join(", ", deleteResult.Errors)}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
    
    [RelayCommand]
    private async Task ToggleActivar(TipoPermiso? tipoPermiso)
    {
        if (tipoPermiso == null) return;
        
        tipoPermiso.Activo = !tipoPermiso.Activo;
        
        var result = await _tipoPermisoService.UpdateAsync(tipoPermiso);
        
        if (result.Success)
        {
            await LoadDataAsync();
        }
        else
        {
            MessageBox.Show($"Error: {string.Join(", ", result.Errors)}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    partial void OnShowInactivosChanged(bool value)
    {
        LoadDataAsync().SafeFireAndForget(onError: ex => 
            MessageBox.Show($"Error al cargar datos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error));
    }
}
