using SGRRHH.Local.Tests.E2E.Helpers;
using SGRRHH.Local.Tests.E2E.PageObjects;

namespace SGRRHH.Local.Tests.E2E.Tests.CambioEstado;

/// <summary>
/// Tests de transiciones de estado permitidas para Operador (secretaria)
/// Operador puede: Activo ↔ EnVacaciones, EnLicencia, EnIncapacidad
/// Operador NO puede: Suspender, Retirar, Aprobar
/// </summary>
[TestFixture]
public class TransicionesOperadorTests : PlaywrightSetup
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

        // Pre-condición: Crear empleado ACTIVO (con aprobador)
        await _auth.LoginAsAprobadorAsync();
        _empleadoActivo = TestDataHelper.GenerarEmpleado();
        _empleadoId = await _empleadoHelper.CrearEmpleadoCompletoAsync(_empleadoActivo);
        await _auth.LogoutAsync();
    }

    /// <summary>
    /// Operador puede cambiar Activo → EnVacaciones
    /// </summary>
    [Test]
    public async Task Operador_PuedeCambiar_ActivoAEnVacaciones()
    {
        // Arrange
        await _auth.LoginAsOperadorAsync();
        var expedientePage = new ExpedientePage(Page, BaseUrl);

        if (_empleadoId == null)
        {
            _empleadoId = await _empleadoHelper.BuscarEmpleadoPorCedulaAsync(_empleadoActivo.Cedula);
        }
        await expedientePage.NavigateAsync(_empleadoId!.Value);

        // Verificar estado inicial
        var estadoInicial = await expedientePage.ObtenerEstadoActualAsync();
        Assert.That(estadoInicial.Contains("ACTIVO"), Is.True,
            $"Pre-condición: Estado debería ser ACTIVO, pero es: {estadoInicial}");

        // Verificar opción disponible
        var opcionDisponible = await expedientePage.OpcionEstadoDisponibleAsync("VACACIONES");
        Assert.That(opcionDisponible, Is.True, "Debería tener opción EN VACACIONES");

        // Act
        await expedientePage.CambiarEstadoAsync("EN VACACIONES");

        // Assert
        var estadoFinal = await expedientePage.ObtenerEstadoActualAsync();
        Assert.That(estadoFinal.Contains("VACACIONES"), Is.True,
            $"Estado debería ser EN VACACIONES, pero es: {estadoFinal}");
    }

    /// <summary>
    /// Operador puede cambiar EnVacaciones → Activo
    /// </summary>
    [Test]
    public async Task Operador_PuedeCambiar_EnVacacionesAActivo()
    {
        // Arrange - Primero poner en vacaciones (con aprobador)
        await _auth.LoginAsAprobadorAsync();
        var expedientePage = new ExpedientePage(Page, BaseUrl);

        if (_empleadoId == null)
        {
            _empleadoId = await _empleadoHelper.BuscarEmpleadoPorCedulaAsync(_empleadoActivo.Cedula);
        }
        await expedientePage.NavigateAsync(_empleadoId!.Value);
        await expedientePage.CambiarEstadoAsync("EN VACACIONES");
        await _auth.LogoutAsync();

        // Act - Ahora como operador, cambiar a activo
        await _auth.LoginAsOperadorAsync();
        await expedientePage.NavigateAsync(_empleadoId!.Value);

        var opcionActivo = await expedientePage.OpcionEstadoDisponibleAsync("ACTIVO");
        Assert.That(opcionActivo, Is.True, "Debería poder volver a ACTIVO");

        await expedientePage.CambiarEstadoAsync("ACTIVO");

        // Assert
        var estadoFinal = await expedientePage.ObtenerEstadoActualAsync();
        Assert.That(estadoFinal.Contains("ACTIVO"), Is.True,
            $"Estado debería ser ACTIVO, pero es: {estadoFinal}");
    }

    /// <summary>
    /// Operador puede cambiar Activo → EnIncapacidad
    /// </summary>
    [Test]
    public async Task Operador_PuedeCambiar_ActivoAEnIncapacidad()
    {
        // Arrange
        await _auth.LoginAsOperadorAsync();
        var expedientePage = new ExpedientePage(Page, BaseUrl);

        if (_empleadoId == null)
        {
            _empleadoId = await _empleadoHelper.BuscarEmpleadoPorCedulaAsync(_empleadoActivo.Cedula);
        }
        await expedientePage.NavigateAsync(_empleadoId!.Value);

        // Verificar opción disponible
        var opcionDisponible = await expedientePage.OpcionEstadoDisponibleAsync("INCAPACIDAD");
        Assert.That(opcionDisponible, Is.True, "Debería tener opción EN INCAPACIDAD");

        // Act
        await expedientePage.CambiarEstadoAsync("EN INCAPACIDAD");

        // Assert
        var estadoFinal = await expedientePage.ObtenerEstadoActualAsync();
        Assert.That(estadoFinal.Contains("INCAPACIDAD"), Is.True,
            $"Estado debería ser EN INCAPACIDAD, pero es: {estadoFinal}");
    }

    /// <summary>
    /// Operador puede cambiar EnIncapacidad → Activo
    /// </summary>
    [Test]
    public async Task Operador_PuedeCambiar_EnIncapacidadAActivo()
    {
        // Arrange - Primero poner en incapacidad
        await _auth.LoginAsAprobadorAsync();
        var expedientePage = new ExpedientePage(Page, BaseUrl);

        if (_empleadoId == null)
        {
            _empleadoId = await _empleadoHelper.BuscarEmpleadoPorCedulaAsync(_empleadoActivo.Cedula);
        }
        await expedientePage.NavigateAsync(_empleadoId!.Value);
        await expedientePage.CambiarEstadoAsync("EN INCAPACIDAD");
        await _auth.LogoutAsync();

        // Act
        await _auth.LoginAsOperadorAsync();
        await expedientePage.NavigateAsync(_empleadoId!.Value);
        await expedientePage.CambiarEstadoAsync("ACTIVO");

        // Assert
        var estadoFinal = await expedientePage.ObtenerEstadoActualAsync();
        Assert.That(estadoFinal.Contains("ACTIVO"), Is.True);
    }

    /// <summary>
    /// Operador puede cambiar Activo → EnLicencia
    /// </summary>
    [Test]
    public async Task Operador_PuedeCambiar_ActivoAEnLicencia()
    {
        // Arrange
        await _auth.LoginAsOperadorAsync();
        var expedientePage = new ExpedientePage(Page, BaseUrl);

        if (_empleadoId == null)
        {
            _empleadoId = await _empleadoHelper.BuscarEmpleadoPorCedulaAsync(_empleadoActivo.Cedula);
        }
        await expedientePage.NavigateAsync(_empleadoId!.Value);

        // Verificar opción disponible
        var opcionDisponible = await expedientePage.OpcionEstadoDisponibleAsync("LICENCIA");
        Assert.That(opcionDisponible, Is.True, "Debería tener opción EN LICENCIA");

        // Act
        await expedientePage.CambiarEstadoAsync("EN LICENCIA");

        // Assert
        var estadoFinal = await expedientePage.ObtenerEstadoActualAsync();
        Assert.That(estadoFinal.Contains("LICENCIA"), Is.True);
    }

    /// <summary>
    /// Operador puede cambiar EnLicencia → Activo
    /// </summary>
    [Test]
    public async Task Operador_PuedeCambiar_EnLicenciaAActivo()
    {
        // Arrange
        await _auth.LoginAsAprobadorAsync();
        var expedientePage = new ExpedientePage(Page, BaseUrl);

        if (_empleadoId == null)
        {
            _empleadoId = await _empleadoHelper.BuscarEmpleadoPorCedulaAsync(_empleadoActivo.Cedula);
        }
        await expedientePage.NavigateAsync(_empleadoId!.Value);
        await expedientePage.CambiarEstadoAsync("EN LICENCIA");
        await _auth.LogoutAsync();

        // Act
        await _auth.LoginAsOperadorAsync();
        await expedientePage.NavigateAsync(_empleadoId!.Value);
        await expedientePage.CambiarEstadoAsync("ACTIVO");

        // Assert
        var estadoFinal = await expedientePage.ObtenerEstadoActualAsync();
        Assert.That(estadoFinal.Contains("ACTIVO"), Is.True);
    }

    /// <summary>
    /// Operador NO puede suspender - la opción no está disponible
    /// </summary>
    [Test]
    public async Task Operador_NoPuedeSuspender_OpcionNoDisponible()
    {
        // Arrange
        await _auth.LoginAsOperadorAsync();
        var expedientePage = new ExpedientePage(Page, BaseUrl);

        if (_empleadoId == null)
        {
            _empleadoId = await _empleadoHelper.BuscarEmpleadoPorCedulaAsync(_empleadoActivo.Cedula);
        }
        await expedientePage.NavigateAsync(_empleadoId!.Value);

        // Assert
        var opcionSuspendido = await expedientePage.OpcionEstadoDisponibleAsync("SUSPENDIDO");
        Assert.That(opcionSuspendido, Is.False,
            "Operador NO debería tener opción SUSPENDIDO");
    }

    /// <summary>
    /// Operador NO puede retirar - la opción no está disponible
    /// </summary>
    [Test]
    public async Task Operador_NoPuedeRetirar_OpcionNoDisponible()
    {
        // Arrange
        await _auth.LoginAsOperadorAsync();
        var expedientePage = new ExpedientePage(Page, BaseUrl);

        if (_empleadoId == null)
        {
            _empleadoId = await _empleadoHelper.BuscarEmpleadoPorCedulaAsync(_empleadoActivo.Cedula);
        }
        await expedientePage.NavigateAsync(_empleadoId!.Value);

        // Assert
        var opcionRetirado = await expedientePage.OpcionEstadoDisponibleAsync("RETIRADO");
        Assert.That(opcionRetirado, Is.False,
            "Operador NO debería tener opción RETIRADO");
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
