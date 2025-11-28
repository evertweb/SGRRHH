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
public partial class DocumentosEmpleadoViewModel : ObservableObject
{
    private readonly IDocumentoEmpleadoService _documentoService;
    private readonly IEmpleadoService _empleadoService;
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
    private bool _isLoading;
    
    [ObservableProperty]
    private string _statusMessage = string.Empty;
    
    [ObservableProperty]
    private bool _isFormVisible;
    
    [ObservableProperty]
    private bool _isEditing;
    
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
        IEmpleadoService empleadoService)
    {
        _documentoService = documentoService;
        _empleadoService = empleadoService;
        
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
            MessageBox.Show($"Error al cargar documentos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            MessageBox.Show("No tiene permisos para subir documentos", "Permiso denegado", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            MessageBox.Show("No tiene permisos para subir documentos", "Permiso denegado", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            MessageBox.Show("No tiene permisos para editar documentos", "Permiso denegado", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        if (SelectedDocumento == null)
        {
            MessageBox.Show("Seleccione un documento para editar", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
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
                MessageBox.Show($"Error al leer el archivo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            MessageBox.Show("El nombre del documento es requerido", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        if (!IsEditing && (FormArchivoBytes == null || FormArchivoBytes.Length == 0))
        {
            MessageBox.Show("Debe seleccionar un archivo", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    MessageBox.Show("Documento actualizado exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    IsFormVisible = false;
                    await LoadDataAsync();
                }
                else
                {
                    MessageBox.Show(result.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    MessageBox.Show("Documento subido exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    IsFormVisible = false;
                    await LoadDataAsync();
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
    /// Abre el documento seleccionado
    /// </summary>
    [RelayCommand]
    private void VerDocumento()
    {
        if (SelectedDocumento == null)
        {
            MessageBox.Show("Seleccione un documento para ver", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        if (!File.Exists(SelectedDocumento.ArchivoPath))
        {
            MessageBox.Show("El archivo no existe en la ubicación especificada", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            MessageBox.Show($"Error al abrir el archivo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            MessageBox.Show("Solo el administrador puede eliminar documentos", "Permiso denegado", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        if (SelectedDocumento == null)
        {
            MessageBox.Show("Seleccione un documento para eliminar", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        var confirmResult = MessageBox.Show(
            $"¿Está seguro de eliminar el documento '{SelectedDocumento.Nombre}'?\n\nEsta acción no se puede deshacer.",
            "Confirmar eliminación",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
            
        if (confirmResult == MessageBoxResult.Yes)
        {
            IsLoading = true;
            
            try
            {
                var rolActual = App.CurrentUser?.Rol ?? RolUsuario.Operador;
                var result = await _documentoService.DeleteAsync(SelectedDocumento.Id, rolActual);
                
                if (result.Success)
                {
                    MessageBox.Show("Documento eliminado exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadDataAsync();
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
    /// Cuando cambia el tipo de documento, actualiza el nombre sugerido
    /// </summary>
    partial void OnFormTipoDocumentoChanged(TipoDocumentoEmpleado value)
    {
        if (!IsEditing && string.IsNullOrWhiteSpace(FormNombre))
        {
            FormNombre = DocumentoEmpleadoService.GetNombreTipoDocumento(value);
        }
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
