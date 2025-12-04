using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// ViewModel para el formulario de crear/editar actividad
/// </summary>
public partial class ActividadFormViewModel : ViewModelBase
{
    private readonly IActividadService _actividadService;
    private readonly IDialogService _dialogService;
    
    /// <summary>
    /// Evento para cerrar la ventana
    /// </summary>
    public event EventHandler<bool>? CloseRequested;
    
    [ObservableProperty]
    private string _windowTitle = "Nueva Actividad";
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private bool _isEditing;
    
    [ObservableProperty]
    private int _editingId;
    
    // Campos del formulario
    [ObservableProperty]
    private string _codigo = string.Empty;
    
    [ObservableProperty]
    private string _nombre = string.Empty;
    
    [ObservableProperty]
    private string _descripcion = string.Empty;
    
    [ObservableProperty]
    private string? _categoriaSeleccionada;
    
    [ObservableProperty]
    private bool _requiereProyecto;
    
    [ObservableProperty]
    private int _orden = 1;
    
    [ObservableProperty]
    private ObservableCollection<string> _categoriasDisponibles = new();
    
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
    
    public ActividadFormViewModel(IActividadService actividadService, IDialogService dialogService)
    {
        _actividadService = actividadService;
        _dialogService = dialogService;
        
        // Inicializar categorías predefinidas
        foreach (var cat in CategoriasPredefinidas)
        {
            CategoriasDisponibles.Add(cat);
        }
    }
    
    /// <summary>
    /// Inicializa el formulario para crear nueva actividad
    /// </summary>
    public async Task InitializeForCreateAsync()
    {
        IsEditing = false;
        EditingId = 0;
        WindowTitle = "➕ Nueva Actividad";
        
        // Obtener siguiente código
        Codigo = await _actividadService.GetNextCodigoAsync();
        Nombre = string.Empty;
        Descripcion = string.Empty;
        CategoriaSeleccionada = CategoriasDisponibles.FirstOrDefault();
        RequiereProyecto = false;
        Orden = 1;
        
        // Agregar categorías existentes que no estén en las predefinidas
        await LoadExistingCategoriasAsync();
    }
    
    /// <summary>
    /// Inicializa el formulario para editar actividad existente
    /// </summary>
    public async Task InitializeForEditAsync(int actividadId)
    {
        IsEditing = true;
        EditingId = actividadId;
        WindowTitle = "✏️ Editar Actividad";
        
        var actividad = await _actividadService.GetByIdAsync(actividadId);
        if (actividad != null)
        {
            Codigo = actividad.Codigo;
            Nombre = actividad.Nombre;
            Descripcion = actividad.Descripcion ?? string.Empty;
            RequiereProyecto = actividad.RequiereProyecto;
            Orden = actividad.Orden;
            
            // Agregar categorías existentes
            await LoadExistingCategoriasAsync();
            
            // Seleccionar la categoría de la actividad
            if (!string.IsNullOrEmpty(actividad.Categoria))
            {
                if (!CategoriasDisponibles.Contains(actividad.Categoria))
                {
                    CategoriasDisponibles.Add(actividad.Categoria);
                }
                CategoriaSeleccionada = actividad.Categoria;
            }
            else
            {
                CategoriaSeleccionada = CategoriasDisponibles.FirstOrDefault();
            }
        }
    }
    
    private async Task LoadExistingCategoriasAsync()
    {
        try
        {
            var categoriasExistentes = await _actividadService.GetCategoriasAsync();
            foreach (var cat in categoriasExistentes)
            {
                if (!string.IsNullOrWhiteSpace(cat) && !CategoriasDisponibles.Contains(cat))
                {
                    CategoriasDisponibles.Add(cat);
                }
            }
        }
        catch
        {
            // Ignorar errores al cargar categorías existentes
        }
    }
    
    [RelayCommand]
    private async Task SaveAsync()
    {
        // Validaciones
        if (string.IsNullOrWhiteSpace(Nombre))
        {
            _dialogService.ShowWarning("El nombre de la actividad es obligatorio.", "Validación");
            return;
        }
        
        if (string.IsNullOrWhiteSpace(CategoriaSeleccionada))
        {
            _dialogService.ShowWarning("Debe seleccionar una categoría.", "Validación");
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
                Descripcion = string.IsNullOrWhiteSpace(Descripcion) ? null : Descripcion.Trim(),
                Categoria = CategoriaSeleccionada,
                RequiereProyecto = RequiereProyecto,
                Orden = Orden
            };
            
            if (IsEditing)
            {
                var result = await _actividadService.UpdateAsync(actividad);
                if (result.Success)
                {
                    _dialogService.ShowSuccess("Actividad actualizada exitosamente.", "Éxito");
                    CloseRequested?.Invoke(this, true);
                }
                else
                {
                    _dialogService.ShowError(result.Message ?? "Error al actualizar la actividad.", "Error");
                }
            }
            else
            {
                var result = await _actividadService.CreateAsync(actividad);
                if (result.Success)
                {
                    _dialogService.ShowSuccess("Actividad creada exitosamente.", "Éxito");
                    CloseRequested?.Invoke(this, true);
                }
                else
                {
                    _dialogService.ShowError(result.Message ?? "Error al crear la actividad.", "Error");
                }
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Error al guardar: {ex.Message}", "Error");
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private void Cancel()
    {
        CloseRequested?.Invoke(this, false);
    }
}
