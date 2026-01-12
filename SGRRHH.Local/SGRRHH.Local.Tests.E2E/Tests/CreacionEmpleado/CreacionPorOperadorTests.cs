using SGRRHH.Local.Tests.E2E.Helpers;
using SGRRHH.Local.Tests.E2E.PageObjects;

namespace SGRRHH.Local.Tests.E2E.Tests.CreacionEmpleado;

/// <summary>
/// Tests para verificar que un Operador (secretaria) crea empleados 
/// con estado PendienteAprobacion
/// </summary>
[TestFixture]
public class CreacionPorOperadorTests : PlaywrightSetup
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
    /// Verifica que cuando un Operador crea un empleado,
    /// el estado inicial es PendienteAprobacion
    /// </summary>
    [Test]
    public async Task Operador_CreaEmpleado_EstadoEsPendienteAprobacion()
    {
        // Arrange
        await _auth.LoginAsOperadorAsync();
        var datosEmpleado = TestDataHelper.GenerarEmpleado();

        // Act - Crear empleado
        var empleadoId = await _empleadoHelper.CrearEmpleadoCompletoAsync(datosEmpleado);

        // Assert - Verificar mensaje o estado de pendiente
        var onboardingPage = new OnboardingPage(Page, BaseUrl);
        var mensajeExito = await onboardingPage.ObtenerMensajeExitoAsync();
        
        // El mensaje debería indicar pendiente de aprobación
        Assert.That(
            mensajeExito.ToUpperInvariant().Contains("PENDIENTE") || 
            Page.Url.Contains("expediente"),
            Is.True,
            "Debería mostrar mensaje de pendiente o redirigir al expediente"
        );

        // Navegar al listado y verificar estado
        var empleadosPage = new EmpleadosPage(Page, BaseUrl);
        await empleadosPage.NavigateAsync();
        await empleadosPage.FiltrarPorEstadoAsync("PENDIENTES");

        // Verificar que el empleado aparece en pendientes
        var existeEnListado = await empleadosPage.EmpleadoExisteEnListadoAsync(datosEmpleado.Cedula);
        Assert.That(existeEnListado, Is.True, 
            $"El empleado {datosEmpleado.Cedula} debería aparecer en el listado de pendientes");
    }

    /// <summary>
    /// Verifica que el Operador NO ve el botón APROBAR en sus propios empleados pendientes
    /// (porque no tiene permiso para aprobar)
    /// </summary>
    [Test]
    public async Task Operador_NoVeBotonAprobar_EnEmpleadosPendientes()
    {
        // Arrange - Login y crear empleado
        await _auth.LoginAsOperadorAsync();
        var datosEmpleado = TestDataHelper.GenerarEmpleado();
        await _empleadoHelper.CrearEmpleadoCompletoAsync(datosEmpleado);

        // Act - Ir al listado
        var empleadosPage = new EmpleadosPage(Page, BaseUrl);
        await empleadosPage.NavigateAsync();
        await empleadosPage.FiltrarPorEstadoAsync("PENDIENTES");

        // Assert - Verificar que NO hay botón APROBAR visible
        var botonAprobarVisible = await empleadosPage.BotonAprobarVisibleAsync(datosEmpleado.Cedula);
        Assert.That(botonAprobarVisible, Is.False, 
            "El Operador NO debería ver el botón APROBAR");
    }

    /// <summary>
    /// Verifica que en el wizard, el campo de estado es de solo lectura para Operadores
    /// y muestra "PENDIENTE DE APROBACIÓN"
    /// </summary>
    [Test]
    public async Task Operador_NoVeSelectorEstadoEnCreacion()
    {
        // Arrange
        await _auth.LoginAsOperadorAsync();
        var onboardingPage = new OnboardingPage(Page, BaseUrl);
        var datosEmpleado = TestDataHelper.GenerarEmpleado();

        // Act - Navegar al wizard y completar hasta paso laboral
        await onboardingPage.NavigateAsync();
        await onboardingPage.CompletarPaso1Async(
            datosEmpleado.Cedula,
            datosEmpleado.Nombre,
            datosEmpleado.Apellido,
            datosEmpleado.FechaNacimiento
        );
        await onboardingPage.CompletarPaso2Async(
            datosEmpleado.Email,
            datosEmpleado.Telefono
        );

        // Assert - Verificar que el campo estado es readonly o muestra texto fijo
        var esReadOnly = await onboardingPage.CampoEstadoEsReadOnlyAsync();
        var estadoMostrado = await onboardingPage.ObtenerEstadoMostradoAsync();

        // Al menos una de estas condiciones debería cumplirse
        var campoNoEditable = esReadOnly || estadoMostrado.ToUpperInvariant().Contains("PENDIENTE");
        
        Assert.That(campoNoEditable, Is.True,
            "El campo estado debería ser readonly o mostrar 'PENDIENTE DE APROBACIÓN'");
    }

    [TearDown]
    public async Task TearDown()
    {
        // Logout al final de cada test
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
