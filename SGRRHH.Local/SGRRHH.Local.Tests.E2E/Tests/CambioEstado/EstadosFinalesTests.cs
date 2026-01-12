using SGRRHH.Local.Tests.E2E.Helpers;
using SGRRHH.Local.Tests.E2E.PageObjects;

namespace SGRRHH.Local.Tests.E2E.Tests.CambioEstado;

/// <summary>
/// Tests para verificar que estados finales (Retirado, Rechazado) 
/// no tienen transiciones disponibles
/// </summary>
[TestFixture]
public class EstadosFinalesTests : PlaywrightSetup
{
    private AuthHelper _auth = null!;
    private EmpleadoHelper _empleadoHelper = null!;

    [SetUp]
    public async Task SetUp()
    {
        _auth = new AuthHelper(Page, Configuration, BaseUrl);
        _empleadoHelper = new EmpleadoHelper(Page, BaseUrl);
    }

    /// <summary>
    /// Retirado es estado final - no hay opciones de transición
    /// </summary>
    [Test]
    public async Task Retirado_EsEstadoFinal_SinOpciones()
    {
        // Arrange - Crear y retirar empleado
        await _auth.LoginAsAdminAsync();
        var datosEmpleado = TestDataHelper.GenerarEmpleado();
        var empleadoId = await _empleadoHelper.CrearEmpleadoCompletoAsync(datosEmpleado);

        var expedientePage = new ExpedientePage(Page, BaseUrl);

        if (empleadoId == null)
        {
            empleadoId = await _empleadoHelper.BuscarEmpleadoPorCedulaAsync(datosEmpleado.Cedula);
        }

        await expedientePage.NavigateAsync(empleadoId!.Value);
        await expedientePage.CambiarEstadoAsync("RETIRADO");

        // Assert
        var estado = await expedientePage.ObtenerEstadoActualAsync();
        Assert.That(estado.Contains("RETIRADO"), Is.True);

        var esEstadoFinal = await expedientePage.EsEstadoFinalAsync();
        Assert.That(esEstadoFinal, Is.True,
            "RETIRADO debería ser estado final sin transiciones");

        var opciones = await expedientePage.ObtenerOpcionesEstadoAsync();
        var opcionesReales = opciones.Where(o => !string.IsNullOrWhiteSpace(o)).ToList();
        Assert.That(opcionesReales.Count, Is.EqualTo(0),
            $"No debería haber opciones de transición, pero hay: {string.Join(", ", opcionesReales)}");
    }

    /// <summary>
    /// Rechazado es estado final - no hay opciones de transición
    /// </summary>
    [Test]
    public async Task Rechazado_EsEstadoFinal_SinOpciones()
    {
        // Arrange - Crear empleado pendiente y rechazarlo
        await _auth.LoginAsOperadorAsync();
        var datosEmpleado = TestDataHelper.GenerarEmpleado();
        var empleadoId = await _empleadoHelper.CrearEmpleadoCompletoAsync(datosEmpleado);
        await _auth.LogoutAsync();

        // Rechazar como aprobador
        await _auth.LoginAsAprobadorAsync();
        var expedientePage = new ExpedientePage(Page, BaseUrl);

        if (empleadoId == null)
        {
            empleadoId = await _empleadoHelper.BuscarEmpleadoPorCedulaAsync(datosEmpleado.Cedula);
        }

        await expedientePage.NavigateAsync(empleadoId!.Value);
        await expedientePage.CambiarEstadoAsync("RECHAZADO");
        await _auth.LogoutAsync();

        // Verificar como Admin (máximos permisos)
        await _auth.LoginAsAdminAsync();
        await expedientePage.NavigateAsync(empleadoId!.Value);

        // Assert
        var estado = await expedientePage.ObtenerEstadoActualAsync();
        Assert.That(estado.Contains("RECHAZADO"), Is.True);

        var esEstadoFinal = await expedientePage.EsEstadoFinalAsync();
        Assert.That(esEstadoFinal, Is.True,
            "RECHAZADO debería ser estado final sin transiciones");
    }

    /// <summary>
    /// Incluso Admin no puede recuperar un empleado retirado
    /// </summary>
    [Test]
    public async Task Admin_NoPuedeRecuperar_EmpleadoRetirado()
    {
        // Arrange
        await _auth.LoginAsAdminAsync();
        var datosEmpleado = TestDataHelper.GenerarEmpleado();
        var empleadoId = await _empleadoHelper.CrearEmpleadoCompletoAsync(datosEmpleado);

        var expedientePage = new ExpedientePage(Page, BaseUrl);
        if (empleadoId == null)
        {
            empleadoId = await _empleadoHelper.BuscarEmpleadoPorCedulaAsync(datosEmpleado.Cedula);
        }

        await expedientePage.NavigateAsync(empleadoId!.Value);
        await expedientePage.CambiarEstadoAsync("RETIRADO");

        // Assert
        var opcionActivo = await expedientePage.OpcionEstadoDisponibleAsync("ACTIVO");
        Assert.That(opcionActivo, Is.False,
            "Ni Admin puede cambiar desde RETIRADO a ACTIVO");
    }

    /// <summary>
    /// Incluso Admin no puede recuperar un empleado rechazado
    /// </summary>
    [Test]
    public async Task Admin_NoPuedeRecuperar_EmpleadoRechazado()
    {
        // Arrange - Crear pendiente y rechazar
        await _auth.LoginAsOperadorAsync();
        var datosEmpleado = TestDataHelper.GenerarEmpleado();
        var empleadoId = await _empleadoHelper.CrearEmpleadoCompletoAsync(datosEmpleado);
        await _auth.LogoutAsync();

        await _auth.LoginAsAdminAsync();
        var expedientePage = new ExpedientePage(Page, BaseUrl);

        if (empleadoId == null)
        {
            empleadoId = await _empleadoHelper.BuscarEmpleadoPorCedulaAsync(datosEmpleado.Cedula);
        }

        await expedientePage.NavigateAsync(empleadoId!.Value);
        await expedientePage.CambiarEstadoAsync("RECHAZADO");

        // Assert
        var opcionActivo = await expedientePage.OpcionEstadoDisponibleAsync("ACTIVO");
        Assert.That(opcionActivo, Is.False,
            "Ni Admin puede cambiar desde RECHAZADO a ACTIVO");
    }

    [TearDown]
    public async Task TearDown()
    {
        try
        {
            await _auth.LogoutAsync();
        }
        catch { }
    }
}
