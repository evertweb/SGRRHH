using SGRRHH.Local.Tests.E2E.Helpers;
using SGRRHH.Local.Tests.E2E.PageObjects;

namespace SGRRHH.Local.Tests.E2E.Tests.AprobacionEmpleado;

/// <summary>
/// Tests para verificar el flujo de aprobación desde el expediente del empleado
/// </summary>
[TestFixture]
public class AprobacionDesdeExpedienteTests : PlaywrightSetup
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

        // Pre-condición: Crear empleado pendiente con secretaria
        await _auth.LoginAsOperadorAsync();
        _empleadoPendiente = TestDataHelper.GenerarEmpleado();
        _empleadoId = await _empleadoHelper.CrearEmpleadoCompletoAsync(_empleadoPendiente);
        await _auth.LogoutAsync();
    }

    /// <summary>
    /// Verifica que el Aprobador puede aprobar desde el expediente
    /// </summary>
    [Test]
    public async Task Aprobador_PuedeAprobar_DesdeExpediente()
    {
        // Arrange
        await _auth.LoginAsAprobadorAsync();
        
        // Buscar el empleado y obtener su ID si no lo tenemos
        if (_empleadoId == null)
        {
            _empleadoId = await _empleadoHelper.BuscarEmpleadoPorCedulaAsync(_empleadoPendiente.Cedula);
        }
        
        Assert.That(_empleadoId, Is.Not.Null, "Debería poder encontrar el empleado creado");

        var expedientePage = new ExpedientePage(Page, BaseUrl);
        await expedientePage.NavigateAsync(_empleadoId!.Value);

        // Verificar estado actual es pendiente
        var estadoInicial = await expedientePage.ObtenerEstadoActualAsync();
        Assert.That(estadoInicial.Contains("PENDIENTE"), Is.True,
            $"Estado inicial debería ser PENDIENTE, pero es: {estadoInicial}");

        // Verificar que tiene opción de aprobar (cambiar a Activo)
        var opcionActivo = await expedientePage.OpcionEstadoDisponibleAsync("ACTIVO");
        Assert.That(opcionActivo, Is.True,
            "Debería tener opción de cambiar a ACTIVO");

        // Act - Cambiar a Activo
        await expedientePage.CambiarEstadoAsync("ACTIVO");

        // Assert - Verificar nuevo estado
        var estadoFinal = await expedientePage.ObtenerEstadoActualAsync();
        Assert.That(estadoFinal.Contains("ACTIVO"), Is.True,
            $"Estado final debería ser ACTIVO, pero es: {estadoFinal}");
    }

    /// <summary>
    /// Verifica que el Operador NO puede aprobar desde expediente
    /// (no tiene la opción ACTIVO disponible para pendientes)
    /// </summary>
    [Test]
    public async Task Operador_NoPuedeAprobar_DesdeExpediente()
    {
        // Arrange
        await _auth.LoginAsOperadorAsync();

        if (_empleadoId == null)
        {
            _empleadoId = await _empleadoHelper.BuscarEmpleadoPorCedulaAsync(_empleadoPendiente.Cedula);
        }

        var expedientePage = new ExpedientePage(Page, BaseUrl);
        await expedientePage.NavigateAsync(_empleadoId!.Value);

        // Assert - NO debería tener opción de cambiar a ACTIVO
        var opcionActivo = await expedientePage.OpcionEstadoDisponibleAsync("ACTIVO");
        Assert.That(opcionActivo, Is.False,
            "Operador NO debería tener opción de cambiar a ACTIVO");
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
