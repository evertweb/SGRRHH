using Microsoft.AspNetCore.Components;

using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Server.Components.Expediente;

public partial class EmpleadoHeader
{
    [Parameter] public Empleado? Empleado { get; set; }
    [Parameter] public List<EstadoEmpleado> TransicionesPermitidas { get; set; } = new();
    [Parameter] public string NuevoEstadoSeleccionado { get; set; } = string.Empty;
    [Parameter] public EventCallback<string> NuevoEstadoSeleccionadoChanged { get; set; }
    [Parameter] public bool IsSavingEstado { get; set; }
    [Parameter] public EventCallback OnCambiarEstado { get; set; }
    [Parameter] public EventCallback OnCambiarFoto { get; set; }
    [Parameter] public EventCallback OnVolver { get; set; }
    [Parameter] public EventCallback OnEditarEmpleado { get; set; }
    [Parameter] public bool PuedeEliminar { get; set; }
    [Parameter] public EventCallback OnConfirmarEliminar { get; set; }

    private string GetFotoUrl()
    {
        if (string.IsNullOrEmpty(Empleado?.FotoPath)) return "/images/default-avatar.png";

        var path = Empleado.FotoPath.Replace("\\", "/");

        if (path.StartsWith("Fotos/", StringComparison.OrdinalIgnoreCase))
        {
            path = path.Substring(6);
            return $"/fotos/{path}?v={DateTime.Now.Ticks}";
        }

        if (!path.Contains("/"))
        {
            return $"/fotos/Empleados/{Empleado.Id}/{path}?v={DateTime.Now.Ticks}";
        }

        return $"/fotos/{path}?v={DateTime.Now.Ticks}";
    }

    private string GetInitials()
    {
        if (Empleado == null) return "??";

        var nombres = Empleado.Nombres?.Split(' ').FirstOrDefault() ?? "";
        var apellidos = Empleado.Apellidos?.Split(' ').FirstOrDefault() ?? "";
        return $"{(nombres.Length > 0 ? nombres[0] : '?')}{(apellidos.Length > 0 ? apellidos[0] : '?')}".ToUpper();
    }
}
