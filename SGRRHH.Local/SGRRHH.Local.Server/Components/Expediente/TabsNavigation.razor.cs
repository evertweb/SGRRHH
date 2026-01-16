using Microsoft.AspNetCore.Components;

namespace SGRRHH.Local.Server.Components.Expediente;

public partial class TabsNavigation
{
    [Parameter] public string ActiveTab { get; set; } = string.Empty;
    [Parameter] public List<TabDefinition> Tabs { get; set; } = new();
    [Parameter] public EventCallback<string> OnTabChanged { get; set; }

    public class TabDefinition
    {
        public string Id { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public int Contador { get; set; }
        public bool MostrarConteo { get; set; }
        public bool Visible { get; set; } = true;
    }
}
