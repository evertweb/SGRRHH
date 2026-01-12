using SGRRHH.Local.Tests.E2E.Helpers;
using SGRRHH.Local.Tests.E2E.PageObjects;

namespace SGRRHH.Local.Tests.E2E.Tests.CambioEstado;

/// <summary>
/// Tests de transiciones de estado permitidas para Aprobador (ingeniera)
/// Aprobador puede: Todo lo del Operador + Suspender, Retirar, Aprobar
/// Aprobador NO puede: Reactivar desde Suspendido (solo Admin)
/// </summary>
[TestFixture]
public class TransicionesAprobadorTests : PlaywrightSetup
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
        await _auth.LoginAsAprobadorAsync();
        _empleadoActivo = TestDataHelper.GenerarEmpleado();
        _empleadoId = await _empleadoHelper.CrearEmpleadoCompletoAsync(_empleadoActivo);
        await _auth.LogoutAsync();
    }

    /// <summary>
    /// Aprobador puede suspender empleado activo
    /// </summary>
    [Test]
    public async Task Aprobador_PuedeSuspender()
    {
        // Arrange
        await _auth.LoginAsAprobadorAsync();
        var expedientePage = new ExpedientePage(Page, BaseUrl);

        if (_empleadoId == null)
        {
            _empleadoId = await _empleadoHelper.BuscarEmpleadoPorCedulaAsync(_empleadoActivo.Cedula);
        }
        await expedientePage.NavigateAsync(_empleadoId!.Value);

        // Verificar opción disponible
        var opcionSuspendido = await expedientePage.OpcionEstadoDisponibleAsync("SUSPENDIDO");
        Assert.That(opcionSuspendido, Is.True,
            "Aprobador debería tener opción SUSPENDIDO");

        // Act
        await expedientePage.CambiarEstadoAsync("SUSPENDIDO");

        // Assert
        var estadoFinal = await expedientePage.ObtenerEstadoActualAsync();
        Assert.That(estadoFinal.Contains("SUSPENDIDO"), Is.True,
            $"Estado debería ser SUSPENDIDO, pero es: {estadoFinal}");
    }

    /// <summary>
    /// Aprobador puede retirar empleado activo
    /// </summary>
    [Test]
    public async Task Aprobador_PuedeRetirar()
    {
        // Arrange
        await _auth.LoginAsAprobadorAsync();
        var expedientePage = new ExpedientePage(Page, BaseUrl);

        if (_empleadoId == null)
        {
            _empleadoId = await _empleadoHelper.BuscarEmpleadoPorCedulaAsync(_empleadoActivo.Cedula);
        }
        await expedientePage.NavigateAsync(_empleadoId!.Value);

        // Verificar opción disponible
        var opcionRetirado = await expedientePage.OpcionEstadoDisponibleAsync("RETIRADO");
        Assert.That(opcionRetirado, Is.True,
            "Aprobador debería tener opción RETIRADO");

        // Act
        await expedientePage.CambiarEstadoAsync("RETIRADO");

        // Assert
        var estadoFinal = await expedientePage.ObtenerEstadoActualAsync();
        Assert.That(estadoFinal.Contains("RETIRADO"), Is.True,
            $"Estado debería ser RETIRADO, pero es: {estadoFinal}");
    }

    /// <summary>
    /// Aprobador puede retirar desde suspendido
    /// </summary>
    [Test]
    public async Task Aprobador_PuedeRetirar_DesdeSuspendido()
    {
        // Arrange - Primero suspender
        await _auth.LoginAsAprobadorAsync();
        var expedientePage = new ExpedientePage(Page, BaseUrl);

        if (_empleadoId == null)
        {
            _empleadoId = await _empleadoHelper.BuscarEmpleadoPorCedulaAsync(_empleadoActivo.Cedula);
        }
        await expedientePage.NavigateAsync(_empleadoId!.Value);
        await expedientePage.CambiarEstadoAsync("SUSPENDIDO");

        // Verificar que puede retirar desde suspendido
        var opcionRetirado = await expedientePage.OpcionEstadoDisponibleAsync("RETIRADO");
        Assert.That(opcionRetirado, Is.True,
            "Aprobador debería poder RETIRAR desde SUSPENDIDO");

        // Act
        await expedientePage.CambiarEstadoAsync("RETIRADO");

        // Assert
        var estadoFinal = await expedientePage.ObtenerEstadoActualAsync();
        Assert.That(estadoFinal.Contains("RETIRADO"), Is.True);
    }

    /// <summary>
    /// Aprobador NO puede reactivar desde suspendido (solo Admin puede)
    /// </summary>
    [Test]
    public async Task Aprobador_NoPuedeReactivarDesdeSuspendido()
    {
        // Arrange - Primero suspender
        await _auth.LoginAsAprobadorAsync();
        var expedientePage = new ExpedientePage(Page, BaseUrl);

        if (_empleadoId == null)
        {
            _empleadoId = await _empleadoHelper.BuscarEmpleadoPorCedulaAsync(_empleadoActivo.Cedula);
        }
        await expedientePage.NavigateAsync(_empleadoId!.Value);
        await expedientePage.CambiarEstadoAsync("SUSPENDIDO");

        // Assert - NO debería poder volver a ACTIVO
        var opcionActivo = await expedientePage.OpcionEstadoDisponibleAsync("ACTIVO");
        Assert.That(opcionActivo, Is.False,
            "Aprobador NO debería poder reactivar desde SUSPENDIDO");
    }

    /// <summary>
    /// Aprobador tiene las mismas opciones que Operador para estados temporales
    /// </summary>
    [Test]
    public async Task Aprobador_TieneOpcionesTemporales()
    {
        // Arrange
        await _auth.LoginAsAprobadorAsync();
        var expedientePage = new ExpedientePage(Page, BaseUrl);

        if (_empleadoId == null)
        {
            _empleadoId = await _empleadoHelper.BuscarEmpleadoPorCedulaAsync(_empleadoActivo.Cedula);
        }
        await expedientePage.NavigateAsync(_empleadoId!.Value);

        // Assert - Debería tener todas las opciones temporales
        var opcionVacaciones = await expedientePage.OpcionEstadoDisponibleAsync("VACACIONES");
        var opcionLicencia = await expedientePage.OpcionEstadoDisponibleAsync("LICENCIA");
        var opcionIncapacidad = await expedientePage.OpcionEstadoDisponibleAsync("INCAPACIDAD");

        Assert.That(opcionVacaciones, Is.True, "Debería tener opción VACACIONES");
        Assert.That(opcionLicencia, Is.True, "Debería tener opción LICENCIA");
        Assert.That(opcionIncapacidad, Is.True, "Debería tener opción INCAPACIDAD");
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
