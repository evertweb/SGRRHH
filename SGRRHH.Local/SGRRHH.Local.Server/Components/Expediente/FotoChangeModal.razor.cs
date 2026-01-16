using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;

using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Server.Components.Expediente;

public partial class FotoChangeModal
{
    [Parameter] public Empleado? Empleado { get; set; }
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }
    [Parameter] public EventCallback OnAbrirScannerFoto { get; set; }
    [Parameter] public EventCallback OnFotoActualizada { get; set; }
    [Parameter] public dynamic? MessageToast { get; set; }

    [Inject] private ILocalStorageService StorageService { get; set; } = default!;
    [Inject] private IEmpleadoRepository EmpleadoRepo { get; set; } = default!;
    [Inject] private ILogger<FotoChangeModal> Logger { get; set; } = default!;

    private async Task Cerrar()
    {
        await IsVisibleChanged.InvokeAsync(false);
    }

    private async Task AbrirScannerFoto()
    {
        await Cerrar();

        if (OnAbrirScannerFoto.HasDelegate)
        {
            await OnAbrirScannerFoto.InvokeAsync();
        }
    }

    private async Task OnFotoArchivoSubido(InputFileChangeEventArgs e)
    {
        if (Empleado == null) return;

        try
        {
            var file = e.File;
            if (file.Size > 5 * 1024 * 1024)
            {
                MessageToast?.ShowError("La foto no puede superar 5MB");
                return;
            }

            if (!file.ContentType.StartsWith("image/"))
            {
                MessageToast?.ShowError("Solo se permiten archivos de imagen");
                return;
            }

            using var ms = new MemoryStream();
            await file.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024).CopyToAsync(ms);
            var bytes = ms.ToArray();

            var extension = Path.GetExtension(file.Name);
            var fileName = $"foto_{DateTime.Now:yyyyMMdd_HHmmss}{extension}";

            if (!string.IsNullOrEmpty(Empleado.FotoPath))
            {
                await StorageService.DeleteEmpleadoFotoAsync(Empleado.Id);
            }

            var storageResult = await StorageService.SaveEmpleadoFotoAsync(
                Empleado.Id,
                bytes,
                fileName);

            if (storageResult.IsSuccess)
            {
                Empleado.FotoPath = storageResult.Value;
                await EmpleadoRepo.UpdateAsync(Empleado);

                await Cerrar();
                await NotificarFotoActualizada();

                MessageToast?.ShowSuccess("✓ Foto actualizada exitosamente");
                Logger.LogInformation("Foto actualizada para empleado {Codigo}: {Path}", Empleado.Codigo, storageResult.Value);
            }
            else
            {
                MessageToast?.ShowError("Error al guardar la foto");
                Logger.LogError("Error guardando foto: {Error}", storageResult.Error);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error al subir foto");
            MessageToast?.ShowError("Error al subir foto: " + ex.Message);
            await Cerrar();
        }
    }

    private async Task EliminarFoto()
    {
        if (Empleado == null || string.IsNullOrEmpty(Empleado.FotoPath)) return;

        try
        {
            await StorageService.DeleteEmpleadoFotoAsync(Empleado.Id);
            Empleado.FotoPath = null;
            await EmpleadoRepo.UpdateAsync(Empleado);

            await Cerrar();
            await NotificarFotoActualizada();

            MessageToast?.ShowSuccess("✓ Foto eliminada");
            Logger.LogInformation("Foto eliminada para empleado {Codigo}", Empleado.Codigo);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error al eliminar foto");
            MessageToast?.ShowError("Error al eliminar foto");
            await Cerrar();
        }
    }

    private async Task NotificarFotoActualizada()
    {
        if (OnFotoActualizada.HasDelegate)
        {
            await OnFotoActualizada.InvokeAsync();
        }
    }

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
