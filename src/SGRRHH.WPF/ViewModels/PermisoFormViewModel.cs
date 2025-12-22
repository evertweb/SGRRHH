using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SGRRHH.Core.Common;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// ViewModel para el formulario de solicitud/edición de permiso
/// </summary>
public partial class PermisoFormViewModel : ViewModelBase
{
    private readonly IPermisoService _permisoService;
    private readonly IEmpleadoService _empleadoService;
    private readonly ITipoPermisoService _tipoPermisoService;
    private readonly IDialogService _dialogService;
    
    private int? _permisoId;
    private bool _isEditing;
    
    [ObservableProperty]
    private ObservableCollection<Empleado> _empleados = new();
    
    [ObservableProperty]
    private ObservableCollection<TipoPermiso> _tiposPermiso = new();
    
    [ObservableProperty]
    private Empleado? _selectedEmpleado;
    
    [ObservableProperty]
    private TipoPermiso? _selectedTipoPermiso;
    
    [ObservableProperty]
    private string _motivo = string.Empty;
    
    [ObservableProperty]
    private DateTime _fechaInicio = DateTime.Today;
    
    [ObservableProperty]
    private DateTime _fechaFin = DateTime.Today;
    
    [ObservableProperty]
    private string? _observaciones;
    
    [ObservableProperty]
    private string? _documentoSoportePath;
    
    [ObservableProperty]
    private DateTime? _fechaCompensacion;
    
    [ObservableProperty]
    private int _totalDias = 1;
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private string _windowTitle = "Solicitar Permiso";
    
    [ObservableProperty]
    private bool _requiereDocumento;
    
    [ObservableProperty]
    private bool _esCompensable;
    
    [ObservableProperty]
    private bool _hasDocument;
    
    /// <summary>
    /// Evento cuando se guarda exitosamente
    /// </summary>
    public event EventHandler<bool>? SaveCompleted;
    
    /// <summary>
    /// Evento para cerrar la ventana
    /// </summary>
    public event EventHandler? CloseRequested;
    
    public PermisoFormViewModel(
        IPermisoService permisoService,
        IEmpleadoService empleadoService,
        ITipoPermisoService tipoPermisoService,
        IDialogService dialogService)
    {
        _permisoService = permisoService;
        _empleadoService = empleadoService;
        _tipoPermisoService = tipoPermisoService;
        _dialogService = dialogService;
    }
    
    public async Task InitializeForCreateAsync()
    {
        _isEditing = false;
        WindowTitle = "Solicitar Permiso";
        await LoadDataAsync();
    }
    
    public async Task InitializeForEditAsync(int permisoId)
    {
        _isEditing = true;
        _permisoId = permisoId;
        WindowTitle = "Editar Permiso";
        
        await LoadDataAsync();
        await LoadPermisoAsync(permisoId);
    }
    
    private async Task LoadDataAsync()
    {
        IsLoading = true;
        
        try
        {
            // Cargar empleados activos
            var empleados = await _empleadoService.GetAllAsync();
            Empleados.Clear();
            foreach (var empleado in empleados
                .Where(e => e.Estado == EstadoEmpleado.Activo)
                .OrderBy(e => e.NombreCompleto))
            {
                Empleados.Add(empleado);
            }
            
            // Cargar tipos de permiso activos
            var tiposResult = await _tipoPermisoService.GetActivosAsync();
            if (tiposResult.Success && tiposResult.Data != null)
            {
                TiposPermiso.Clear();
                foreach (var tipo in tiposResult.Data.OrderBy(t => t.Nombre))
                {
                    TiposPermiso.Add(tipo);
                }
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Error al cargar datos: {ex.Message}", "Error");
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    private async Task LoadPermisoAsync(int permisoId)
    {
        var result = await _permisoService.GetByIdAsync(permisoId);
        
        if (result.Success && result.Data != null)
        {
            var permiso = result.Data;
            
            SelectedEmpleado = Empleados.FirstOrDefault(e => e.Id == permiso.EmpleadoId);
            SelectedTipoPermiso = TiposPermiso.FirstOrDefault(t => t.Id == permiso.TipoPermisoId);
            Motivo = permiso.Motivo;
            FechaInicio = permiso.FechaInicio;
            FechaFin = permiso.FechaFin;
            Observaciones = permiso.Observaciones;
            DocumentoSoportePath = permiso.DocumentoSoportePath;
            FechaCompensacion = permiso.FechaCompensacion;
            TotalDias = permiso.TotalDias;
            HasDocument = !string.IsNullOrEmpty(DocumentoSoportePath);
        }
        else
        {
            _dialogService.ShowError("No se pudo cargar el permiso", "Error");
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
    
    [RelayCommand]
    private void SelectDocument()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Seleccionar documento soporte",
            Filter = "Documentos PDF|*.pdf|Imágenes|*.jpg;*.jpeg;*.png|Todos los archivos|*.*"
        };
        
        if (dialog.ShowDialog() == true)
        {
            // Copiar archivo a carpeta de documentos usando DataPaths
            try
            {
                var docsPath = Helpers.DataPaths.EnsureDirectory(Helpers.DataPaths.Permisos);
                
                var fileName = $"{DateTime.Now:yyyyMMddHHmmss}_{Path.GetFileName(dialog.FileName)}";
                var destPath = Path.Combine(docsPath, fileName);
                
                File.Copy(dialog.FileName, destPath);
                DocumentoSoportePath = destPath;
                HasDocument = true;
                
                _dialogService.ShowSuccess("Documento adjuntado exitosamente", "Éxito");
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Error al adjuntar documento: {ex.Message}", "Error");
            }
        }
    }
    
    [RelayCommand]
    private void RemoveDocument()
    {
        if (!string.IsNullOrEmpty(DocumentoSoportePath))
        {
            if (_dialogService.Confirm("¿Desea eliminar el documento adjunto?", "Confirmar"))
            {
                // Eliminar archivo si existe y es edición
                if (!_isEditing && File.Exists(DocumentoSoportePath))
                {
                    try { File.Delete(DocumentoSoportePath); } catch { }
                }
                
                DocumentoSoportePath = null;
                HasDocument = false;
            }
        }
    }
    
    [RelayCommand]
    private async Task Save()
    {
        // Validar formulario
        var validationErrors = ValidateForm();
        if (validationErrors.Any())
        {
            _dialogService.ShowWarning(
                "Por favor corrija los siguientes errores:\n\n• " + string.Join("\n• ", validationErrors),
                "Validación");
            return;
        }
        
        // Verificación adicional de null (aunque la validación ya lo cubre)
        if (SelectedEmpleado == null || SelectedTipoPermiso == null)
        {
            _dialogService.ShowError("Datos incompletos. Verifique empleado y tipo de permiso.", "Error");
            return;
        }
        
        IsLoading = true;
        
        try
        {
            var permiso = new Permiso
            {
                EmpleadoId = SelectedEmpleado.Id,
                TipoPermisoId = SelectedTipoPermiso.Id,
                Motivo = Motivo,
                FechaInicio = FechaInicio,
                FechaFin = FechaFin,
                Observaciones = Observaciones,
                DocumentoSoportePath = DocumentoSoportePath,
                FechaCompensacion = FechaCompensacion,
                DiasPendientesCompensacion = EsCompensable ? TotalDias : null
            };
            
            if (_isEditing && _permisoId.HasValue)
            {
                permiso.Id = _permisoId.Value;
                var result = await _permisoService.UpdateAsync(permiso);
                
                if (result.Success)
                {
                    _dialogService.ShowSuccess("Permiso actualizado exitosamente", "Éxito");
                    SaveCompleted?.Invoke(this, true);
                }
                else
                {
                    _dialogService.ShowError($"Error: {string.Join(", ", result.Errors)}", "Error");
                }
            }
            else
            {
                var result = await _permisoService.SolicitarPermisoAsync(permiso, App.CurrentUser!.Id);
                
                if (result.Success)
                {
                    _dialogService.ShowSuccess($"Permiso solicitado exitosamente.\n\nNúmero de acta: {result.Data?.NumeroActa}", "Éxito");
                    SaveCompleted?.Invoke(this, true);
                }
                else
                {
                    _dialogService.ShowError($"Error: {string.Join(", ", result.Errors)}", "Error");
                }
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Error: {ex.Message}", "Error");
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private void Cancel()
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
    
    /// <summary>
    /// Valida los datos del formulario de permiso
    /// </summary>
    private List<string> ValidateForm()
    {
        var errors = new List<string>();
        
        // Validaciones de campos obligatorios
        if (SelectedEmpleado == null)
            errors.Add("Debe seleccionar un empleado");
        
        if (SelectedTipoPermiso == null)
            errors.Add("Debe seleccionar un tipo de permiso");
        
        if (string.IsNullOrWhiteSpace(Motivo))
            errors.Add("Debe ingresar el motivo del permiso");
        
        // Validación de longitud mínima del motivo
        if (!string.IsNullOrWhiteSpace(Motivo) && Motivo.Trim().Length < 10)
            errors.Add("El motivo debe tener al menos 10 caracteres");
        
        // Validación de fechas
        if (FechaInicio > FechaFin)
            errors.Add("La fecha de inicio no puede ser posterior a la fecha de fin");
        
        // Validación de fecha de inicio no en el pasado (solo para nuevos permisos)
        if (!_isEditing && FechaInicio < DateTime.Today)
            errors.Add("La fecha de inicio no puede ser anterior a hoy para nuevas solicitudes");
        
        // Validación de máximo 30 días para evitar errores
        if (TotalDias > 30)
            errors.Add("El permiso no puede exceder 30 días");
        
        // Validación de documento soporte
        if (RequiereDocumento && string.IsNullOrEmpty(DocumentoSoportePath))
            errors.Add("Este tipo de permiso requiere adjuntar un documento soporte");
        
        // Validación de fecha de compensación
        if (EsCompensable && !FechaCompensacion.HasValue)
            errors.Add("Este tipo de permiso es compensable. Debe indicar la fecha de compensación");
        
        // Validación de fecha de compensación futura
        if (FechaCompensacion.HasValue && FechaCompensacion.Value < DateTime.Today)
            errors.Add("La fecha de compensación debe ser igual o posterior a hoy");
        
        // Validación de observaciones (si existe)
        if (!string.IsNullOrWhiteSpace(Observaciones) && Observaciones.Length > 500)
            errors.Add("Las observaciones no pueden exceder 500 caracteres");
        
        return errors;
    }
    
    partial void OnSelectedTipoPermisoChanged(TipoPermiso? value)
    {
        if (value != null)
        {
            RequiereDocumento = value.RequiereDocumento;
            EsCompensable = value.EsCompensable;
            
            // Ajustar fecha fin según días por defecto
            if (!_isEditing && value.DiasPorDefecto > 0)
            {
                FechaFin = FechaInicio.AddDays(value.DiasPorDefecto - 1);
            }
        }
        else
        {
            RequiereDocumento = false;
            EsCompensable = false;
        }
    }
    
    partial void OnFechaInicioChanged(DateTime value)
    {
        CalcularTotalDias();
        
        // Si hay tipo seleccionado, ajustar fecha fin
        if (!_isEditing && SelectedTipoPermiso != null && SelectedTipoPermiso.DiasPorDefecto > 0)
        {
            FechaFin = value.AddDays(SelectedTipoPermiso.DiasPorDefecto - 1);
        }
    }
    
    partial void OnFechaFinChanged(DateTime value)
    {
        CalcularTotalDias();
    }
    
    private void CalcularTotalDias()
    {
        TotalDias = Math.Max(1, (FechaFin - FechaInicio).Days + 1);
    }
}
