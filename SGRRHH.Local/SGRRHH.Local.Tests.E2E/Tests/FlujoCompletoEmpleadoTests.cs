using SGRRHH.Local.Tests.E2E.Helpers;
using SGRRHH.Local.Tests.E2E.PageObjects;

namespace SGRRHH.Local.Tests.E2E.Tests;

/// <summary>
/// Test E2E del flujo completo de vida de un empleado:
/// 1. Operador crea → Pendiente
/// 2. Aprobador aprueba → Activo
/// 3. Cambios de estado temporales
/// 4. Suspensión
/// 5. Admin reactiva
/// 6. Retiro (estado final)
/// </summary>
[TestFixture]
[Order(99)] // Ejecutar al final (es un test largo)
public class FlujoCompletoEmpleadoTests : PlaywrightSetup
{
    private AuthHelper _auth = null!;
    private EmpleadoHelper _empleadoHelper = null!;
    private EmpleadoTestData _empleado = null!;
    private int? _empleadoId;

    [SetUp]
    public async Task SetUp()
    {
        _auth = new AuthHelper(Page, Configuration, BaseUrl);
        _empleadoHelper = new EmpleadoHelper(Page, BaseUrl);
        _empleado = TestDataHelper.GenerarEmpleado();
    }

    /// <summary>
    /// Flujo completo desde creación hasta retiro
    /// </summary>
    [Test]
    public async Task FlujoCompleto_CreacionHastaRetiro()
    {
        var empleadosPage = new EmpleadosPage(Page, BaseUrl);
        var expedientePage = new ExpedientePage(Page, BaseUrl);

        // ========== PASO 1: Secretaria crea empleado ==========
        TestContext.WriteLine("=== PASO 1: Secretaria crea empleado ===");
        await _auth.LoginAsOperadorAsync();
        _empleadoId = await _empleadoHelper.CrearEmpleadoCompletoAsync(_empleado);
        TestContext.WriteLine($"Empleado creado: {_empleado.NombreCompleto} ({_empleado.Cedula})");

        // Verificar estado pendiente
        await empleadosPage.NavigateAsync();
        await empleadosPage.FiltrarPorEstadoAsync("PENDIENTES");
        var existeEnPendientes = await empleadosPage.EmpleadoExisteEnListadoAsync(_empleado.Cedula);
        Assert.That(existeEnPendientes, Is.True, "PASO 1: Empleado debería estar en PENDIENTES");

        // Secretaria NO ve botón aprobar
        var botonAprobarSecretaria = await empleadosPage.BotonAprobarVisibleAsync(_empleado.Cedula);
        Assert.That(botonAprobarSecretaria, Is.False, "PASO 1: Secretaria NO ve botón APROBAR");

        await _auth.LogoutAsync();
        TestContext.WriteLine("PASO 1 completado ✓");

        // ========== PASO 2: Ingeniera aprueba ==========
        TestContext.WriteLine("=== PASO 2: Ingeniera aprueba ===");
        await _auth.LoginAsAprobadorAsync();
        await empleadosPage.NavigateAsync();
        await empleadosPage.FiltrarPorEstadoAsync("PENDIENTES");

        // Ingeniera SÍ ve botón aprobar
        var botonAprobarIngeniera = await empleadosPage.BotonAprobarVisibleAsync(_empleado.Cedula);
        Assert.That(botonAprobarIngeniera, Is.True, "PASO 2: Ingeniera SÍ ve botón APROBAR");

        // Aprobar
        await empleadosPage.AprobarEmpleadoAsync(_empleado.Cedula);

        // Verificar estado activo
        await empleadosPage.FiltrarPorEstadoAsync("ACTIVOS");
        var existeEnActivos = await empleadosPage.EmpleadoExisteEnListadoAsync(_empleado.Cedula);
        Assert.That(existeEnActivos, Is.True, "PASO 2: Empleado debería estar en ACTIVOS");

        TestContext.WriteLine("PASO 2 completado ✓");

        // ========== PASO 3: Cambio a vacaciones ==========
        TestContext.WriteLine("=== PASO 3: Cambio a vacaciones ===");
        if (_empleadoId == null)
        {
            _empleadoId = await _empleadoHelper.BuscarEmpleadoPorCedulaAsync(_empleado.Cedula);
        }
        await expedientePage.NavigateAsync(_empleadoId!.Value);
        await expedientePage.CambiarEstadoAsync("EN VACACIONES");

        var estadoVacaciones = await expedientePage.ObtenerEstadoActualAsync();
        Assert.That(estadoVacaciones.Contains("VACACIONES"), Is.True, "PASO 3: Estado EN VACACIONES");

        TestContext.WriteLine("PASO 3 completado ✓");

        // ========== PASO 4: Retorno a activo ==========
        TestContext.WriteLine("=== PASO 4: Retorno a activo ===");
        await expedientePage.CambiarEstadoAsync("ACTIVO");

        var estadoActivoRetorno = await expedientePage.ObtenerEstadoActualAsync();
        Assert.That(estadoActivoRetorno.Contains("ACTIVO"), Is.True, "PASO 4: Estado ACTIVO");

        TestContext.WriteLine("PASO 4 completado ✓");

        // ========== PASO 5: Suspensión ==========
        TestContext.WriteLine("=== PASO 5: Suspensión ===");
        await expedientePage.CambiarEstadoAsync("SUSPENDIDO");

        var estadoSuspendido = await expedientePage.ObtenerEstadoActualAsync();
        Assert.That(estadoSuspendido.Contains("SUSPENDIDO"), Is.True, "PASO 5: Estado SUSPENDIDO");

        TestContext.WriteLine("PASO 5 completado ✓");

        // ========== PASO 6: Ingeniera intenta reactivar (NO puede) ==========
        TestContext.WriteLine("=== PASO 6: Ingeniera intenta reactivar ===");
        var opcionActivoIngeniera = await expedientePage.OpcionEstadoDisponibleAsync("ACTIVO");
        Assert.That(opcionActivoIngeniera, Is.False, "PASO 6: Ingeniera NO puede reactivar");

        await _auth.LogoutAsync();
        TestContext.WriteLine("PASO 6 completado ✓");

        // ========== PASO 7: Admin reactiva ==========
        TestContext.WriteLine("=== PASO 7: Admin reactiva ===");
        await _auth.LoginAsAdminAsync();
        await expedientePage.NavigateAsync(_empleadoId!.Value);

        var opcionActivoAdmin = await expedientePage.OpcionEstadoDisponibleAsync("ACTIVO");
        Assert.That(opcionActivoAdmin, Is.True, "PASO 7: Admin SÍ puede reactivar");

        await expedientePage.CambiarEstadoAsync("ACTIVO");

        var estadoReactivado = await expedientePage.ObtenerEstadoActualAsync();
        Assert.That(estadoReactivado.Contains("ACTIVO"), Is.True, "PASO 7: Estado ACTIVO reactivado");

        TestContext.WriteLine("PASO 7 completado ✓");

        // ========== PASO 8: Retiro (estado final) ==========
        TestContext.WriteLine("=== PASO 8: Retiro ===");
        await expedientePage.CambiarEstadoAsync("RETIRADO");

        var estadoRetirado = await expedientePage.ObtenerEstadoActualAsync();
        Assert.That(estadoRetirado.Contains("RETIRADO"), Is.True, "PASO 8: Estado RETIRADO");

        // Verificar que es estado final
        var esEstadoFinal = await expedientePage.EsEstadoFinalAsync();
        Assert.That(esEstadoFinal, Is.True, "PASO 8: RETIRADO es estado final");

        TestContext.WriteLine("PASO 8 completado ✓");

        // ========== RESUMEN ==========
        TestContext.WriteLine("========================================");
        TestContext.WriteLine("FLUJO COMPLETO EXITOSO ✓");
        TestContext.WriteLine($"Empleado: {_empleado.NombreCompleto}");
        TestContext.WriteLine($"Cédula: {_empleado.Cedula}");
        TestContext.WriteLine("Estados recorridos:");
        TestContext.WriteLine("  1. PendienteAprobación (creado por Operador)");
        TestContext.WriteLine("  2. Activo (aprobado por Aprobador)");
        TestContext.WriteLine("  3. EnVacaciones");
        TestContext.WriteLine("  4. Activo (retorno)");
        TestContext.WriteLine("  5. Suspendido");
        TestContext.WriteLine("  6. Activo (reactivado por Admin)");
        TestContext.WriteLine("  7. Retirado (FINAL)");
        TestContext.WriteLine("========================================");
    }

    [TearDown]
    public async Task TearDown()
    {
        try { await _auth.LogoutAsync(); } catch { }
    }
}
