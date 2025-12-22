using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;
using SGRRHH.WPF.Helpers;
using System.Collections.ObjectModel;
using System.Windows;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// ViewModel para la lista de actividades
/// </summary>
public partial class ActividadesListViewModel : ViewModelBase
{
    private readonly IActividadService _actividadService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IDialogService _dialogService;
    
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
    private int _totalActividades;
    
    // Propiedades de navegación de vistas
    [ObservableProperty]
    private bool _isHomeVisible = true;
    
    [ObservableProperty]
    private bool _isListViewVisible;
    
    /// <summary>
    /// Texto de la fecha actual para el footer
    /// </summary>
    public string CurrentDateText => DateTime.Now.ToString("dddd, dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("es-ES"));
    
    // Lista de categorías predefinidas
    private static readonly string[] CategoriasPredefinidas = new[]
    {
        "Administrativa",
        "Técnica",
        "Campo",
        "Reuniones",
        "Capacitación",
        "Supervisión",
        "Documentación",
        "Mantenimiento",
        "Soporte",
        "Otro"
    };
    
    /// <summary>
    /// Evento para solicitar creación de actividad (abre ventana modal)
    /// </summary>
    public event EventHandler? CreateActividadRequested;
    
    /// <summary>
    /// Evento para solicitar edición de actividad (abre ventana modal)
    /// </summary>
    public event EventHandler<Actividad>? EditActividadRequested;
    
    public ActividadesListViewModel(IActividadService actividadService, IServiceProvider serviceProvider, IDialogService dialogService)
    {
        _actividadService = actividadService;
        _serviceProvider = serviceProvider;
        _dialogService = dialogService;
    }
    
    /// <summary>
    /// Muestra la pantalla de inicio
    /// </summary>
    [RelayCommand]
    private void ShowHome()
    {
        IsHomeVisible = true;
        IsListViewVisible = false;
    }
    
    /// <summary>
    /// Muestra la lista de actividades
    /// </summary>
    [RelayCommand]
    private async Task ShowListAsync()
    {
        IsHomeVisible = false;
        IsListViewVisible = true;
        await LoadDataAsync();
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
            await LoadCategoriasAsync();
            await SearchActividadesAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            _dialogService.ShowError($"Error al cargar los datos: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    private async Task LoadCategoriasAsync()
    {
        Categorias.Clear();
        Categorias.Add("Todas");
        
        // Agregar categorías predefinidas
        foreach (var cat in CategoriasPredefinidas)
        {
            if (!Categorias.Contains(cat))
            {
                Categorias.Add(cat);
            }
        }
        
        // Agregar categorías existentes en la BD
        try
        {
            var categoriasExistentes = await _actividadService.GetCategoriasAsync();
            foreach (var cat in categoriasExistentes)
            {
                if (!string.IsNullOrWhiteSpace(cat) && !Categorias.Contains(cat))
                {
                    Categorias.Add(cat);
                }
            }
        }
        catch
        {
            // Ignorar errores al cargar categorías
        }
        
        SelectedCategoria = "Todas";
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
            
            // Aplicar filtros adicionales si hay búsqueda y categoría
            if (!string.IsNullOrWhiteSpace(SelectedCategoria) && SelectedCategoria != "Todas" && 
                !string.IsNullOrWhiteSpace(SearchText))
            {
                resultado = resultado.Where(a => a.Categoria == SelectedCategoria);
            }
            
            Actividades.Clear();
            foreach (var actividad in resultado.OrderBy(a => a.Orden).ThenBy(a => a.Nombre))
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
    /// Abre ventana para crear nueva actividad
    /// </summary>
    [RelayCommand]
    private void NewActividad()
    {
        CreateActividadRequested?.Invoke(this, EventArgs.Empty);
    }
    
    /// <summary>
    /// Abre ventana para editar actividad seleccionada
    /// </summary>
    [RelayCommand]
    private void EditActividad()
    {
        if (SelectedActividad == null)
        {
            _dialogService.ShowInfo("Seleccione una actividad para editar.");
            return;
        }
        
        EditActividadRequested?.Invoke(this, SelectedActividad);
    }
    
    /// <summary>
    /// Elimina la actividad seleccionada
    /// </summary>
    [RelayCommand]
    private async Task DeleteActividadAsync()
    {
        if (SelectedActividad == null)
        {
            _dialogService.ShowInfo("Seleccione una actividad para eliminar.");
            return;
        }
        
        if (!_dialogService.ConfirmWarning(
            $"¿Está seguro de eliminar la actividad '{SelectedActividad.Nombre}'?\n\nEsta acción no se puede deshacer.",
            "Confirmar eliminación"))
            return;
        
        IsLoading = true;
        
        try
        {
            var result = await _actividadService.DeleteAsync(SelectedActividad.Id);
            
            if (result.Success)
            {
                _dialogService.ShowSuccess("Actividad eliminada exitosamente.");
                await LoadDataAsync();
            }
            else
            {
                _dialogService.ShowError(result.Message ?? "Error al eliminar la actividad.");
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Error al eliminar: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    partial void OnSelectedCategoriaChanged(string? value)
    {
        if (value != null && !IsLoading)
        {
            SearchActividadesAsync().SafeFireAndForget(showErrorMessage: false);
        }
    }
}
