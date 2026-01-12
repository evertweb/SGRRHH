using SGRRHH.Local.Tests.E2E.Helpers;
using SGRRHH.Local.Tests.E2E.PageObjects;

namespace SGRRHH.Local.Tests.E2E.Tests.CreacionEmpleado;

/// <summary>
/// Tests para verificar que Aprobadores y Admins crean empleados
/// con estado Activo directamente
/// </summary>
[TestFixture]
public class CreacionPorAprobadorTests : PlaywrightSetup
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
    /// Verifica que cuando un Aprobador (ingeniera) crea un empleado,
    /// el estado inicial es Activo
    /// </summary>
    [Test]
    public async Task Aprobador_CreaEmpleado_EstadoEsActivo()
    {
        // Arrange
        await _auth.LoginAsAprobadorAsync();
        var datosEmpleado = TestDataHelper.GenerarEmpleado();

        // Act - Crear empleado
        var empleadoId = await _empleadoHelper.CrearEmpleadoCompletoAsync(datosEmpleado);

        // Assert - Navegar al listado y verificar estado ACTIVO
        var empleadosPage = new EmpleadosPage(Page, BaseUrl);
        await empleadosPage.NavigateAsync();

        // El empleado debería aparecer en el listado
        var existeEnListado = await empleadosPage.EmpleadoExisteEnListadoAsync(datosEmpleado.Cedula);
        Assert.That(existeEnListado, Is.True,
            $"El empleado {datosEmpleado.Cedula} debería aparecer en el listado");

        // Verificar estado ACTIVO
        var estado = await empleadosPage.ObtenerEstadoEmpleadoAsync(datosEmpleado.Cedula);
        Assert.That(estado.ToUpperInvariant().Contains("ACTIVO"), Is.True,
            $"El estado debería ser ACTIVO, pero es: {estado}");
    }

    /// <summary>
    /// Verifica que cuando un Admin crea un empleado,
    /// el estado inicial es Activo
    /// </summary>
    [Test]
    public async Task Admin_CreaEmpleado_EstadoEsActivo()
    {
        // Arrange
        await _auth.LoginAsAdminAsync();
        var datosEmpleado = TestDataHelper.GenerarEmpleado();

        // Act - Crear empleado
        var empleadoId = await _empleadoHelper.CrearEmpleadoCompletoAsync(datosEmpleado);

        // Assert - Verificar estado ACTIVO
        var empleadosPage = new EmpleadosPage(Page, BaseUrl);
        await empleadosPage.NavigateAsync();

        var estado = await empleadosPage.ObtenerEstadoEmpleadoAsync(datosEmpleado.Cedula);
        Assert.That(estado.ToUpperInvariant().Contains("ACTIVO"), Is.True,
            $"El estado debería ser ACTIVO, pero es: {estado}");
    }

    /// <summary>
    /// Verifica que el Aprobador no ve el botón APROBAR en empleados que ya están activos
    /// (porque ya no necesitan aprobación)
    /// </summary>
    [Test]
    public async Task Aprobador_NoVeBotonAprobar_EnEmpleadosActivos()
    {
        // Arrange - Crear empleado como aprobador (estará activo)
        await _auth.LoginAsAprobadorAsync();
        var datosEmpleado = TestDataHelper.GenerarEmpleado();
        await _empleadoHelper.CrearEmpleadoCompletoAsync(datosEmpleado);

        // Act - Ir al listado
        var empleadosPage = new EmpleadosPage(Page, BaseUrl);
        await empleadosPage.NavigateAsync();

        // Assert - No debería haber botón APROBAR para empleados activos
        var botonAprobarVisible = await empleadosPage.BotonAprobarVisibleAsync(datosEmpleado.Cedula);
        Assert.That(botonAprobarVisible, Is.False,
            "No debería haber botón APROBAR para empleados ya activos");
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
            // Ignorar errores de logout
        }
    }
}
