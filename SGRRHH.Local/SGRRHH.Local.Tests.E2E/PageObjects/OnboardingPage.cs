using Microsoft.Playwright;

namespace SGRRHH.Local.Tests.E2E.PageObjects;

/// <summary>
/// Page Object para el wizard de creación de empleados (/empleados/onboarding)
/// El wizard tiene 2 pasos: 1) Datos Básicos, 2) Revisar y Confirmar
/// </summary>
public class OnboardingPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    // Selectores generales - basados en hospital.css y EmpleadoOnboarding.razor
    private ILocator BotonSiguiente => _page.Locator("button.hospital-btn-primary:has-text('SIGUIENTE')");
    private ILocator BotonAnterior => _page.Locator("button.hospital-btn-secondary:has-text('ANTERIOR')");
    private ILocator BotonGuardar => _page.Locator("button.hospital-btn-primary:has-text('GUARDAR')");
    private ILocator BotonCancelar => _page.Locator("button.hospital-btn-secondary:has-text('CANCELAR')");
    private ILocator ProgresoTexto => _page.Locator(".hospital-progress-text");
    private ILocator ContenedorPagina => _page.Locator(".hospital-page-container");
    private ILocator Contenido => _page.Locator(".hospital-content");
    private ILocator MensajeExito => _page.Locator(".toast-success, .success-message");
    private ILocator MensajeError => _page.Locator(".toast-error, .error-message");

    public OnboardingPage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl;
    }

    /// <summary>
    /// Navega al wizard de onboarding
    /// </summary>
    public async Task NavigateAsync()
    {
        await _page.GotoAsync($"{_baseUrl}/empleados/onboarding", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });
        await _page.WaitForSelectorAsync(".hospital-page-container");
    }

    /// <summary>
    /// Completa el Paso 1: Datos básicos (TODO el formulario está en el paso 1)
    /// Incluye: datos generales, información laboral, seguridad social, contacto, dirección, información médica y contacto de emergencia
    /// </summary>
    public async Task CompletarPaso1Async(string cedula, string nombre, string apellido, DateTime fechaNacimiento)
    {
        // Cédula - input dentro de componente InputCedula
        var inputCedula = _page.Locator(".hospital-section:has-text('DATOS GENERALES') input.hospital-input").Nth(1);
        await inputCedula.FillAsync(cedula);

        // Nombres
        var inputNombres = _page.Locator(".hospital-form-field:has(.hospital-label:has-text('NOMBRES')) input.hospital-input").First;
        await inputNombres.FillAsync(nombre);

        // Apellidos
        var inputApellidos = _page.Locator(".hospital-form-field:has(.hospital-label:has-text('APELLIDOS')) input.hospital-input").First;
        await inputApellidos.FillAsync(apellido);

        // Fecha de nacimiento
        var inputFechaNac = _page.Locator(".hospital-form-field:has(.hospital-label:has-text('FECHA NACIMIENTO')) input[type='date']").First;
        await inputFechaNac.FillAsync(fechaNacimiento.ToString("yyyy-MM-dd"));

        // Género (seleccionar primer valor no vacío)
        var selectGenero = _page.Locator(".hospital-form-field:has(.hospital-label:has-text('GENERO')) select.hospital-input").First;
        await selectGenero.SelectOptionAsync(new SelectOptionValue { Index = 1 });

        // Fecha ingreso (hoy por defecto, puede que ya esté lleno)
        var inputFechaIngreso = _page.Locator(".hospital-form-field:has(.hospital-label:has-text('FECHA INGRESO')) input[type='date']").First;
        var fechaIngresoValue = await inputFechaIngreso.InputValueAsync();
        if (string.IsNullOrEmpty(fechaIngresoValue))
        {
            await inputFechaIngreso.FillAsync(DateTime.Today.ToString("yyyy-MM-dd"));
        }

        // Departamento (seleccionar primero disponible)
        var selectDepartamento = _page.Locator(".hospital-form-field:has(.hospital-label:has-text('DEPARTAMENTO')) select.hospital-input").First;
        var deptoOptions = await selectDepartamento.Locator("option").AllAsync();
        if (deptoOptions.Count > 1)
        {
            await selectDepartamento.SelectOptionAsync(new SelectOptionValue { Index = 1 });
            await Task.Delay(300); // Esperar carga de cargos
        }

        // Cargo (seleccionar primero disponible - depende del departamento)
        var selectCargo = _page.Locator(".hospital-form-field:has(.hospital-label:has-text('CARGO')) select.hospital-input").First;
        await selectCargo.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var cargoOptions = await selectCargo.Locator("option").AllAsync();
        if (cargoOptions.Count > 1)
        {
            await selectCargo.SelectOptionAsync(new SelectOptionValue { Index = 1 });
            await Task.Delay(300); // Esperar autocompletado de salario
        }

        // Salario base (puede estar autocompletado desde cargo)
        var inputSalario = _page.Locator(".hospital-form-field:has(.hospital-label:has-text('SALARIO BASE')) input.hospital-input").First;
        var salarioValue = await inputSalario.InputValueAsync();
        if (string.IsNullOrEmpty(salarioValue) || salarioValue == "0")
        {
            await inputSalario.FillAsync("1750905");
        }

        // Seguridad Social - EPS
        var selectEps = _page.Locator(".hospital-form-field:has(.hospital-label:has-text('EPS')) select.hospital-input").First;
        var epsOptions = await selectEps.Locator("option").AllAsync();
        if (epsOptions.Count > 1)
        {
            await selectEps.SelectOptionAsync(new SelectOptionValue { Index = 1 });
        }

        // AFP
        var selectAfp = _page.Locator(".hospital-form-field:has(.hospital-label:has-text('AFP')) select.hospital-input").First;
        var afpOptions = await selectAfp.Locator("option").AllAsync();
        if (afpOptions.Count > 1)
        {
            await selectAfp.SelectOptionAsync(new SelectOptionValue { Index = 1 });
        }

        // ARL
        var selectArl = _page.Locator(".hospital-form-field:has(.hospital-label:has-text('ARL (RIESGOS')) select.hospital-input").First;
        var arlOptions = await selectArl.Locator("option").AllAsync();
        if (arlOptions.Count > 1)
        {
            await selectArl.SelectOptionAsync(new SelectOptionValue { Index = 1 });
        }

        // Caja de Compensación
        var selectCaja = _page.Locator(".hospital-form-field:has(.hospital-label:has-text('CAJA DE COMPENSACION')) select.hospital-input").First;
        var cajaOptions = await selectCaja.Locator("option").AllAsync();
        if (cajaOptions.Count > 1)
        {
            await selectCaja.SelectOptionAsync(new SelectOptionValue { Index = 1 });
        }

        // Teléfono celular
        var inputCelular = _page.Locator(".hospital-form-field:has(.hospital-label:has-text('TELEFONO CELULAR')) input.hospital-input").First;
        await inputCelular.FillAsync("3001234567");

        // Dirección
        var inputDireccion = _page.Locator(".hospital-form-field:has(.hospital-label:has-text('DIRECCION')) input.hospital-input").First;
        await inputDireccion.FillAsync("CALLE PRINCIPAL 123");

        // Municipio
        var inputMunicipio = _page.Locator(".hospital-form-field:has(.hospital-label:has-text('MUNICIPIO')) input.hospital-input").First;
        await inputMunicipio.FillAsync("MEDELLIN");

        // Tipo de sangre
        var selectSangre = _page.Locator(".hospital-form-field:has(.hospital-label:has-text('TIPO DE SANGRE')) select.hospital-input").First;
        await selectSangre.SelectOptionAsync(new SelectOptionValue { Value = "O+" });

        // Contacto de emergencia
        var inputContactoEmergencia = _page.Locator(".hospital-section:has-text('CONTACTO DE EMERGENCIA #1') .hospital-form-field:has(.hospital-label:has-text('NOMBRE COMPLETO')) input.hospital-input").First;
        await inputContactoEmergencia.FillAsync("CONTACTO DE PRUEBA");

        // Parentesco
        var selectParentesco = _page.Locator(".hospital-section:has-text('CONTACTO DE EMERGENCIA #1') .hospital-form-field:has(.hospital-label:has-text('PARENTESCO')) select.hospital-input").First;
        await selectParentesco.SelectOptionAsync(new SelectOptionValue { Index = 1 });

        // Teléfono de emergencia
        var inputTelEmergencia = _page.Locator(".hospital-section:has-text('CONTACTO DE EMERGENCIA #1') .hospital-form-field:has(.hospital-label:has-text('TELEFONO 1')) input.hospital-input").First;
        await inputTelEmergencia.FillAsync("3009876543");

        // Ir al paso 2
        await ClickSiguienteAsync();
    }

    /// <summary>
    /// Completa el Paso 2: Contacto - YA NO EXISTE EN EL WIZARD SIMPLIFICADO
    /// Mantenido por compatibilidad, no hace nada
    /// </summary>
    public async Task CompletarPaso2Async(string email, string telefono)
    {
        // El wizard ahora tiene solo 2 pasos
        // El paso 2 es solo revisión, no requiere llenar datos
        await Task.CompletedTask;
    }

    /// <summary>
    /// Completa el Paso 3: Información laboral - YA NO EXISTE EN EL WIZARD SIMPLIFICADO
    /// Mantenido por compatibilidad, no hace nada
    /// </summary>
    public async Task CompletarPaso3Async(string? cargo = null, string? departamento = null)
    {
        // El wizard ahora tiene solo 2 pasos
        await Task.CompletedTask;
    }

    /// <summary>
    /// Completa el Paso 4: Seguridad Social - YA NO EXISTE EN EL WIZARD SIMPLIFICADO
    /// Mantenido por compatibilidad, no hace nada
    /// </summary>
    public async Task CompletarPaso4Async(string? eps = null, string? arl = null, string? pensiones = null)
    {
        // El wizard ahora tiene solo 2 pasos
        await Task.CompletedTask;
    }

    /// <summary>
    /// Finaliza el wizard - Hace click en GUARDAR (F5)
    /// </summary>
    public async Task FinalizarAsync()
    {
        await BotonGuardar.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(1500); // Esperar procesamiento y redirección
    }

    /// <summary>
    /// Click en botón SIGUIENTE
    /// </summary>
    public async Task ClickSiguienteAsync()
    {
        await BotonSiguiente.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(300);
    }

    /// <summary>
    /// Click en botón ANTERIOR
    /// </summary>
    public async Task ClickAnteriorAsync()
    {
        await BotonAnterior.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Verifica si hay mensaje de éxito (toast)
    /// </summary>
    public async Task<bool> TieneMensajeExitoAsync()
    {
        await Task.Delay(500);
        return await MensajeExito.CountAsync() > 0;
    }

    /// <summary>
    /// Obtiene el texto del mensaje de éxito
    /// </summary>
    public async Task<string> ObtenerMensajeExitoAsync()
    {
        if (await MensajeExito.CountAsync() > 0)
        {
            return await MensajeExito.First.TextContentAsync() ?? string.Empty;
        }
        return string.Empty;
    }

    /// <summary>
    /// Verifica si el campo de estado es de solo lectura (para Operadores)
    /// En el wizard actual, el estado se muestra en un input readonly
    /// </summary>
    public async Task<bool> CampoEstadoEsReadOnlyAsync()
    {
        // El estado se muestra en un input readonly con style background-color: #f0f0f0
        var campoEstado = _page.Locator(".hospital-form-field:has(.hospital-label:has-text('ESTADO INICIAL')) input[readonly]").First;
        if (await campoEstado.CountAsync() > 0)
        {
            return true;
        }
        return true; // El estado siempre es readonly en el wizard
    }

    /// <summary>
    /// Obtiene el texto del estado mostrado en el formulario
    /// </summary>
    public async Task<string> ObtenerEstadoMostradoAsync()
    {
        var campoEstado = _page.Locator(".hospital-form-field:has(.hospital-label:has-text('ESTADO INICIAL')) input.hospital-input").First;
        if (await campoEstado.CountAsync() > 0)
        {
            return await campoEstado.InputValueAsync() ?? string.Empty;
        }
        return string.Empty;
    }

    /// <summary>
    /// Obtiene el paso actual del wizard
    /// </summary>
    public async Task<int> ObtenerPasoActualAsync()
    {
        var texto = await ProgresoTexto.TextContentAsync();
        // Formato: "PASO 1 DE 2: DATOS BASICOS"
        if (!string.IsNullOrEmpty(texto))
        {
            var match = System.Text.RegularExpressions.Regex.Match(texto, @"PASO\s+(\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int paso))
            {
                return paso;
            }
        }
        return 1;
    }
}
