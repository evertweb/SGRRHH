using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// ViewModel para el control diario de actividades
/// </summary>
public partial class ControlDiarioViewModel : ObservableObject
{
    private readonly IControlDiarioService _controlDiarioService;
    private readonly IEmpleadoService _empleadoService;
    private readonly IActividadService _actividadService;
    private readonly IProyectoService _proyectoService;
    
    [ObservableProperty]
    private ObservableCollection<Empleado> _empleados = new();
    
    [ObservableProperty]
    private ObservableCollection<Actividad> _actividades = new();
    
    [ObservableProperty]
    private ObservableCollection<Proyecto> _proyectos = new();
    
    [ObservableProperty]
    private ObservableCollection<DetalleActividad> _detallesActividades = new();
    
    [ObservableProperty]
    private ObservableCollection<RegistroDiario> _registrosRecientes = new();
    
    [ObservableProperty]
    private Empleado? _selectedEmpleado;
    
    [ObservableProperty]
    private DateTime _selectedFecha = DateTime.Today;
    
    [ObservableProperty]
    private RegistroDiario? _registroActual;
    
    [ObservableProperty]
    private TimeSpan? _horaEntrada;
    
    [ObservableProperty]
    private TimeSpan? _horaSalida;
    
    [ObservableProperty]
    private string _observaciones = string.Empty;
    
    [ObservableProperty]
    private DetalleActividad? _selectedDetalle;
    
    [ObservableProperty]
    private Actividad? _selectedActividad;
    
    [ObservableProperty]
    private Proyecto? _selectedProyecto;
    
    [ObservableProperty]
    private decimal _horas = 1;
    
    [ObservableProperty]
    private string _descripcionActividad = string.Empty;
    
    [ObservableProperty]
    private decimal _totalHoras;
    
    [ObservableProperty]
    private decimal _horasMesActual;
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private string _statusMessage = string.Empty;
    
    [ObservableProperty]
    private bool _isEditMode;
    
    [ObservableProperty]
    private bool _canEdit = true;
    
    [ObservableProperty]
    private bool _requiereProyecto;
    
    public ControlDiarioViewModel(
        IControlDiarioService controlDiarioService,
        IEmpleadoService empleadoService,
        IActividadService actividadService,
        IProyectoService proyectoService)
    {
        _controlDiarioService = controlDiarioService;
        _empleadoService = empleadoService;
        _actividadService = actividadService;
        _proyectoService = proyectoService;
    }
    
    /// <summary>
    /// Carga inicial de datos
    /// </summary>
    public async Task LoadDataAsync()
    {
        IsLoading = true;
        StatusMessage = "Cargando datos...";
        
        try
        {
            // Cargar empleados activos
            var empleados = await _empleadoService.GetAllAsync();
            Empleados.Clear();
            foreach (var emp in empleados)
            {
                Empleados.Add(emp);
            }
            
            // Cargar actividades
            var actividades = await _actividadService.GetAllAsync();
            Actividades.Clear();
            foreach (var act in actividades)
            {
                Actividades.Add(act);
            }
            
            // Cargar proyectos activos
            var proyectos = await _proyectoService.GetByEstadoAsync(EstadoProyecto.Activo);
            Proyectos.Clear();
            Proyectos.Add(new Proyecto { Id = 0, Nombre = "Sin proyecto" });
            foreach (var pry in proyectos)
            {
                Proyectos.Add(pry);
            }
            
            StatusMessage = "Datos cargados. Seleccione un empleado.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al cargar datos: {ex.Message}";
            MessageBox.Show($"Error al cargar datos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    /// <summary>
    /// Carga el registro del empleado para la fecha seleccionada
    /// </summary>
    [RelayCommand]
    private async Task LoadRegistroAsync()
    {
        if (SelectedEmpleado == null)
        {
            StatusMessage = "Seleccione un empleado";
            return;
        }
        
        IsLoading = true;
        StatusMessage = "Cargando registro...";
        
        try
        {
            var registro = await _controlDiarioService.GetRegistroByFechaEmpleadoAsync(SelectedFecha, SelectedEmpleado.Id);
            
            if (registro != null)
            {
                RegistroActual = registro;
                HoraEntrada = registro.HoraEntrada;
                HoraSalida = registro.HoraSalida;
                Observaciones = registro.Observaciones ?? string.Empty;
                
                DetallesActividades.Clear();
                if (registro.DetallesActividades != null)
                {
                    foreach (var detalle in registro.DetallesActividades.OrderBy(d => d.Orden))
                    {
                        DetallesActividades.Add(detalle);
                    }
                }
                
                CanEdit = registro.Estado == EstadoRegistroDiario.Borrador;
                StatusMessage = CanEdit ? "Registro cargado" : "Registro completado (solo lectura)";
            }
            else
            {
                // Crear nuevo registro en memoria
                RegistroActual = new RegistroDiario
                {
                    Fecha = SelectedFecha,
                    EmpleadoId = SelectedEmpleado.Id,
                    Empleado = SelectedEmpleado,
                    Estado = EstadoRegistroDiario.Borrador
                };
                
                HoraEntrada = null;
                HoraSalida = null;
                Observaciones = string.Empty;
                DetallesActividades.Clear();
                CanEdit = true;
                StatusMessage = "Nuevo registro";
            }
            
            // Cargar registros recientes
            await LoadRegistrosRecientesAsync();
            
            // Calcular horas del mes
            HorasMesActual = await _controlDiarioService.GetHorasMesActualAsync(SelectedEmpleado.Id);
            
            UpdateTotalHoras();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            MessageBox.Show($"Error al cargar el registro: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    private async Task LoadRegistrosRecientesAsync()
    {
        if (SelectedEmpleado == null) return;
        
        var registros = await _controlDiarioService.GetByEmpleadoMesActualAsync(SelectedEmpleado.Id);
        RegistrosRecientes.Clear();
        foreach (var reg in registros.Take(10))
        {
            RegistrosRecientes.Add(reg);
        }
    }
    
    /// <summary>
    /// Guarda el registro diario (crea o actualiza)
    /// </summary>
    [RelayCommand]
    private async Task SaveRegistroAsync()
    {
        if (SelectedEmpleado == null)
        {
            MessageBox.Show("Seleccione un empleado", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        if (!CanEdit)
        {
            MessageBox.Show("El registro ya está completado y no se puede modificar", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        IsLoading = true;
        StatusMessage = "Guardando registro...";
        
        try
        {
            var registro = RegistroActual ?? new RegistroDiario();
            registro.Fecha = SelectedFecha;
            registro.EmpleadoId = SelectedEmpleado.Id;
            registro.HoraEntrada = HoraEntrada;
            registro.HoraSalida = HoraSalida;
            registro.Observaciones = Observaciones;
            
            var result = await _controlDiarioService.SaveRegistroAsync(registro);
            
            if (result.Success)
            {
                RegistroActual = result.Data;
                StatusMessage = result.Message;
                MessageBox.Show(result.Message, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                StatusMessage = result.Message;
                MessageBox.Show(result.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            MessageBox.Show($"Error al guardar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    /// <summary>
    /// Agrega una actividad al registro
    /// </summary>
    [RelayCommand]
    private async Task AddActividadAsync()
    {
        if (SelectedEmpleado == null)
        {
            MessageBox.Show("Seleccione un empleado primero", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        if (SelectedActividad == null)
        {
            MessageBox.Show("Seleccione una actividad", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        if (Horas <= 0 || Horas > 24)
        {
            MessageBox.Show("Las horas deben ser entre 0.1 y 24", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        if (SelectedActividad.RequiereProyecto && (SelectedProyecto == null || SelectedProyecto.Id == 0))
        {
            MessageBox.Show("Esta actividad requiere seleccionar un proyecto", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        if (!CanEdit)
        {
            MessageBox.Show("El registro ya está completado", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        IsLoading = true;
        
        try
        {
            // Primero asegurar que el registro existe
            if (RegistroActual == null || RegistroActual.Id == 0)
            {
                var registro = new RegistroDiario
                {
                    Fecha = SelectedFecha,
                    EmpleadoId = SelectedEmpleado.Id,
                    HoraEntrada = HoraEntrada,
                    HoraSalida = HoraSalida,
                    Observaciones = Observaciones
                };
                
                var saveResult = await _controlDiarioService.SaveRegistroAsync(registro);
                if (!saveResult.Success)
                {
                    MessageBox.Show(saveResult.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                RegistroActual = saveResult.Data;
            }
            
            var detalle = new DetalleActividad
            {
                ActividadId = SelectedActividad.Id,
                Actividad = SelectedActividad,
                ProyectoId = SelectedProyecto?.Id > 0 ? SelectedProyecto.Id : null,
                Proyecto = SelectedProyecto?.Id > 0 ? SelectedProyecto : null,
                Horas = Horas,
                Descripcion = DescripcionActividad
            };
            
            var result = await _controlDiarioService.AddActividadAsync(RegistroActual!.Id, detalle);
            
            if (result.Success)
            {
                DetallesActividades.Add(result.Data!);
                UpdateTotalHoras();
                
                // Limpiar formulario de actividad
                SelectedActividad = null;
                SelectedProyecto = Proyectos.FirstOrDefault();
                Horas = 1;
                DescripcionActividad = string.Empty;
                
                StatusMessage = "Actividad agregada";
            }
            else
            {
                MessageBox.Show(result.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
    
    /// <summary>
    /// Elimina una actividad del registro
    /// </summary>
    [RelayCommand]
    private async Task DeleteActividadAsync()
    {
        if (SelectedDetalle == null)
        {
            MessageBox.Show("Seleccione una actividad para eliminar", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        if (!CanEdit)
        {
            MessageBox.Show("El registro ya está completado", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        var confirm = MessageBox.Show(
            $"¿Está seguro de eliminar la actividad '{SelectedDetalle.Actividad?.Nombre}'?",
            "Confirmar eliminación",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
            
        if (confirm != MessageBoxResult.Yes) return;
        
        IsLoading = true;
        
        try
        {
            var result = await _controlDiarioService.DeleteActividadAsync(SelectedDetalle.Id);
            
            if (result.Success)
            {
                DetallesActividades.Remove(SelectedDetalle);
                SelectedDetalle = null;
                UpdateTotalHoras();
                StatusMessage = "Actividad eliminada";
            }
            else
            {
                MessageBox.Show(result.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
    
    /// <summary>
    /// Marca el registro como completado
    /// </summary>
    [RelayCommand]
    private async Task CompletarRegistroAsync()
    {
        if (RegistroActual == null || RegistroActual.Id == 0)
        {
            MessageBox.Show("Guarde el registro primero", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        if (!DetallesActividades.Any())
        {
            MessageBox.Show("Debe agregar al menos una actividad", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        var confirm = MessageBox.Show(
            "¿Está seguro de marcar el registro como completado?\nNo podrá modificarlo después.",
            "Confirmar completar",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
            
        if (confirm != MessageBoxResult.Yes) return;
        
        IsLoading = true;
        
        try
        {
            var result = await _controlDiarioService.CompletarRegistroAsync(RegistroActual.Id);
            
            if (result.Success)
            {
                CanEdit = false;
                RegistroActual.Estado = EstadoRegistroDiario.Completado;
                StatusMessage = "Registro completado";
                MessageBox.Show(result.Message, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Recargar registros recientes
                await LoadRegistrosRecientesAsync();
            }
            else
            {
                MessageBox.Show(result.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
    
    /// <summary>
    /// Navega al día anterior
    /// </summary>
    [RelayCommand]
    private async Task DiaAnteriorAsync()
    {
        SelectedFecha = SelectedFecha.AddDays(-1);
        await LoadRegistroAsync();
    }
    
    /// <summary>
    /// Navega al día siguiente
    /// </summary>
    [RelayCommand]
    private async Task DiaSiguienteAsync()
    {
        if (SelectedFecha.Date < DateTime.Today)
        {
            SelectedFecha = SelectedFecha.AddDays(1);
            await LoadRegistroAsync();
        }
    }
    
    /// <summary>
    /// Va al día de hoy
    /// </summary>
    [RelayCommand]
    private async Task IrAHoyAsync()
    {
        SelectedFecha = DateTime.Today;
        await LoadRegistroAsync();
    }
    
    private void UpdateTotalHoras()
    {
        TotalHoras = DetallesActividades.Sum(d => d.Horas);
    }
    
    partial void OnSelectedEmpleadoChanged(Empleado? value)
    {
        if (value != null)
        {
            _ = LoadRegistroAsync();
        }
    }
    
    partial void OnSelectedFechaChanged(DateTime value)
    {
        if (SelectedEmpleado != null)
        {
            _ = LoadRegistroAsync();
        }
    }
    
    partial void OnSelectedActividadChanged(Actividad? value)
    {
        RequiereProyecto = value?.RequiereProyecto ?? false;
    }
}
