using Microsoft.AspNetCore.Components;

namespace SGRRHH.Local.Server.Components.ControlDiario;

public partial class FiltrosDiarios : ComponentBase
{
    [Parameter] public string SearchTerm { get; set; } = string.Empty;

    [Parameter] public EventCallback<string> SearchTermChanged { get; set; }

    [Parameter] public EventCallback OnFilterChanged { get; set; }

    private string searchLocal = string.Empty;

    protected override void OnParametersSet()
    {
        searchLocal = SearchTerm;
    }

    private async Task NotificarCambio()
    {
        SearchTerm = searchLocal ?? string.Empty;
        await SearchTermChanged.InvokeAsync(SearchTerm);
        await OnFilterChanged.InvokeAsync();
    }

    private async Task Limpiar()
    {
        searchLocal = string.Empty;
        await NotificarCambio();
    }
}
