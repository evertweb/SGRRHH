using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;
using SGRRHH.Infrastructure.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// ViewModel para la gestión de documentos del empleado
/// Controla permisos por rol:
/// - Administrador: Ver, Subir, Editar, Eliminar
/// - Operador: Ver, Subir, Editar
/// - Aprobador: Solo Ver
/// </summary>
public partial class DocumentosEmpleadoViewModel : ViewModelBase
{
    private readonly IDocumentoEmpleadoService _documentoService;
    private readonly IEmpleadoService _empleadoService;
    private readonly IDialogService _dialogService;
    private int _empleadoId;
    
    [ObservableProperty]
    private Empleado? _empleado;
    
    [ObservableProperty]
    private ObservableCollection<DocumentoEmpleado> _documentos = new();
    
    [ObservableProperty]
    private ObservableCollection<DocumentoChecklistItem> _checklist = new();
    
    [ObservableProperty]
    private DocumentoEmpleado? _selectedDocumento;
    
    [ObservableProperty]
    private bool _isFormVisible;
    
    [ObservableProperty]
    private bool _isEditing;
    
    /// <summary>
    /// Título del formulario basado en si está editando o creando
    /// </summary>
    public string FormTitle => IsEditing ? "📝 Editar Documento" : "📝 Nuevo Documento";
    
    // Propiedades de permisos
    [ObservableProperty]
    private bool _puedeGestionar;
    
    [ObservableProperty]
    private bool _puedeEliminar;
    
    // Propiedades del formulario
    [ObservableProperty]
    private TipoDocumentoEmpleado _formTipoDocumento = TipoDocumentoEmpleado.Otro;
    
    [ObservableProperty]
    private string _formNombre = string.Empty;
    
    [ObservableProperty]
    private string _formDescripcion = string.Empty;
    
    [ObservableProperty]
    private DateTime? _formFechaEmision;
    
    [ObservableProperty]
    private DateTime? _formFechaVencimiento;
    
    [ObservableProperty]
    private string? _formArchivoPath;
    
    [ObservableProperty]
    private string? _formArchivoNombreOriginal;
    
    [ObservableProperty]
    private byte[]? _formArchivoBytes;
    
    /// <summary>
    /// Lista de tipos de documento para el ComboBox
    /// </summary>
    public ObservableCollection<TipoDocumentoItem> TiposDocumento { get; } = new();
    
    /// <summary>
    /// Progreso del checklist de documentos requeridos
    /// </summary>
    public string ProgresoChecklist
    {
        get
        {
            var requeridos = Checklist.Where(c => c.EsRequerido).ToList();
            var completados = requeridos.Count(c => c.TieneDocumento && !c.EstaVencido);
            return $"{completados}/{requeridos.Count} documentos requeridos";
        }
    }
    
    public DocumentosEmpleadoViewModel(
        IDocumentoEmpleadoService documentoService,
        IEmpleadoService empleadoService,
        IDialogService dialogService)
    {
        _documentoService = documentoService;
        _empleadoService = empleadoService;
        _dialogService = dialogService;
        
        // Determinar permisos según rol del usuario actual
        var rolActual = App.CurrentUser?.Rol ?? RolUsuario.Operador;
        PuedeGestionar = _documentoService.PuedeGestionarDocumentos(rolActual);
        PuedeEliminar = _documentoService.PuedeEliminarDocumentos(rolActual);
        
        // Inicializar tipos de documento
        InitializeTiposDocumento();
    }
    
    /// <summary>
    /// Inicializa el ViewModel con el ID del empleado
    /// </summary>
    public void Initialize(int empleadoId)
    {
        _empleadoId = empleadoId;
    }
    
    private void InitializeTiposDocumento()
    {
        var tipos = Enum.GetValues<TipoDocumentoEmpleado>();
        foreach (var tipo in tipos)
        {
            TiposDocumento.Add(new TipoDocumentoItem
            {
                Tipo = tipo,
                Nombre = DocumentoEmpleadoService.GetNombreTipoDocumento(tipo)
            });
        }
    }
    
    /// <summary>
    /// Carga los datos iniciales
    /// </summary>
    public async Task LoadDataAsync()
    {
        IsLoading = true;
        StatusMessage = "Cargando documentos...";
        
        try
        {
            // Cargar información del empleado
            var empleado = await _empleadoService.GetByIdAsync(_empleadoId);
            Empleado = empleado;
            
            // Cargar documentos
            var result = await _documentoService.GetByEmpleadoIdAsync(_empleadoId);
            Documentos.Clear();
            if (result.Success && result.Data != null)
            {
                foreach (var doc in result.Data)
                {
                    Documentos.Add(doc);
                }
            }
            
            // Cargar checklist
            var checklistResult = await _documentoService.GetChecklistDocumentosAsync(_empleadoId);
            Checklist.Clear();
            if (checklistResult.Success && checklistResult.Data != null)
            {
                foreach (var item in checklistResult.Data)
                {
                    Checklist.Add(item);
                }
            }
            
            OnPropertyChanged(nameof(ProgresoChecklist));
            StatusMessage = $"{Documentos.Count} documentos";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            _dialogService.ShowError($"Error al cargar documentos: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    /// <summary>
    /// Muestra el formulario para nuevo documento
    /// </summary>
    [RelayCommand]
    private void NuevoDocumento()
    {
        if (!PuedeGestionar)
        {
            _dialogService.ShowWarning("No tiene permisos para subir documentos", "Permiso denegado");
            return;
        }
        
        IsEditing = false;
        FormTipoDocumento = TipoDocumentoEmpleado.Otro;
        FormNombre = string.Empty;
        FormDescripcion = string.Empty;
        FormFechaEmision = null;
        FormFechaVencimiento = null;
        FormArchivoPath = null;
        FormArchivoNombreOriginal = null;
        FormArchivoBytes = null;
        IsFormVisible = true;
    }
    
    /// <summary>
    /// Muestra el formulario para nuevo documento de un tipo específico (desde checklist)
    /// </summary>
    [RelayCommand]
    private void NuevoDocumentoTipo(TipoDocumentoEmpleado tipo)
    {
        if (!PuedeGestionar)
        {
            _dialogService.ShowWarning("No tiene permisos para subir documentos", "Permiso denegado");
            return;
        }
        
        IsEditing = false;
        FormTipoDocumento = tipo;
        FormNombre = DocumentoEmpleadoService.GetNombreTipoDocumento(tipo);
        FormDescripcion = string.Empty;
        FormFechaEmision = null;
        FormFechaVencimiento = null;
        FormArchivoPath = null;
        FormArchivoNombreOriginal = null;
        FormArchivoBytes = null;
        IsFormVisible = true;
    }
    
    /// <summary>
    /// Muestra el formulario para editar documento
    /// </summary>
    [RelayCommand]
    private void EditarDocumento()
    {
        if (!PuedeGestionar)
        {
            _dialogService.ShowWarning("No tiene permisos para editar documentos", "Permiso denegado");
            return;
        }
        
        if (SelectedDocumento == null)
        {
            _dialogService.ShowInfo("Seleccione un documento para editar");
            return;
        }
        
        IsEditing = true;
        FormTipoDocumento = SelectedDocumento.TipoDocumento;
        FormNombre = SelectedDocumento.Nombre;
        FormDescripcion = SelectedDocumento.Descripcion ?? string.Empty;
        FormFechaEmision = SelectedDocumento.FechaEmision;
        FormFechaVencimiento = SelectedDocumento.FechaVencimiento;
        FormArchivoPath = SelectedDocumento.ArchivoPath;
        FormArchivoNombreOriginal = SelectedDocumento.NombreArchivoOriginal;
        FormArchivoBytes = null; // No se puede cambiar el archivo
        IsFormVisible = true;
    }
    
    /// <summary>
    /// Selecciona un archivo para subir
    /// </summary>
    [RelayCommand]
    private void SeleccionarArchivo()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Seleccionar documento",
            Filter = "Documentos PDF|*.pdf|Imágenes|*.jpg;*.jpeg;*.png|Documentos Word|*.doc;*.docx|Todos los archivos|*.*",
            CheckFileExists = true
        };
        
        if (dialog.ShowDialog() == true)
        {
            try
            {
                FormArchivoBytes = File.ReadAllBytes(dialog.FileName);
                FormArchivoNombreOriginal = Path.GetFileName(dialog.FileName);
                FormArchivoPath = dialog.FileName;
                
                // Si no hay nombre, usar el nombre del archivo
                if (string.IsNullOrWhiteSpace(FormNombre))
                {
                    FormNombre = Path.GetFileNameWithoutExtension(dialog.FileName);
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Error al leer el archivo: {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// Guarda el documento (nuevo o editado)
    /// </summary>
    [RelayCommand]
    private async Task GuardarDocumentoAsync()
    {
        // Validaciones
        if (string.IsNullOrWhiteSpace(FormNombre))
        {
            _dialogService.ShowWarning("El nombre del documento es requerido", "Validación");
            return;
        }
        
        if (!IsEditing && (FormArchivoBytes == null || FormArchivoBytes.Length == 0))
        {
            _dialogService.ShowWarning("Debe seleccionar un archivo", "Validación");
            return;
        }
        
        IsLoading = true;
        
        try
        {
            var rolActual = App.CurrentUser?.Rol ?? RolUsuario.Operador;
            var usuarioId = App.CurrentUser?.Id ?? 0;
            
            if (IsEditing && SelectedDocumento != null)
            {
                // Actualizar documento existente
                SelectedDocumento.Nombre = FormNombre;
                SelectedDocumento.Descripcion = FormDescripcion;
                SelectedDocumento.TipoDocumento = FormTipoDocumento;
                SelectedDocumento.FechaEmision = FormFechaEmision;
                SelectedDocumento.FechaVencimiento = FormFechaVencimiento;
                
                var result = await _documentoService.UpdateAsync(SelectedDocumento, rolActual);
                
                if (result.Success)
                {
                    _dialogService.ShowSuccess("Documento actualizado exitosamente");
                    IsFormVisible = false;
                    await LoadDataAsync();
                }
                else
                {
                    _dialogService.ShowError(result.Message);
                }
            }
            else
            {
                // Crear nuevo documento
                // Detectar tipo MIME
                var extension = Path.GetExtension(FormArchivoNombreOriginal ?? "").ToLowerInvariant();
                var tipoMime = extension switch
                {
                    ".pdf" => "application/pdf",
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".doc" => "application/msword",
                    ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    _ => "application/octet-stream"
                };
                
                var documento = new DocumentoEmpleado
                {
                    EmpleadoId = _empleadoId,
                    TipoDocumento = FormTipoDocumento,
                    Nombre = FormNombre,
                    Descripcion = FormDescripcion,
                    NombreArchivoOriginal = FormArchivoNombreOriginal ?? "documento",
                    TipoMime = tipoMime,
                    FechaEmision = FormFechaEmision,
                    FechaVencimiento = FormFechaVencimiento,
                    SubidoPorNombre = App.CurrentUser?.NombreCompleto
                };
                
                var result = await _documentoService.SubirDocumentoAsync(documento, FormArchivoBytes!, usuarioId, rolActual);
                
                if (result.Success)
                {
                    _dialogService.ShowSuccess("Documento subido exitosamente");
                    IsFormVisible = false;
                    await LoadDataAsync();
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
    /// Abre el documento seleccionado
    /// </summary>
    [RelayCommand]
    private void VerDocumento()
    {
        if (SelectedDocumento == null)
        {
            _dialogService.ShowInfo("Seleccione un documento para ver");
            return;
        }
        
        if (!File.Exists(SelectedDocumento.ArchivoPath))
        {
            _dialogService.ShowWarning("El archivo no existe en la ubicación especificada");
            return;
        }
        
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = SelectedDocumento.ArchivoPath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Error al abrir el archivo: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Elimina el documento seleccionado
    /// </summary>
    [RelayCommand]
    private async Task EliminarDocumentoAsync()
    {
        if (!PuedeEliminar)
        {
            _dialogService.ShowWarning("Solo el administrador puede eliminar documentos", "Permiso denegado");
            return;
        }
        
        if (SelectedDocumento == null)
        {
            _dialogService.ShowInfo("Seleccione un documento para eliminar");
            return;
        }
        
        if (!_dialogService.ConfirmWarning($"¿Está seguro de eliminar el documento '{SelectedDocumento.Nombre}'?\n\nEsta acción no se puede deshacer.", "Confirmar eliminación"))
            return;
            
        IsLoading = true;
        
        try
        {
            var rolActual = App.CurrentUser?.Rol ?? RolUsuario.Operador;
            var result = await _documentoService.DeleteAsync(SelectedDocumento.Id, rolActual);
            
            if (result.Success)
            {
                _dialogService.ShowSuccess("Documento eliminado exitosamente");
                await LoadDataAsync();
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
    /// Cuando cambia el tipo de documento, actualiza el nombre sugerido
    /// </summary>
    partial void OnFormTipoDocumentoChanged(TipoDocumentoEmpleado value)
    {
        if (!IsEditing && string.IsNullOrWhiteSpace(FormNombre))
        {
            FormNombre = DocumentoEmpleadoService.GetNombreTipoDocumento(value);
        }
    }
    
    /// <summary>
    /// Notifica que el título del formulario cambió cuando cambia IsEditing
    /// </summary>
    partial void OnIsEditingChanged(bool value)
    {
        OnPropertyChanged(nameof(FormTitle));
    }
}

/// <summary>
/// Item para el ComboBox de tipos de documento
/// </summary>
public class TipoDocumentoItem
{
    public TipoDocumentoEmpleado Tipo { get; set; }
    public string Nombre { get; set; } = string.Empty;
}
