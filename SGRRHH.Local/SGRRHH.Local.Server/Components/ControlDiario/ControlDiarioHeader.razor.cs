using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace SGRRHH.Local.Server.Components.ControlDiario;

public partial class ControlDiarioHeader : ComponentBase
{
    [Parameter] public DateTime FechaSeleccionada { get; set; }

    [Parameter] public string? ErrorMessage { get; set; }

    [Parameter] public string? SuccessMessage { get; set; }

    [Parameter] public EventCallback OnClearError { get; set; }

    private string FechaFormateada =>
        FechaSeleccionada.ToString("dddd, dd MMMM yyyy", new CultureInfo("es-CO"))
            .ToUpper(new CultureInfo("es-CO"));
}
