using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// Estados de los pasos del wizard para registro de actividades diarias
/// </summary>
public enum WizardStep
{
    SelectEmployees = 0,
    SelectDate = 1,
    EnterEntrada = 2,
    EnterSalida = 3,
    SelectActivity = 4,
    SelectProject = 5,
    EnterHours = 6,
    EnterDescription = 7,
    Review = 8
}

/// <summary>
/// ViewModel para el wizard conversacional de 7 pasos que permite registrar 
/// actividades diarias para múltiples empleados simultáneamente
/// </summary>
public partial class DailyActivityWizardViewModel : ViewModelBase
{
    private readonly IControlDiarioService _controlDiarioService;
    private readonly IEmpleadoService _empleadoService;
    private readonly IActividadService _actividadService;
    private readonly IProyectoService _proyectoService;

    #region ObservableProperties

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressPercentage))]
    [NotifyPropertyChangedFor(nameof(IsSelectEmployeesVisible))]
    [NotifyPropertyChangedFor(nameof(IsSelectDateVisible))]
    [NotifyPropertyChangedFor(nameof(IsEnterEntradaVisible))]
    [NotifyPropertyChangedFor(nameof(IsEnterSalidaVisible))]
    [NotifyPropertyChangedFor(nameof(IsSelectActivityVisible))]
    [NotifyPropertyChangedFor(nameof(IsSelectProjectVisible))]
    [NotifyPropertyChangedFor(nameof(IsEnterHoursVisible))]
    [NotifyPropertyChangedFor(nameof(IsEnterDescriptionVisible))]
    [NotifyPropertyChangedFor(nameof(IsReviewVisible))]
    [NotifyPropertyChangedFor(nameof(NextButtonText))]
    [NotifyPropertyChangedFor(nameof(IsLastStep))]
    private WizardStep _currentStep = WizardStep.SelectEmployees;

    [ObservableProperty]
    private string _stepTitle = "Seleccionar Empleados";

    [ObservableProperty]
    private string _stepDescription = "Seleccione los empleados para los que desea registrar actividades";

    [ObservableProperty]
    private int _progressPercentage;

    [ObservableProperty]
    private ObservableCollection<Empleado> _selectedEmpleados = new();

    [ObservableProperty]
    private ObservableCollection<Empleado> _allEmpleados = new();

    [ObservableProperty]
    private DateTime _selectedDate = DateTime.Today;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HorasCalculadasText))]
    private TimeSpan? _horaEntrada = new TimeSpan(8, 0, 0); // 08:00 por defecto

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HorasCalculadasText))]
    private TimeSpan? _horaSalida = new TimeSpan(17, 0, 0); // 17:00 por defecto

    // Propiedades intermedias para binding de horas de entrada (como string para mejor validación)
    private string _horaEntradaHorasText = "8";
    public string HoraEntradaHorasText
    {
        get => _horaEntradaHorasText;
        set
        {
            if (SetProperty(ref _horaEntradaHorasText, value))
            {
                if (int.TryParse(value, out int horas))
                {
                    HoraEntradaHoras = Math.Max(0, Math.Min(23, horas));
                }
            }
        }
    }

    private string _horaEntradaMinutosText = "0";
    public string HoraEntradaMinutosText
    {
        get => _horaEntradaMinutosText;
        set
        {
            if (SetProperty(ref _horaEntradaMinutosText, value))
            {
                if (int.TryParse(value, out int minutos))
                {
                    HoraEntradaMinutos = Math.Max(0, Math.Min(59, minutos));
                }
            }
        }
    }

    // Propiedades intermedias para binding de horas de salida (como string para mejor validación)
    private string _horaSalidaHorasText = "17";
    public string HoraSalidaHorasText
    {
        get => _horaSalidaHorasText;
        set
        {
            if (SetProperty(ref _horaSalidaHorasText, value))
            {
                if (int.TryParse(value, out int horas))
                {
                    HoraSalidaHoras = Math.Max(0, Math.Min(23, horas));
                }
            }
        }
    }

    private string _horaSalidaMinutosText = "0";
    public string HoraSalidaMinutosText
    {
        get => _horaSalidaMinutosText;
        set
        {
            if (SetProperty(ref _horaSalidaMinutosText, value))
            {
                if (int.TryParse(value, out int minutos))
                {
                    HoraSalidaMinutos = Math.Max(0, Math.Min(59, minutos));
                }
            }
        }
    }

    // Propiedades internas (int) para el cálculo
    [ObservableProperty]
    private int _horaEntradaHoras = 8;

    [ObservableProperty]
    private int _horaEntradaMinutos = 0;

    [ObservableProperty]
    private int _horaSalidaHoras = 17;

    [ObservableProperty]
    private int _horaSalidaMinutos = 0;

    [ObservableProperty]
    private Actividad? _selectedActivity;

    [ObservableProperty]
    private ObservableCollection<Actividad> _allActividades = new();

    [ObservableProperty]
    private Proyecto? _selectedProject;

    [ObservableProperty]
    private ObservableCollection<Proyecto> _allProyectos = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HorasCalculadasText))]
    private double _hours = 1.0;

    /// <summary>
    /// Texto que muestra las horas calculadas automáticamente
    /// </summary>
    public string HorasCalculadasText
    {
        get
        {
            if (HoraEntrada.HasValue && HoraSalida.HasValue)
            {
                var diferencia = HoraSalida.Value - HoraEntrada.Value;
                var horasCalculadas = diferencia.TotalHours;
                return $"{horasCalculadas:N1} horas (calculadas desde {HoraEntrada.Value:hh\\:mm} hasta {HoraSalida.Value:hh\\:mm})";
            }
            return "Esperando horarios de entrada y salida...";
        }
    }

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isSuccessVisible;

    [ObservableProperty]
    private string _successMessage = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsWizardVisible))]
    [NotifyPropertyChangedFor(nameof(IsHomeVisible))]
    private bool _isDashboardVisible;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsWizardVisible))]
    [NotifyPropertyChangedFor(nameof(IsHomeVisible))]
    private bool _isInWizardMode;

    /// <summary>
    /// Propiedad calculada para la visibilidad del wizard
    /// </summary>
    public bool IsWizardVisible => IsInWizardMode && !IsDashboardVisible;

    /// <summary>
    /// Propiedad calculada para la visibilidad de la pantalla de inicio
    /// </summary>
    public bool IsHomeVisible => !IsInWizardMode && !IsDashboardVisible;

    /// <summary>
    /// Texto de la fecha actual para mostrar en el footer
    /// </summary>
    public string CurrentDateText => DateTime.Now.ToString("dddd, dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("es-ES"));

    public ControlDiarioViewModel DashboardViewModel { get; }

    #endregion

    public DailyActivityWizardViewModel(
        IControlDiarioService controlDiarioService,
        IEmpleadoService empleadoService,
        IActividadService actividadService,
        IProyectoService proyectoService,
        ControlDiarioViewModel controlDiarioViewModel)
    {
        _controlDiarioService = controlDiarioService;
        _empleadoService = empleadoService;
        _actividadService = actividadService;
        _proyectoService = proyectoService;
        DashboardViewModel = controlDiarioViewModel;
        
        // Calcular horas automáticamente desde los horarios por defecto
        CalculateHoursFromTimes();
    }

    #region Property Changed Handlers for Time

    /// <summary>
    /// Actualiza HoraEntrada cuando cambian las horas
    /// </summary>
    partial void OnHoraEntradaHorasChanged(int value)
    {
        UpdateHoraEntrada();
    }

    /// <summary>
    /// Actualiza HoraEntrada cuando cambian los minutos
    /// </summary>
    partial void OnHoraEntradaMinutosChanged(int value)
    {
        UpdateHoraEntrada();
    }

    /// <summary>
    /// Actualiza HoraSalida cuando cambian las horas
    /// </summary>
    partial void OnHoraSalidaHorasChanged(int value)
    {
        UpdateHoraSalida();
    }

    /// <summary>
    /// Actualiza HoraSalida cuando cambian los minutos
    /// </summary>
    partial void OnHoraSalidaMinutosChanged(int value)
    {
        UpdateHoraSalida();
    }

    private void UpdateHoraEntrada()
    {
        try
        {
            // Validar rangos
            var horas = Math.Max(0, Math.Min(23, HoraEntradaHoras));
            var minutos = Math.Max(0, Math.Min(59, HoraEntradaMinutos));

            HoraEntrada = new TimeSpan(horas, minutos, 0);

            // Calcular horas automáticamente
            CalculateHoursFromTimes();
        }
        catch
        {
            // Si hay error, mantener valor anterior
        }
    }

    private void UpdateHoraSalida()
    {
        try
        {
            // Validar rangos
            var horas = Math.Max(0, Math.Min(23, HoraSalidaHoras));
            var minutos = Math.Max(0, Math.Min(59, HoraSalidaMinutos));

            HoraSalida = new TimeSpan(horas, minutos, 0);

            // Calcular horas automáticamente
            CalculateHoursFromTimes();
        }
        catch
        {
            // Si hay error, mantener valor anterior
        }
    }

    /// <summary>
    /// Calcula automáticamente las horas trabajadas basándose en entrada y salida
    /// </summary>
    private void CalculateHoursFromTimes()
    {
        if (HoraEntrada.HasValue && HoraSalida.HasValue && HoraSalida.Value > HoraEntrada.Value)
        {
            var diferencia = HoraSalida.Value - HoraEntrada.Value;
            Hours = diferencia.TotalHours;
        }
    }

    #endregion



    #region RelayCommands

    /// <summary>
    /// Muestra la vista del Wizard (Nuevo Registro)
    /// </summary>
    [RelayCommand]
    public void ShowWizard()
    {
        IsInWizardMode = true;
        IsDashboardVisible = false;
    }

    /// <summary>
    /// Muestra la vista del Dashboard (Historial y Logs)
    /// </summary>
    [RelayCommand]
    public async Task ShowDashboardAsync()
    {
        IsInWizardMode = false;
        IsDashboardVisible = true;
        await DashboardViewModel.LoadDataAsync();
    }

    /// <summary>
    /// Vuelve a la pantalla de inicio
    /// </summary>
    [RelayCommand]
    public void GoHome()
    {
        IsInWizardMode = false;
        IsDashboardVisible = false;
    }

    /// <summary>
    /// Cambia entre el modo Wizard y el modo Dashboard (legacy - mantener para compatibilidad)
    /// </summary>
    [RelayCommand]
    public async Task ToggleViewModeAsync()
    {
        IsDashboardVisible = !IsDashboardVisible;
        
        if (IsDashboardVisible)
        {
            await DashboardViewModel.LoadDataAsync();
        }
    }

    /// <summary>
    /// Avanza al siguiente paso con validación del paso actual
    /// </summary>
    [RelayCommand]
    public async Task NextStepAsync()
    {
        if (!ValidateCurrentStep())
        {
            return;
        }

        if ((int)CurrentStep < (int)WizardStep.Review)
        {
            CurrentStep++;
            UpdateStepInfo();
            UpdateProgressPercentage();
        }
        else if (CurrentStep == WizardStep.Review)
        {
            // En Review, se ejecuta SaveCommand
            await SaveAsync();
        }
    }

    /// <summary>
    /// Retrocede al paso anterior sin validación
    /// </summary>
    [RelayCommand]
    public void PreviousStep()
    {
        if ((int)CurrentStep > 0)
        {
            CurrentStep--;
            UpdateStepInfo();
            UpdateProgressPercentage();
        }
    }

    /// <summary>
    /// Agrega o remueve un empleado de la selección
    /// </summary>
    [RelayCommand]
    public void ToggleEmployee(Empleado empleado)
    {
        if (empleado == null) return;

        if (SelectedEmpleados.Contains(empleado))
        {
            SelectedEmpleados.Remove(empleado);
            StatusMessage = $"Empleado {empleado.NombreCompleto} removido";
        }
        else
        {
            SelectedEmpleados.Add(empleado);
            StatusMessage = $"Empleado {empleado.NombreCompleto} agregado";
        }
    }

    /// <summary>
    /// Guarda los registros de actividades para todos los empleados seleccionados
    /// </summary>
    [RelayCommand]
    public async Task SaveAsync()
    {
        if (SelectedEmpleados.Count == 0)
        {
            StatusMessage = "Error: No hay empleados seleccionados";
            return;
        }

        IsLoading = true;
        StatusMessage = "Guardando registros...";

        try
        {
            int successCount = 0;
            int errorCount = 0;
            var errors = new List<string>();

            foreach (var empleado in SelectedEmpleados)
            {
                try
                {
                    // Obtener o crear registro diario
                    var registroDiario = await _controlDiarioService.GetRegistroByFechaEmpleadoAsync(SelectedDate, empleado.Id);

                    if (registroDiario == null)
                    {
                        registroDiario = new RegistroDiario
                        {
                            Fecha = SelectedDate,
                            EmpleadoId = empleado.Id,
                            Empleado = empleado,
                            HoraEntrada = HoraEntrada,
                            HoraSalida = HoraSalida,
                            Estado = EstadoRegistroDiario.Borrador,
                            DetallesActividades = new List<DetalleActividad>()
                        };
                    }
                    else
                    {
                        // FIX #7: Validar que el registro esté en estado Borrador antes de actualizar
                        if (registroDiario.Estado != EstadoRegistroDiario.Borrador)
                        {
                            errorCount++;
                            errors.Add($"El registro de {empleado.NombreCompleto} para {SelectedDate:dd/MM/yyyy} está en estado {registroDiario.Estado} y no se puede modificar");
                            continue; // Saltar a siguiente empleado
                        }
                        
                        // Actualizar horarios si ya existe el registro
                        registroDiario.HoraEntrada = HoraEntrada;
                        registroDiario.HoraSalida = HoraSalida;

                        if (registroDiario.DetallesActividades == null)
                        {
                            registroDiario.DetallesActividades = new List<DetalleActividad>();
                        }
                    }

                    // Agregar detalle de actividad
                    if (SelectedActivity != null)
                    {
                        // Si el proyecto tiene Id = 0 (Sin proyecto), usar null
                        var proyectoId = SelectedProject?.Id == 0 ? (int?)null : SelectedProject?.Id;
                        var proyecto = SelectedProject?.Id == 0 ? null : SelectedProject;
                        
                        var detalleActividad = new DetalleActividad
                        {
                            ActividadId = SelectedActivity.Id,
                            Actividad = SelectedActivity,
                            ProyectoId = proyectoId,
                            Proyecto = proyecto,
                            Horas = Convert.ToDecimal(Hours),
                            Descripcion = Description,
                            FechaCreacion = DateTime.Now,
                            Activo = true
                        };

                        registroDiario.DetallesActividades.Add(detalleActividad);
                    }

                    // Guardar registro
                    var result = await _controlDiarioService.SaveRegistroAsync(registroDiario);

                    if (result.Success)
                    {
                        successCount++;
                    }
                    else
                    {
                        errorCount++;
                        errors.Add($"Error para {empleado.NombreCompleto}: {result.Message}");
                    }
                }
                catch (Exception ex)
                {
                    errorCount++;
                    // Incluir stack trace para mejor diagnóstico
                    var errorDetail = $"Excepción para {empleado.NombreCompleto}: {ex.Message}";
                    if (ex.InnerException != null)
                        errorDetail += $"\n  → Inner: {ex.InnerException.Message}";
                    errorDetail += $"\n  → En: {ex.StackTrace?.Split('\n').FirstOrDefault()?.Trim()}";
                    errors.Add(errorDetail);
                }
            }

            // Mostrar resultado
            if (errorCount == 0)
            {
                // Éxito total
                SuccessMessage = $"¡Excelente! Se han guardado {successCount} registros correctamente.";
                IsSuccessVisible = true;
                
                // Esperar un momento para que el usuario vea el mensaje
                await Task.Delay(2000);
                
                IsSuccessVisible = false;
                
                // Volver al menú principal
                GoHome();
                
                // Reiniciar wizard en segundo plano
                await ResetAsync();
            }
            else
            {
                StatusMessage = $"⚠ Se guardaron {successCount} registros, pero hubo {errorCount} errores";
                foreach (var error in errors)
                {
                    StatusMessage += $"\n  • {error}";
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error crítico al guardar: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Reinicia el wizard a su estado inicial
    /// </summary>
    [RelayCommand]
    public async Task ResetAsync()
    {
        CurrentStep = WizardStep.SelectEmployees;
        SelectedEmpleados.Clear();
        SelectedDate = DateTime.Today;
        HoraEntradaHorasText = "8";
        HoraEntradaMinutosText = "0";
        HoraSalidaHorasText = "17";
        HoraSalidaMinutosText = "0";
        HoraEntradaHoras = 8;
        HoraEntradaMinutos = 0;
        HoraSalidaHoras = 17;
        HoraSalidaMinutos = 0;
        HoraEntrada = new TimeSpan(8, 0, 0);
        HoraSalida = new TimeSpan(17, 0, 0);
        SelectedActivity = null;
        SelectedProject = null;
        Description = string.Empty;
        
        // Calcular horas automáticamente desde los horarios establecidos
        CalculateHoursFromTimes();
        
        StatusMessage = "Wizard reiniciado. Listo para registrar nuevas actividades.";
        UpdateStepInfo();
        UpdateProgressPercentage();

        await Task.CompletedTask;
    }

    /// <summary>
    /// Carga empleados, actividades y proyectos desde los servicios
    /// </summary>
    [RelayCommand]
    public async Task LoadDataAsync()
    {
        IsLoading = true;
        StatusMessage = "Cargando datos...";

        try
        {
            // Ejecutar cargas en paralelo
            var empleadosTask = _empleadoService.GetAllAsync();
            var actividadesTask = _actividadService.GetAllAsync();
            var proyectosTask = _proyectoService.GetByEstadoAsync(EstadoProyecto.Activo);

            await Task.WhenAll(empleadosTask, actividadesTask, proyectosTask);

            // Llenar colecciones
            AllEmpleados.Clear();
            foreach (var emp in await empleadosTask)
            {
                AllEmpleados.Add(emp);
            }

            AllActividades.Clear();
            foreach (var act in await actividadesTask)
            {
                AllActividades.Add(act);
            }

            AllProyectos.Clear();
            AllProyectos.Add(new Proyecto { Id = 0, Nombre = "Sin proyecto" });
            foreach (var pry in await proyectosTask)
            {
                AllProyectos.Add(pry);
            }

            StatusMessage = "Datos cargados correctamente";
            UpdateStepInfo();
            UpdateProgressPercentage();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al cargar datos: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Valida el paso actual antes de avanzar
    /// </summary>
    private bool ValidateCurrentStep()
    {
        switch (CurrentStep)
        {
            case WizardStep.SelectEmployees:
                if (SelectedEmpleados.Count == 0)
                {
                    StatusMessage = "Debe seleccionar al menos un empleado";
                    return false;
                }
                StatusMessage = $"{SelectedEmpleados.Count} empleado(s) seleccionado(s)";
                return true;

            case WizardStep.SelectDate:
                if (SelectedDate == default)
                {
                    StatusMessage = "Debe seleccionar una fecha";
                    return false;
                }
                StatusMessage = $"Fecha seleccionada: {SelectedDate:dd/MM/yyyy}";
                return true;

            case WizardStep.EnterEntrada:
                if (!HoraEntrada.HasValue)
                {
                    StatusMessage = "Debe ingresar la hora de entrada";
                    return false;
                }
                StatusMessage = $"Hora de entrada: {HoraEntrada.Value:hh\\:mm}";
                return true;

            case WizardStep.EnterSalida:
                if (!HoraSalida.HasValue)
                {
                    StatusMessage = "Debe ingresar la hora de salida";
                    return false;
                }
                if (HoraEntrada.HasValue && HoraSalida.HasValue && HoraSalida.Value <= HoraEntrada.Value)
                {
                    StatusMessage = "La hora de salida debe ser posterior a la hora de entrada";
                    return false;
                }
                StatusMessage = $"Hora de salida: {HoraSalida.Value:hh\\:mm}";
                return true;

            case WizardStep.SelectActivity:
                if (SelectedActivity == null)
                {
                    StatusMessage = "Debe seleccionar una actividad";
                    return false;
                }
                StatusMessage = $"Actividad: {SelectedActivity.Nombre}";
                return true;

            case WizardStep.SelectProject:
                // Proyecto es opcional, pero validamos si está seleccionado
                StatusMessage = SelectedProject != null 
                    ? $"Proyecto: {SelectedProject.Nombre}" 
                    : "Sin proyecto seleccionado";
                return true;

            case WizardStep.EnterHours:
                if (Hours <= 0 || Hours > 24)
                {
                    StatusMessage = "Error en el cálculo de horas. Verifique horarios de entrada y salida";
                    return false;
                }
                StatusMessage = $"✓ {Hours:N1} horas calculadas automáticamente";
                return true;

            case WizardStep.EnterDescription:
                if (string.IsNullOrWhiteSpace(Description))
                {
                    StatusMessage = "Debe ingresar una descripción";
                    return false;
                }
                StatusMessage = $"Descripción ingresada";
                return true;

            case WizardStep.Review:
                // Review está listo para guardar
                StatusMessage = "Listo para guardar";
                return true;

            default:
                return true;
        }
    }

    /// <summary>
    /// Actualiza el título y descripción basado en el paso actual
    /// </summary>
    private void UpdateStepInfo()
    {
        var (title, description) = GetStepInfo();
        StepTitle = title;
        StepDescription = description;
    }

    /// <summary>
    /// Obtiene el título y descripción para cada paso
    /// </summary>
    private (string Title, string Description) GetStepInfo()
    {
        return CurrentStep switch
        {
            WizardStep.SelectEmployees => (
                "1. Seleccionar Empleados",
                "Seleccione los empleados para los que desea registrar actividades"
            ),
            WizardStep.SelectDate => (
                "2. Seleccionar Fecha",
                "Seleccione la fecha del registro"
            ),
            WizardStep.EnterEntrada => (
                "3. Hora de Entrada",
                "Ingrese la hora de entrada al trabajo"
            ),
            WizardStep.EnterSalida => (
                "4. Hora de Salida",
                "Ingrese la hora de salida del trabajo"
            ),
            WizardStep.SelectActivity => (
                "5. Seleccionar Actividad",
                "Seleccione la actividad realizada"
            ),
            WizardStep.SelectProject => (
                "6. Seleccionar Proyecto",
                "Seleccione el proyecto asociado (opcional)"
            ),
            WizardStep.EnterHours => (
                "7. Horas Trabajadas",
                "Las horas se calculan automáticamente desde el horario de entrada y salida"
            ),
            WizardStep.EnterDescription => (
                "8. Descripción de la Actividad",
                "Ingrese una descripción detallada de lo realizado"
            ),
            WizardStep.Review => (
                "9. Revisar y Confirmar",
                "Revise los datos antes de guardar"
            ),
            _ => ("Paso Desconocido", "")
        };
    }

    /// <summary>
    /// Actualiza el porcentaje de progreso basado en el paso actual
    /// </summary>
    private void UpdateProgressPercentage()
    {
        ProgressPercentage = ((int)CurrentStep + 1) * 100 / 9;
    }

    #endregion

    #region Computed Properties

    /// <summary>
    /// Propiedades de visibilidad para cada paso
    /// </summary>
    public bool IsSelectEmployeesVisible => CurrentStep == WizardStep.SelectEmployees;
    public bool IsSelectDateVisible => CurrentStep == WizardStep.SelectDate;
    public bool IsEnterEntradaVisible => CurrentStep == WizardStep.EnterEntrada;
    public bool IsEnterSalidaVisible => CurrentStep == WizardStep.EnterSalida;
    public bool IsSelectActivityVisible => CurrentStep == WizardStep.SelectActivity;
    public bool IsSelectProjectVisible => CurrentStep == WizardStep.SelectProject;
    public bool IsEnterHoursVisible => CurrentStep == WizardStep.EnterHours;
    public bool IsEnterDescriptionVisible => CurrentStep == WizardStep.EnterDescription;
    public bool IsReviewVisible => CurrentStep == WizardStep.Review;

    /// <summary>
    /// Texto dinámico del botón siguiente/guardar
    /// </summary>
    public string NextButtonText => CurrentStep == WizardStep.Review ? "✓ Guardar" : "Siguiente →";

    /// <summary>
    /// Indica si estamos en el último paso del wizard
    /// </summary>
    public bool IsLastStep => CurrentStep == WizardStep.Review;

    #endregion
}
