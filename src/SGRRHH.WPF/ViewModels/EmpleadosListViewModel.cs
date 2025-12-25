using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGRRHH.Core.Common;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;
using SGRRHH.WPF.Helpers;
using SGRRHH.WPF.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// ViewModel para la lista de empleados
/// Permisos según matriz:
/// - Administrador: Todo (crear, editar, desactivar, eliminar, aprobar)
/// - Operador (Secretaria): Crear, Editar, Desactivar, Ver
/// - Aprobador (Ingeniera): Solo Ver y Aprobar/Rechazar
/// </summary>
public partial class EmpleadosListViewModel : ViewModelBase
{
    private readonly IEmpleadoService _empleadoService;
    private readonly IDepartamentoService _departamentoService;
    private readonly IDialogService _dialogService;
    
    // Control de permisos por rol
    [ObservableProperty]
    private bool _puedeCrear; // Crear empleados
    
    [ObservableProperty]
    private bool _puedeEditar; // Editar empleados
    
    [ObservableProperty]
    private bool _puedeEliminar; // Eliminar permanentemente - Solo Admin
    
    [ObservableProperty]
    private ObservableCollection<Empleado> _empleados = new();
    
    [ObservableProperty]
    private ObservableCollection<Empleado> _empleadosPendientes = new();
    
    [ObservableProperty]
    private ObservableCollection<Departamento> _departamentos = new();
    
    [ObservableProperty]
    private Empleado? _selectedEmpleado;
    
    [ObservableProperty]
    private Empleado? _selectedPendiente;
    
    [ObservableProperty]
    private string _searchText = string.Empty;
    
    [ObservableProperty]
    private Departamento? _selectedDepartamento;
    
    [ObservableProperty]
    private EstadoEmpleado? _selectedEstado;
    
    [ObservableProperty]
    private int _totalEmpleados;
    
    [ObservableProperty]
    private int _totalPendientes;
    
    [ObservableProperty]
    private bool _canApprove;
    
    [ObservableProperty]
    private bool _showPendientes;
    
    /// <summary>
    /// Controla la visibilidad de la pantalla de bienvenida
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsListVisible))]
    private bool _isHomeVisible = true;
    
    /// <summary>
    /// Indica si la lista de empleados debe estar visible
    /// </summary>
    public bool IsListVisible => !IsHomeVisible;
    
    /// <summary>
    /// Texto con la fecha actual para el footer
    /// </summary>
    public string CurrentDateText => DateTime.Now.ToString("dddd, dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("es-ES"));
    
    /// <summary>
    /// Lista de estados para el filtro
    /// </summary>
    public ObservableCollection<EnumComboItem<EstadoEmpleado>> Estados { get; } = new()
    {
        new EnumComboItem<EstadoEmpleado> { Nombre = "Todos", Valor = null },
        new EnumComboItem<EstadoEmpleado> { Nombre = "Pendiente Aprobación", Valor = EstadoEmpleado.PendienteAprobacion },
        new EnumComboItem<EstadoEmpleado> { Nombre = "Activo", Valor = EstadoEmpleado.Activo },
        new EnumComboItem<EstadoEmpleado> { Nombre = "Vacaciones", Valor = EstadoEmpleado.EnVacaciones },
        new EnumComboItem<EstadoEmpleado> { Nombre = "Licencia", Valor = EstadoEmpleado.EnLicencia },
        new EnumComboItem<EstadoEmpleado> { Nombre = "Suspendido", Valor = EstadoEmpleado.Suspendido },
        new EnumComboItem<EstadoEmpleado> { Nombre = "Retirado", Valor = EstadoEmpleado.Retirado },
        new EnumComboItem<EstadoEmpleado> { Nombre = "Rechazado", Valor = EstadoEmpleado.Rechazado }
    };
    
    /// <summary>
    /// Evento para solicitar apertura de formulario de nuevo empleado
    /// </summary>
    public event EventHandler? CreateEmpleadoRequested;
    
    /// <summary>
    /// Evento para solicitar apertura de formulario de edición
    /// </summary>
    public event EventHandler<Empleado>? EditEmpleadoRequested;
    
    /// <summary>
    /// Evento para solicitar ver detalle de empleado
    /// </summary>
    public event EventHandler<Empleado>? ViewEmpleadoRequested;
    
    public EmpleadosListViewModel(
        IEmpleadoService empleadoService, 
        IDepartamentoService departamentoService,
        IDialogService dialogService)
    {
        _empleadoService = empleadoService;
        _departamentoService = departamentoService;
        _dialogService = dialogService;
        
        // Determinar permisos según rol del usuario actual
        var currentUser = App.CurrentUser;
        var rolActual = currentUser?.Rol ?? RolUsuario.Operador;
        
        // Verificar si el usuario puede aprobar (Admin o Aprobador)
        CanApprove = currentUser != null && 
            (rolActual == RolUsuario.Administrador || rolActual == RolUsuario.Aprobador);
        ShowPendientes = CanApprove;
        
        // Ingeniera (Aprobador) NO puede crear ni editar - solo ver y aprobar
        // Admin y Secretaria (Operador) pueden crear/editar
        PuedeCrear = rolActual == RolUsuario.Administrador || rolActual == RolUsuario.Operador;
        PuedeEditar = rolActual == RolUsuario.Administrador || rolActual == RolUsuario.Operador;
        
        // Solo Admin puede eliminar permanentemente
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
            // Cargar departamentos para el filtro
            var departamentos = await _departamentoService.GetAllAsync();
            Departamentos.Clear();
            Departamentos.Add(new Departamento { Id = 0, Nombre = "Todos los departamentos" });
            foreach (var dep in departamentos)
            {
                Departamentos.Add(dep);
            }
            
            // Cargar empleados pendientes si puede aprobar
            if (CanApprove)
            {
                await LoadPendientesAsync();
            }
            
            // Cargar empleados
            await SearchEmpleadosAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al cargar datos: {ex.Message}";
            _dialogService.ShowError($"Error al cargar los datos: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    /// <summary>
    /// Carga empleados pendientes de aprobación
    /// </summary>
    private async Task LoadPendientesAsync()
    {
        var pendientes = await _empleadoService.GetPendientesAprobacionAsync();
        EmpleadosPendientes.Clear();
        foreach (var emp in pendientes)
        {
            EmpleadosPendientes.Add(emp);
        }
        TotalPendientes = EmpleadosPendientes.Count;
    }
    
    /// <summary>
    /// Busca empleados según los filtros actuales
    /// </summary>
    [RelayCommand]
    private async Task SearchEmpleadosAsync()
    {
        IsLoading = true;
        StatusMessage = "Buscando...";
        
        try
        {
            IEnumerable<Empleado> resultado;
            
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                resultado = await _empleadoService.SearchAsync(SearchText);
            }
            else if (SelectedDepartamento != null && SelectedDepartamento.Id > 0)
            {
                resultado = await _empleadoService.GetByDepartamentoAsync(SelectedDepartamento.Id);
            }
            else if (SelectedEstado.HasValue)
            {
                resultado = await _empleadoService.GetByEstadoAsync(SelectedEstado.Value);
            }
            else
            {
                resultado = await _empleadoService.GetAllAsync();
            }
            
            // Aplicar filtros adicionales si es necesario
            if (SelectedDepartamento != null && SelectedDepartamento.Id > 0 && !string.IsNullOrWhiteSpace(SearchText))
            {
                resultado = resultado.Where(e => e.DepartamentoId == SelectedDepartamento.Id);
            }
            
            if (SelectedEstado.HasValue && (SelectedDepartamento?.Id > 0 || !string.IsNullOrWhiteSpace(SearchText)))
            {
                resultado = resultado.Where(e => e.Estado == SelectedEstado.Value);
            }
            
            Empleados.Clear();
            foreach (var empleado in resultado)
            {
                Empleados.Add(empleado);
            }
            
            TotalEmpleados = Empleados.Count;
            StatusMessage = $"{TotalEmpleados} empleado(s) encontrado(s)";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error en la búsqueda: {ex.Message}";
            _dialogService.ShowError($"Error en la búsqueda: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    /// <summary>
    /// Limpia los filtros y recarga todos los empleados
    /// </summary>
    [RelayCommand]
    private async Task ClearFiltersAsync()
    {
        SearchText = string.Empty;
        SelectedDepartamento = Departamentos.FirstOrDefault();
        SelectedEstado = null;
        await SearchEmpleadosAsync();
    }
    
    /// <summary>
    /// Abre el formulario para crear un nuevo empleado
    /// </summary>
    [RelayCommand]
    private void CreateEmpleado()
    {
        if (!PuedeCrear)
        {
            _dialogService.ShowWarning("No tiene permisos para crear empleados", "Permiso denegado");
            return;
        }
        
        CreateEmpleadoRequested?.Invoke(this, EventArgs.Empty);
    }
    
    /// <summary>
    /// Abre el formulario para editar el empleado seleccionado
    /// </summary>
    [RelayCommand]
    private void EditEmpleado()
    {
        if (!PuedeEditar)
        {
            _dialogService.ShowWarning("No tiene permisos para editar empleados", "Permiso denegado");
            return;
        }
        
        if (SelectedEmpleado == null)
        {
            _dialogService.ShowInfo("Seleccione un empleado para editar");
            return;
        }
        
        EditEmpleadoRequested?.Invoke(this, SelectedEmpleado);
    }
    
    /// <summary>
    /// Muestra el detalle del empleado seleccionado
    /// </summary>
    [RelayCommand]
    private void ViewEmpleado()
    {
        if (SelectedEmpleado == null)
        {
            _dialogService.ShowInfo("Seleccione un empleado para ver su detalle");
            return;
        }
        
        ViewEmpleadoRequested?.Invoke(this, SelectedEmpleado);
    }
    
    /// <summary>
    /// Muestra el detalle del empleado pendiente seleccionado
    /// </summary>
    [RelayCommand]
    private void ViewPendiente()
    {
        if (SelectedPendiente == null)
        {
            _dialogService.ShowInfo("Seleccione un empleado pendiente para ver su detalle");
            return;
        }
        
        ViewEmpleadoRequested?.Invoke(this, SelectedPendiente);
    }
    
    /// <summary>
    /// Desactiva el empleado seleccionado
    /// </summary>
    [RelayCommand]
    private async Task DeactivateEmpleadoAsync()
    {
        if (!PuedeEditar)
        {
            _dialogService.ShowWarning("No tiene permisos para desactivar empleados", "Permiso denegado");
            return;
        }
        
        if (SelectedEmpleado == null)
        {
            _dialogService.ShowInfo("Seleccione un empleado para desactivar");
            return;
        }
        
        var confirmado = _dialogService.Confirm(
            $"¿Está seguro de desactivar al empleado {SelectedEmpleado.NombreCompleto}?",
            "Confirmar desactivación");
            
        if (confirmado)
        {
            try
            {
                var serviceResult = await _empleadoService.DeactivateAsync(SelectedEmpleado.Id);
                
                if (serviceResult.Success)
                {
                    await SearchEmpleadosAsync();
                    _dialogService.ShowSuccess(serviceResult.Message);
                }
                else
                {
                    _dialogService.ShowError(serviceResult.Message);
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Error al desactivar el empleado: {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// Elimina permanentemente el empleado seleccionado
    /// </summary>
    [RelayCommand]
    private async Task DeleteEmpleadoAsync()
    {
        if (!PuedeEliminar)
        {
            _dialogService.ShowWarning("Solo el administrador puede eliminar empleados permanentemente", "Permiso denegado");
            return;
        }
        
        if (SelectedEmpleado == null)
        {
            _dialogService.ShowInfo("Seleccione un empleado para eliminar");
            return;
        }
        
        var primeraConfirmacion = _dialogService.ConfirmWarning(
            $"⚠️ ADVERTENCIA: Esta acción eliminará PERMANENTEMENTE al empleado {SelectedEmpleado.NombreCompleto} y todos sus datos asociados.\n\n¿Está completamente seguro?",
            "Confirmar eliminación permanente");
            
        if (primeraConfirmacion)
        {
            // Segunda confirmación
            var segundaConfirmacion = _dialogService.ConfirmWarning(
                "Esta acción NO se puede deshacer.\n\n¿Desea continuar con la eliminación permanente?",
                "Última confirmación");
                
            if (segundaConfirmacion)
            {
                try
                {
                    var serviceResult = await _empleadoService.DeletePermanentlyAsync(SelectedEmpleado.Id);
                    
                    if (serviceResult.Success)
                    {
                        await SearchEmpleadosAsync();
                        _dialogService.ShowSuccess(serviceResult.Message);
                    }
                    else
                    {
                        _dialogService.ShowError(serviceResult.Message);
                    }
                }
                catch (Exception ex)
                {
                    _dialogService.ShowError($"Error al eliminar el empleado: {ex.Message}");
                }
            }
        }
    }
    
    /// <summary>
    /// Aprueba el empleado pendiente seleccionado
    /// </summary>
    [RelayCommand]
    private async Task AprobarEmpleadoAsync()
    {
        if (SelectedPendiente == null)
        {
            _dialogService.ShowInfo("Seleccione un empleado pendiente para aprobar");
            return;
        }

        // Guardar el nombre antes de recargar los datos
        var nombreEmpleado = SelectedPendiente.NombreCompleto;
        var empleadoId = SelectedPendiente.Id;

        var confirmado = _dialogService.Confirm(
            $"¿Está seguro de aprobar al empleado {nombreEmpleado}?\n\nEl empleado quedará activo en el sistema.",
            "Confirmar aprobación");

        if (confirmado)
        {
            try
            {
                var currentUser = App.CurrentUser;
                if (currentUser == null)
                {
                    _dialogService.ShowError("Error: Usuario no identificado");
                    return;
                }

                var serviceResult = await _empleadoService.AprobarAsync(empleadoId, currentUser.Id, currentUser.Rol);

                if (serviceResult.Success)
                {
                    await LoadPendientesAsync();
                    await SearchEmpleadosAsync();
                    _dialogService.ShowSuccess($"✅ {nombreEmpleado} ha sido aprobado exitosamente.");
                }
                else
                {
                    _dialogService.ShowError(serviceResult.Message);
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Error al aprobar el empleado: {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// Rechaza el empleado pendiente seleccionado
    /// </summary>
    [RelayCommand]
    private async Task RechazarEmpleadoAsync()
    {
        if (SelectedPendiente == null)
        {
            _dialogService.ShowInfo("Seleccione un empleado pendiente para rechazar");
            return;
        }

        // Guardar el nombre antes de potenciales cambios
        var nombreEmpleado = SelectedPendiente.NombreCompleto;
        var empleadoId = SelectedPendiente.Id;

        // Pedir motivo del rechazo
        var motivo = _dialogService.ShowInputDialog(
            $"Ingrese el motivo del rechazo para:\n{nombreEmpleado}",
            "Motivo de Rechazo");

        if (string.IsNullOrWhiteSpace(motivo))
        {
            return;
        }

        var confirmado = _dialogService.ConfirmWarning(
            $"¿Está seguro de rechazar al empleado {nombreEmpleado}?\n\nMotivo: {motivo}",
            "Confirmar rechazo");

        if (confirmado)
        {
            try
            {
                var currentUser = App.CurrentUser;
                if (currentUser == null)
                {
                    _dialogService.ShowError("Error: Usuario no identificado");
                    return;
                }

                var serviceResult = await _empleadoService.RechazarAsync(empleadoId, currentUser.Id, motivo);

                if (serviceResult.Success)
                {
                    await LoadPendientesAsync();
                    await SearchEmpleadosAsync();
                    _dialogService.ShowSuccess($"❌ {nombreEmpleado} ha sido rechazado.");
                }
                else
                {
                    _dialogService.ShowError(serviceResult.Message);
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Error al rechazar el empleado: {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// Muestra la pantalla de inicio
    /// </summary>
    [RelayCommand]
    private void ShowHome()
    {
        IsHomeVisible = true;
    }
    
    /// <summary>
    /// Muestra la lista de empleados y carga los datos
    /// </summary>
    [RelayCommand]
    private async Task ShowListAsync()
    {
        IsHomeVisible = false;
        await LoadDataAsync();
    }
    
    /// <summary>
    /// Se ejecuta cuando cambia el texto de búsqueda
    /// </summary>
    partial void OnSearchTextChanged(string value)
    {
        // Búsqueda automática con debounce se implementaría aquí
        // Por simplicidad, el usuario debe presionar el botón de buscar
    }
    
    /// <summary>
    /// Se ejecuta cuando cambia el departamento seleccionado
    /// </summary>
    partial void OnSelectedDepartamentoChanged(Departamento? value)
    {
        if (value != null)
        {
            SearchEmpleadosAsync().SafeFireAndForget(showErrorMessage: false);
        }
    }
}
