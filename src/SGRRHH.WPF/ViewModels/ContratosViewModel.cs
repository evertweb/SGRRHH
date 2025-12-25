using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SGRRHH.Core.Common;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;
using SGRRHH.WPF.Helpers;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// ViewModel para la gestión de contratos
/// Permisos según matriz:
/// - Administrador: Todo (crear, editar, renovar, finalizar, eliminar)
/// - Operador (Secretaria): Crear, Editar, Renovar, Ver
/// - Aprobador (Ingeniera): Solo Ver
/// </summary>
public partial class ContratosViewModel : ViewModelBase
{
    private readonly IContratoService _contratoService;
    private readonly IEmpleadoService _empleadoService;
    private readonly ICargoService _cargoService;
    private readonly IDialogService _dialogService;
    
    // Propiedades para la pantalla de bienvenida
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsContentVisible))]
    private bool _isHomeVisible = true;
    
    /// <summary>
    /// Indica si el contenido principal (gestión de contratos) está visible
    /// </summary>
    public bool IsContentVisible => !IsHomeVisible;
    
    /// <summary>
    /// Texto con la fecha actual para el footer de la pantalla de bienvenida
    /// </summary>
    public string CurrentDateText => DateTime.Now.ToString("dddd, dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("es-ES"));
    
    // Control de permisos por rol
    [ObservableProperty]
    private bool _puedeGestionar; // Crear/Editar/Renovar contratos
    
    [ObservableProperty]
    private bool _puedeFinalizar; // Finalizar contratos - Solo Admin
    
    [ObservableProperty]
    private bool _puedeEliminar; // Eliminar contratos - Solo Admin
    
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
    private string? _formArchivoAdjuntoPath;
    
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
    
    public ContratosViewModel(IContratoService contratoService, IEmpleadoService empleadoService, ICargoService cargoService, IDialogService dialogService)
    {
        _contratoService = contratoService;
        _empleadoService = empleadoService;
        _cargoService = cargoService;
        _dialogService = dialogService;
        
        // Determinar permisos según rol del usuario actual
        var rolActual = App.CurrentUser?.Rol ?? RolUsuario.Operador;
        
        // Ingeniera (Aprobador) solo puede ver - NO puede gestionar
        // Admin y Secretaria (Operador) pueden crear/editar/renovar
        PuedeGestionar = rolActual == RolUsuario.Administrador || rolActual == RolUsuario.Operador;
        
        // Solo Admin puede finalizar y eliminar
        PuedeFinalizar = rolActual == RolUsuario.Administrador;
        PuedeEliminar = rolActual == RolUsuario.Administrador;
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
            _dialogService.ShowError($"Error al cargar datos: {ex.Message}");
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
            LoadContratosEmpleadoAsync().SafeFireAndForget(showErrorMessage: false);
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
    /// Muestra la pantalla de bienvenida
    /// </summary>
    [RelayCommand]
    private void ShowHome()
    {
        IsHomeVisible = true;
    }
    
    /// <summary>
    /// Muestra el contenido principal (gestión de contratos)
    /// </summary>
    [RelayCommand]
    private void ShowContent()
    {
        IsHomeVisible = false;
    }
    
    /// <summary>
    /// Muestra el formulario para nuevo contrato
    /// </summary>
    [RelayCommand]
    private void NuevoContrato()
    {
        if (!PuedeGestionar)
        {
            _dialogService.ShowWarning("No tiene permisos para crear contratos", "Permiso denegado");
            return;
        }
        
        // Si estamos en la pantalla de bienvenida, ir al contenido principal
        if (IsHomeVisible)
        {
            IsHomeVisible = false;
        }
        
        if (SelectedEmpleado == null)
        {
            _dialogService.ShowInfo("Seleccione un empleado primero");
            return;
        }
        
        if (ContratoActivo != null)
        {
            _dialogService.ShowWarning("El empleado ya tiene un contrato activo. Use la opción de renovar o finalice el contrato actual.");
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
        FormArchivoAdjuntoPath = null;
        IsFormVisible = true;
    }
    
    /// <summary>
    /// Muestra el formulario para editar contrato
    /// </summary>
    [RelayCommand]
    private void EditarContrato()
    {
        if (!PuedeGestionar)
        {
            _dialogService.ShowWarning("No tiene permisos para editar contratos", "Permiso denegado");
            return;
        }
        
        if (SelectedContrato == null)
        {
            _dialogService.ShowInfo("Seleccione un contrato para editar");
            return;
        }
        
        if (SelectedContrato.Estado != EstadoContrato.Activo)
        {
            _dialogService.ShowWarning("Solo se pueden editar contratos activos");
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
        FormArchivoAdjuntoPath = SelectedContrato.ArchivoAdjuntoPath;
        IsFormVisible = true;
    }
    
    /// <summary>
    /// Muestra el formulario para renovar contrato
    /// </summary>
    [RelayCommand]
    private void RenovarContrato()
    {
        if (!PuedeGestionar)
        {
            _dialogService.ShowWarning("No tiene permisos para renovar contratos", "Permiso denegado");
            return;
        }
        
        if (ContratoActivo == null)
        {
            _dialogService.ShowWarning("No hay contrato activo para renovar");
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
        FormArchivoAdjuntoPath = null;
        IsFormVisible = true;
    }
    
    /// <summary>
    /// Guarda el contrato (nuevo, editado o renovación)
    /// </summary>
    [RelayCommand]
    private async Task GuardarContratoAsync()
    {
        if (!PuedeGestionar)
        {
            _dialogService.ShowWarning("No tiene permisos para gestionar contratos", "Permiso denegado");
            return;
        }
        
        if (SelectedEmpleado == null || FormCargo == null) return;
        
        if (FormSalario <= 0)
        {
            _dialogService.ShowWarning("El salario debe ser mayor a cero", "Validación");
            return;
        }
        
        if (FormTipoContrato != TipoContrato.Indefinido && !FormFechaFin.HasValue)
        {
            _dialogService.ShowWarning("Para contratos a término fijo debe especificar la fecha de fin", "Validación");
            return;
        }
        
        if (FormFechaFin.HasValue && FormFechaFin.Value <= FormFechaInicio)
        {
            _dialogService.ShowWarning("La fecha de fin debe ser posterior a la fecha de inicio", "Validación");
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
                    Observaciones = FormObservaciones,
                    ArchivoAdjuntoPath = FormArchivoAdjuntoPath
                };
                
                var result = await _contratoService.RenovarContratoAsync(ContratoActivo.Id, nuevoContrato);
                
                if (result.Success)
                {
                    _dialogService.ShowSuccess("Contrato renovado exitosamente");
                    IsFormVisible = false;
                    await LoadContratosEmpleadoAsync();
                    await LoadContratosProximosAVencerAsync();
                }
                else
                {
                    _dialogService.ShowError(result.Message);
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
                SelectedContrato.ArchivoAdjuntoPath = FormArchivoAdjuntoPath;
                
                var result = await _contratoService.UpdateAsync(SelectedContrato);
                
                if (result.Success)
                {
                    _dialogService.ShowSuccess("Contrato actualizado exitosamente");
                    IsFormVisible = false;
                    await LoadContratosEmpleadoAsync();
                    await LoadContratosProximosAVencerAsync();
                }
                else
                {
                    _dialogService.ShowError(result.Message);
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
                    Observaciones = FormObservaciones,
                    ArchivoAdjuntoPath = FormArchivoAdjuntoPath
                };
                
                var result = await _contratoService.CreateAsync(nuevoContrato);
                
                if (result.Success)
                {
                    _dialogService.ShowSuccess("Contrato creado exitosamente");
                    IsFormVisible = false;
                    await LoadContratosEmpleadoAsync();
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
    /// Extensiones permitidas para archivos de contrato
    /// </summary>
    private static readonly string[] ExtensionesPermitidas = { ".pdf", ".jpg", ".jpeg", ".png" };
    
    /// <summary>
    /// Tamaño máximo de archivo en bytes (10 MB)
    /// </summary>
    private const long TamanoMaximoBytes = 10 * 1024 * 1024;
    
    /// <summary>
    /// Selecciona un archivo PDF para adjuntar al contrato
    /// </summary>
    [RelayCommand]
    private void SeleccionarArchivo()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Seleccionar contrato firmado",
            Filter = "Documentos PDF|*.pdf|Imágenes|*.jpg;*.jpeg;*.png",
            CheckFileExists = true
        };
        
        if (dialog.ShowDialog() == true)
        {
            try
            {
                // FIX #8: Validar extensión del archivo
                var extension = Path.GetExtension(dialog.FileName).ToLowerInvariant();
                if (!ExtensionesPermitidas.Contains(extension))
                {
                    _dialogService.ShowWarning(
                        $"Formato de archivo no permitido. Use: {string.Join(", ", ExtensionesPermitidas)}", 
                        "Formato inválido");
                    return;
                }
                
                // FIX #8: Validar tamaño del archivo
                var fileInfo = new FileInfo(dialog.FileName);
                if (fileInfo.Length > TamanoMaximoBytes)
                {
                    _dialogService.ShowWarning(
                        $"El archivo excede el tamaño máximo permitido (10 MB). Tamaño actual: {fileInfo.Length / 1024 / 1024:N1} MB", 
                        "Archivo muy grande");
                    return;
                }
                
                // Copiar archivo a carpeta de contratos usando DataPaths
                var contratosPath = Helpers.DataPaths.EnsureDirectory(Helpers.DataPaths.Contratos);
                
                var fileName = $"contrato_{SelectedEmpleado?.Cedula ?? "temp"}_{DateTime.Now:yyyyMMdd_HHmmss}{extension}";
                var destPath = Path.Combine(contratosPath, fileName);
                
                File.Copy(dialog.FileName, destPath, true);
                FormArchivoAdjuntoPath = destPath;
                
                _dialogService.ShowSuccess("Archivo adjuntado correctamente");
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Error al adjuntar archivo: {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// Abre el archivo adjunto del contrato
    /// </summary>
    [RelayCommand]
    private void VerArchivo()
    {
        var path = FormArchivoAdjuntoPath ?? SelectedContrato?.ArchivoAdjuntoPath;
        
        if (string.IsNullOrEmpty(path))
        {
            _dialogService.ShowInfo("No hay archivo adjunto");
            return;
        }
        
        if (!File.Exists(path))
        {
            _dialogService.ShowWarning("El archivo no existe en la ubicación especificada");
            return;
        }
        
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Error al abrir el archivo: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Elimina el archivo adjunto del formulario
    /// </summary>
    [RelayCommand]
    private void EliminarArchivo()
    {
        if (string.IsNullOrEmpty(FormArchivoAdjuntoPath))
        {
            _dialogService.ShowInfo("No hay archivo para eliminar");
            return;
        }
        
        if (_dialogService.Confirm("¿Está seguro de eliminar el archivo adjunto?", "Confirmar"))
        {
            FormArchivoAdjuntoPath = null;
        }
    }
    
    /// <summary>
    /// Ver archivo del contrato seleccionado en la tabla o contrato activo
    /// </summary>
    [RelayCommand]
    private void VerArchivoContrato()
    {
        // Priorizar el contrato seleccionado, luego el activo
        var contrato = SelectedContrato ?? ContratoActivo;
        
        if (contrato == null)
        {
            _dialogService.ShowInfo("Seleccione un contrato");
            return;
        }
        
        if (string.IsNullOrEmpty(contrato.ArchivoAdjuntoPath))
        {
            _dialogService.ShowInfo("Este contrato no tiene archivo adjunto");
            return;
        }
        
        if (!File.Exists(contrato.ArchivoAdjuntoPath))
        {
            _dialogService.ShowWarning("El archivo no existe en la ubicación especificada");
            return;
        }
        
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = contrato.ArchivoAdjuntoPath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Error al abrir el archivo: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Finaliza el contrato activo
    /// </summary>
    [RelayCommand]
    private async Task FinalizarContratoAsync()
    {
        if (!PuedeFinalizar)
        {
            _dialogService.ShowWarning("Solo el administrador puede finalizar contratos", "Permiso denegado");
            return;
        }
        
        if (ContratoActivo == null)
        {
            _dialogService.ShowWarning("No hay contrato activo para finalizar");
            return;
        }
        
        if (!_dialogService.ConfirmWarning($"¿Está seguro de finalizar el contrato del empleado {SelectedEmpleado?.NombreCompleto}?\n\nEsta acción no se puede deshacer.", "Confirmar finalización"))
            return;
            
        IsLoading = true;
        
        try
        {
            var result = await _contratoService.FinalizarContratoAsync(ContratoActivo.Id, DateTime.Today);
            
            if (result.Success)
            {
                _dialogService.ShowSuccess("Contrato finalizado exitosamente");
                await LoadContratosEmpleadoAsync();
                await LoadContratosProximosAVencerAsync();
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
    
    /// <summary>
    /// Elimina un contrato
    /// </summary>
    [RelayCommand]
    private async Task EliminarContratoAsync()
    {
        if (!PuedeEliminar)
        {
            _dialogService.ShowWarning("Solo el administrador puede eliminar contratos", "Permiso denegado");
            return;
        }
        
        if (SelectedContrato == null)
        {
            _dialogService.ShowInfo("Seleccione un contrato para eliminar");
            return;
        }
        
        if (SelectedContrato.Estado == EstadoContrato.Finalizado)
        {
            _dialogService.ShowWarning("No se puede eliminar un contrato finalizado");
            return;
        }
        
        if (!_dialogService.ConfirmWarning("¿Está seguro de eliminar este contrato?", "Confirmar eliminación"))
            return;
            
        IsLoading = true;
        
        try
        {
            var result = await _contratoService.DeleteAsync(SelectedContrato.Id);
            
            if (result.Success)
            {
                _dialogService.ShowSuccess("Contrato eliminado exitosamente");
                await LoadContratosEmpleadoAsync();
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
