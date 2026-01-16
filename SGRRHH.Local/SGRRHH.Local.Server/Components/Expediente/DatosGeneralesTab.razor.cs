using Microsoft.AspNetCore.Components;

using SGRRHH.Local.Domain.Entities;

namespace SGRRHH.Local.Server.Components.Expediente;

public partial class DatosGeneralesTab
{
    [Parameter] public Empleado? Empleado { get; set; }
}
