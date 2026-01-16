using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Server.Components.Pages;

public partial class ControlDiario
{
    [Parameter] public string? FechaParam { get; set; }

    [Inject] private IAuthService AuthService { get; set; } = default!;
    [Inject] private IRegistroDiarioService RegistroDiarioService { get; set; } = default!;
    [Inject] private IEmpleadoRepository EmpleadoRepository { get; set; } = default!;
    [Inject] private IDepartamentoRepository DepartamentoRepository { get; set; } = default!;
    [Inject] private IActividadRepository ActividadRepository { get; set; } = default!;
    [Inject] private ICategoriaActividadRepository CategoriaActividadRepository { get; set; } = default!;
    [Inject] private IProyectoRepository ProyectoRepository { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private ILogger<ControlDiario> Logger { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    // COLECCIONES DE DATOS
    private List<RegistroDiario> registrosDia = new();
    private List<RegistroDiario> registrosFiltrados = new();
    private List<Empleado> todosEmpleados = new();
    private List<Empleado> empleadosSinRegistro = new();
    private List<Departamento> departamentos = new();
    private List<Actividad> actividades = new();
    private List<Actividad> actividadesFiltradas = new();
    private List<CategoriaActividad> categoriasActividad = new();
    private List<Proyecto> proyectos = new();

    // ESTADO DE LA PAGINA
    private DateTime fechaSeleccionada = DateTime.Today;
    private RegistroDiario? registroSeleccionado;
    private DetalleActividad? actividadEdit;
    private DetalleActividad? actividadAEliminar;
    private Actividad? actividadSeleccionadaObj;

    private bool isLoading;
    private bool isSaving;
    private bool showModalActividad;
    private bool showConfirmModal;
    private bool isEditingActividad;
    private bool mostrarDetalle;
    private bool editandoObservaciones;

    // EDICION DE HORARIOS
    private int? editandoEntrada;
    private int? editandoSalida;

    // FILTROS Y BUSQUEDA
    private string searchTerm = string.Empty;
    private string filtroEstado = string.Empty;
    private string filtroDepartamento = string.Empty;
    private bool mostrarSoloActivos = true;
    private int categoriaFiltroId;

    // MENSAJES
    private string? errorMessage;
    private string? successMessage;
    private string? confirmMessage;
    private Func<Task>? confirmarAccionCallback;

    private bool actividadSeleccionadaRequiereProyecto =>
        actividadSeleccionadaObj?.RequiereProyecto ?? false;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            if (!AuthService.IsAuthenticated)
            {
                Navigation.NavigateTo("/login");
                return;
            }

            if (!string.IsNullOrEmpty(FechaParam) && DateTime.TryParse(FechaParam, out var fecha))
            {
                fechaSeleccionada = fecha;
            }

            await CargarDatosIniciales();
            Logger.LogInformation("ControlDiario inicializado para fecha {Fecha}", fechaSeleccionada.ToString("yyyy-MM-dd"));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error al inicializar ControlDiario");
            errorMessage = $"ERROR AL CARGAR LA PAGINA: {ex.Message}";
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JSRuntime.InvokeVoidAsync("eval",
                "document.addEventListener('keydown', function(e) {" +
                "    if (e.key === 'F2') {" +
                "        e.preventDefault();" +
                "        var searchInput = document.querySelector('.search-input');" +
                "        if (searchInput) { searchInput.focus(); searchInput.select(); }" +
                "    }" +
                "    if (e.key === 'F3') {" +
                "        e.preventDefault();" +
                "        var btnNuevo = document.querySelector('.hospital-btn-primary');" +
                "        if (btnNuevo && !btnNuevo.disabled) { btnNuevo.click(); }" +
                "    }" +
                "    if (e.key === 'F5') {" +
                "        e.preventDefault();" +
                "        var btnGuardar = document.querySelector('.hospital-modal-actions .hospital-btn');" +
                "        if (btnGuardar && !btnGuardar.disabled) { btnGuardar.click(); }" +
                "        else { var btnActualizar = document.querySelector('button[title*=Actualizar]'); if (btnActualizar) btnActualizar.click(); }" +
                "    }" +
                "    if (e.key === 'F8') {" +
                "        e.preventDefault();" +
                "        var modales = document.querySelectorAll('.hospital-modal-actions');" +
                "        if (modales.length > 0) { var btns = modales[modales.length-1].querySelectorAll('.hospital-btn'); if (btns.length > 1) btns[btns.length-1].click(); }" +
                "    }" +
                "    if (e.key === 'Escape') {" +
                "        var modal = document.querySelector('.hospital-modal-overlay');" +
                "        if (modal) { var btnCerrar = modal.querySelector('.hospital-modal-header .hospital-btn'); if (btnCerrar) btnCerrar.click(); }" +
                "        else { var searchInput = document.querySelector('.search-input'); if (searchInput && searchInput.value) { searchInput.value = ''; searchInput.dispatchEvent(new Event('input', { bubbles: true })); } }" +
                "    }" +
                "});");
        }
    }

    private async Task CargarDatosIniciales()
    {
        isLoading = true;
        StateHasChanged();

        try
        {
            var empleadosTask = EmpleadoRepository.GetAllActiveWithRelationsAsync();
            var departamentosTask = DepartamentoRepository.GetAllActiveAsync();
            var actividadesTask = ActividadRepository.GetAllActiveWithCategoriaAsync();
            var categoriasTask = CategoriaActividadRepository.GetAllActiveAsync();
            var proyectosTask = ProyectoRepository.GetByEstadoAsync(EstadoProyecto.Activo);

            await Task.WhenAll(empleadosTask, departamentosTask, actividadesTask, categoriasTask, proyectosTask);

            todosEmpleados = (await empleadosTask).ToList();
            departamentos = (await departamentosTask).ToList();
            actividades = (await actividadesTask).OrderBy(a => a.Categoria?.Orden ?? 999).ThenBy(a => a.Orden).ToList();
            categoriasActividad = (await categoriasTask).ToList();
            proyectos = (await proyectosTask).OrderBy(p => p.Nombre).ToList();

            actividadesFiltradas = actividades.ToList();

            await CargarRegistros();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error al cargar datos iniciales");
            errorMessage = $"ERROR AL CARGAR DATOS: {ex.Message}";
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    private async Task CargarRegistros()
    {
        isLoading = true;
        errorMessage = null;
        StateHasChanged();

        try
        {
            var registros = await RegistroDiarioService.GetRegistrosByFechaAsync(fechaSeleccionada);
            var empleadosPorId = todosEmpleados.ToDictionary(e => e.Id);
            foreach (var registro in registros)
            {
                if (registro.Empleado == null && empleadosPorId.TryGetValue(registro.EmpleadoId, out var empleado))
                {
                    registro.Empleado = empleado;
                }
            }

            registrosDia = registros.OrderBy(r => r.Empleado?.NombreCompleto).ToList();

            var empleadosConRegistro = registrosDia.Select(r => r.EmpleadoId).ToHashSet();
            empleadosSinRegistro = todosEmpleados
                .Where(e => e.Estado == EstadoEmpleado.Activo && !empleadosConRegistro.Contains(e.Id))
                .OrderBy(e => e.NombreCompleto)
                .ToList();

            await FiltrarRegistros();

            Logger.LogInformation("Cargados {Count} registros para fecha {Fecha}",
                registrosDia.Count, fechaSeleccionada.ToString("yyyy-MM-dd"));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error al cargar registros");
            errorMessage = $"ERROR AL CARGAR REGISTROS: {ex.Message}";
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    private async Task CambiarFecha()
    {
        await CargarRegistros();
        Navigation.NavigateTo($"/control-diario/{fechaSeleccionada:yyyy-MM-dd}");
    }

    private async Task FiltrarRegistros()
    {
        registrosFiltrados = registrosDia;

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var termino = searchTerm.ToLower();
            registrosFiltrados = registrosFiltrados.Where(r =>
                r.Empleado?.NombreCompleto.ToLower().Contains(termino) == true ||
                r.Empleado?.Codigo.ToLower().Contains(termino) == true ||
                r.Empleado?.Cedula.Contains(termino) == true
            ).ToList();
        }

        if (!string.IsNullOrEmpty(filtroEstado) && int.TryParse(filtroEstado, out var estadoInt))
        {
            var estado = (EstadoRegistroDiario)estadoInt;
            registrosFiltrados = registrosFiltrados.Where(r => r.Estado == estado).ToList();
        }

        if (!string.IsNullOrEmpty(filtroDepartamento) && int.TryParse(filtroDepartamento, out var deptId))
        {
            registrosFiltrados = registrosFiltrados.Where(r => r.Empleado?.DepartamentoId == deptId).ToList();
        }

        if (mostrarSoloActivos)
        {
            registrosFiltrados = registrosFiltrados
                .Where(r => r.Empleado?.Estado == EstadoEmpleado.Activo)
                .ToList();
        }

        StateHasChanged();
        await Task.CompletedTask;
    }

    private void SeleccionarRegistro(RegistroDiario registro)
    {
        registroSeleccionado = registro;
        StateHasChanged();
    }

    private async Task CrearRegistrosParaTodos()
    {
        if (!empleadosSinRegistro.Any())
        {
            errorMessage = "NO HAY EMPLEADOS SIN REGISTRO";
            return;
        }

        isSaving = true;
        errorMessage = null;
        StateHasChanged();

        try
        {
            var registrosCreados = await RegistroDiarioService.CrearRegistrosParaEmpleadosAsync(
                empleadosSinRegistro, fechaSeleccionada, AuthService.CurrentUserId ?? 0);

            successMessage = $"REGISTROS CREADOS: {registrosCreados.Count}";
            Logger.LogInformation("Creados {Count} registros para fecha {Fecha}",
                registrosCreados.Count, fechaSeleccionada.ToString("yyyy-MM-dd"));

            await CargarRegistros();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error al crear registros masivos");
            errorMessage = $"ERROR AL CREAR REGISTROS: {ex.Message}";
        }
        finally
        {
            isSaving = false;
            StateHasChanged();

            if (successMessage != null)
            {
                await Task.Delay(3000);
                successMessage = null;
                StateHasChanged();
            }
        }
    }

    private void EditarEntrada(RegistroDiario registro)
    {
        editandoEntrada = registro.Id;
        editandoSalida = null;
        StateHasChanged();
    }

    private void EditarSalida(RegistroDiario registro)
    {
        editandoSalida = registro.Id;
        editandoEntrada = null;
        StateHasChanged();
    }

    private async Task GuardarHorario(RegistroDiario registro)
    {
        try
        {
            await RegistroDiarioService.ActualizarRegistroAsync(registro);
            editandoEntrada = null;
            editandoSalida = null;

            Logger.LogInformation("Actualizado horario del registro {Id}", registro.Id);
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error al actualizar horario del registro {Id}", registro.Id);
            errorMessage = $"ERROR AL GUARDAR HORARIO: {ex.Message}";
            StateHasChanged();
        }
    }

    private async Task RegistrarEntradaParaRegistro(RegistroDiario registro)
    {
        if (registro.HoraEntrada != null) return;

        registro.HoraEntrada = DateTime.Now.TimeOfDay;
        await GuardarHorario(registro);
        successMessage = $"ENTRADA REGISTRADA: {registro.Empleado?.NombreCompleto}";
        StateHasChanged();
        await Task.Delay(2000);
        successMessage = null;
        StateHasChanged();
    }

    private async Task RegistrarSalidaParaRegistro(RegistroDiario registro)
    {
        if (registro.HoraSalida != null) return;

        registro.HoraSalida = DateTime.Now.TimeOfDay;
        await GuardarHorario(registro);
        successMessage = $"SALIDA REGISTRADA: {registro.Empleado?.NombreCompleto}";
        StateHasChanged();
        await Task.Delay(2000);
        successMessage = null;
        StateHasChanged();
    }

    private async Task GuardarObservaciones()
    {
        if (registroSeleccionado == null) return;

        try
        {
            await RegistroDiarioService.ActualizarRegistroAsync(registroSeleccionado);
            editandoObservaciones = false;

            Logger.LogInformation("Actualizadas observaciones del registro {Id}", registroSeleccionado.Id);
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error al actualizar observaciones del registro {Id}", registroSeleccionado.Id);
            errorMessage = $"ERROR AL GUARDAR OBSERVACIONES: {ex.Message}";
            StateHasChanged();
        }
    }

    private void IniciarEdicionObservaciones()
    {
        editandoObservaciones = true;
    }

    private void CancelarEdicionObservaciones()
    {
        editandoObservaciones = false;
    }

    private async Task AgregarActividad()
    {
        if (registroSeleccionado == null)
        {
            errorMessage = "DEBE SELECCIONAR UN REGISTRO";
            return;
        }

        await AgregarActividadParaRegistro(registroSeleccionado);
    }

    private async Task AgregarActividadParaRegistro(RegistroDiario registro)
    {
        registroSeleccionado = registro;
        isEditingActividad = false;

        var maxOrden = registro.DetallesActividades?.Any() == true
            ? registro.DetallesActividades.Max(d => d.Orden)
            : 0;

        actividadEdit = new DetalleActividad
        {
            RegistroDiarioId = registro.Id,
            RegistroDiario = registro,
            Horas = 1.0m,
            Orden = maxOrden + 1,
            FechaCreacion = DateTime.Now
        };

        actividadSeleccionadaObj = null;
        categoriaFiltroId = 0;
        actividadesFiltradas = actividades.OrderByDescending(a => a.EsDestacada).ThenBy(a => a.Orden).ToList();
        showModalActividad = true;
        StateHasChanged();

        await Task.CompletedTask;
    }

    private async Task EditarActividad(DetalleActividad detalle)
    {
        isEditingActividad = true;
        actividadEdit = new DetalleActividad
        {
            RegistroDiarioId = detalle.RegistroDiarioId,
            ActividadId = detalle.ActividadId,
            ProyectoId = detalle.ProyectoId,
            Horas = detalle.Horas,
            Descripcion = detalle.Descripcion,
            HoraInicio = detalle.HoraInicio,
            HoraFin = detalle.HoraFin,
            Orden = detalle.Orden,
            Cantidad = detalle.Cantidad,
            UnidadMedida = detalle.UnidadMedida,
            LoteEspecifico = detalle.LoteEspecifico
        };
        actividadEdit.Id = detalle.Id;

        actividadSeleccionadaObj = actividades.FirstOrDefault(a => a.Id == detalle.ActividadId);
        categoriaFiltroId = actividadSeleccionadaObj?.CategoriaId ?? 0;
        FiltrarActividadesPorCategoria();
        showModalActividad = true;
        StateHasChanged();

        await Task.CompletedTask;
    }

    private async Task GuardarActividad()
    {
        if (actividadEdit == null || registroSeleccionado == null)
        {
            errorMessage = "DATOS INCOMPLETOS";
            return;
        }

        if (actividadEdit.ActividadId <= 0)
        {
            errorMessage = "DEBE SELECCIONAR UNA ACTIVIDAD";
            return;
        }

        if (actividadSeleccionadaRequiereProyecto && (actividadEdit.ProyectoId == null || actividadEdit.ProyectoId <= 0))
        {
            errorMessage = "ESTA ACTIVIDAD REQUIERE UN PROYECTO";
            return;
        }

        if (actividadEdit.Horas <= 0)
        {
            errorMessage = "LAS HORAS DEBEN SER MAYOR A 0";
            return;
        }

        if (actividadSeleccionadaObj?.RequiereCantidad == true)
        {
            if (!actividadEdit.Cantidad.HasValue || actividadEdit.Cantidad <= 0)
            {
                errorMessage = "DEBE INGRESAR LA CANTIDAD REALIZADA";
                return;
            }

            actividadEdit.UnidadMedida = actividadSeleccionadaObj.UnidadAbreviatura;
        }

        var detallesValidacion = registroSeleccionado.DetallesActividades?.ToList() ?? new List<DetalleActividad>();
        if (isEditingActividad)
        {
            var index = detallesValidacion.FindIndex(d => d.Id == actividadEdit.Id);
            if (index >= 0) detallesValidacion[index] = actividadEdit;
        }
        else
        {
            detallesValidacion.Add(actividadEdit);
        }

        if (!await RegistroDiarioService.ValidarHorasTotalesAsync(detallesValidacion))
        {
            errorMessage = "LAS HORAS TOTALES NO PUEDEN EXCEDER 24";
            return;
        }

        isSaving = true;
        errorMessage = null;
        StateHasChanged();

        try
        {
            await RegistroDiarioService.GuardarDetalleAsync(registroSeleccionado.Id, actividadEdit, isEditingActividad);
            successMessage = isEditingActividad ? "ACTIVIDAD ACTUALIZADA" : "ACTIVIDAD AGREGADA";

            var registroActualizado = await RegistroDiarioService.GetRegistroByIdWithDetallesAsync(registroSeleccionado.Id);
            if (registroActualizado != null)
            {
                var index = registrosDia.FindIndex(r => r.Id == registroActualizado.Id);
                if (index >= 0)
                {
                    registrosDia[index] = registroActualizado;
                }

                registroSeleccionado = registroActualizado;
            }

            await FiltrarRegistros();
            CerrarModalActividad();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error al guardar actividad");
            errorMessage = $"ERROR AL GUARDAR ACTIVIDAD: {ex.Message}";
        }
        finally
        {
            isSaving = false;
            StateHasChanged();

            if (successMessage != null)
            {
                await Task.Delay(3000);
                successMessage = null;
                StateHasChanged();
            }
        }
    }

    private async Task EliminarActividad(DetalleActividad detalle)
    {
        actividadAEliminar = detalle;
        confirmMessage = "ESTA SEGURO QUE DESEA ELIMINAR ESTA ACTIVIDAD?";
        confirmarAccionCallback = async () => await ConfirmarEliminarActividad();
        showConfirmModal = true;
        StateHasChanged();

        await Task.CompletedTask;
    }

    private async Task ConfirmarEliminarActividad()
    {
        if (actividadAEliminar == null || registroSeleccionado == null)
        {
            return;
        }

        isSaving = true;
        StateHasChanged();

        try
        {
            await RegistroDiarioService.EliminarDetalleAsync(registroSeleccionado.Id, actividadAEliminar.Id);

            Logger.LogInformation("Eliminada actividad {Id} del registro {RegistroId}",
                actividadAEliminar.Id, registroSeleccionado.Id);

            var registroActualizado = await RegistroDiarioService.GetRegistroByIdWithDetallesAsync(registroSeleccionado.Id);
            if (registroActualizado != null)
            {
                var index = registrosDia.FindIndex(r => r.Id == registroActualizado.Id);
                if (index >= 0)
                {
                    registrosDia[index] = registroActualizado;
                }

                registroSeleccionado = registroActualizado;
            }

            await FiltrarRegistros();

            successMessage = "ACTIVIDAD ELIMINADA";
            showConfirmModal = false;
            actividadAEliminar = null;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error al eliminar actividad");
            errorMessage = $"ERROR AL ELIMINAR ACTIVIDAD: {ex.Message}";
        }
        finally
        {
            isSaving = false;
            StateHasChanged();

            if (successMessage != null)
            {
                await Task.Delay(3000);
                successMessage = null;
                StateHasChanged();
            }
        }
    }

    private async Task CompletarRegistro(RegistroDiario registro)
    {
        if (!registro.EstaCompleto)
        {
            errorMessage = "EL REGISTRO DEBE TENER HORA DE ENTRADA, SALIDA Y AL MENOS UNA ACTIVIDAD";
            return;
        }

        try
        {
            registro.Estado = EstadoRegistroDiario.Completado;
            registro.FechaModificacion = DateTime.Now;

            await RegistroDiarioService.ActualizarRegistroAsync(registro);

            Logger.LogInformation("Completado registro {Id}", registro.Id);

            successMessage = $"REGISTRO COMPLETADO: {registro.Empleado?.NombreCompleto}";
            await FiltrarRegistros();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error al completar registro {Id}", registro.Id);
            errorMessage = $"ERROR AL COMPLETAR REGISTRO: {ex.Message}";
        }
        finally
        {
            StateHasChanged();

            if (successMessage != null)
            {
                await Task.Delay(3000);
                successMessage = null;
                StateHasChanged();
            }
        }
    }

    private async Task OnActividadChanged()
    {
        if (actividadEdit != null && actividadEdit.ActividadId > 0)
        {
            actividadSeleccionadaObj = actividades.FirstOrDefault(a => a.Id == actividadEdit.ActividadId);

            if (actividadSeleccionadaObj?.RequiereProyecto == false)
            {
                actividadEdit.ProyectoId = null;
            }
        }
        else
        {
            actividadSeleccionadaObj = null;
        }

        StateHasChanged();
        await Task.CompletedTask;
    }

    private async Task CalcularHorasFin()
    {
        if (actividadEdit?.HoraInicio != null && actividadEdit.Horas > 0)
        {
            var horaInicio = actividadEdit.HoraInicio.Value;
            var horasTotales = actividadEdit.Horas;
            var horas = (int)horasTotales;
            var minutos = (int)((horasTotales - horas) * 60);

            actividadEdit.HoraFin = horaInicio.Add(new TimeSpan(horas, minutos, 0));
            StateHasChanged();
        }

        await Task.CompletedTask;
    }

    private async Task CalcularHorasDesdeRango()
    {
        if (actividadEdit?.HoraInicio != null && actividadEdit.HoraFin != null)
        {
            var diferencia = actividadEdit.HoraFin.Value - actividadEdit.HoraInicio.Value;
            if (diferencia.TotalHours > 0)
            {
                actividadEdit.Horas = (decimal)diferencia.TotalHours;
                StateHasChanged();
            }
        }

        await Task.CompletedTask;
    }

    private async Task VerDetalleRegistro(RegistroDiario registro)
    {
        registroSeleccionado = registro;
        mostrarDetalle = true;
        StateHasChanged();
        await Task.CompletedTask;
    }

    private async Task CerrarDetalle()
    {
        mostrarDetalle = false;
        StateHasChanged();
        await Task.CompletedTask;
    }

    private void CerrarModalActividad()
    {
        showModalActividad = false;
        actividadEdit = null;
        actividadSeleccionadaObj = null;
        isEditingActividad = false;
        StateHasChanged();
    }

    private async Task ConfirmarAccion()
    {
        if (confirmarAccionCallback != null)
        {
            await confirmarAccionCallback.Invoke();
        }
    }

    private void IrAlWizard()
    {
        Navigation.NavigateTo("/control-diario-wizard");
    }

    private void IrAReportes()
    {
        Navigation.NavigateTo("/reportes");
    }

    private void IrAProductividad()
    {
        Navigation.NavigateTo("/dashboard-productividad");
    }

    private void IrAExpediente(int empleadoId)
    {
        if (empleadoId > 0)
        {
            Navigation.NavigateTo($"/empleado-expediente/{empleadoId}");
        }
    }

    private void FiltrarActividadesPorCategoria()
    {
        if (categoriaFiltroId > 0)
        {
            actividadesFiltradas = actividades.Where(a => a.CategoriaId == categoriaFiltroId).ToList();
        }
        else
        {
            actividadesFiltradas = actividades.ToList();
        }

        actividadesFiltradas = actividadesFiltradas
            .OrderByDescending(a => a.EsDestacada)
            .ThenBy(a => a.Orden)
            .ThenBy(a => a.Nombre)
            .ToList();

        StateHasChanged();
    }

    private void LimpiarError()
    {
        errorMessage = null;
    }
}
