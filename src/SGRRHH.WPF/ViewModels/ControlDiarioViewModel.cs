using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;
using SGRRHH.WPF.Helpers;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// ViewModel para el historial de registros de control diario
/// Vista simplificada con tabla y operaciones CRUD
/// </summary>
public partial class ControlDiarioViewModel : ViewModelBase
{
    private readonly IControlDiarioService _controlDiarioService;
    private readonly IEmpleadoService _empleadoService;
    private readonly IDialogService _dialogService;
    
    // Control de permisos por rol
    [ObservableProperty]
    private bool _puedeEliminar; // Solo Admin
    
    [ObservableProperty]
    private bool _puedeEditar; // Admin y Operador
    
    #region Colecciones
    
    [ObservableProperty]
    private ObservableCollection<Empleado> _empleados = new();
    
    [ObservableProperty]
    private ObservableCollection<RegistroDiario> _registrosFiltrados = new();
    
    [ObservableProperty]
    private ObservableCollection<string> _meses = new();
    
    [ObservableProperty]
    private ObservableCollection<int> _anios = new();
    
    #endregion
    
    #region Filtros
    
    [ObservableProperty]
    private Empleado? _selectedEmpleado;
    
    [ObservableProperty]
    private string? _selectedMes;
    
    [ObservableProperty]
    private int _selectedAnio;
    
    #endregion
    
    #region Selección y Estado

    [ObservableProperty]
    private RegistroDiario? _selectedRegistro;

    [ObservableProperty]
    private ObservableCollection<RegistroDiario> _registrosSeleccionados = new();

    [ObservableProperty]
    private int _totalRegistros;
    
    [ObservableProperty]
    private decimal _horasMesActual;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoRegistros))]
    private bool _hasRegistros;
    
    public bool HasNoRegistros => !HasRegistros;
    
    #endregion
    
    public ControlDiarioViewModel(
        IControlDiarioService controlDiarioService,
        IEmpleadoService empleadoService,
        IDialogService dialogService)
    {
        _controlDiarioService = controlDiarioService;
        _empleadoService = empleadoService;
        _dialogService = dialogService;
        
        // Inicializar permisos según rol
        var rolActual = App.CurrentUser?.Rol ?? RolUsuario.Operador;
        PuedeEliminar = rolActual == RolUsuario.Administrador;
        PuedeEditar = rolActual == RolUsuario.Administrador || rolActual == RolUsuario.Operador;
        
        // Inicializar meses
        var culture = new CultureInfo("es-ES");
        for (int i = 1; i <= 12; i++)
        {
            Meses.Add(culture.DateTimeFormat.GetMonthName(i));
        }
        
        // Inicializar años (últimos 3 años)
        var currentYear = DateTime.Now.Year;
        for (int i = currentYear - 2; i <= currentYear; i++)
        {
            Anios.Add(i);
        }
        
        // Valores por defecto
        SelectedMes = Meses[DateTime.Now.Month - 1];
        SelectedAnio = currentYear;
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
            // Cargar empleados activos
            var empleados = await _empleadoService.GetAllAsync();
            Empleados.Clear();
            Empleados.Add(new Empleado { Id = 0, Nombres = "Todos", Apellidos = "los empleados" });
            foreach (var emp in empleados)
            {
                Empleados.Add(emp);
            }
            
            // Seleccionar "Todos" por defecto
            SelectedEmpleado = Empleados.FirstOrDefault();
            
            StatusMessage = "Seleccione filtros para ver los registros";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al cargar datos: {ex.Message}";
            _dialogService.ShowError($"Error al cargar datos: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    /// <summary>
    /// Carga los registros filtrados
    /// </summary>
    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsLoading = true;
        StatusMessage = "Cargando registros...";
        
        try
        {
            // Obtener mes seleccionado como número
            int mesNumero = Meses.IndexOf(SelectedMes ?? Meses[0]) + 1;
            
            IEnumerable<RegistroDiario> registros;
            
            if (SelectedEmpleado == null || SelectedEmpleado.Id == 0)
            {
                // Cargar todos los registros del mes/año
                registros = await _controlDiarioService.GetByMesAnioAsync(mesNumero, SelectedAnio);
            }
            else
            {
                // Cargar registros del empleado específico
                registros = await _controlDiarioService.GetByEmpleadoMesAnioAsync(
                    SelectedEmpleado.Id, mesNumero, SelectedAnio);
            }
            
            RegistrosFiltrados.Clear();
            foreach (var reg in registros.OrderByDescending(r => r.Fecha))
            {
                RegistrosFiltrados.Add(reg);
            }
            
            TotalRegistros = RegistrosFiltrados.Count;
            HorasMesActual = RegistrosFiltrados.Sum(r => r.TotalHoras);
            HasRegistros = RegistrosFiltrados.Any();
            
            StatusMessage = $"Se encontraron {TotalRegistros} registros";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            _dialogService.ShowError($"Error al cargar registros: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    /// <summary>
    /// Alterna la selección de un registro (para checkbox)
    /// </summary>
    [RelayCommand]
    private void ToggleRegistroSelection(RegistroDiario registro)
    {
        if (registro == null) return;

        if (RegistrosSeleccionados.Contains(registro))
        {
            RegistrosSeleccionados.Remove(registro);
        }
        else
        {
            RegistrosSeleccionados.Add(registro);
        }
    }

    /// <summary>
    /// Ver detalles de un registro o múltiples registros seleccionados
    /// </summary>
    [RelayCommand]
    private void ViewRegistro()
    {
        List<RegistroDiario> registrosAMostrar;

        // Si hay registros seleccionados con checkbox, usar esos
        if (RegistrosSeleccionados.Any())
        {
            registrosAMostrar = RegistrosSeleccionados.ToList();
        }
        // Si no hay selección múltiple, usar el registro seleccionado
        else if (SelectedRegistro != null)
        {
            registrosAMostrar = new List<RegistroDiario> { SelectedRegistro };
        }
        else
        {
            _dialogService.ShowInfo("Seleccione un registro para ver o use los checkboxes para seleccionar múltiples registros");
            return;
        }

        // Abrir ventana de previsualización completa
        var viewModel = new RegistrosDetalleViewModel(_dialogService);
        viewModel.Initialize(registrosAMostrar);

        var window = new Views.RegistrosDetalleWindow(viewModel)
        {
            Owner = Application.Current.MainWindow
        };

        window.ShowDialog();
    }
    
    /// <summary>
    /// Editar un registro (solo si está en borrador)
    /// </summary>
    [RelayCommand]
    private void EditRegistro()
    {
        if (SelectedRegistro == null)
        {
            _dialogService.ShowInfo("Seleccione un registro para editar");
            return;
        }
        
        if (!PuedeEditar)
        {
            _dialogService.ShowWarning("No tiene permisos para editar registros", "Permiso denegado");
            return;
        }
        
        if (SelectedRegistro.Estado != EstadoRegistroDiario.Borrador)
        {
            _dialogService.ShowInfo("Solo se pueden editar registros en estado Borrador");
            return;
        }
        
        // TODO: Abrir ventana de edición o navegar al wizard con el registro cargado
        _dialogService.ShowInfo("Función de edición en desarrollo.\nPor ahora, use el wizard para crear nuevos registros.");
    }
    
    /// <summary>
    /// Eliminar un registro
    /// </summary>
    [RelayCommand]
    private async Task DeleteRegistroAsync()
    {
        if (SelectedRegistro == null)
        {
            _dialogService.ShowInfo("Seleccione un registro para eliminar");
            return;
        }
        
        if (!PuedeEliminar)
        {
            _dialogService.ShowWarning("Solo el Administrador puede eliminar registros", "Permiso denegado");
            return;
        }
        
        if (SelectedRegistro.Estado != EstadoRegistroDiario.Borrador)
        {
            _dialogService.ShowInfo("Solo se pueden eliminar registros en estado Borrador");
            return;
        }
        
        var confirmado = _dialogService.ConfirmWarning(
            $"¿Está seguro de eliminar el registro del {SelectedRegistro.Fecha:dd/MM/yyyy}?\n" +
            $"Empleado: {SelectedRegistro.Empleado?.NombreCompleto}\n\n" +
            "Esta acción no se puede deshacer.",
            "Confirmar eliminación");
        
        if (!confirmado) return;
        
        IsLoading = true;
        
        try
        {
            var result = await _controlDiarioService.DeleteRegistroAsync(SelectedRegistro.Id);
            
            if (result.Success)
            {
                RegistrosFiltrados.Remove(SelectedRegistro);
                SelectedRegistro = null;
                TotalRegistros = RegistrosFiltrados.Count;
                HorasMesActual = RegistrosFiltrados.Sum(r => r.TotalHoras);
                HasRegistros = RegistrosFiltrados.Any();
                StatusMessage = "Registro eliminado correctamente";
                _dialogService.ShowSuccess("Registro eliminado correctamente");
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
    /// Exportar registros (placeholder)
    /// </summary>
    [RelayCommand]
    private void Export()
    {
        if (!RegistrosFiltrados.Any())
        {
            _dialogService.ShowInfo("No hay registros para exportar");
            return;
        }
        
        // TODO: Implementar exportación a Excel/CSV
        _dialogService.ShowInfo("Función de exportación en desarrollo");
    }
    
    #region Property Changed Handlers
    
    partial void OnSelectedEmpleadoChanged(Empleado? value)
    {
        if (value != null && Empleados.Count > 0)
        {
            _ = RefreshAsync();
        }
    }
    
    partial void OnSelectedMesChanged(string? value)
    {
        if (value != null && Empleados.Count > 0)
        {
            _ = RefreshAsync();
        }
    }
    
    partial void OnSelectedAnioChanged(int value)
    {
        if (value > 0 && Empleados.Count > 0)
        {
            _ = RefreshAsync();
        }
    }
    
    #endregion
}
