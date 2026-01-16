using Microsoft.AspNetCore.Components;
using SGRRHH.Local.Domain.Entities;

namespace SGRRHH.Local.Server.Components.ControlDiario;

public partial class ActividadSelector : ComponentBase
{
    [Parameter] public bool MostrarModal { get; set; }

    [Parameter] public bool IsEditingActividad { get; set; }

    [Parameter] public bool IsSaving { get; set; }

    [Parameter] public DetalleActividad? ActividadEdit { get; set; }

    [Parameter] public RegistroDiario? RegistroSeleccionado { get; set; }

    [Parameter] public List<CategoriaActividad> CategoriasActividad { get; set; } = new();

    [Parameter] public List<Actividad> ActividadesFiltradas { get; set; } = new();

    [Parameter] public List<Proyecto> Proyectos { get; set; } = new();

    [Parameter] public int CategoriaFiltroId { get; set; }

    [Parameter] public EventCallback<int> CategoriaFiltroIdChanged { get; set; }

    [Parameter] public Actividad? ActividadSeleccionadaObj { get; set; }

    [Parameter] public bool ActividadSeleccionadaRequiereProyecto { get; set; }

    [Parameter] public DateTime FechaSeleccionada { get; set; }

    [Parameter] public EventCallback OnActividadChanged { get; set; }

    [Parameter] public EventCallback OnFiltrarCategoria { get; set; }

    [Parameter] public EventCallback OnGuardarActividad { get; set; }

    [Parameter] public EventCallback OnCerrarModal { get; set; }

    [Parameter] public EventCallback OnCalcularHorasFin { get; set; }

    [Parameter] public EventCallback OnCalcularHorasDesdeRango { get; set; }

    private int categoriaLocal;

    protected override void OnParametersSet()
    {
        categoriaLocal = CategoriaFiltroId;
    }

    private async Task AfterCategoriaChanged()
    {
        CategoriaFiltroId = categoriaLocal;
        await CategoriaFiltroIdChanged.InvokeAsync(CategoriaFiltroId);
        await OnFiltrarCategoria.InvokeAsync();
    }

    private async Task AfterActividadChanged()
    {
        await OnActividadChanged.InvokeAsync();
    }

    private async Task OnHoraInicioChanged(ChangeEventArgs args)
    {
        if (ActividadEdit == null)
        {
            return;
        }

        ActividadEdit.HoraInicio = ParseTimeSpan(args.Value?.ToString());
        await OnCalcularHorasFin.InvokeAsync();
    }

    private async Task OnHoraFinChanged(ChangeEventArgs args)
    {
        if (ActividadEdit == null)
        {
            return;
        }

        ActividadEdit.HoraFin = ParseTimeSpan(args.Value?.ToString());
        await OnCalcularHorasDesdeRango.InvokeAsync();
    }

    private static string FormatTimeSpan(TimeSpan? time) => time?.ToString(@"hh\:mm") ?? string.Empty;

    private static TimeSpan? ParseTimeSpan(string? value)
    {
        if (string.IsNullOrEmpty(value)) return null;
        return TimeSpan.TryParse(value, out var time) ? time : null;
    }

    private string GetClasificacionCss(decimal porcentaje)
    {
        return porcentaje switch
        {
            >= 120 => "productividad-excelente",
            >= 100 => "productividad-optimo",
            >= 80 => "productividad-aceptable",
            >= 60 => "productividad-bajo",
            _ => "productividad-critico"
        };
    }

    private string GetClasificacionTexto(decimal porcentaje)
    {
        return porcentaje switch
        {
            >= 120 => "EXCELENTE",
            >= 100 => "ÓPTIMO",
            >= 80 => "ACEPTABLE",
            >= 60 => "BAJO",
            _ => "CRÍTICO"
        };
    }
}
