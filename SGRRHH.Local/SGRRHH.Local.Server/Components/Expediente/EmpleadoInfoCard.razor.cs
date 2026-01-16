using Microsoft.AspNetCore.Components;

using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Server.Components.Expediente;

public partial class EmpleadoInfoCard
{
    [Parameter] public Empleado? Empleado { get; set; }
    [Parameter] public List<EstadoEmpleado> TransicionesPermitidas { get; set; } = new();
    [Parameter] public string NuevoEstadoSeleccionado { get; set; } = string.Empty;
    [Parameter] public EventCallback<string> NuevoEstadoSeleccionadoChanged { get; set; }
    [Parameter] public bool IsSavingEstado { get; set; }
    [Parameter] public EventCallback OnCambiarEstado { get; set; }

    private async Task OnNuevoEstadoChanged(ChangeEventArgs e)
    {
        var value = e.Value?.ToString() ?? string.Empty;
        NuevoEstadoSeleccionado = value;

        if (NuevoEstadoSeleccionadoChanged.HasDelegate)
        {
            await NuevoEstadoSeleccionadoChanged.InvokeAsync(value);
        }
    }
}
