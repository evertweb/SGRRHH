using SGRRHH.Local.Tests.E2E.Helpers;
using SGRRHH.Local.Tests.E2E.PageObjects;

namespace SGRRHH.Local.Tests.E2E.Tests.Permisos;

/// <summary>
/// Tests para verificar que SOLO Admin puede reactivar desde Suspendido
/// </summary>
[TestFixture]
public class SoloAdminReactivaTests : PlaywrightSetup
{
    private AuthHelper _auth = null!;
    private EmpleadoHelper _empleadoHelper = null!;
    private EmpleadoTestData _empleadoSuspendido = null!;
    private int? _empleadoId;

    [SetUp]
    public async Task SetUp()
    {
        _auth = new AuthHelper(Page, Configuration, BaseUrl);
        _empleadoHelper = new EmpleadoHelper(Page, BaseUrl);

        // Crear empleado y suspenderlo
        await _auth.LoginAsAprobadorAsync();
        _empleadoSuspendido = TestDataHelper.GenerarEmpleado();
        _empleadoId = await _empleadoHelper.CrearEmpleadoCompletoAsync(_empleadoSuspendido);

        // Suspender
        var expedientePage = new ExpedientePage(Page, BaseUrl);
        if (_empleadoId == null)
        {
            _empleadoId = await _empleadoHelper.BuscarEmpleadoPorCedulaAsync(_empleadoSuspendido.Cedula);
        }
        await expedientePage.NavigateAsync(_empleadoId!.Value);
        await expedientePage.CambiarEstadoAsync("SUSPENDIDO");
        await _auth.LogoutAsync();
    }

    /// <summary>
    /// Operador NO puede reactivar desde Suspendido
    /// </summary>
    [Test]
    public async Task Operador_NoPuedeReactivar_DesdeSuspendido()
    {
        await _auth.LoginAsOperadorAsync();
        var expedientePage = new ExpedientePage(Page, BaseUrl);
        await expedientePage.NavigateAsync(_empleadoId!.Value);

        // Verificar estado actual
        var estado = await expedientePage.ObtenerEstadoActualAsync();
        Assert.That(estado.Contains("SUSPENDIDO"), Is.True, "Pre-condición: empleado suspendido");

        // Verificar que NO tiene opción ACTIVO
        var opcionActivo = await expedientePage.OpcionEstadoDisponibleAsync("ACTIVO");
        Assert.That(opcionActivo, Is.False,
            "Operador NO puede reactivar desde SUSPENDIDO");
    }

    /// <summary>
    /// Aprobador NO puede reactivar desde Suspendido
    /// </summary>
    [Test]
    public async Task Aprobador_NoPuedeReactivar_DesdeSuspendido()
    {
        await _auth.LoginAsAprobadorAsync();
        var expedientePage = new ExpedientePage(Page, BaseUrl);
        await expedientePage.NavigateAsync(_empleadoId!.Value);

        var opcionActivo = await expedientePage.OpcionEstadoDisponibleAsync("ACTIVO");
        Assert.That(opcionActivo, Is.False,
            "Aprobador NO puede reactivar desde SUSPENDIDO");
    }

    /// <summary>
    /// Solo Admin SÍ puede reactivar desde Suspendido
    /// </summary>
    [Test]
    public async Task Admin_SiPuedeReactivar_DesdeSuspendido()
    {
        await _auth.LoginAsAdminAsync();
        var expedientePage = new ExpedientePage(Page, BaseUrl);
        await expedientePage.NavigateAsync(_empleadoId!.Value);

        // Verificar que SÍ tiene opción ACTIVO
        var opcionActivo = await expedientePage.OpcionEstadoDisponibleAsync("ACTIVO");
        Assert.That(opcionActivo, Is.True,
            "Admin SÍ puede reactivar desde SUSPENDIDO");

        // Ejecutar la reactivación
        await expedientePage.CambiarEstadoAsync("ACTIVO");

        // Verificar estado final
        var estadoFinal = await expedientePage.ObtenerEstadoActualAsync();
        Assert.That(estadoFinal.Contains("ACTIVO"), Is.True,
            $"Estado debería ser ACTIVO después de reactivar, pero es: {estadoFinal}");
    }

    /// <summary>
    /// Aprobador SÍ puede retirar desde Suspendido (pero no reactivar)
    /// </summary>
    [Test]
    public async Task Aprobador_SiPuedeRetirar_DesdeSuspendido()
    {
        await _auth.LoginAsAprobadorAsync();
        var expedientePage = new ExpedientePage(Page, BaseUrl);
        await expedientePage.NavigateAsync(_empleadoId!.Value);

        // Aprobador puede retirar desde suspendido
        var opcionRetirado = await expedientePage.OpcionEstadoDisponibleAsync("RETIRADO");
        Assert.That(opcionRetirado, Is.True,
            "Aprobador SÍ puede RETIRAR desde SUSPENDIDO");
    }

    [TearDown]
    public async Task TearDown()
    {
        try { await _auth.LogoutAsync(); } catch { }
    }
}
