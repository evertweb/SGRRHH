using Microsoft.AspNetCore.Components;

namespace SGRRHH.Local.Server.Components.ControlDiario;

public partial class ResumenDiarioCard : ComponentBase
{
    [Parameter] public int TotalRegistros { get; set; }

    [Parameter] public int RegistrosFiltrados { get; set; }

    [Parameter] public int Completados { get; set; }

    [Parameter] public int Pendientes { get; set; }

    [Parameter] public decimal TotalHoras { get; set; }

    [Parameter] public string SearchTerm { get; set; } = string.Empty;
}
