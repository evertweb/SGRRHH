using Microsoft.AspNetCore.Components;
using SGRRHH.Local.Domain.Entities;

namespace SGRRHH.Local.Server.Components.Tabs
{
    public class SeguridadSocialTabBase : ComponentBase
    {
        [Parameter] public Empleado? Empleado { get; set; }
    }
}
