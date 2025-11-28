using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;
using SGRRHH.WPF.Views;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// ViewModel para ver el detalle/expediente de un empleado
/// </summary>
public partial class EmpleadoDetailViewModel : ObservableObject
{
    private readonly IEmpleadoService _empleadoService;
    
    [ObservableProperty]
    private Empleado? _empleado;
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private string _windowTitle = "Expediente del Empleado";
    
    // Propiedades para mostrar información formateada
    public string NombreCompleto => Empleado?.NombreCompleto ?? string.Empty;
    public string Codigo => Empleado?.Codigo ?? string.Empty;
    public string Cedula => Empleado?.Cedula ?? string.Empty;
    public string FotoPath => Empleado?.FotoPath ?? string.Empty;
    
    public string FechaNacimiento => Empleado?.FechaNacimiento?.ToString("dd/MM/yyyy") ?? "No especificada";
    public string Genero => Empleado?.Genero?.ToString() ?? "No especificado";
    public string EstadoCivil => GetEstadoCivilText();
    public string Direccion => Empleado?.Direccion ?? "No especificada";
    public string Telefono => Empleado?.Telefono ?? "No especificado";
    public string TelefonoEmergencia => Empleado?.TelefonoEmergencia ?? "No especificado";
    public string ContactoEmergencia => Empleado?.ContactoEmergencia ?? "No especificado";
    public string Email => Empleado?.Email ?? "No especificado";
    
    public string Departamento => Empleado?.Departamento?.Nombre ?? "Sin asignar";
    public string Cargo => Empleado?.Cargo?.Nombre ?? "Sin asignar";
    public string Supervisor => Empleado?.Supervisor?.NombreCompleto ?? "Sin supervisor";
    public string FechaIngreso => Empleado?.FechaIngreso.ToString("dd/MM/yyyy") ?? string.Empty;
    public string TipoContrato => GetTipoContratoText();
    public string Estado => GetEstadoText();
    public string Antiguedad => $"{Empleado?.Antiguedad ?? 0} años";
    public string Observaciones => Empleado?.Observaciones ?? "Sin observaciones";
    
    /// <summary>
    /// Evento para solicitar edición
    /// </summary>
    public event EventHandler<Empleado>? EditRequested;
    
    /// <summary>
    /// Evento para ver documentos del empleado
    /// </summary>
    public event EventHandler<int>? ViewDocumentsRequested;
    
    /// <summary>
    /// Evento para cerrar la ventana
    /// </summary>
    public event EventHandler? CloseRequested;
    
    public EmpleadoDetailViewModel(IEmpleadoService empleadoService)
    {
        _empleadoService = empleadoService;
    }
    
    /// <summary>
    /// Carga los datos del empleado
    /// </summary>
    public async Task LoadEmpleadoAsync(int empleadoId)
    {
        IsLoading = true;
        
        try
        {
            Empleado = await _empleadoService.GetByIdAsync(empleadoId);
            
            if (Empleado != null)
            {
                WindowTitle = $"Expediente: {Empleado.NombreCompleto}";
                
                // Notificar cambios en las propiedades calculadas
                OnPropertyChanged(nameof(NombreCompleto));
                OnPropertyChanged(nameof(Codigo));
                OnPropertyChanged(nameof(Cedula));
                OnPropertyChanged(nameof(FotoPath));
                OnPropertyChanged(nameof(FechaNacimiento));
                OnPropertyChanged(nameof(Genero));
                OnPropertyChanged(nameof(EstadoCivil));
                OnPropertyChanged(nameof(Direccion));
                OnPropertyChanged(nameof(Telefono));
                OnPropertyChanged(nameof(TelefonoEmergencia));
                OnPropertyChanged(nameof(ContactoEmergencia));
                OnPropertyChanged(nameof(Email));
                OnPropertyChanged(nameof(Departamento));
                OnPropertyChanged(nameof(Cargo));
                OnPropertyChanged(nameof(Supervisor));
                OnPropertyChanged(nameof(FechaIngreso));
                OnPropertyChanged(nameof(TipoContrato));
                OnPropertyChanged(nameof(Estado));
                OnPropertyChanged(nameof(Antiguedad));
                OnPropertyChanged(nameof(Observaciones));
            }
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private void Edit()
    {
        if (Empleado != null)
        {
            EditRequested?.Invoke(this, Empleado);
        }
    }
    
    [RelayCommand]
    private void VerDocumentos()
    {
        if (Empleado != null)
        {
            ViewDocumentsRequested?.Invoke(this, Empleado.Id);
        }
    }
    
    [RelayCommand]
    private void Close()
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
    
    private string GetEstadoCivilText() => Empleado?.EstadoCivil switch
    {
        Core.Enums.EstadoCivil.Soltero => "Soltero(a)",
        Core.Enums.EstadoCivil.Casado => "Casado(a)",
        Core.Enums.EstadoCivil.UnionLibre => "Unión Libre",
        Core.Enums.EstadoCivil.Divorciado => "Divorciado(a)",
        Core.Enums.EstadoCivil.Viudo => "Viudo(a)",
        _ => "No especificado"
    };
    
    private string GetTipoContratoText() => Empleado?.TipoContrato switch
    {
        Core.Enums.TipoContrato.Indefinido => "Indefinido",
        Core.Enums.TipoContrato.Fijo => "Término Fijo",
        Core.Enums.TipoContrato.ObraLabor => "Obra o Labor",
        Core.Enums.TipoContrato.PrestacionServicios => "Prestación de Servicios",
        Core.Enums.TipoContrato.Aprendizaje => "Aprendizaje",
        _ => "No especificado"
    };
    
    private string GetEstadoText() => Empleado?.Estado switch
    {
        Core.Enums.EstadoEmpleado.Activo => "Activo",
        Core.Enums.EstadoEmpleado.EnVacaciones => "En Vacaciones",
        Core.Enums.EstadoEmpleado.EnLicencia => "En Licencia",
        Core.Enums.EstadoEmpleado.Suspendido => "Suspendido",
        Core.Enums.EstadoEmpleado.Retirado => "Retirado",
        _ => "Desconocido"
    };
}
