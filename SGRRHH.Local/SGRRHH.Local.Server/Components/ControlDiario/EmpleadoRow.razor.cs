using Microsoft.AspNetCore.Components;
using SGRRHH.Local.Domain.Entities;

namespace SGRRHH.Local.Server.Components.ControlDiario;

public partial class EmpleadoRow : ComponentBase
{
    [Parameter] public RegistroDiario Registro { get; set; } = new();

    [Parameter] public bool IsSelected { get; set; }

    [Parameter] public int? EditandoEntradaId { get; set; }

    [Parameter] public int? EditandoSalidaId { get; set; }

    [Parameter] public EventCallback<RegistroDiario> OnSeleccionar { get; set; }

    [Parameter] public EventCallback<RegistroDiario> OnEditarEntrada { get; set; }

    [Parameter] public EventCallback<RegistroDiario> OnEditarSalida { get; set; }

    [Parameter] public EventCallback<RegistroDiario> OnGuardarHorario { get; set; }

    [Parameter] public EventCallback<RegistroDiario> OnRegistrarEntrada { get; set; }

    [Parameter] public EventCallback<RegistroDiario> OnRegistrarSalida { get; set; }

    [Parameter] public EventCallback<RegistroDiario> OnAgregarActividad { get; set; }

    [Parameter] public EventCallback<RegistroDiario> OnCompletar { get; set; }

    [Parameter] public EventCallback<RegistroDiario> OnVerDetalle { get; set; }

    [Parameter] public EventCallback<int> OnIrAExpediente { get; set; }

    private bool EstaEditandoEntrada => EditandoEntradaId == Registro.Id;

    private bool EstaEditandoSalida => EditandoSalidaId == Registro.Id;

    private async Task Seleccionar()
    {
        await OnSeleccionar.InvokeAsync(Registro);
    }

    private async Task EditarEntrada()
    {
        await OnEditarEntrada.InvokeAsync(Registro);
    }

    private async Task EditarSalida()
    {
        await OnEditarSalida.InvokeAsync(Registro);
    }

    private async Task OnHoraEntradaChanged(ChangeEventArgs args)
    {
        Registro.HoraEntrada = ParseTimeSpan(args.Value?.ToString());
        await OnGuardarHorario.InvokeAsync(Registro);
    }

    private async Task OnHoraSalidaChanged(ChangeEventArgs args)
    {
        Registro.HoraSalida = ParseTimeSpan(args.Value?.ToString());
        await OnGuardarHorario.InvokeAsync(Registro);
    }

    private async Task RegistrarEntrada()
    {
        await OnRegistrarEntrada.InvokeAsync(Registro);
    }

    private async Task RegistrarSalida()
    {
        await OnRegistrarSalida.InvokeAsync(Registro);
    }

    private async Task AgregarActividad()
    {
        await OnAgregarActividad.InvokeAsync(Registro);
    }

    private async Task CompletarRegistro()
    {
        await OnCompletar.InvokeAsync(Registro);
    }

    private async Task VerDetalle()
    {
        await OnVerDetalle.InvokeAsync(Registro);
    }

    private async Task IrAExpediente()
    {
        var empleadoId = Registro.Empleado?.Id ?? 0;
        if (empleadoId > 0)
        {
            await OnIrAExpediente.InvokeAsync(empleadoId);
        }
    }

    private static string FormatTimeSpan(TimeSpan? time) => time?.ToString(@"hh\:mm") ?? string.Empty;

    private static TimeSpan? ParseTimeSpan(string? value)
    {
        if (string.IsNullOrEmpty(value)) return null;
        return TimeSpan.TryParse(value, out var time) ? time : null;
    }
}
