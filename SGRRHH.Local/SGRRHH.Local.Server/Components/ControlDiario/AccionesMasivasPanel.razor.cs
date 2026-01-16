using Microsoft.AspNetCore.Components;

namespace SGRRHH.Local.Server.Components.ControlDiario;

public partial class AccionesMasivasPanel : ComponentBase
{
    [Parameter] public int EmpleadosSinRegistroCount { get; set; }

    [Parameter] public EventCallback OnCargarRegistros { get; set; }

    [Parameter] public EventCallback OnCrearRegistros { get; set; }
}
