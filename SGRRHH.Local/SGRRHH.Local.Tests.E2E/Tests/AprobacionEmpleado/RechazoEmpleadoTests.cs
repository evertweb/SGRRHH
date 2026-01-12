using SGRRHH.Local.Tests.E2E.Helpers;
using SGRRHH.Local.Tests.E2E.PageObjects;

namespace SGRRHH.Local.Tests.E2E.Tests.AprobacionEmpleado;

/// <summary>
/// Tests para verificar el flujo de rechazo de empleados pendientes
/// </summary>
[TestFixture]
public class RechazoEmpleadoTests : PlaywrightSetup
{
    private AuthHelper _auth = null!;
    private EmpleadoHelper _empleadoHelper = null!;
    private EmpleadoTestData _empleadoPendiente = null!;
    private int? _empleadoId;

    [SetUp]
    public async Task SetUp()
    {
        _auth = new AuthHelper(Page, Configuration, BaseUrl);
        _empleadoHelper = new EmpleadoHelper(Page, BaseUrl);

        // Pre-condición: Crear empleado pendiente
        await _auth.LoginAsOperadorAsync();
        _empleadoPendiente = TestDataHelper.GenerarEmpleado();
        _empleadoId = await _empleadoHelper.CrearEmpleadoCompletoAsync(_empleadoPendiente);
        await _auth.LogoutAsync();
    }

    /// <summary>
    /// Verifica que el Aprobador puede rechazar un empleado pendiente
    /// </summary>
    [Test]
    public async Task Aprobador_PuedeRechazar_DesdeExpediente()
    {
        // Arrange
        await _auth.LoginAsAprobadorAsync();

        if (_empleadoId == null)
        {
            _empleadoId = await _empleadoHelper.BuscarEmpleadoPorCedulaAsync(_empleadoPendiente.Cedula);
        }

        var expedientePage = new ExpedientePage(Page, BaseUrl);
        await expedientePage.NavigateAsync(_empleadoId!.Value);

        // Verificar que tiene opción de rechazar
        var opcionRechazado = await expedientePage.OpcionEstadoDisponibleAsync("RECHAZADO");
        Assert.That(opcionRechazado, Is.True,
            "Aprobador debería tener opción de RECHAZAR");

        // Act
        await expedientePage.CambiarEstadoAsync("RECHAZADO");

        // Assert
        var estadoFinal = await expedientePage.ObtenerEstadoActualAsync();
        Assert.That(estadoFinal.Contains("RECHAZADO"), Is.True,
            $"Estado final debería ser RECHAZADO, pero es: {estadoFinal}");
    }

    /// <summary>
    /// Verifica que el rechazo es un estado final (sin más transiciones)
    /// </summary>
    [Test]
    public async Task Rechazado_EsEstadoFinal()
    {
        // Arrange - Rechazar primero
        await _auth.LoginAsAprobadorAsync();

        if (_empleadoId == null)
        {
            _empleadoId = await _empleadoHelper.BuscarEmpleadoPorCedulaAsync(_empleadoPendiente.Cedula);
        }

        var expedientePage = new ExpedientePage(Page, BaseUrl);
        await expedientePage.NavigateAsync(_empleadoId!.Value);
        await expedientePage.CambiarEstadoAsync("RECHAZADO");

        // Logout y login de nuevo para refrescar
        await _auth.LogoutAsync();
        await _auth.LoginAsAdminAsync();
        await expedientePage.NavigateAsync(_empleadoId!.Value);

        // Assert - Ni siquiera Admin puede cambiar desde rechazado
        var esEstadoFinal = await expedientePage.EsEstadoFinalAsync();
        Assert.That(esEstadoFinal, Is.True,
            "RECHAZADO debería ser un estado final sin transiciones disponibles");
    }

    /// <summary>
    /// Verifica que el Operador NO puede rechazar
    /// </summary>
    [Test]
    public async Task Operador_NoPuedeRechazar()
    {
        // Arrange
        await _auth.LoginAsOperadorAsync();

        if (_empleadoId == null)
        {
            _empleadoId = await _empleadoHelper.BuscarEmpleadoPorCedulaAsync(_empleadoPendiente.Cedula);
        }

        var expedientePage = new ExpedientePage(Page, BaseUrl);
        await expedientePage.NavigateAsync(_empleadoId!.Value);

        // Assert
        var opcionRechazado = await expedientePage.OpcionEstadoDisponibleAsync("RECHAZADO");
        Assert.That(opcionRechazado, Is.False,
            "Operador NO debería tener opción de RECHAZAR");
    }

    [TearDown]
    public async Task TearDown()
    {
        try
        {
            await _auth.LogoutAsync();
        }
        catch
        {
            // Ignorar
        }
    }
}
