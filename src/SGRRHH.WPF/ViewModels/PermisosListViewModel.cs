using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGRRHH.Core.Common;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// ViewModel para la lista de permisos con filtros
/// </summary>
public partial class PermisosListViewModel : ObservableObject
{
    private readonly IPermisoService _permisoService;
    private readonly IEmpleadoService _empleadoService;
    private readonly ITipoPermisoService _tipoPermisoService;
    
    [ObservableProperty]
    private ObservableCollection<Permiso> _permisos = new();
    
    [ObservableProperty]
    private ObservableCollection<Empleado> _empleados = new();
    
    [ObservableProperty]
    private ObservableCollection<TipoPermiso> _tiposPermiso = new();
    
    [ObservableProperty]
    private Permiso? _selectedPermiso;
    
    [ObservableProperty]
    private Empleado? _selectedEmpleado;
    
    [ObservableProperty]
    private EstadoPermiso? _selectedEstado;
    
    [ObservableProperty]
    private DateTime? _fechaDesde;
    
    [ObservableProperty]
    private DateTime? _fechaHasta;
    
    [ObservableProperty]
    private string _searchText = string.Empty;
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private int _totalPermisos;
    
    [ObservableProperty]
    private int _permisosPendientes;
    
    [ObservableProperty]
    private int _permisosAprobados;
    
    [ObservableProperty]
    private int _permisosRechazados;
    
    /// <summary>
    /// Lista de estados para el filtro
    /// </summary>
    public ObservableCollection<EstadoPermiso?> Estados { get; } = new()
    {
        null, // Todos
        EstadoPermiso.Pendiente,
        EstadoPermiso.Aprobado,
        EstadoPermiso.Rechazado,
        EstadoPermiso.Cancelado
    };
    
    /// <summary>
    /// Evento para crear nuevo permiso
    /// </summary>
    public event EventHandler? CreatePermisoRequested;
    
    /// <summary>
    /// Evento para editar permiso
    /// </summary>
    public event EventHandler<Permiso>? EditPermisoRequested;
    
    /// <summary>
    /// Evento para ver detalle de permiso
    /// </summary>
    public event EventHandler<Permiso>? ViewPermisoRequested;
    
    public PermisosListViewModel(
        IPermisoService permisoService,
        IEmpleadoService empleadoService,
        ITipoPermisoService tipoPermisoService)
    {
        _permisoService = permisoService;
        _empleadoService = empleadoService;
        _tipoPermisoService = tipoPermisoService;
        
        // Establecer fechas por defecto (último mes)
        FechaDesde = DateTime.Today.AddMonths(-1);
        FechaHasta = DateTime.Today.AddMonths(1);
    }
    
    public async Task LoadDataAsync()
    {
        IsLoading = true;
        
        try
        {
            // Cargar empleados
            var empleados = await _empleadoService.GetAllAsync();
            Empleados.Clear();
            foreach (var empleado in empleados.OrderBy(e => e.NombreCompleto))
            {
                Empleados.Add(empleado);
            }
            
            // Cargar tipos de permiso
            var tiposResult = await _tipoPermisoService.GetActivosAsync();
            if (tiposResult.Success && tiposResult.Data != null)
            {
                TiposPermiso.Clear();
                foreach (var tipo in tiposResult.Data)
                {
                    TiposPermiso.Add(tipo);
                }
            }
            
            // Cargar permisos
            await LoadPermisosAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar datos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    private async Task LoadPermisosAsync()
    {
        var result = await _permisoService.GetAllAsync(
            SelectedEmpleado?.Id,
            SelectedEstado,
            FechaDesde,
            FechaHasta);
            
        if (result.Success && result.Data != null)
        {
            var permisos = result.Data.ToList();
            
            // Aplicar filtro de búsqueda si existe
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.ToLower();
                permisos = permisos.Where(p =>
                    p.NumeroActa.ToLower().Contains(search) ||
                    p.Empleado.NombreCompleto.ToLower().Contains(search) ||
                    p.TipoPermiso.Nombre.ToLower().Contains(search) ||
                    p.Motivo.ToLower().Contains(search)
                ).ToList();
            }
            
            Permisos.Clear();
            foreach (var permiso in permisos)
            {
                Permisos.Add(permiso);
            }
            
            // Actualizar estadísticas
            TotalPermisos = Permisos.Count;
            PermisosPendientes = Permisos.Count(p => p.Estado == EstadoPermiso.Pendiente);
            PermisosAprobados = Permisos.Count(p => p.Estado == EstadoPermiso.Aprobado);
            PermisosRechazados = Permisos.Count(p => p.Estado == EstadoPermiso.Rechazado);
        }
    }
    
    [RelayCommand]
    private async Task Search()
    {
        await LoadPermisosAsync();
    }
    
    [RelayCommand]
    private async Task ClearFilters()
    {
        SelectedEmpleado = null;
        SelectedEstado = null;
        FechaDesde = DateTime.Today.AddMonths(-1);
        FechaHasta = DateTime.Today.AddMonths(1);
        SearchText = string.Empty;
        
        await LoadPermisosAsync();
    }
    
    [RelayCommand]
    private void Create()
    {
        CreatePermisoRequested?.Invoke(this, EventArgs.Empty);
    }
    
    [RelayCommand]
    private void Edit(Permiso? permiso)
    {
        if (permiso == null) return;
        
        if (permiso.Estado != EstadoPermiso.Pendiente)
        {
            MessageBox.Show("Solo se pueden editar permisos pendientes", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        EditPermisoRequested?.Invoke(this, permiso);
    }
    
    [RelayCommand]
    private void View(Permiso? permiso)
    {
        if (permiso == null) return;
        ViewPermisoRequested?.Invoke(this, permiso);
    }
    
    [RelayCommand]
    private async Task Cancel(Permiso? permiso)
    {
        if (permiso == null) return;
        
        if (permiso.Estado != EstadoPermiso.Pendiente)
        {
            MessageBox.Show("Solo se pueden cancelar permisos pendientes", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        var result = MessageBox.Show(
            $"¿Está seguro de cancelar el permiso {permiso.NumeroActa}?",
            "Confirmar cancelación",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
            
        if (result == MessageBoxResult.Yes)
        {
            var cancelResult = await _permisoService.CancelarPermisoAsync(permiso.Id, App.CurrentUser!.Id);
            
            if (cancelResult.Success)
            {
                MessageBox.Show("Permiso cancelado exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadPermisosAsync();
            }
            else
            {
                MessageBox.Show($"Error: {string.Join(", ", cancelResult.Errors)}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
    [RelayCommand]
    private async Task Delete(Permiso? permiso)
    {
        if (permiso == null) return;
        
        if (permiso.Estado != EstadoPermiso.Pendiente)
        {
            MessageBox.Show("Solo se pueden eliminar permisos pendientes", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        var result = MessageBox.Show(
            $"¿Está seguro de eliminar el permiso {permiso.NumeroActa}?",
            "Confirmar eliminación",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
            
        if (result == MessageBoxResult.Yes)
        {
            var deleteResult = await _permisoService.DeleteAsync(permiso.Id);
            
            if (deleteResult.Success)
            {
                MessageBox.Show("Permiso eliminado exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadPermisosAsync();
            }
            else
            {
                MessageBox.Show($"Error: {string.Join(", ", deleteResult.Errors)}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
    partial void OnSelectedEmpleadoChanged(Empleado? value)
    {
        _ = LoadPermisosAsync();
    }
    
    partial void OnSelectedEstadoChanged(EstadoPermiso? value)
    {
        _ = LoadPermisosAsync();
    }
    
    partial void OnFechaDesdeChanged(DateTime? value)
    {
        _ = LoadPermisosAsync();
    }
    
    partial void OnFechaHastaChanged(DateTime? value)
    {
        _ = LoadPermisosAsync();
    }
}
