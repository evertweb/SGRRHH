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
/// ViewModel para la gestión de contratos
/// </summary>
public partial class ContratosViewModel : ObservableObject
{
    private readonly IContratoService _contratoService;
    private readonly IEmpleadoService _empleadoService;
    private readonly ICargoService _cargoService;
    
    [ObservableProperty]
    private ObservableCollection<Empleado> _empleados = new();
    
    [ObservableProperty]
    private ObservableCollection<Contrato> _contratos = new();
    
    [ObservableProperty]
    private ObservableCollection<Contrato> _contratosProximosAVencer = new();
    
    [ObservableProperty]
    private ObservableCollection<Cargo> _cargos = new();
    
    [ObservableProperty]
    private Empleado? _selectedEmpleado;
    
    [ObservableProperty]
    private Contrato? _selectedContrato;
    
    [ObservableProperty]
    private Contrato? _contratoActivo;
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private string _statusMessage = string.Empty;
    
    [ObservableProperty]
    private bool _isFormVisible;
    
    [ObservableProperty]
    private bool _isEditing;
    
    [ObservableProperty]
    private bool _isRenovando;
    
    // Propiedades del formulario
    [ObservableProperty]
    private TipoContrato _formTipoContrato = TipoContrato.Fijo;
    
    [ObservableProperty]
    private DateTime _formFechaInicio = DateTime.Today;
    
    [ObservableProperty]
    private DateTime? _formFechaFin = DateTime.Today.AddYears(1);
    
    [ObservableProperty]
    private decimal _formSalario;
    
    [ObservableProperty]
    private Cargo? _formCargo;
    
    [ObservableProperty]
    private string _formObservaciones = string.Empty;
    
    [ObservableProperty]
    private int _diasRestantesContrato;
    
    /// <summary>
    /// Lista de tipos de contrato
    /// </summary>
    public ObservableCollection<TipoContrato> TiposContrato { get; } = new()
    {
        TipoContrato.Indefinido,
        TipoContrato.Fijo,
        TipoContrato.ObraLabor,
        TipoContrato.PrestacionServicios,
        TipoContrato.Aprendizaje
    };
    
    public ContratosViewModel(IContratoService contratoService, IEmpleadoService empleadoService, ICargoService cargoService)
    {
        _contratoService = contratoService;
        _empleadoService = empleadoService;
        _cargoService = cargoService;
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
            // Cargar empleados
            var empleados = await _empleadoService.GetAllAsync();
            Empleados.Clear();
            foreach (var emp in empleados.Where(e => e.Estado == EstadoEmpleado.Activo))
            {
                Empleados.Add(emp);
            }
            
            // Cargar cargos
            var cargos = await _cargoService.GetAllAsync();
            Cargos.Clear();
            foreach (var cargo in cargos)
            {
                Cargos.Add(cargo);
            }
            
            // Cargar contratos próximos a vencer
            await LoadContratosProximosAVencerAsync();
            
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
    /// Carga contratos próximos a vencer (30 días)
    /// </summary>
    private async Task LoadContratosProximosAVencerAsync()
    {
        var result = await _contratoService.GetContratosProximosAVencerAsync(30);
        ContratosProximosAVencer.Clear();
        
        if (result.Success && result.Data != null)
        {
            foreach (var contrato in result.Data)
            {
                ContratosProximosAVencer.Add(contrato);
            }
        }
    }
    
    /// <summary>
    /// Se ejecuta cuando cambia el empleado seleccionado
    /// </summary>
    partial void OnSelectedEmpleadoChanged(Empleado? value)
    {
        if (value != null)
        {
            _ = LoadContratosEmpleadoAsync();
        }
        else
        {
            Contratos.Clear();
            ContratoActivo = null;
        }
    }
    
    /// <summary>
    /// Se ejecuta cuando cambia el tipo de contrato en el formulario
    /// </summary>
    partial void OnFormTipoContratoChanged(TipoContrato value)
    {
        // Si es indefinido, limpiar fecha fin
        if (value == TipoContrato.Indefinido)
        {
            FormFechaFin = null;
        }
        else if (!FormFechaFin.HasValue)
        {
            FormFechaFin = FormFechaInicio.AddYears(1);
        }
    }
    
    /// <summary>
    /// Carga los contratos del empleado seleccionado
    /// </summary>
    private async Task LoadContratosEmpleadoAsync()
    {
        if (SelectedEmpleado == null) return;
        
        IsLoading = true;
        StatusMessage = "Cargando contratos...";
        
        try
        {
            // Cargar historial de contratos
            var result = await _contratoService.GetByEmpleadoIdAsync(SelectedEmpleado.Id);
            Contratos.Clear();
            
            if (result.Success && result.Data != null)
            {
                foreach (var contrato in result.Data)
                {
                    Contratos.Add(contrato);
                }
            }
            
            // Cargar contrato activo
            var activoResult = await _contratoService.GetContratoActivoAsync(SelectedEmpleado.Id);
            ContratoActivo = activoResult.Data;
            
            // Calcular días restantes
            if (ContratoActivo?.FechaFin.HasValue == true)
            {
                DiasRestantesContrato = Math.Max(0, (ContratoActivo.FechaFin.Value - DateTime.Today).Days);
            }
            else
            {
                DiasRestantesContrato = -1; // Indefinido
            }
            
            StatusMessage = $"{Contratos.Count} contratos en historial";
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
    /// Muestra el formulario para nuevo contrato
    /// </summary>
    [RelayCommand]
    private void NuevoContrato()
    {
        if (SelectedEmpleado == null)
        {
            MessageBox.Show("Seleccione un empleado primero", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        if (ContratoActivo != null)
        {
            MessageBox.Show("El empleado ya tiene un contrato activo. Use la opción de renovar o finalice el contrato actual.", "Información", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        IsEditing = false;
        IsRenovando = false;
        FormTipoContrato = TipoContrato.Fijo;
        FormFechaInicio = DateTime.Today;
        FormFechaFin = DateTime.Today.AddYears(1);
        FormSalario = 0;
        FormCargo = Cargos.FirstOrDefault(c => c.Id == SelectedEmpleado.CargoId);
        FormObservaciones = string.Empty;
        IsFormVisible = true;
    }
    
    /// <summary>
    /// Muestra el formulario para editar contrato
    /// </summary>
    [RelayCommand]
    private void EditarContrato()
    {
        if (SelectedContrato == null)
        {
            MessageBox.Show("Seleccione un contrato para editar", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        if (SelectedContrato.Estado != EstadoContrato.Activo)
        {
            MessageBox.Show("Solo se pueden editar contratos activos", "Información", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        IsEditing = true;
        IsRenovando = false;
        FormTipoContrato = SelectedContrato.TipoContrato;
        FormFechaInicio = SelectedContrato.FechaInicio;
        FormFechaFin = SelectedContrato.FechaFin;
        FormSalario = SelectedContrato.Salario;
        FormCargo = Cargos.FirstOrDefault(c => c.Id == SelectedContrato.CargoId);
        FormObservaciones = SelectedContrato.Observaciones ?? string.Empty;
        IsFormVisible = true;
    }
    
    /// <summary>
    /// Muestra el formulario para renovar contrato
    /// </summary>
    [RelayCommand]
    private void RenovarContrato()
    {
        if (ContratoActivo == null)
        {
            MessageBox.Show("No hay contrato activo para renovar", "Información", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        IsEditing = false;
        IsRenovando = true;
        FormTipoContrato = ContratoActivo.TipoContrato;
        FormFechaInicio = ContratoActivo.FechaFin?.AddDays(1) ?? DateTime.Today;
        FormFechaFin = FormFechaInicio.AddYears(1);
        FormSalario = ContratoActivo.Salario;
        FormCargo = Cargos.FirstOrDefault(c => c.Id == ContratoActivo.CargoId);
        FormObservaciones = "Renovación de contrato";
        IsFormVisible = true;
    }
    
    /// <summary>
    /// Guarda el contrato (nuevo, editado o renovación)
    /// </summary>
    [RelayCommand]
    private async Task GuardarContratoAsync()
    {
        if (SelectedEmpleado == null || FormCargo == null) return;
        
        if (FormSalario <= 0)
        {
            MessageBox.Show("El salario debe ser mayor a cero", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        if (FormTipoContrato != TipoContrato.Indefinido && !FormFechaFin.HasValue)
        {
            MessageBox.Show("Para contratos a término fijo debe especificar la fecha de fin", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        if (FormFechaFin.HasValue && FormFechaFin.Value <= FormFechaInicio)
        {
            MessageBox.Show("La fecha de fin debe ser posterior a la fecha de inicio", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        IsLoading = true;
        
        try
        {
            if (IsRenovando && ContratoActivo != null)
            {
                // Renovar contrato
                var nuevoContrato = new Contrato
                {
                    EmpleadoId = SelectedEmpleado.Id,
                    TipoContrato = FormTipoContrato,
                    FechaInicio = FormFechaInicio,
                    FechaFin = FormTipoContrato == TipoContrato.Indefinido ? null : FormFechaFin,
                    Salario = FormSalario,
                    CargoId = FormCargo.Id,
                    Observaciones = FormObservaciones
                };
                
                var result = await _contratoService.RenovarContratoAsync(ContratoActivo.Id, nuevoContrato);
                
                if (result.Success)
                {
                    MessageBox.Show("Contrato renovado exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    IsFormVisible = false;
                    await LoadContratosEmpleadoAsync();
                    await LoadContratosProximosAVencerAsync();
                }
                else
                {
                    MessageBox.Show(result.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (IsEditing && SelectedContrato != null)
            {
                // Actualizar contrato existente
                SelectedContrato.TipoContrato = FormTipoContrato;
                SelectedContrato.FechaInicio = FormFechaInicio;
                SelectedContrato.FechaFin = FormTipoContrato == TipoContrato.Indefinido ? null : FormFechaFin;
                SelectedContrato.Salario = FormSalario;
                SelectedContrato.CargoId = FormCargo.Id;
                SelectedContrato.Observaciones = FormObservaciones;
                
                var result = await _contratoService.UpdateAsync(SelectedContrato);
                
                if (result.Success)
                {
                    MessageBox.Show("Contrato actualizado exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    IsFormVisible = false;
                    await LoadContratosEmpleadoAsync();
                    await LoadContratosProximosAVencerAsync();
                }
                else
                {
                    MessageBox.Show(result.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                // Crear nuevo contrato
                var nuevoContrato = new Contrato
                {
                    EmpleadoId = SelectedEmpleado.Id,
                    TipoContrato = FormTipoContrato,
                    FechaInicio = FormFechaInicio,
                    FechaFin = FormTipoContrato == TipoContrato.Indefinido ? null : FormFechaFin,
                    Salario = FormSalario,
                    CargoId = FormCargo.Id,
                    Observaciones = FormObservaciones
                };
                
                var result = await _contratoService.CreateAsync(nuevoContrato);
                
                if (result.Success)
                {
                    MessageBox.Show("Contrato creado exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    IsFormVisible = false;
                    await LoadContratosEmpleadoAsync();
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
    /// Finaliza el contrato activo
    /// </summary>
    [RelayCommand]
    private async Task FinalizarContratoAsync()
    {
        if (ContratoActivo == null)
        {
            MessageBox.Show("No hay contrato activo para finalizar", "Información", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        var confirmResult = MessageBox.Show(
            $"¿Está seguro de finalizar el contrato del empleado {SelectedEmpleado?.NombreCompleto}?\n\nEsta acción no se puede deshacer.",
            "Confirmar finalización",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
            
        if (confirmResult == MessageBoxResult.Yes)
        {
            IsLoading = true;
            
            try
            {
                var result = await _contratoService.FinalizarContratoAsync(ContratoActivo.Id, DateTime.Today);
                
                if (result.Success)
                {
                    MessageBox.Show("Contrato finalizado exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadContratosEmpleadoAsync();
                    await LoadContratosProximosAVencerAsync();
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
    /// Elimina un contrato
    /// </summary>
    [RelayCommand]
    private async Task EliminarContratoAsync()
    {
        if (SelectedContrato == null)
        {
            MessageBox.Show("Seleccione un contrato para eliminar", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        if (SelectedContrato.Estado == EstadoContrato.Finalizado)
        {
            MessageBox.Show("No se puede eliminar un contrato finalizado", "Información", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        var confirmResult = MessageBox.Show(
            "¿Está seguro de eliminar este contrato?",
            "Confirmar eliminación",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
            
        if (confirmResult == MessageBoxResult.Yes)
        {
            IsLoading = true;
            
            try
            {
                var result = await _contratoService.DeleteAsync(SelectedContrato.Id);
                
                if (result.Success)
                {
                    MessageBox.Show("Contrato eliminado exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadContratosEmpleadoAsync();
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
}
