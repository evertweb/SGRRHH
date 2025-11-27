using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGRRHH.Core.Common;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// ViewModel para la lista de actividades
/// </summary>
public partial class ActividadesListViewModel : ObservableObject
{
    private readonly IActividadService _actividadService;
    
    [ObservableProperty]
    private ObservableCollection<Actividad> _actividades = new();
    
    [ObservableProperty]
    private ObservableCollection<string> _categorias = new();
    
    [ObservableProperty]
    private Actividad? _selectedActividad;
    
    [ObservableProperty]
    private string _searchText = string.Empty;
    
    [ObservableProperty]
    private string? _selectedCategoria;
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private string _statusMessage = string.Empty;
    
    [ObservableProperty]
    private int _totalActividades;
    
    // Campos para el formulario
    [ObservableProperty]
    private bool _isEditing;
    
    [ObservableProperty]
    private int _editingId;
    
    [ObservableProperty]
    private string _codigo = string.Empty;
    
    [ObservableProperty]
    private string _nombre = string.Empty;
    
    [ObservableProperty]
    private string _descripcion = string.Empty;
    
    [ObservableProperty]
    private string _categoria = string.Empty;
    
    [ObservableProperty]
    private bool _requiereProyecto;
    
    [ObservableProperty]
    private int _orden = 1;
    
    public ActividadesListViewModel(IActividadService actividadService)
    {
        _actividadService = actividadService;
    }
    
    /// <summary>
    /// Carga inicial de datos
    /// </summary>
    public async Task LoadDataAsync()
    {
        IsLoading = true;
        StatusMessage = "Cargando actividades...";
        
        try
        {
            // Cargar categorías para el filtro
            var categorias = await _actividadService.GetCategoriasAsync();
            Categorias.Clear();
            Categorias.Add("Todas");
            foreach (var cat in categorias)
            {
                Categorias.Add(cat);
            }
            
            await SearchActividadesAsync();
            var nextCode = await _actividadService.GetNextCodigoAsync();
            Codigo = nextCode;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            MessageBox.Show($"Error al cargar los datos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    /// <summary>
    /// Busca actividades según los filtros
    /// </summary>
    [RelayCommand]
    private async Task SearchActividadesAsync()
    {
        IsLoading = true;
        StatusMessage = "Buscando...";
        
        try
        {
            IEnumerable<Actividad> resultado;
            
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                resultado = await _actividadService.SearchAsync(SearchText);
            }
            else if (!string.IsNullOrWhiteSpace(SelectedCategoria) && SelectedCategoria != "Todas")
            {
                resultado = await _actividadService.GetByCategoriaAsync(SelectedCategoria);
            }
            else
            {
                resultado = await _actividadService.GetAllAsync();
            }
            
            // Aplicar filtros adicionales
            if (!string.IsNullOrWhiteSpace(SelectedCategoria) && SelectedCategoria != "Todas" && 
                !string.IsNullOrWhiteSpace(SearchText))
            {
                resultado = resultado.Where(a => a.Categoria == SelectedCategoria);
            }
            
            Actividades.Clear();
            foreach (var actividad in resultado)
            {
                Actividades.Add(actividad);
            }
            
            TotalActividades = Actividades.Count;
            StatusMessage = $"{TotalActividades} actividad(es) encontrada(s)";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    /// <summary>
    /// Limpia los filtros
    /// </summary>
    [RelayCommand]
    private async Task ClearFiltersAsync()
    {
        SearchText = string.Empty;
        SelectedCategoria = "Todas";
        await SearchActividadesAsync();
    }
    
    /// <summary>
    /// Prepara el formulario para crear nueva actividad
    /// </summary>
    [RelayCommand]
    private async Task NewActividadAsync()
    {
        IsEditing = false;
        EditingId = 0;
        Codigo = await _actividadService.GetNextCodigoAsync();
        Nombre = string.Empty;
        Descripcion = string.Empty;
        Categoria = string.Empty;
        RequiereProyecto = false;
        Orden = 1;
    }
    
    /// <summary>
    /// Prepara el formulario para editar actividad
    /// </summary>
    [RelayCommand]
    private void EditActividad()
    {
        if (SelectedActividad == null)
        {
            MessageBox.Show("Seleccione una actividad para editar", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        IsEditing = true;
        EditingId = SelectedActividad.Id;
        Codigo = SelectedActividad.Codigo;
        Nombre = SelectedActividad.Nombre;
        Descripcion = SelectedActividad.Descripcion ?? string.Empty;
        Categoria = SelectedActividad.Categoria ?? string.Empty;
        RequiereProyecto = SelectedActividad.RequiereProyecto;
        Orden = SelectedActividad.Orden;
    }
    
    /// <summary>
    /// Guarda la actividad (crear o actualizar)
    /// </summary>
    [RelayCommand]
    private async Task SaveActividadAsync()
    {
        if (string.IsNullOrWhiteSpace(Nombre))
        {
            MessageBox.Show("El nombre de la actividad es obligatorio", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        IsLoading = true;
        
        try
        {
            var actividad = new Actividad
            {
                Id = EditingId,
                Codigo = Codigo,
                Nombre = Nombre.Trim(),
                Descripcion = Descripcion,
                Categoria = Categoria,
                RequiereProyecto = RequiereProyecto,
                Orden = Orden
            };
            
            ServiceResult result;
            
            if (IsEditing)
            {
                result = await _actividadService.UpdateAsync(actividad);
            }
            else
            {
                var createResult = await _actividadService.CreateAsync(actividad);
                result = createResult;
            }
            
            if (result.Success)
            {
                MessageBox.Show(result.Message, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Recargar categorías
                var categorias = await _actividadService.GetCategoriasAsync();
                Categorias.Clear();
                Categorias.Add("Todas");
                foreach (var cat in categorias)
                {
                    Categorias.Add(cat);
                }
                
                await SearchActividadesAsync();
                await NewActividadAsync();
            }
            else
            {
                MessageBox.Show(result.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al guardar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    /// <summary>
    /// Elimina la actividad seleccionada
    /// </summary>
    [RelayCommand]
    private async Task DeleteActividadAsync()
    {
        if (SelectedActividad == null)
        {
            MessageBox.Show("Seleccione una actividad para eliminar", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        var confirm = MessageBox.Show(
            $"¿Está seguro de eliminar la actividad '{SelectedActividad.Nombre}'?",
            "Confirmar eliminación",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
            
        if (confirm != MessageBoxResult.Yes) return;
        
        IsLoading = true;
        
        try
        {
            var result = await _actividadService.DeleteAsync(SelectedActividad.Id);
            
            if (result.Success)
            {
                MessageBox.Show(result.Message, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                await SearchActividadesAsync();
            }
            else
            {
                MessageBox.Show(result.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al eliminar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
    private async Task CancelEditAsync()
    {
        await NewActividadAsync();
    }
    
    partial void OnSelectedCategoriaChanged(string? value)
    {
        if (value != null)
        {
            _ = SearchActividadesAsync();
        }
    }
}
