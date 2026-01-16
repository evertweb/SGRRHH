using Microsoft.AspNetCore.Components;

namespace SGRRHH.Local.Server.Components.ControlDiario;

public partial class DateNavigator : ComponentBase
{
    [Parameter] public DateTime FechaSeleccionada { get; set; } = DateTime.Today;

    [Parameter] public EventCallback<DateTime> FechaSeleccionadaChanged { get; set; }

    [Parameter] public EventCallback OnFechaChanged { get; set; }

    private DateTime fechaLocal = DateTime.Today;

    protected override void OnParametersSet()
    {
        fechaLocal = FechaSeleccionada;
    }

    private async Task Anterior()
    {
        await IrAFecha(fechaLocal.AddDays(-1));
    }

    private async Task Siguiente()
    {
        await IrAFecha(fechaLocal.AddDays(1));
    }

    private async Task IrHoy()
    {
        await IrAFecha(DateTime.Today);
    }

    private async Task IrAFecha(DateTime fecha)
    {
        fechaLocal = fecha;
        await NotificarCambio();
    }

    private async Task NotificarCambio()
    {
        await FechaSeleccionadaChanged.InvokeAsync(fechaLocal);
        await OnFechaChanged.InvokeAsync();
    }
}
