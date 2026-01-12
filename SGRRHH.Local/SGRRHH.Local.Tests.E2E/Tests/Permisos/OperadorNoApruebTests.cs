using SGRRHH.Local.Tests.E2E.Helpers;
using SGRRHH.Local.Tests.E2E.PageObjects;

namespace SGRRHH.Local.Tests.E2E.Tests.Permisos;

/// <summary>
/// Tests para verificar que Operador NO puede aprobar empleados
/// </summary>
[TestFixture]
public class OperadorNoApruebTests : PlaywrightSetup
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

        // Crear empleado pendiente
        await _auth.LoginAsOperadorAsync();
        _empleadoPendiente = TestDataHelper.GenerarEmpleado();
        _empleadoId = await _empleadoHelper.CrearEmpleadoCompletoAsync(_empleadoPendiente);
        await _auth.LogoutAsync();
    }

    /// <summary>
    /// Operador no ve botón APROBAR en listado
    /// </summary>
    [Test]
    public async Task Operador_NoVeBotonAprobar_EnListado()
    {
        await _auth.LoginAsOperadorAsync();
        var empleadosPage = new EmpleadosPage(Page, BaseUrl);
        await empleadosPage.NavigateAsync();
        await empleadosPage.FiltrarPorEstadoAsync("PENDIENTES");

        var botonVisible = await empleadosPage.BotonAprobarVisibleAsync(_empleadoPendiente.Cedula);
        Assert.That(botonVisible, Is.False,
            "Operador NO debe ver botón APROBAR");
    }

    /// <summary>
    /// Operador no tiene opción ACTIVO para empleados pendientes
    /// </summary>
    [Test]
    public async Task Operador_NoTieneOpcionActivo_ParaPendientes()
    {
        await _auth.LoginAsOperadorAsync();
        var expedientePage = new ExpedientePage(Page, BaseUrl);

        if (_empleadoId == null)
        {
            _empleadoId = await _empleadoHelper.BuscarEmpleadoPorCedulaAsync(_empleadoPendiente.Cedula);
        }
        await expedientePage.NavigateAsync(_empleadoId!.Value);

        var opcionActivo = await expedientePage.OpcionEstadoDisponibleAsync("ACTIVO");
        Assert.That(opcionActivo, Is.False,
            "Operador NO debe tener opción ACTIVO para aprobar");
    }

    /// <summary>
    /// Operador no tiene opción RECHAZADO
    /// </summary>
    [Test]
    public async Task Operador_NoTieneOpcionRechazado()
    {
        await _auth.LoginAsOperadorAsync();
        var expedientePage = new ExpedientePage(Page, BaseUrl);

        if (_empleadoId == null)
        {
            _empleadoId = await _empleadoHelper.BuscarEmpleadoPorCedulaAsync(_empleadoPendiente.Cedula);
        }
        await expedientePage.NavigateAsync(_empleadoId!.Value);

        var opcionRechazado = await expedientePage.OpcionEstadoDisponibleAsync("RECHAZADO");
        Assert.That(opcionRechazado, Is.False,
            "Operador NO debe tener opción RECHAZADO");
    }

    [TearDown]
    public async Task TearDown()
    {
        try { await _auth.LogoutAsync(); } catch { }
    }
}
