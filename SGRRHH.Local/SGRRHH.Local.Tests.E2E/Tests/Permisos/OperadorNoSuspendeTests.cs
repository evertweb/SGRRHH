using SGRRHH.Local.Tests.E2E.Helpers;
using SGRRHH.Local.Tests.E2E.PageObjects;

namespace SGRRHH.Local.Tests.E2E.Tests.Permisos;

/// <summary>
/// Tests para verificar que Operador NO puede suspender ni retirar empleados
/// </summary>
[TestFixture]
public class OperadorNoSuspendeTests : PlaywrightSetup
{
    private AuthHelper _auth = null!;
    private EmpleadoHelper _empleadoHelper = null!;
    private EmpleadoTestData _empleadoActivo = null!;
    private int? _empleadoId;

    [SetUp]
    public async Task SetUp()
    {
        _auth = new AuthHelper(Page, Configuration, BaseUrl);
        _empleadoHelper = new EmpleadoHelper(Page, BaseUrl);

        // Crear empleado ACTIVO (con aprobador)
        await _auth.LoginAsAprobadorAsync();
        _empleadoActivo = TestDataHelper.GenerarEmpleado();
        _empleadoId = await _empleadoHelper.CrearEmpleadoCompletoAsync(_empleadoActivo);
        await _auth.LogoutAsync();
    }

    /// <summary>
    /// Operador no tiene opción SUSPENDIDO
    /// </summary>
    [Test]
    public async Task Operador_NoTieneOpcionSuspendido()
    {
        await _auth.LoginAsOperadorAsync();
        var expedientePage = new ExpedientePage(Page, BaseUrl);

        if (_empleadoId == null)
        {
            _empleadoId = await _empleadoHelper.BuscarEmpleadoPorCedulaAsync(_empleadoActivo.Cedula);
        }
        await expedientePage.NavigateAsync(_empleadoId!.Value);

        var opcionSuspendido = await expedientePage.OpcionEstadoDisponibleAsync("SUSPENDIDO");
        Assert.That(opcionSuspendido, Is.False,
            "Operador NO debe tener opción SUSPENDIDO");
    }

    /// <summary>
    /// Operador no tiene opción RETIRADO
    /// </summary>
    [Test]
    public async Task Operador_NoTieneOpcionRetirado()
    {
        await _auth.LoginAsOperadorAsync();
        var expedientePage = new ExpedientePage(Page, BaseUrl);

        if (_empleadoId == null)
        {
            _empleadoId = await _empleadoHelper.BuscarEmpleadoPorCedulaAsync(_empleadoActivo.Cedula);
        }
        await expedientePage.NavigateAsync(_empleadoId!.Value);

        var opcionRetirado = await expedientePage.OpcionEstadoDisponibleAsync("RETIRADO");
        Assert.That(opcionRetirado, Is.False,
            "Operador NO debe tener opción RETIRADO");
    }

    /// <summary>
    /// Operador solo ve opciones permitidas (estados temporales)
    /// </summary>
    [Test]
    public async Task Operador_SoloVeOpcionesTemporales()
    {
        await _auth.LoginAsOperadorAsync();
        var expedientePage = new ExpedientePage(Page, BaseUrl);

        if (_empleadoId == null)
        {
            _empleadoId = await _empleadoHelper.BuscarEmpleadoPorCedulaAsync(_empleadoActivo.Cedula);
        }
        await expedientePage.NavigateAsync(_empleadoId!.Value);

        var opciones = await expedientePage.ObtenerOpcionesEstadoAsync();

        // Debería tener estados temporales
        Assert.That(opciones.Any(o => o.Contains("VACACIONES")), Is.True, "Debería ver VACACIONES");
        Assert.That(opciones.Any(o => o.Contains("LICENCIA")), Is.True, "Debería ver LICENCIA");
        Assert.That(opciones.Any(o => o.Contains("INCAPACIDAD")), Is.True, "Debería ver INCAPACIDAD");

        // NO debería tener estados restrictivos
        Assert.That(opciones.Any(o => o.Contains("SUSPENDIDO")), Is.False, "NO debería ver SUSPENDIDO");
        Assert.That(opciones.Any(o => o.Contains("RETIRADO")), Is.False, "NO debería ver RETIRADO");
    }

    [TearDown]
    public async Task TearDown()
    {
        try { await _auth.LogoutAsync(); } catch { }
    }
}
