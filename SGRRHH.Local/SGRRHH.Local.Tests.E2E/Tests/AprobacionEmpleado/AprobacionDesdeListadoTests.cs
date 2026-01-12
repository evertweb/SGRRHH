using SGRRHH.Local.Tests.E2E.Helpers;
using SGRRHH.Local.Tests.E2E.PageObjects;

namespace SGRRHH.Local.Tests.E2E.Tests.AprobacionEmpleado;

/// <summary>
/// Tests para verificar el flujo de aprobación desde el listado de empleados
/// </summary>
[TestFixture]
public class AprobacionDesdeListadoTests : PlaywrightSetup
{
    private AuthHelper _auth = null!;
    private EmpleadoHelper _empleadoHelper = null!;
    private EmpleadoTestData _empleadoPendiente = null!;

    [SetUp]
    public async Task SetUp()
    {
        _auth = new AuthHelper(Page, Configuration, BaseUrl);
        _empleadoHelper = new EmpleadoHelper(Page, BaseUrl);
        
        // Pre-condición: Crear un empleado pendiente con secretaria
        await _auth.LoginAsOperadorAsync();
        _empleadoPendiente = TestDataHelper.GenerarEmpleado();
        await _empleadoHelper.CrearEmpleadoCompletoAsync(_empleadoPendiente);
        await _auth.LogoutAsync();
    }

    /// <summary>
    /// Verifica que el Aprobador ve el botón APROBAR en empleados pendientes
    /// </summary>
    [Test]
    public async Task Aprobador_VeBotonAprobar_EnEmpleadosPendientes()
    {
        // Arrange
        await _auth.LoginAsAprobadorAsync();

        // Act
        var empleadosPage = new EmpleadosPage(Page, BaseUrl);
        await empleadosPage.NavigateAsync();
        await empleadosPage.FiltrarPorEstadoAsync("PENDIENTES");

        // Assert
        var botonVisible = await empleadosPage.BotonAprobarVisibleAsync(_empleadoPendiente.Cedula);
        Assert.That(botonVisible, Is.True,
            "El Aprobador debería ver el botón APROBAR en empleados pendientes");
    }

    /// <summary>
    /// Verifica que al aprobar desde listado, el empleado pasa a estado Activo
    /// </summary>
    [Test]
    public async Task Aprobador_ApruebaDesdeLlistado_EmpleadoPasaAActivo()
    {
        // Arrange
        await _auth.LoginAsAprobadorAsync();
        var empleadosPage = new EmpleadosPage(Page, BaseUrl);
        await empleadosPage.NavigateAsync();
        await empleadosPage.FiltrarPorEstadoAsync("PENDIENTES");

        // Verificar que existe antes
        Assert.That(
            await empleadosPage.EmpleadoExisteEnListadoAsync(_empleadoPendiente.Cedula),
            Is.True,
            "Pre-condición: El empleado debería existir en pendientes"
        );

        // Act - Aprobar
        await empleadosPage.AprobarEmpleadoAsync(_empleadoPendiente.Cedula);

        // Assert - Verificar que ya no está en pendientes
        await empleadosPage.FiltrarPorEstadoAsync("PENDIENTES");
        var sigueEnPendientes = await empleadosPage.EmpleadoExisteEnListadoAsync(_empleadoPendiente.Cedula);
        Assert.That(sigueEnPendientes, Is.False,
            "El empleado ya no debería estar en pendientes después de aprobar");

        // Verificar que ahora está activo
        await empleadosPage.FiltrarPorEstadoAsync("ACTIVOS");
        var estaEnActivos = await empleadosPage.EmpleadoExisteEnListadoAsync(_empleadoPendiente.Cedula);
        Assert.That(estaEnActivos, Is.True,
            "El empleado debería aparecer en activos después de aprobar");
    }

    /// <summary>
    /// Verifica que el botón APROBAR desaparece después de aprobar
    /// </summary>
    [Test]
    public async Task BotonAprobar_DesapareceDespuesDeAprobar()
    {
        // Arrange
        await _auth.LoginAsAprobadorAsync();
        var empleadosPage = new EmpleadosPage(Page, BaseUrl);
        await empleadosPage.NavigateAsync();
        await empleadosPage.FiltrarPorEstadoAsync("PENDIENTES");

        // Act
        await empleadosPage.AprobarEmpleadoAsync(_empleadoPendiente.Cedula);

        // Navegar de nuevo al listado (sin filtro para ver el empleado)
        await empleadosPage.NavigateAsync();

        // Assert
        var botonVisible = await empleadosPage.BotonAprobarVisibleAsync(_empleadoPendiente.Cedula);
        Assert.That(botonVisible, Is.False,
            "El botón APROBAR debería desaparecer después de aprobar");
    }

    /// <summary>
    /// Verifica que el Operador NO ve el botón APROBAR
    /// </summary>
    [Test]
    public async Task Operador_NoVeBotonAprobar()
    {
        // Arrange
        await _auth.LoginAsOperadorAsync();

        // Act
        var empleadosPage = new EmpleadosPage(Page, BaseUrl);
        await empleadosPage.NavigateAsync();
        await empleadosPage.FiltrarPorEstadoAsync("PENDIENTES");

        // Assert
        var botonVisible = await empleadosPage.BotonAprobarVisibleAsync(_empleadoPendiente.Cedula);
        Assert.That(botonVisible, Is.False,
            "El Operador NO debería ver el botón APROBAR");
    }

    /// <summary>
    /// Verifica que el Admin también puede aprobar
    /// </summary>
    [Test]
    public async Task Admin_PuedeAprobarDesdeLlistado()
    {
        // Arrange
        await _auth.LoginAsAdminAsync();
        var empleadosPage = new EmpleadosPage(Page, BaseUrl);
        await empleadosPage.NavigateAsync();
        await empleadosPage.FiltrarPorEstadoAsync("PENDIENTES");

        // Assert - Admin ve botón aprobar
        var botonVisible = await empleadosPage.BotonAprobarVisibleAsync(_empleadoPendiente.Cedula);
        Assert.That(botonVisible, Is.True,
            "El Admin debería ver el botón APROBAR");

        // Act - Aprobar
        await empleadosPage.AprobarEmpleadoAsync(_empleadoPendiente.Cedula);

        // Assert - Ya no está en pendientes
        await empleadosPage.FiltrarPorEstadoAsync("ACTIVOS");
        var estaEnActivos = await empleadosPage.EmpleadoExisteEnListadoAsync(_empleadoPendiente.Cedula);
        Assert.That(estaEnActivos, Is.True,
            "El empleado debería estar activo después de que Admin apruebe");
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
            // Ignorar errores
        }
    }
}
