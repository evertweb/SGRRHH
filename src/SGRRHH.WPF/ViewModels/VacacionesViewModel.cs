using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGRRHH.Core.Common;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;
using SGRRHH.Core.Models;
using SGRRHH.Infrastructure.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// ViewModel para la gestión de vacaciones
/// </summary>
public partial class VacacionesViewModel : ObservableObject
{
    private readonly IVacacionService _vacacionService;
    private readonly IEmpleadoService _empleadoService;
    
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
    private bool _isLoading;
    
    [ObservableProperty]
    private string _statusMessage = string.Empty;
    
    [ObservableProperty]
    private bool _isFormVisible;
    
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
    private EstadoVacacion _formEstado = EstadoVacacion.Programada;
    
    [ObservableProperty]
    private string _formObservaciones = string.Empty;
    
    [ObservableProperty]
    private int _formDiasCalculados;
    
    /// <summary>
    /// Lista de estados de vacación disponibles
    /// </summary>
    public ObservableCollection<EstadoVacacion> EstadosVacacion { get; } = new()
    {
        EstadoVacacion.Programada,
        EstadoVacacion.Disfrutada,
        EstadoVacacion.Cancelada
    };
    
    /// <summary>
    /// Lista de periodos disponibles (años)
    /// </summary>
    public ObservableCollection<int> PeriodosDisponibles { get; } = new();
    
    public VacacionesViewModel(IVacacionService vacacionService, IEmpleadoService empleadoService)
    {
        _vacacionService = vacacionService;
        _empleadoService = empleadoService;
        
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
            MessageBox.Show($"Error al cargar datos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            _ = LoadVacacionesEmpleadoAsync();
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
            int dias = 0;
            var fecha = FormFechaInicio;
            while (fecha <= FormFechaFin)
            {
                if (fecha.DayOfWeek != DayOfWeek.Saturday && fecha.DayOfWeek != DayOfWeek.Sunday)
                    dias++;
                fecha = fecha.AddDays(1);
            }
            FormDiasCalculados = dias;
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
        if (SelectedEmpleado == null)
        {
            MessageBox.Show("Seleccione un empleado primero", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
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
        if (SelectedVacacion == null)
        {
            MessageBox.Show("Seleccione una vacación para editar", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        if (SelectedVacacion.Estado == EstadoVacacion.Disfrutada && SelectedVacacion.FechaFin < DateTime.Today)
        {
            MessageBox.Show("No se puede editar una vacación ya disfrutada", "Información", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            MessageBox.Show("La fecha de fin debe ser mayor o igual a la fecha de inicio", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    MessageBox.Show("Vacación actualizada exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    IsFormVisible = false;
                    await LoadVacacionesEmpleadoAsync();
                }
                else
                {
                    MessageBox.Show(result.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    Observaciones = FormObservaciones
                };
                
                var result = await _vacacionService.CreateAsync(nuevaVacacion);
                
                if (result.Success)
                {
                    MessageBox.Show("Vacación programada exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    IsFormVisible = false;
                    await LoadVacacionesEmpleadoAsync();
                }
                else
                {
                    MessageBox.Show(result.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
    
    /// <summary>
    /// Cancela el formulario
    /// </summary>
    [RelayCommand]
    private void CancelarFormulario()
    {
        IsFormVisible = false;
    }
    
    /// <summary>
    /// Elimina (cancela) la vacación seleccionada
    /// </summary>
    [RelayCommand]
    private async Task EliminarVacacionAsync()
    {
        if (SelectedVacacion == null)
        {
            MessageBox.Show("Seleccione una vacación para eliminar", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        var confirmResult = MessageBox.Show(
            $"¿Está seguro de eliminar esta vacación?\n\nFechas: {SelectedVacacion.FechaInicio:dd/MM/yyyy} - {SelectedVacacion.FechaFin:dd/MM/yyyy}\nDías: {SelectedVacacion.DiasTomados}",
            "Confirmar eliminación",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
            
        if (confirmResult == MessageBoxResult.Yes)
        {
            IsLoading = true;
            
            try
            {
                var result = await _vacacionService.DeleteAsync(SelectedVacacion.Id);
                
                if (result.Success)
                {
                    MessageBox.Show("Vacación eliminada exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadVacacionesEmpleadoAsync();
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
    }
    
    /// <summary>
    /// Marca una vacación programada como disfrutada
    /// </summary>
    [RelayCommand]
    private async Task MarcarDisfrutadaAsync()
    {
        if (SelectedVacacion == null || SelectedVacacion.Estado != EstadoVacacion.Programada)
        {
            MessageBox.Show("Seleccione una vacación programada para marcar como disfrutada", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        IsLoading = true;
        
        try
        {
            var result = await _vacacionService.MarcarComoDisfrutadaAsync(SelectedVacacion.Id);
            
            if (result.Success)
            {
                MessageBox.Show("Vacación marcada como disfrutada", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadVacacionesEmpleadoAsync();
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
}
