using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGRRHH.Core.Common;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using Microsoft.Win32;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// ViewModel para el formulario de empleado (crear/editar)
/// </summary>
public partial class EmpleadoFormViewModel : ViewModelBase
{
    private readonly IEmpleadoService _empleadoService;
    private readonly IDepartamentoService _departamentoService;
    private readonly ICargoService _cargoService;
    private readonly IDialogService _dialogService;
    private readonly IDocumentoEmpleadoService? _documentoService;
    
    private int _empleadoId;
    private bool _isEditing;
    private byte[]? _newFotoData;
    private string? _newFotoExtension;
    
    // Documentos pendientes para subir después de crear el empleado
    private readonly Dictionary<TipoDocumentoEmpleado, PendingDocument> _pendingDocuments = new();
    
    [ObservableProperty]
    private string _windowTitle = "Nuevo Empleado";
    
    /// <summary>
    /// Indica si estamos editando un empleado existente (vs. crear uno nuevo)
    /// </summary>
    public bool IsEditMode => _isEditing;
    
    // Datos del empleado
    [ObservableProperty]
    private string _codigo = string.Empty;
    
    [ObservableProperty]
    private string _cedula = string.Empty;
    
    [ObservableProperty]
    private string _nombres = string.Empty;
    
    [ObservableProperty]
    private string _apellidos = string.Empty;
    
    [ObservableProperty]
    private DateTime? _fechaNacimiento;
    
    [ObservableProperty]
    private Genero? _genero;
    
    [ObservableProperty]
    private EstadoCivil? _estadoCivil;
    
    [ObservableProperty]
    private string? _direccion;
    
    [ObservableProperty]
    private string? _telefono;
    
    [ObservableProperty]
    private string? _telefonoEmergencia;
    
    [ObservableProperty]
    private string? _contactoEmergencia;
    
    [ObservableProperty]
    private string? _email;
    
    [ObservableProperty]
    private string? _fotoPath;
    
    [ObservableProperty]
    private DateTime _fechaIngreso = DateTime.Today;
    
    [ObservableProperty]
    private TipoContrato _tipoContrato = TipoContrato.Indefinido;
    
    [ObservableProperty]
    private EstadoEmpleado _estado = EstadoEmpleado.Activo;
    
    [ObservableProperty]
    private Departamento? _selectedDepartamento;
    
    [ObservableProperty]
    private Cargo? _selectedCargo;
    
    [ObservableProperty]
    private Empleado? _selectedSupervisor;
    
    [ObservableProperty]
    private string? _observaciones;
    
    // Propiedades para documentos pendientes (nombre de archivo seleccionado)
    [ObservableProperty]
    private string? _docCedulaFileName;
    
    [ObservableProperty]
    private string? _docHojaVidaFileName;
    
    [ObservableProperty]
    private string? _docContratoFileName;
    
    [ObservableProperty]
    private string? _docCertificadoBancarioFileName;
    
    // Colecciones para combos
    [ObservableProperty]
    private ObservableCollection<Departamento> _departamentos = new();
    
    [ObservableProperty]
    private ObservableCollection<Cargo> _cargos = new();
    
    [ObservableProperty]
    private ObservableCollection<Cargo> _cargosDisponibles = new();
    
    [ObservableProperty]
    private ObservableCollection<Empleado> _supervisores = new();
    
    /// <summary>
    /// Lista de géneros
    /// </summary>
    public ObservableCollection<GeneroItem> Generos { get; } = new()
    {
        new GeneroItem { Nombre = "Masculino", Valor = SGRRHH.Core.Enums.Genero.Masculino },
        new GeneroItem { Nombre = "Femenino", Valor = SGRRHH.Core.Enums.Genero.Femenino },
        new GeneroItem { Nombre = "Otro", Valor = SGRRHH.Core.Enums.Genero.Otro }
    };
    
    /// <summary>
    /// Lista de estados civiles
    /// </summary>
    public ObservableCollection<EstadoCivilItem> EstadosCiviles { get; } = new()
    {
        new EstadoCivilItem { Nombre = "Soltero(a)", Valor = SGRRHH.Core.Enums.EstadoCivil.Soltero },
        new EstadoCivilItem { Nombre = "Casado(a)", Valor = SGRRHH.Core.Enums.EstadoCivil.Casado },
        new EstadoCivilItem { Nombre = "Unión Libre", Valor = SGRRHH.Core.Enums.EstadoCivil.UnionLibre },
        new EstadoCivilItem { Nombre = "Divorciado(a)", Valor = SGRRHH.Core.Enums.EstadoCivil.Divorciado },
        new EstadoCivilItem { Nombre = "Viudo(a)", Valor = SGRRHH.Core.Enums.EstadoCivil.Viudo }
    };
    
    /// <summary>
    /// Lista de tipos de contrato
    /// </summary>
    public ObservableCollection<TipoContratoItem> TiposContrato { get; } = new()
    {
        new TipoContratoItem { Nombre = "Indefinido", Valor = SGRRHH.Core.Enums.TipoContrato.Indefinido },
        new TipoContratoItem { Nombre = "Fijo", Valor = SGRRHH.Core.Enums.TipoContrato.Fijo },
        new TipoContratoItem { Nombre = "Obra o Labor", Valor = SGRRHH.Core.Enums.TipoContrato.ObraLabor },
        new TipoContratoItem { Nombre = "Prestación de Servicios", Valor = SGRRHH.Core.Enums.TipoContrato.PrestacionServicios },
        new TipoContratoItem { Nombre = "Aprendizaje", Valor = SGRRHH.Core.Enums.TipoContrato.Aprendizaje }
    };
    
    /// <summary>
    /// Lista de estados de empleado
    /// </summary>
    public ObservableCollection<EstadoEmpleadoFormItem> EstadosEmpleado { get; } = new()
    {
        new EstadoEmpleadoFormItem { Nombre = "Activo", Valor = SGRRHH.Core.Enums.EstadoEmpleado.Activo },
        new EstadoEmpleadoFormItem { Nombre = "En Vacaciones", Valor = SGRRHH.Core.Enums.EstadoEmpleado.EnVacaciones },
        new EstadoEmpleadoFormItem { Nombre = "En Licencia", Valor = SGRRHH.Core.Enums.EstadoEmpleado.EnLicencia },
        new EstadoEmpleadoFormItem { Nombre = "Suspendido", Valor = SGRRHH.Core.Enums.EstadoEmpleado.Suspendido },
        new EstadoEmpleadoFormItem { Nombre = "Retirado", Valor = SGRRHH.Core.Enums.EstadoEmpleado.Retirado }
    };
    
    /// <summary>
    /// Evento cuando se guarda exitosamente
    /// </summary>
    public event EventHandler? SaveCompleted;
    
    /// <summary>
    /// Evento para cerrar el formulario
    /// </summary>
    public event EventHandler? CloseRequested;
    
    /// <summary>
    /// Evento para abrir la ventana de documentos del empleado recién creado
    /// </summary>
    public event EventHandler<int>? OpenDocumentosRequested;
    
    public EmpleadoFormViewModel(
        IEmpleadoService empleadoService, 
        IDepartamentoService departamentoService,
        ICargoService cargoService,
        IDialogService dialogService,
        IDocumentoEmpleadoService? documentoService = null)
    {
        _empleadoService = empleadoService;
        _departamentoService = departamentoService;
        _cargoService = cargoService;
        _dialogService = dialogService;
        _documentoService = documentoService;
    }
    
    /// <summary>
    /// Inicializa para crear un nuevo empleado
    /// </summary>
    public async Task InitializeForCreateAsync()
    {
        _isEditing = false;
        WindowTitle = "Nuevo Empleado";
        
        IsLoading = true;
        try
        {
            await LoadCatalogosAsync();
            Codigo = await _empleadoService.GetNextCodigoAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    /// <summary>
    /// Inicializa para editar un empleado existente
    /// </summary>
    public async Task InitializeForEditAsync(int empleadoId)
    {
        _isEditing = true;
        _empleadoId = empleadoId;
        WindowTitle = "Editar Empleado";
        
        IsLoading = true;
        try
        {
            await LoadCatalogosAsync();
            
            var empleado = await _empleadoService.GetByIdAsync(empleadoId);
            if (empleado != null)
            {
                LoadEmpleadoData(empleado);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    private async Task LoadCatalogosAsync()
    {
        // Cargar departamentos
        var departamentos = await _departamentoService.GetAllAsync();
        Departamentos.Clear();
        foreach (var dep in departamentos)
        {
            Departamentos.Add(dep);
        }
        
        // Cargar todos los cargos
        var cargos = await _cargoService.GetAllAsync();
        Cargos.Clear();
        foreach (var cargo in cargos)
        {
            Cargos.Add(cargo);
        }
        
        // Cargar empleados para supervisores
        var empleados = await _empleadoService.GetAllAsync();
        Supervisores.Clear();
        Supervisores.Add(new Empleado { Id = 0, Nombres = "(Sin supervisor)", Apellidos = "" });
        foreach (var emp in empleados)
        {
            if (emp.Id != _empleadoId) // No incluir al empleado actual si está editando
            {
                Supervisores.Add(emp);
            }
        }
    }
    
    private void LoadEmpleadoData(Empleado empleado)
    {
        Codigo = empleado.Codigo;
        Cedula = empleado.Cedula;
        Nombres = empleado.Nombres;
        Apellidos = empleado.Apellidos;
        FechaNacimiento = empleado.FechaNacimiento;
        Genero = empleado.Genero;
        EstadoCivil = empleado.EstadoCivil;
        Direccion = empleado.Direccion;
        Telefono = empleado.Telefono;
        TelefonoEmergencia = empleado.TelefonoEmergencia;
        ContactoEmergencia = empleado.ContactoEmergencia;
        Email = empleado.Email;
        FotoPath = empleado.FotoPath;
        FechaIngreso = empleado.FechaIngreso;
        TipoContrato = empleado.TipoContrato;
        Estado = empleado.Estado;
        Observaciones = empleado.Observaciones;
        
        // Seleccionar departamento
        SelectedDepartamento = Departamentos.FirstOrDefault(d => d.Id == empleado.DepartamentoId);
        
        // Seleccionar cargo
        SelectedCargo = Cargos.FirstOrDefault(c => c.Id == empleado.CargoId);
        
        // Seleccionar supervisor
        SelectedSupervisor = Supervisores.FirstOrDefault(s => s.Id == empleado.SupervisorId);
    }
    
    /// <summary>
    /// Actualiza los cargos disponibles según el departamento seleccionado
    /// </summary>
    partial void OnSelectedDepartamentoChanged(Departamento? value)
    {
        CargosDisponibles.Clear();
        
        if (value != null && value.Id > 0)
        {
            var cargosDepartamento = Cargos.Where(c => c.DepartamentoId == value.Id || c.DepartamentoId == null);
            foreach (var cargo in cargosDepartamento)
            {
                CargosDisponibles.Add(cargo);
            }
        }
        else
        {
            foreach (var cargo in Cargos)
            {
                CargosDisponibles.Add(cargo);
            }
        }
    }
    
    /// <summary>
    /// Constantes de validación
    /// </summary>
    private const long MaxFotoSizeBytes = 5 * 1024 * 1024; // 5MB
    
    /// <summary>
    /// Selecciona una foto para el empleado
    /// </summary>
    [RelayCommand]
    private void SelectFoto()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Imágenes|*.jpg;*.jpeg;*.png;*.bmp|Todos los archivos|*.*",
            Title = "Seleccionar foto del empleado"
        };
        
        if (dialog.ShowDialog() == true)
        {
            try
            {
                var fileInfo = new FileInfo(dialog.FileName);
                
                // Validar tamaño máximo de foto (5MB)
                if (fileInfo.Length > MaxFotoSizeBytes)
                {
                    _dialogService.ShowWarning(
                        $"La imagen es muy grande ({fileInfo.Length / 1024 / 1024:F1} MB).\nTamaño máximo permitido: 5 MB.",
                        "Imagen demasiado grande");
                    return;
                }
                
                _newFotoData = File.ReadAllBytes(dialog.FileName);
                _newFotoExtension = Path.GetExtension(dialog.FileName).ToLower();
                
                // Validar que sea una extensión de imagen válida
                var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp" };
                if (!validExtensions.Contains(_newFotoExtension))
                {
                    _dialogService.ShowWarning(
                        "Formato de imagen no válido.\nFormatos permitidos: JPG, JPEG, PNG, BMP.",
                        "Formato inválido");
                    _newFotoData = null;
                    _newFotoExtension = null;
                    return;
                }
                
                FotoPath = dialog.FileName;
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Error al cargar la imagen: {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// Elimina la foto del empleado
    /// </summary>
    [RelayCommand]
    private void RemoveFoto()
    {
        _newFotoData = null;
        _newFotoExtension = null;
        FotoPath = null;
    }
    
    /// <summary>
    /// Abre la ventana de documentos del empleado
    /// </summary>
    [RelayCommand]
    private void OpenDocumentos()
    {
        if (_empleadoId > 0)
        {
            OpenDocumentosRequested?.Invoke(this, _empleadoId);
        }
    }
    
    /// <summary>
    /// Selecciona documento de cédula
    /// </summary>
    [RelayCommand]
    private void SelectDocCedula()
    {
        SelectPendingDocument(TipoDocumentoEmpleado.Cedula, val => DocCedulaFileName = val);
    }
    
    /// <summary>
    /// Selecciona documento de hoja de vida
    /// </summary>
    [RelayCommand]
    private void SelectDocHojaVida()
    {
        SelectPendingDocument(TipoDocumentoEmpleado.HojaVida, val => DocHojaVidaFileName = val);
    }
    
    /// <summary>
    /// Selecciona documento de contrato
    /// </summary>
    [RelayCommand]
    private void SelectDocContrato()
    {
        SelectPendingDocument(TipoDocumentoEmpleado.ContratoFirmado, val => DocContratoFileName = val);
    }
    
    /// <summary>
    /// Selecciona certificado bancario
    /// </summary>
    [RelayCommand]
    private void SelectDocCertificadoBancario()
    {
        SelectPendingDocument(TipoDocumentoEmpleado.CertificadoBancario, val => DocCertificadoBancarioFileName = val);
    }
    
    /// <summary>
    /// Método genérico para seleccionar un documento pendiente
    /// </summary>
    private void SelectPendingDocument(TipoDocumentoEmpleado tipo, Action<string?> setFileName)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Documentos PDF|*.pdf|Imágenes|*.jpg;*.jpeg;*.png|Todos los archivos|*.*",
            Title = $"Seleccionar {SGRRHH.Infrastructure.Services.DocumentoEmpleadoService.GetNombreTipoDocumento(tipo)}"
        };
        
        if (dialog.ShowDialog() == true)
        {
            try
            {
                var fileBytes = File.ReadAllBytes(dialog.FileName);
                var fileName = Path.GetFileName(dialog.FileName);
                
                _pendingDocuments[tipo] = new PendingDocument
                {
                    TipoDocumento = tipo,
                    FileBytes = fileBytes,
                    FileName = fileName,
                    OriginalPath = dialog.FileName
                };
                
                setFileName(fileName);
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Error al cargar el archivo: {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// Elimina documento de cédula pendiente
    /// </summary>
    [RelayCommand]
    private void RemoveDocCedula()
    {
        _pendingDocuments.Remove(TipoDocumentoEmpleado.Cedula);
        DocCedulaFileName = null;
    }
    
    /// <summary>
    /// Elimina documento de hoja de vida pendiente
    /// </summary>
    [RelayCommand]
    private void RemoveDocHojaVida()
    {
        _pendingDocuments.Remove(TipoDocumentoEmpleado.HojaVida);
        DocHojaVidaFileName = null;
    }
    
    /// <summary>
    /// Elimina documento de contrato pendiente
    /// </summary>
    [RelayCommand]
    private void RemoveDocContrato()
    {
        _pendingDocuments.Remove(TipoDocumentoEmpleado.ContratoFirmado);
        DocContratoFileName = null;
    }
    
    /// <summary>
    /// Elimina certificado bancario pendiente
    /// </summary>
    [RelayCommand]
    private void RemoveDocCertificadoBancario()
    {
        _pendingDocuments.Remove(TipoDocumentoEmpleado.CertificadoBancario);
        DocCertificadoBancarioFileName = null;
    }
    
    /// <summary>
    /// Valida los datos del formulario
    /// </summary>
    private List<string> ValidateForm()
    {
        var errors = new List<string>();
        
        // Validaciones de campos obligatorios
        if (string.IsNullOrWhiteSpace(Cedula))
            errors.Add("La cédula es obligatoria");
            
        if (string.IsNullOrWhiteSpace(Nombres))
            errors.Add("Los nombres son obligatorios");
            
        if (string.IsNullOrWhiteSpace(Apellidos))
            errors.Add("Los apellidos son obligatorios");
            
        if (FechaIngreso == default)
            errors.Add("La fecha de ingreso es obligatoria");
        
        // Validación de formato de cédula colombiana (solo números, 6-10 dígitos)
        if (!string.IsNullOrWhiteSpace(Cedula))
        {
            var cedulaLimpia = Cedula.Trim().Replace(".", "").Replace(",", "");
            if (!Regex.IsMatch(cedulaLimpia, @"^\d{6,10}$"))
                errors.Add("La cédula debe contener entre 6 y 10 dígitos numéricos");
        }
        
        // Validación de formato de teléfono (solo números, 7-15 dígitos)
        if (!string.IsNullOrWhiteSpace(Telefono) && !Regex.IsMatch(Telefono.Trim(), @"^\d{7,15}$"))
            errors.Add("El teléfono debe contener solo números (7-15 dígitos)");
        
        // Validación de formato de teléfono de emergencia
        if (!string.IsNullOrWhiteSpace(TelefonoEmergencia) && !Regex.IsMatch(TelefonoEmergencia.Trim(), @"^\d{7,15}$"))
            errors.Add("El teléfono de emergencia debe contener solo números (7-15 dígitos)");
            
        // Validación de fecha de ingreso
        if (FechaIngreso > DateTime.Today)
            errors.Add("La fecha de ingreso no puede ser futura");
        
        // Validación de fecha de nacimiento
        if (FechaNacimiento.HasValue)
        {
            if (FechaNacimiento.Value > DateTime.Today)
                errors.Add("La fecha de nacimiento no puede ser futura");
            
            // Validar edad mínima de 16 años
            var edad = DateTime.Today.Year - FechaNacimiento.Value.Year;
            if (DateTime.Today < FechaNacimiento.Value.AddYears(edad))
                edad--;
            
            if (edad < 16)
                errors.Add("El empleado debe tener al menos 16 años de edad");
            
            // Validar edad máxima razonable (100 años)
            if (edad > 100)
                errors.Add("La fecha de nacimiento no parece válida");
        }
            
        // Validación de correo electrónico
        if (!string.IsNullOrWhiteSpace(Email) && !IsValidEmail(Email))
            errors.Add("El correo electrónico no tiene un formato válido");
        
        // Validación de nombres (solo letras y espacios)
        if (!string.IsNullOrWhiteSpace(Nombres) && !Regex.IsMatch(Nombres.Trim(), @"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$"))
            errors.Add("Los nombres solo pueden contener letras");
        
        // Validación de apellidos (solo letras y espacios)
        if (!string.IsNullOrWhiteSpace(Apellidos) && !Regex.IsMatch(Apellidos.Trim(), @"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$"))
            errors.Add("Los apellidos solo pueden contener letras");
        
        // Validaciones de longitud máxima de campos
        if (!string.IsNullOrWhiteSpace(Nombres) && Nombres.Length > 100)
            errors.Add("Los nombres no pueden exceder 100 caracteres");
        
        if (!string.IsNullOrWhiteSpace(Apellidos) && Apellidos.Length > 100)
            errors.Add("Los apellidos no pueden exceder 100 caracteres");
        
        if (!string.IsNullOrWhiteSpace(Direccion) && Direccion.Length > 300)
            errors.Add("La dirección no puede exceder 300 caracteres");
        
        if (!string.IsNullOrWhiteSpace(Email) && Email.Length > 150)
            errors.Add("El email no puede exceder 150 caracteres");
        
        if (!string.IsNullOrWhiteSpace(ContactoEmergencia) && ContactoEmergencia.Length > 100)
            errors.Add("El contacto de emergencia no puede exceder 100 caracteres");
        
        if (!string.IsNullOrWhiteSpace(Observaciones) && Observaciones.Length > 1000)
            errors.Add("Las observaciones no pueden exceder 1000 caracteres");
            
        return errors;
    }
    
    private static bool IsValidEmail(string email)
    {
        try
        {
            // Usar expresión regular más estricta para validar email
            var emailRegex = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            if (!Regex.IsMatch(email.Trim(), emailRegex))
                return false;
            
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email.Trim();
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Guarda el empleado
    /// </summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        var validationErrors = ValidateForm();
        if (validationErrors.Any())
        {
            _dialogService.ShowWarning(
                "Por favor corrija los siguientes errores:\n\n• " + string.Join("\n• ", validationErrors),
                "Validación");
            return;
        }
        
        IsLoading = true;
        
        try
        {
            var empleado = new Empleado
            {
                Id = _empleadoId,
                Codigo = Codigo,
                Cedula = Cedula,
                Nombres = Nombres,
                Apellidos = Apellidos,
                FechaNacimiento = FechaNacimiento,
                Genero = Genero,
                EstadoCivil = EstadoCivil,
                Direccion = Direccion,
                Telefono = Telefono,
                TelefonoEmergencia = TelefonoEmergencia,
                ContactoEmergencia = ContactoEmergencia,
                Email = Email,
                FechaIngreso = FechaIngreso,
                TipoContrato = TipoContrato,
                Estado = Estado,
                CargoId = SelectedCargo?.Id,
                Cargo = SelectedCargo,  // Save complete entity for denormalization
                DepartamentoId = SelectedDepartamento?.Id,
                Departamento = SelectedDepartamento,  // Save complete entity for denormalization
                SupervisorId = SelectedSupervisor?.Id > 0 ? SelectedSupervisor.Id : null,
                Supervisor = SelectedSupervisor?.Id > 0 ? SelectedSupervisor : null,  // Save complete entity
                Observaciones = Observaciones
            };
            
            ServiceResult result;
            
            if (_isEditing)
            {
                result = await _empleadoService.UpdateAsync(empleado);
            }
            else
            {
                // Usar el nuevo método que maneja la lógica de roles en el servicio
                var currentUser = App.CurrentUser;
                if (currentUser != null)
                {
                    var createResult = await _empleadoService.CreateWithRoleAsync(
                        empleado, 
                        currentUser.Id, 
                        currentUser.Rol);
                    result = createResult;
                    if (createResult.Success && createResult.Data != null)
                    {
                        _empleadoId = createResult.Data.Id;
                    }
                }
                else
                {
                    // Fallback sin rol (caso raro)
                    var createResult = await _empleadoService.CreateAsync(empleado);
                    result = createResult;
                    if (createResult.Success && createResult.Data != null)
                    {
                        _empleadoId = createResult.Data.Id;
                    }
                }
            }
            
            if (result.Success)
            {
                // Guardar foto si se seleccionó una nueva
                if (_newFotoData != null && _newFotoExtension != null)
                {
                    await _empleadoService.SaveFotoAsync(_empleadoId, _newFotoData, _newFotoExtension);
                }
                
                // Subir documentos pendientes
                var docsUploadedCount = 0;
                if (_pendingDocuments.Any() && _documentoService != null)
                {
                    var currentUser = App.CurrentUser;
                    foreach (var pending in _pendingDocuments.Values)
                    {
                        try
                        {
                            var doc = new DocumentoEmpleado
                            {
                                EmpleadoId = _empleadoId,
                                TipoDocumento = pending.TipoDocumento,
                                Nombre = SGRRHH.Infrastructure.Services.DocumentoEmpleadoService.GetNombreTipoDocumento(pending.TipoDocumento),
                                NombreArchivoOriginal = pending.FileName,
                                Descripcion = $"Cargado al crear empleado"
                            };
                            
                            var uploadResult = await _documentoService.SubirDocumentoAsync(
                                doc, 
                                pending.FileBytes, 
                                currentUser?.Id ?? 0, 
                                currentUser?.Rol ?? RolUsuario.Operador);
                            
                            if (uploadResult.Success)
                                docsUploadedCount++;
                        }
                        catch
                        {
                            // Continuar con los demás documentos
                        }
                    }
                }
                
                SaveCompleted?.Invoke(this, EventArgs.Empty);
                
                // Mostrar mensaje de éxito
                var mensaje = $"Empleado '{Nombres} {Apellidos}' {(_isEditing ? "actualizado" : "creado")} exitosamente.";
                if (docsUploadedCount > 0)
                {
                    mensaje += $"\n\n{docsUploadedCount} documento(s) cargado(s).";
                }
                
                // Solo preguntar por más documentos si es nuevo y no se cargaron todos los requeridos
                var pendingRequiredDocs = !_isEditing && _pendingDocuments.Count < 4;
                if (pendingRequiredDocs)
                {
                    var cargarMasDocumentos = _dialogService.Confirm(
                        mensaje + "\n\n¿Desea cargar más documentos ahora?",
                        "Documentos Adicionales");
                    
                    if (cargarMasDocumentos)
                    {
                        OpenDocumentosRequested?.Invoke(this, _empleadoId);
                    }
                }
                else
                {
                    _dialogService.ShowSuccess(mensaje);
                }
                
                CloseRequested?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                _dialogService.ShowError(result.Message);
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Error al guardar: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    /// <summary>
    /// Cancela y cierra el formulario
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}

// Clases auxiliares para los combos
public class GeneroItem
{
    public string Nombre { get; set; } = string.Empty;
    public Genero Valor { get; set; }
}

public class EstadoCivilItem
{
    public string Nombre { get; set; } = string.Empty;
    public EstadoCivil Valor { get; set; }
}

public class TipoContratoItem
{
    public string Nombre { get; set; } = string.Empty;
    public TipoContrato Valor { get; set; }
}

public class EstadoEmpleadoFormItem
{
    public string Nombre { get; set; } = string.Empty;
    public EstadoEmpleado Valor { get; set; }
}

/// <summary>
/// Representa un documento pendiente de subir
/// </summary>
public class PendingDocument
{
    public TipoDocumentoEmpleado TipoDocumento { get; set; }
    public byte[] FileBytes { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string OriginalPath { get; set; } = string.Empty;
}
