using SGRRHH.Local.Tests.E2E.Helpers;
using SGRRHH.Local.Tests.E2E.PageObjects;

namespace SGRRHH.Local.Tests.E2E.Tests.CambioEstado;

/// <summary>
/// Tests de transiciones de estado para Admin
/// Admin puede: TODO, incluyendo reactivar desde Suspendido
/// </summary>
[TestFixture]
public class TransicionesAdminTests : PlaywrightSetup
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

        // Pre-condición: Crear empleado ACTIVO
        await _auth.LoginAsAdminAsync();
        _empleadoActivo = TestDataHelper.GenerarEmpleado();
        _empleadoId = await _empleadoHelper.CrearEmpleadoCompletoAsync(_empleadoActivo);
        await _auth.LogoutAsync();
    }

    /// <summary>
    /// Admin puede reactivar desde Suspendido (transición exclusiva de Admin)
    /// </summary>
    [Test]
    public async Task Admin_PuedeReactivarDesdeSuspendido()
    {
        // Arrange - Primero suspender con aprobador
        await _auth.LoginAsAprobadorAsync();
        var expedientePage = new ExpedientePage(Page, BaseUrl);

        if (_empleadoId == null)
        {
            _empleadoId = await _empleadoHelper.BuscarEmpleadoPorCedulaAsync(_empleadoActivo.Cedula);
        }
        await expedientePage.NavigateAsync(_empleadoId!.Value);
        await expedientePage.CambiarEstadoAsync("SUSPENDIDO");
        await _auth.LogoutAsync();

        // Act - Admin reactiva
        await _auth.LoginAsAdminAsync();
        await expedientePage.NavigateAsync(_empleadoId!.Value);

        // Verificar que Admin sí tiene opción ACTIVO
        var opcionActivo = await expedientePage.OpcionEstadoDisponibleAsync("ACTIVO");
        Assert.That(opcionActivo, Is.True,
            "Admin debería poder reactivar desde SUSPENDIDO");

        await expedientePage.CambiarEstadoAsync("ACTIVO");

        // Assert
        var estadoFinal = await expedientePage.ObtenerEstadoActualAsync();
        Assert.That(estadoFinal.Contains("ACTIVO"), Is.True,
            $"Estado debería ser ACTIVO, pero es: {estadoFinal}");
    }

    /// <summary>
    /// Admin tiene todas las opciones de transición desde Activo
    /// </summary>
    [Test]
    public async Task Admin_TieneTodasLasTransiciones_DesdeActivo()
    {
        // Arrange
        await _auth.LoginAsAdminAsync();
        var expedientePage = new ExpedientePage(Page, BaseUrl);

        if (_empleadoId == null)
        {
            _empleadoId = await _empleadoHelper.BuscarEmpleadoPorCedulaAsync(_empleadoActivo.Cedula);
        }
        await expedientePage.NavigateAsync(_empleadoId!.Value);

        // Assert - Debería tener TODAS las opciones desde Activo
        var opciones = await expedientePage.ObtenerOpcionesEstadoAsync();

        Assert.That(opciones.Any(o => o.Contains("VACACIONES")), Is.True, "Debería tener VACACIONES");
        Assert.That(opciones.Any(o => o.Contains("LICENCIA")), Is.True, "Debería tener LICENCIA");
        Assert.That(opciones.Any(o => o.Contains("INCAPACIDAD")), Is.True, "Debería tener INCAPACIDAD");
        Assert.That(opciones.Any(o => o.Contains("SUSPENDIDO")), Is.True, "Debería tener SUSPENDIDO");
        Assert.That(opciones.Any(o => o.Contains("RETIRADO")), Is.True, "Debería tener RETIRADO");
    }

    /// <summary>
    /// Admin puede suspender directamente
    /// </summary>
    [Test]
    public async Task Admin_PuedeSuspender()
    {
        // Arrange
        await _auth.LoginAsAdminAsync();
        var expedientePage = new ExpedientePage(Page, BaseUrl);

        if (_empleadoId == null)
        {
            _empleadoId = await _empleadoHelper.BuscarEmpleadoPorCedulaAsync(_empleadoActivo.Cedula);
        }
        await expedientePage.NavigateAsync(_empleadoId!.Value);

        // Act
        await expedientePage.CambiarEstadoAsync("SUSPENDIDO");

        // Assert
        var estadoFinal = await expedientePage.ObtenerEstadoActualAsync();
        Assert.That(estadoFinal.Contains("SUSPENDIDO"), Is.True);
    }

    /// <summary>
    /// Admin puede retirar directamente
    /// </summary>
    [Test]
    public async Task Admin_PuedeRetirar()
    {
        // Arrange
        await _auth.LoginAsAdminAsync();
        var expedientePage = new ExpedientePage(Page, BaseUrl);

        if (_empleadoId == null)
        {
            _empleadoId = await _empleadoHelper.BuscarEmpleadoPorCedulaAsync(_empleadoActivo.Cedula);
        }
        await expedientePage.NavigateAsync(_empleadoId!.Value);

        // Act
        await expedientePage.CambiarEstadoAsync("RETIRADO");

        // Assert
        var estadoFinal = await expedientePage.ObtenerEstadoActualAsync();
        Assert.That(estadoFinal.Contains("RETIRADO"), Is.True);
    }

    /// <summary>
    /// Ni siquiera Admin puede cambiar desde Retirado (estado final)
    /// </summary>
    [Test]
    public async Task Admin_NoPuedeCambiar_DesdeRetirado()
    {
        // Arrange - Retirar primero
        await _auth.LoginAsAdminAsync();
        var expedientePage = new ExpedientePage(Page, BaseUrl);

        if (_empleadoId == null)
        {
            _empleadoId = await _empleadoHelper.BuscarEmpleadoPorCedulaAsync(_empleadoActivo.Cedula);
        }
        await expedientePage.NavigateAsync(_empleadoId!.Value);
        await expedientePage.CambiarEstadoAsync("RETIRADO");

        // Assert - Debería ser estado final
        var esEstadoFinal = await expedientePage.EsEstadoFinalAsync();
        Assert.That(esEstadoFinal, Is.True,
            "RETIRADO es estado final, no debería tener transiciones");
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
