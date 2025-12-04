using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGRRHH.Core.Common;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;
using SGRRHH.WPF.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// ViewModel para la lista de proyectos
/// </summary>
public partial class ProyectosListViewModel : ViewModelBase
{
    private readonly IProyectoService _proyectoService;
    private readonly IDialogService _dialogService;
    
    [ObservableProperty]
    private ObservableCollection<Proyecto> _proyectos = new();
    
    [ObservableProperty]
    private Proyecto? _selectedProyecto;
    
    [ObservableProperty]
    private string _searchText = string.Empty;
    
    [ObservableProperty]
    private EstadoProyecto? _selectedEstado;
    
    [ObservableProperty]
    private int _totalProyectos;
    
    // Propiedades de navegación de vistas
    [ObservableProperty]
    private bool _isHomeVisible = true;
    
    [ObservableProperty]
    private bool _isFormViewVisible;
    
    [ObservableProperty]
    private bool _isListViewVisible;
    
    /// <summary>
    /// Texto de la fecha actual para el footer
    /// </summary>
    public string CurrentDateText => DateTime.Now.ToString("dddd, dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("es-ES"));
    
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
    public ObservableCollection<EnumComboItem<EstadoProyecto>> Estados { get; } = new()
    {
        new EnumComboItem<EstadoProyecto> { Nombre = "Todos", Valor = null },
        new EnumComboItem<EstadoProyecto> { Nombre = "Activo", Valor = EstadoProyecto.Activo },
        new EnumComboItem<EstadoProyecto> { Nombre = "Suspendido", Valor = EstadoProyecto.Suspendido },
        new EnumComboItem<EstadoProyecto> { Nombre = "Finalizado", Valor = EstadoProyecto.Finalizado },
        new EnumComboItem<EstadoProyecto> { Nombre = "Cancelado", Valor = EstadoProyecto.Cancelado }
    };
    
    /// <summary>
    /// Lista de estados para el formulario
    /// </summary>
    public ObservableCollection<EnumComboItem<EstadoProyecto>> EstadosForm { get; } = new()
    {
        new EnumComboItem<EstadoProyecto> { Nombre = "Activo", Valor = EstadoProyecto.Activo },
        new EnumComboItem<EstadoProyecto> { Nombre = "Suspendido", Valor = EstadoProyecto.Suspendido },
        new EnumComboItem<EstadoProyecto> { Nombre = "Finalizado", Valor = EstadoProyecto.Finalizado },
        new EnumComboItem<EstadoProyecto> { Nombre = "Cancelado", Valor = EstadoProyecto.Cancelado }
    };
    
    public ProyectosListViewModel(IProyectoService proyectoService, IDialogService dialogService)
    {
        _proyectoService = proyectoService;
        _dialogService = dialogService;
    }
    
    /// <summary>
    /// Muestra la pantalla de inicio
    /// </summary>
    [RelayCommand]
    private void ShowHome()
    {
        IsHomeVisible = true;
        IsFormViewVisible = false;
        IsListViewVisible = false;
    }
    
    /// <summary>
    /// Muestra el formulario para crear nuevo proyecto
    /// </summary>
    [RelayCommand]
    private async Task ShowFormAsync()
    {
        IsHomeVisible = false;
        IsFormViewVisible = true;
        IsListViewVisible = false;
        await NewProyectoAsync();
    }
    
    /// <summary>
    /// Muestra la lista de proyectos
    /// </summary>
    [RelayCommand]
    private async Task ShowListAsync()
    {
        IsHomeVisible = false;
        IsFormViewVisible = false;
        IsListViewVisible = true;
        await SearchProyectosAsync();
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
            _dialogService.ShowError($"Error al cargar los datos: {ex.Message}");
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
            _dialogService.ShowInfo("Seleccione un proyecto para editar");
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
        
        // Cambiar a vista de formulario
        IsHomeVisible = false;
        IsFormViewVisible = true;
        IsListViewVisible = false;
    }
    
    /// <summary>
    /// Guarda el proyecto (crear o actualizar)
    /// </summary>
    [RelayCommand]
    private async Task SaveProyectoAsync()
    {
        if (string.IsNullOrWhiteSpace(Nombre))
        {
            _dialogService.ShowWarning("El nombre del proyecto es obligatorio", "Validación");
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
                _dialogService.ShowSuccess(result.Message);
                await SearchProyectosAsync();
                ShowHome();
            }
            else
            {
                _dialogService.ShowError(result.Message);
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Error al guardar: {ex.Message}");
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
            _dialogService.ShowInfo("Seleccione un proyecto para eliminar");
            return;
        }
        
        var confirmado = _dialogService.Confirm(
            $"¿Está seguro de eliminar el proyecto '{SelectedProyecto.Nombre}'?",
            "Confirmar eliminación");
            
        if (!confirmado) return;
        
        IsLoading = true;
        
        try
        {
            var result = await _proyectoService.DeleteAsync(SelectedProyecto.Id);
            
            if (result.Success)
            {
                _dialogService.ShowSuccess(result.Message);
                await SearchProyectosAsync();
            }
            else
            {
                _dialogService.ShowError(result.Message);
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
    
    /// <summary>
    /// Cancela la edición
    /// </summary>
    [RelayCommand]
    private void CancelEdit()
    {
        ShowHome();
    }
}
