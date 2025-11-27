using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGRRHH.Core.Common;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// ViewModel para la lista de proyectos
/// </summary>
public partial class ProyectosListViewModel : ObservableObject
{
    private readonly IProyectoService _proyectoService;
    
    [ObservableProperty]
    private ObservableCollection<Proyecto> _proyectos = new();
    
    [ObservableProperty]
    private Proyecto? _selectedProyecto;
    
    [ObservableProperty]
    private string _searchText = string.Empty;
    
    [ObservableProperty]
    private EstadoProyecto? _selectedEstado;
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private string _statusMessage = string.Empty;
    
    [ObservableProperty]
    private int _totalProyectos;
    
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
    private string _cliente = string.Empty;
    
    [ObservableProperty]
    private DateTime? _fechaInicio;
    
    [ObservableProperty]
    private DateTime? _fechaFin;
    
    [ObservableProperty]
    private EstadoProyecto _estado = EstadoProyecto.Activo;
    
    /// <summary>
    /// Lista de estados para el filtro
    /// </summary>
    public ObservableCollection<EstadoProyectoItem> Estados { get; } = new()
    {
        new EstadoProyectoItem { Nombre = "Todos", Valor = null },
        new EstadoProyectoItem { Nombre = "Activo", Valor = EstadoProyecto.Activo },
        new EstadoProyectoItem { Nombre = "Suspendido", Valor = EstadoProyecto.Suspendido },
        new EstadoProyectoItem { Nombre = "Finalizado", Valor = EstadoProyecto.Finalizado },
        new EstadoProyectoItem { Nombre = "Cancelado", Valor = EstadoProyecto.Cancelado }
    };
    
    /// <summary>
    /// Lista de estados para el formulario
    /// </summary>
    public ObservableCollection<EstadoProyectoItem> EstadosForm { get; } = new()
    {
        new EstadoProyectoItem { Nombre = "Activo", Valor = EstadoProyecto.Activo },
        new EstadoProyectoItem { Nombre = "Suspendido", Valor = EstadoProyecto.Suspendido },
        new EstadoProyectoItem { Nombre = "Finalizado", Valor = EstadoProyecto.Finalizado },
        new EstadoProyectoItem { Nombre = "Cancelado", Valor = EstadoProyecto.Cancelado }
    };
    
    public ProyectosListViewModel(IProyectoService proyectoService)
    {
        _proyectoService = proyectoService;
    }
    
    /// <summary>
    /// Carga inicial de datos
    /// </summary>
    public async Task LoadDataAsync()
    {
        IsLoading = true;
        StatusMessage = "Cargando proyectos...";
        
        try
        {
            await SearchProyectosAsync();
            var nextCode = await _proyectoService.GetNextCodigoAsync();
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
    /// Busca proyectos según los filtros
    /// </summary>
    [RelayCommand]
    private async Task SearchProyectosAsync()
    {
        IsLoading = true;
        StatusMessage = "Buscando...";
        
        try
        {
            IEnumerable<Proyecto> resultado;
            
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                resultado = await _proyectoService.SearchAsync(SearchText);
            }
            else if (SelectedEstado.HasValue)
            {
                resultado = await _proyectoService.GetByEstadoAsync(SelectedEstado.Value);
            }
            else
            {
                resultado = await _proyectoService.GetAllAsync();
            }
            
            // Aplicar filtros adicionales
            if (SelectedEstado.HasValue && !string.IsNullOrWhiteSpace(SearchText))
            {
                resultado = resultado.Where(p => p.Estado == SelectedEstado.Value);
            }
            
            Proyectos.Clear();
            foreach (var proyecto in resultado)
            {
                Proyectos.Add(proyecto);
            }
            
            TotalProyectos = Proyectos.Count;
            StatusMessage = $"{TotalProyectos} proyecto(s) encontrado(s)";
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
        SelectedEstado = null;
        await SearchProyectosAsync();
    }
    
    /// <summary>
    /// Prepara el formulario para crear nuevo proyecto
    /// </summary>
    [RelayCommand]
    private async Task NewProyectoAsync()
    {
        IsEditing = false;
        EditingId = 0;
        Codigo = await _proyectoService.GetNextCodigoAsync();
        Nombre = string.Empty;
        Descripcion = string.Empty;
        Cliente = string.Empty;
        FechaInicio = DateTime.Today;
        FechaFin = null;
        Estado = EstadoProyecto.Activo;
    }
    
    /// <summary>
    /// Prepara el formulario para editar proyecto
    /// </summary>
    [RelayCommand]
    private void EditProyecto()
    {
        if (SelectedProyecto == null)
        {
            MessageBox.Show("Seleccione un proyecto para editar", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        IsEditing = true;
        EditingId = SelectedProyecto.Id;
        Codigo = SelectedProyecto.Codigo;
        Nombre = SelectedProyecto.Nombre;
        Descripcion = SelectedProyecto.Descripcion ?? string.Empty;
        Cliente = SelectedProyecto.Cliente ?? string.Empty;
        FechaInicio = SelectedProyecto.FechaInicio;
        FechaFin = SelectedProyecto.FechaFin;
        Estado = SelectedProyecto.Estado;
    }
    
    /// <summary>
    /// Guarda el proyecto (crear o actualizar)
    /// </summary>
    [RelayCommand]
    private async Task SaveProyectoAsync()
    {
        if (string.IsNullOrWhiteSpace(Nombre))
        {
            MessageBox.Show("El nombre del proyecto es obligatorio", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        IsLoading = true;
        
        try
        {
            var proyecto = new Proyecto
            {
                Id = EditingId,
                Codigo = Codigo,
                Nombre = Nombre.Trim(),
                Descripcion = Descripcion,
                Cliente = Cliente,
                FechaInicio = FechaInicio,
                FechaFin = FechaFin,
                Estado = Estado
            };
            
            ServiceResult result;
            
            if (IsEditing)
            {
                result = await _proyectoService.UpdateAsync(proyecto);
            }
            else
            {
                var createResult = await _proyectoService.CreateAsync(proyecto);
                result = createResult;
            }
            
            if (result.Success)
            {
                MessageBox.Show(result.Message, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                await SearchProyectosAsync();
                await NewProyectoAsync();
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
    /// Elimina el proyecto seleccionado
    /// </summary>
    [RelayCommand]
    private async Task DeleteProyectoAsync()
    {
        if (SelectedProyecto == null)
        {
            MessageBox.Show("Seleccione un proyecto para eliminar", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        var confirm = MessageBox.Show(
            $"¿Está seguro de eliminar el proyecto '{SelectedProyecto.Nombre}'?",
            "Confirmar eliminación",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
            
        if (confirm != MessageBoxResult.Yes) return;
        
        IsLoading = true;
        
        try
        {
            var result = await _proyectoService.DeleteAsync(SelectedProyecto.Id);
            
            if (result.Success)
            {
                MessageBox.Show(result.Message, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                await SearchProyectosAsync();
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
        await NewProyectoAsync();
    }
}

/// <summary>
/// Item para el combo de estados de proyecto
/// </summary>
public class EstadoProyectoItem
{
    public string Nombre { get; set; } = string.Empty;
    public EstadoProyecto? Valor { get; set; }
}
