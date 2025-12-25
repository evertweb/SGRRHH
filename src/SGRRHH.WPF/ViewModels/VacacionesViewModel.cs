using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGRRHH.Core.Common;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;
using SGRRHH.Core.Models;
using SGRRHH.Infrastructure.Services;
using SGRRHH.WPF.Helpers;
using System.Collections.ObjectModel;
using System.Windows;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// ViewModel para la gestión de vacaciones
/// Permisos según matriz:
/// - Administrador: Todo (programar, editar, eliminar)
/// - Operador (Secretaria): Programar, Editar, Ver
/// - Aprobador (Ingeniera): Solo Ver
/// </summary>
public partial class VacacionesViewModel : ViewModelBase
{
    private readonly IVacacionService _vacacionService;
    private readonly IEmpleadoService _empleadoService;
    private readonly IDialogService _dialogService;
    private readonly IDateCalculationService _dateCalculationService;
    
    // Control de permisos por rol
    [ObservableProperty]
    private bool _puedeGestionar; // Crear/Editar vacaciones
    
    [ObservableProperty]
    private bool _puedeEliminar; // Solo Admin
    
    [ObservableProperty]
    private ObservableCollection<Empleado> _empleados = new();
    
    [ObservableProperty]
    private ObservableCollection<Vacacion> _vacaciones = new();
    
    [ObservableProperty]
    private Empleado? _selectedEmpleado;
    
    [ObservableProperty]
    private Vacacion? _selectedVacacion;
    
    [ObservableProperty]
    private ResumenVacaciones? _resumenVacaciones;
    
    [ObservableProperty]
    private bool _isFormVisible;
    
    // Propiedades para la pantalla de bienvenida
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsContentVisible))]
    private bool _isHomeVisible = true;
    
    /// <summary>
    /// Indica si el contenido principal debe mostrarse (inverso de IsHomeVisible)
    /// </summary>
    public bool IsContentVisible => !IsHomeVisible;
    
    /// <summary>
    /// Fecha actual formateada para el footer
    /// </summary>
    public string CurrentDateText => DateTime.Today.ToString("dddd, dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("es-ES"));
    
    [ObservableProperty]
    private bool _isEditing;
    
    // Propiedades del formulario
    [ObservableProperty]
    private DateTime _formFechaInicio = DateTime.Today.AddDays(7);
    
    [ObservableProperty]
    private DateTime _formFechaFin = DateTime.Today.AddDays(21);
    
    [ObservableProperty]
    private int _formPeriodo = DateTime.Today.Year;
    
    [ObservableProperty]
    private EstadoVacacion _formEstado = EstadoVacacion.Pendiente;
    
    [ObservableProperty]
    private string _formObservaciones = string.Empty;
    
    [ObservableProperty]
    private int _formDiasCalculados;
    
    /// <summary>
    /// Lista de estados de vacación disponibles (para visualización, no para cambio manual)
    /// </summary>
    public ObservableCollection<EstadoVacacion> EstadosVacacion { get; } = new()
    {
        EstadoVacacion.Pendiente,
        EstadoVacacion.Aprobada,
        EstadoVacacion.Rechazada,
        EstadoVacacion.Programada,
        EstadoVacacion.Disfrutada,
        EstadoVacacion.Cancelada
    };
    
    /// <summary>
    /// Lista de periodos disponibles (años)
    /// </summary>
    public ObservableCollection<int> PeriodosDisponibles { get; } = new();
    
    public VacacionesViewModel(
        IVacacionService vacacionService, 
        IEmpleadoService empleadoService,
        IDialogService dialogService,
        IDateCalculationService dateCalculationService)
    {
        _vacacionService = vacacionService;
        _empleadoService = empleadoService;
        _dialogService = dialogService;
        _dateCalculationService = dateCalculationService;
        
        // Determinar permisos según rol del usuario actual
        var rolActual = App.CurrentUser?.Rol ?? RolUsuario.Operador;
        
        // Ingeniera (Aprobador) solo puede ver - NO puede gestionar
        // Admin y Secretaria (Operador) pueden programar/editar
        PuedeGestionar = rolActual == RolUsuario.Administrador || rolActual == RolUsuario.Operador;
        
        // Solo Admin puede eliminar
        PuedeEliminar = rolActual == RolUsuario.Administrador;
        
        // Inicializar periodos (últimos 3 años y próximo año)
        var currentYear = DateTime.Today.Year;
        for (int year = currentYear - 2; year <= currentYear + 1; year++)
        {
            PeriodosDisponibles.Add(year);
        }
    }
    
    /// <summary>
    /// Carga inicial de datos
    /// </summary>
    public async Task LoadDataAsync()
    {
        IsLoading = true;
        StatusMessage = "Cargando empleados...";
        
        try
        {
            var empleados = await _empleadoService.GetAllAsync();
            Empleados.Clear();
            foreach (var emp in empleados.Where(e => e.Estado == EstadoEmpleado.Activo))
            {
                Empleados.Add(emp);
            }
            
            StatusMessage = $"{Empleados.Count} empleados activos";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            _dialogService.ShowError($"Error al cargar datos: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    /// <summary>
    /// Se ejecuta cuando cambia el empleado seleccionado
    /// </summary>
    partial void OnSelectedEmpleadoChanged(Empleado? value)
    {
        if (value != null)
        {
            LoadVacacionesEmpleadoAsync().SafeFireAndForget(showErrorMessage: false);
        }
        else
        {
            Vacaciones.Clear();
            ResumenVacaciones = null;
        }
    }
    
    /// <summary>
    /// Carga las vacaciones del empleado seleccionado
    /// </summary>
    private async Task LoadVacacionesEmpleadoAsync()
    {
        if (SelectedEmpleado == null) return;
        
        IsLoading = true;
        StatusMessage = "Cargando vacaciones...";
        
        try
        {
            // Cargar vacaciones
            var result = await _vacacionService.GetByEmpleadoIdAsync(SelectedEmpleado.Id);
            Vacaciones.Clear();
            
            if (result.Success && result.Data != null)
            {
                foreach (var vac in result.Data)
                {
                    Vacaciones.Add(vac);
                }
            }
            
            // Cargar resumen
            var resumenResult = await _vacacionService.GetResumenVacacionesAsync(SelectedEmpleado.Id);
            if (resumenResult.Success)
            {
                ResumenVacaciones = resumenResult.Data;
            }
            
            StatusMessage = $"{Vacaciones.Count} registros de vacaciones";
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
    /// Calcula los días hábiles cuando cambian las fechas
    /// </summary>
    partial void OnFormFechaInicioChanged(DateTime value) => CalcularDias();
    partial void OnFormFechaFinChanged(DateTime value) => CalcularDias();
    
    private void CalcularDias()
    {
        if (FormFechaFin >= FormFechaInicio)
        {
            // Fix #2: Usar servicio centralizado que resta festivos colombianos
            FormDiasCalculados = _dateCalculationService.CalcularDiasHabilesSinFestivos(FormFechaInicio, FormFechaFin);
        }
        else
        {
            FormDiasCalculados = 0;
        }
    }
    
    /// <summary>
    /// Muestra el formulario para nueva vacación
    /// </summary>
    [RelayCommand]
    private void NuevaVacacion()
    {
        if (!PuedeGestionar)
        {
            _dialogService.ShowWarning("No tiene permisos para programar vacaciones", "Permiso denegado");
            return;
        }
        
        if (SelectedEmpleado == null)
        {
            _dialogService.ShowInfo("Seleccione un empleado primero");
            return;
        }
        
        IsEditing = false;
        FormFechaInicio = DateTime.Today.AddDays(7);
        FormFechaFin = DateTime.Today.AddDays(21);
        FormPeriodo = DateTime.Today.Year;
        FormEstado = EstadoVacacion.Programada;
        FormObservaciones = string.Empty;
        CalcularDias();
        IsFormVisible = true;
    }
    
    /// <summary>
    /// Muestra el formulario para editar vacación
    /// </summary>
    [RelayCommand]
    private void EditarVacacion()
    {
        if (!PuedeGestionar)
        {
            _dialogService.ShowWarning("No tiene permisos para editar vacaciones", "Permiso denegado");
            return;
        }
        
        if (SelectedVacacion == null)
        {
            _dialogService.ShowInfo("Seleccione una vacación para editar");
            return;
        }
        
        if (SelectedVacacion.Estado == EstadoVacacion.Disfrutada && SelectedVacacion.FechaFin < DateTime.Today)
        {
            _dialogService.ShowWarning("No se puede editar una vacación ya disfrutada");
            return;
        }
        
        IsEditing = true;
        FormFechaInicio = SelectedVacacion.FechaInicio;
        FormFechaFin = SelectedVacacion.FechaFin;
        FormPeriodo = SelectedVacacion.PeriodoCorrespondiente;
        FormEstado = SelectedVacacion.Estado;
        FormObservaciones = SelectedVacacion.Observaciones ?? string.Empty;
        CalcularDias();
        IsFormVisible = true;
    }
    
    /// <summary>
    /// Guarda la vacación (nueva o editada)
    /// </summary>
    [RelayCommand]
    private async Task GuardarVacacionAsync()
    {
        if (SelectedEmpleado == null) return;
        
        if (FormFechaFin < FormFechaInicio)
        {
            _dialogService.ShowWarning("La fecha de fin debe ser mayor o igual a la fecha de inicio", "Validación");
            return;
        }
        
        IsLoading = true;
        
        try
        {
            if (IsEditing && SelectedVacacion != null)
            {
                // Actualizar vacación existente
                SelectedVacacion.FechaInicio = FormFechaInicio;
                SelectedVacacion.FechaFin = FormFechaFin;
                SelectedVacacion.PeriodoCorrespondiente = FormPeriodo;
                SelectedVacacion.Estado = FormEstado;
                SelectedVacacion.Observaciones = FormObservaciones;
                
                var result = await _vacacionService.UpdateAsync(SelectedVacacion);
                
                if (result.Success)
                {
                    _dialogService.ShowSuccess("Vacación actualizada exitosamente");
                    IsFormVisible = false;
                    await LoadVacacionesEmpleadoAsync();
                }
                else
                {
                    _dialogService.ShowError(result.Message);
                }
            }
            else
            {
                // Crear nueva vacación
                var nuevaVacacion = new Vacacion
                {
                    EmpleadoId = SelectedEmpleado.Id,
                    FechaInicio = FormFechaInicio,
                    FechaFin = FormFechaFin,
                    PeriodoCorrespondiente = FormPeriodo,
                    Estado = FormEstado,
                    Observaciones = FormObservaciones,
                    SolicitadoPorId = App.CurrentUser?.Id // Usuario que crea la solicitud
                };
                
                var result = await _vacacionService.CreateAsync(nuevaVacacion);
                
                if (result.Success)
                {
                    _dialogService.ShowSuccess("Vacación programada exitosamente");
                    IsFormVisible = false;
                    await LoadVacacionesEmpleadoAsync();
                }
                else
                {
                    _dialogService.ShowError(result.Message);
                }
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Error: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    /// <summary>
    /// Cancela el formulario
    /// </summary>
    [RelayCommand]
    private void CancelarFormulario()
    {
        IsFormVisible = false;
    }
    
    /// <summary>
    /// Muestra la pantalla de bienvenida
    /// </summary>
    [RelayCommand]
    private void ShowHome()
    {
        IsHomeVisible = true;
        IsFormVisible = false;
    }
    
    /// <summary>
    /// Muestra el contenido principal (historial)
    /// </summary>
    [RelayCommand]
    private void ShowContent()
    {
        IsHomeVisible = false;
    }
    
    /// <summary>
    /// Inicia el flujo de solicitar vacaciones desde la pantalla de bienvenida
    /// </summary>
    [RelayCommand]
    private void SolicitarVacaciones()
    {
        IsHomeVisible = false;
        // Llamar al comando de nueva vacación después de mostrar el contenido
        NuevaVacacion();
    }
    
    /// <summary>
    /// Elimina (cancela) la vacación seleccionada
    /// </summary>
    [RelayCommand]
    private async Task EliminarVacacionAsync()
    {
        if (!PuedeEliminar)
        {
            _dialogService.ShowWarning("Solo el administrador puede eliminar vacaciones", "Permiso denegado");
            return;
        }
        
        if (SelectedVacacion == null)
        {
            _dialogService.ShowInfo("Seleccione una vacación para eliminar");
            return;
        }
        
        var confirmado = _dialogService.Confirm(
            $"¿Está seguro de eliminar esta vacación?\n\nFechas: {SelectedVacacion.FechaInicio:dd/MM/yyyy} - {SelectedVacacion.FechaFin:dd/MM/yyyy}\nDías: {SelectedVacacion.DiasTomados}",
            "Confirmar eliminación");
            
        if (confirmado)
        {
            IsLoading = true;
            
            try
            {
                var result = await _vacacionService.DeleteAsync(SelectedVacacion.Id);
                
                if (result.Success)
                {
                    _dialogService.ShowSuccess("Vacación eliminada exitosamente");
                    await LoadVacacionesEmpleadoAsync();
                }
                else
                {
                    _dialogService.ShowError(result.Message);
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
    
    /// <summary>
    /// Marca una vacación programada como disfrutada
    /// </summary>
    [RelayCommand]
    private async Task MarcarDisfrutadaAsync()
    {
        if (!PuedeGestionar)
        {
            _dialogService.ShowWarning("No tiene permisos para modificar vacaciones", "Permiso denegado");
            return;
        }
        
        if (SelectedVacacion == null || SelectedVacacion.Estado != EstadoVacacion.Programada)
        {
            _dialogService.ShowInfo("Seleccione una vacación programada para marcar como disfrutada");
            return;
        }
        
        IsLoading = true;
        
        try
        {
            var result = await _vacacionService.MarcarComoDisfrutadaAsync(SelectedVacacion.Id);
            
            if (result.Success)
            {
                _dialogService.ShowSuccess("Vacación marcada como disfrutada");
                await LoadVacacionesEmpleadoAsync();
            }
            else
            {
                _dialogService.ShowError(result.Message);
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Error: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}
