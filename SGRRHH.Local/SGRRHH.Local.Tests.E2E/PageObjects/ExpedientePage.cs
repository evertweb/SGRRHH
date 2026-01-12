using Microsoft.Playwright;

namespace SGRRHH.Local.Tests.E2E.PageObjects;

/// <summary>
/// Page Object para el expediente de empleado (/empleados/{id}/expediente)
/// Basado en EmpleadoExpediente.razor con estilos inline
/// </summary>
public class ExpedientePage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    // Selectores - basados en EmpleadoExpediente.razor
    private ILocator BadgeEstado => _page.Locator(".expediente-estado");
    private ILocator SelectorEstado => _page.Locator(".estado-selector select");
    private ILocator BotonAplicar => _page.Locator(".estado-selector button:not(:disabled)");
    private ILocator MensajeExito => _page.Locator(".toast-success, .success-message");
    private ILocator MensajeError => _page.Locator(".toast-error, .error-message");
    private ILocator NombreEmpleado => _page.Locator(".expediente-nombre");
    private ILocator CodigoEmpleado => _page.Locator(".expediente-codigo");
    private ILocator ContenedorExpediente => _page.Locator(".expediente-container");

    public ExpedientePage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl;
    }

    /// <summary>
    /// Navega al expediente de un empleado
    /// </summary>
    public async Task NavigateAsync(int empleadoId)
    {
        await _page.GotoAsync($"{_baseUrl}/empleados/{empleadoId}/expediente", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });
        await _page.WaitForSelectorAsync(".expediente-container, .expediente-estado");
    }

    /// <summary>
    /// Obtiene el estado actual del empleado
    /// </summary>
    public async Task<string> ObtenerEstadoActualAsync()
    {
        if (await BadgeEstado.CountAsync() > 0)
        {
            var texto = await BadgeEstado.First.TextContentAsync();
            return texto?.Trim().ToUpperInvariant() ?? string.Empty;
        }
        return string.Empty;
    }

    /// <summary>
    /// Verifica si el selector de cambio de estado está visible
    /// </summary>
    public async Task<bool> SelectorEstadoVisibleAsync()
    {
        var selector = _page.Locator(".estado-selector");
        if (await selector.CountAsync() == 0)
            return false;
        
        return await SelectorEstado.CountAsync() > 0 && await SelectorEstado.IsVisibleAsync();
    }

    /// <summary>
    /// Obtiene las opciones disponibles en el selector de estado
    /// </summary>
    public async Task<List<string>> ObtenerOpcionesEstadoAsync()
    {
        if (!await SelectorEstadoVisibleAsync())
            return new List<string>();

        var opciones = await SelectorEstado.Locator("option").AllTextContentsAsync();
        return opciones.Select(o => o.Trim().ToUpperInvariant()).ToList();
    }

    /// <summary>
    /// Verifica si una opción de estado está disponible
    /// </summary>
    public async Task<bool> OpcionEstadoDisponibleAsync(string estado)
    {
        var opciones = await ObtenerOpcionesEstadoAsync();
        return opciones.Any(o => o.Contains(estado.ToUpperInvariant()));
    }

    /// <summary>
    /// Cambia el estado del empleado
    /// </summary>
    public async Task CambiarEstadoAsync(string nuevoEstado)
    {
        // Seleccionar el nuevo estado
        await SelectorEstado.SelectOptionAsync(new SelectOptionValue { Label = nuevoEstado });
        await Task.Delay(200);

        // Click en aplicar
        await BotonAplicar.ClickAsync();
        
        // Esperar a que se procese
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(500);
    }

    /// <summary>
    /// Verifica si hay mensaje de éxito después de cambiar estado
    /// </summary>
    public async Task<bool> TieneMensajeExitoAsync()
    {
        await Task.Delay(300);
        return await MensajeExito.CountAsync() > 0;
    }

    /// <summary>
    /// Verifica si hay mensaje de error
    /// </summary>
    public async Task<bool> TieneMensajeErrorAsync()
    {
        return await MensajeError.CountAsync() > 0;
    }

    /// <summary>
    /// Obtiene el texto del mensaje de error
    /// </summary>
    public async Task<string> ObtenerMensajeErrorAsync()
    {
        if (await MensajeError.CountAsync() > 0)
        {
            return await MensajeError.First.TextContentAsync() ?? string.Empty;
        }
        return string.Empty;
    }

    /// <summary>
    /// Verifica si el empleado está en un estado final (sin opciones de transición)
    /// Si no hay selector visible o no hay opciones reales, es estado final
    /// </summary>
    public async Task<bool> EsEstadoFinalAsync()
    {
        // Si no hay selector visible, es estado final
        if (!await SelectorEstadoVisibleAsync())
            return true;

        var opciones = await ObtenerOpcionesEstadoAsync();
        // Filtrar opciones vacías o placeholder
        var opcionesReales = opciones.Where(o => 
            !string.IsNullOrWhiteSpace(o) && 
            !o.Contains("SELECCIONAR") && 
            !o.Contains("CAMBIAR ESTADO")).ToList();
        
        return opcionesReales.Count == 0;
    }

    /// <summary>
    /// Obtiene información general del empleado mostrada en el expediente
    /// </summary>
    public async Task<EmpleadoExpedienteInfo> ObtenerInfoEmpleadoAsync()
    {
        var info = new EmpleadoExpedienteInfo();

        // Nombre completo
        if (await NombreEmpleado.CountAsync() > 0)
        {
            info.NombreCompleto = (await NombreEmpleado.TextContentAsync())?.Trim() ?? string.Empty;
        }

        // Código y cédula (formato: "CODIGO | CÉDULA: 1.234.567")
        if (await CodigoEmpleado.CountAsync() > 0)
        {
            var texto = await CodigoEmpleado.TextContentAsync();
            if (!string.IsNullOrEmpty(texto))
            {
                // Extraer cédula del formato
                var match = System.Text.RegularExpressions.Regex.Match(texto, @"CÉDULA:\s*([0-9.]+)");
                if (match.Success)
                {
                    info.Cedula = match.Groups[1].Value;
                }
            }
        }

        // Estado
        info.Estado = await ObtenerEstadoActualAsync();

        return info;
    }

    /// <summary>
    /// Navega a la pestaña de datos personales
    /// </summary>
    public async Task IrATabDatosAsync()
    {
        var tab = _page.Locator(".expediente-tab:has-text('DATOS PERSONALES')");
        await tab.ClickAsync();
        await Task.Delay(200);
    }

    /// <summary>
    /// Navega a la pestaña de documentos
    /// </summary>
    public async Task IrATabDocumentosAsync()
    {
        var tab = _page.Locator(".expediente-tab:has-text('DOCUMENTOS')");
        await tab.ClickAsync();
        await Task.Delay(200);
    }

    /// <summary>
    /// Navega a la pestaña de contratos
    /// </summary>
    public async Task IrATabContratosAsync()
    {
        var tab = _page.Locator(".expediente-tab:has-text('CONTRATOS')");
        await tab.ClickAsync();
        await Task.Delay(200);
    }

    /// <summary>
    /// Navega a la pestaña de seguridad social
    /// </summary>
    public async Task IrATabSeguridadSocialAsync()
    {
        var tab = _page.Locator(".expediente-tab:has-text('SEGURIDAD SOCIAL')");
        await tab.ClickAsync();
        await Task.Delay(200);
    }

    /// <summary>
    /// Click en botón Volver
    /// </summary>
    public async Task VolverAsync()
    {
        var boton = _page.Locator("button.btn:has-text('VOLVER')");
        await boton.ClickAsync();
        await _page.WaitForURLAsync(url => url.EndsWith("/empleados") || !url.Contains("/expediente"));
    }

    /// <summary>
    /// Click en botón Editar
    /// </summary>
    public async Task EditarAsync()
    {
        var boton = _page.Locator("button.btn:has-text('EDITAR')");
        await boton.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
}

/// <summary>
/// Información de empleado mostrada en expediente
/// </summary>
public class EmpleadoExpedienteInfo
{
    public string NombreCompleto { get; set; } = string.Empty;
    public string Cedula { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
}
