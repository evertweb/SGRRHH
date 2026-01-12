using Microsoft.Playwright;
using SGRRHH.Local.Tests.E2E.PageObjects;

namespace SGRRHH.Local.Tests.E2E.Helpers;

/// <summary>
/// Helper para crear empleados a través del wizard de onboarding
/// El wizard ahora tiene 2 pasos: 1) Datos Básicos, 2) Revisar y Confirmar
/// </summary>
public class EmpleadoHelper
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public EmpleadoHelper(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl;
    }

    /// <summary>
    /// Crea un empleado completo a través del wizard de onboarding
    /// Retorna el ID del empleado creado si es posible obtenerlo
    /// </summary>
    public async Task<int?> CrearEmpleadoCompletoAsync(EmpleadoTestData datos)
    {
        var onboardingPage = new OnboardingPage(_page, _baseUrl);
        await onboardingPage.NavigateAsync();

        // El wizard ahora tiene solo 2 pasos:
        // Paso 1: Todos los datos básicos (datos personales, laboral, seguridad social, contacto, emergencia)
        // Paso 2: Revisar y confirmar
        
        await onboardingPage.CompletarPaso1Async(
            datos.Cedula,
            datos.Nombre,
            datos.Apellido,
            datos.FechaNacimiento
        );

        // Los siguientes métodos están mantenidos por compatibilidad pero no hacen nada
        await onboardingPage.CompletarPaso2Async(datos.Email, datos.Telefono);
        await onboardingPage.CompletarPaso3Async();
        await onboardingPage.CompletarPaso4Async();

        // Finalizar (Guardar)
        await onboardingPage.FinalizarAsync();

        // Intentar obtener el ID del empleado creado desde la URL
        return await ObtenerIdEmpleadoCreadoAsync();
    }

    /// <summary>
    /// Crea un empleado con datos mínimos (usa valores por defecto donde sea posible)
    /// </summary>
    public async Task<int?> CrearEmpleadoRapidoAsync()
    {
        var datos = TestDataHelper.GenerarEmpleado();
        return await CrearEmpleadoCompletoAsync(datos);
    }

    /// <summary>
    /// Intenta obtener el ID del empleado desde la URL después de creación
    /// Después de guardar, el sistema redirige a /documentos/{id}
    /// </summary>
    private async Task<int?> ObtenerIdEmpleadoCreadoAsync()
    {
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(500); // Esperar redirección
        
        var url = _page.Url;
        
        // Después de guardar, redirige a /documentos/{id}
        var matchDocumentos = System.Text.RegularExpressions.Regex.Match(url, @"/documentos/(\d+)");
        if (matchDocumentos.Success && int.TryParse(matchDocumentos.Groups[1].Value, out int idDoc))
        {
            return idDoc;
        }
        
        // También intentar extraer ID de patrones como /empleados/123/expediente o /empleados/123
        var matchEmpleados = System.Text.RegularExpressions.Regex.Match(url, @"/empleados/(\d+)");
        if (matchEmpleados.Success && int.TryParse(matchEmpleados.Groups[1].Value, out int id))
        {
            return id;
        }

        return null;
    }

    /// <summary>
    /// Navega al expediente de un empleado por su ID
    /// </summary>
    public async Task NavegarAExpedienteAsync(int empleadoId)
    {
        await _page.GotoAsync($"{_baseUrl}/empleados/{empleadoId}/expediente", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });
    }

    /// <summary>
    /// Busca un empleado por cédula en el listado y retorna su ID
    /// </summary>
    public async Task<int?> BuscarEmpleadoPorCedulaAsync(string cedula)
    {
        var empleadosPage = new EmpleadosPage(_page, _baseUrl);
        await empleadosPage.NavigateAsync();
        
        // Buscar en el listado
        await empleadosPage.BuscarAsync(cedula);
        
        // Intentar obtener el ID desde el link del botón EDITAR
        var filaEmpleado = _page.Locator($"table.data-table tbody tr:has-text('{cedula}')").First;
        if (await filaEmpleado.CountAsync() > 0)
        {
            // Click en editar y extraer ID de la URL
            var botonEditar = filaEmpleado.Locator("button.btn-table:has-text('EDITAR')");
            if (await botonEditar.CountAsync() > 0)
            {
                await botonEditar.ClickAsync();
                await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                
                var url = _page.Url;
                var match = System.Text.RegularExpressions.Regex.Match(url, @"/empleados/(\d+)");
                if (match.Success && int.TryParse(match.Groups[1].Value, out int id))
                {
                    return id;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Obtiene el estado actual de un empleado por su cédula
    /// </summary>
    public async Task<string> ObtenerEstadoEmpleadoAsync(string cedula)
    {
        var empleadosPage = new EmpleadosPage(_page, _baseUrl);
        await empleadosPage.NavigateAsync();
        return await empleadosPage.ObtenerEstadoEmpleadoAsync(cedula);
    }
}
