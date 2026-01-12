using Microsoft.Playwright;

namespace SGRRHH.Local.Tests.E2E.PageObjects;

/// <summary>
/// Page Object para el listado de empleados (/empleados)
/// Basado en Empleados.razor con estilos inline
/// </summary>
public class EmpleadosPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    // Selectores - basados en Empleados.razor
    private ILocator BotonNuevo => _page.Locator("button.btn:has-text('NUEVO (F3)')");
    private ILocator FiltroEstado => _page.Locator("select.filter-select");
    private ILocator CampoBusqueda => _page.Locator("input#searchInput, input.filter-input");
    private ILocator TablaEmpleados => _page.Locator("table.data-table");
    private ILocator FilasEmpleados => _page.Locator("table.data-table tbody tr");
    private ILocator BotonActualizar => _page.Locator("button.btn:has-text('ACTUALIZAR (F5)')");
    private ILocator BotonBuscar => _page.Locator("button.btn:has-text('BUSCAR (F2)')");
    private ILocator BotonLimpiarFiltros => _page.Locator("button.btn:has-text('LIMPIAR FILTROS')");

    public EmpleadosPage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl;
    }

    /// <summary>
    /// Navega al listado de empleados
    /// </summary>
    public async Task NavigateAsync()
    {
        await _page.GotoAsync($"{_baseUrl}/empleados", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });
        // Esperar a que cargue la tabla
        await _page.WaitForSelectorAsync("table.data-table", new PageWaitForSelectorOptions
        {
            Timeout = 10000
        });
    }

    /// <summary>
    /// Click en botón NUEVO para crear empleado
    /// </summary>
    public async Task ClickNuevoAsync()
    {
        await BotonNuevo.ClickAsync();
        await _page.WaitForURLAsync(url => url.Contains("/onboarding"));
    }

    /// <summary>
    /// Filtra empleados por estado usando el select
    /// </summary>
    public async Task FiltrarPorEstadoAsync(string estado)
    {
        await FiltroEstado.SelectOptionAsync(new SelectOptionValue { Label = estado });
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(500); // Esperar renderizado Blazor
    }

    /// <summary>
    /// Busca empleados por texto
    /// </summary>
    public async Task BuscarAsync(string texto)
    {
        await CampoBusqueda.FillAsync(texto);
        await _page.Keyboard.PressAsync("Enter");
        await Task.Delay(500);
    }

    /// <summary>
    /// Verifica si un empleado aparece en el listado (por cédula o nombre)
    /// </summary>
    public async Task<bool> EmpleadoExisteEnListadoAsync(string textoBusqueda)
    {
        var fila = _page.Locator($"table.data-table tbody tr:has-text('{textoBusqueda}')");
        return await fila.CountAsync() > 0;
    }

    /// <summary>
    /// Obtiene el estado mostrado de un empleado en la tabla
    /// </summary>
    public async Task<string> ObtenerEstadoEmpleadoAsync(string cedulaONombre)
    {
        var fila = _page.Locator($"table.data-table tbody tr:has-text('{cedulaONombre}')").First;
        if (await fila.CountAsync() == 0)
            return string.Empty;

        // La celda de estado tiene clase cell-estado y estado-{estado}
        var celdaEstado = fila.Locator("td.cell-estado").First;
        if (await celdaEstado.CountAsync() > 0)
        {
            return (await celdaEstado.TextContentAsync())?.Trim().ToUpperInvariant() ?? string.Empty;
        }

        return string.Empty;
    }

    /// <summary>
    /// Verifica si el botón APROBAR está visible para un empleado
    /// </summary>
    public async Task<bool> BotonAprobarVisibleAsync(string cedulaONombre)
    {
        var fila = _page.Locator($"table.data-table tbody tr:has-text('{cedulaONombre}')").First;
        if (await fila.CountAsync() == 0)
            return false;

        // El botón APROBAR tiene estilo color: #006600
        var botonAprobar = fila.Locator("button.btn-table:has-text('APROBAR')");
        return await botonAprobar.CountAsync() > 0 && await botonAprobar.IsVisibleAsync();
    }

    /// <summary>
    /// Aprueba un empleado desde el listado
    /// </summary>
    public async Task AprobarEmpleadoAsync(string cedulaONombre)
    {
        var fila = _page.Locator($"table.data-table tbody tr:has-text('{cedulaONombre}')").First;
        var botonAprobar = fila.Locator("button.btn-table:has-text('APROBAR')");
        
        await botonAprobar.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(500);
    }

    /// <summary>
    /// Navega al expediente de un empleado (click en EDITAR)
    /// </summary>
    public async Task IrAExpedienteAsync(string cedulaONombre)
    {
        var fila = _page.Locator($"table.data-table tbody tr:has-text('{cedulaONombre}')").First;
        
        // El botón EDITAR redirige al expediente
        var botonEditar = fila.Locator("button.btn-table:has-text('EDITAR')");
        
        if (await botonEditar.CountAsync() > 0)
        {
            await botonEditar.ClickAsync();
        }
        else
        {
            // Click en la fila si no hay botón específico
            await fila.ClickAsync();
        }
        
        await _page.WaitForURLAsync(url => url.Contains("/expediente"));
    }

    /// <summary>
    /// Obtiene el número de empleados en el listado
    /// </summary>
    public async Task<int> ContarEmpleadosAsync()
    {
        var filas = await FilasEmpleados.AllAsync();
        // Filtrar filas que son mensajes (cargando, vacío)
        int count = 0;
        foreach (var fila in filas)
        {
            var texto = await fila.TextContentAsync();
            if (!string.IsNullOrEmpty(texto) && 
                !texto.Contains("CARGANDO") && 
                !texto.Contains("NO HAY REGISTROS"))
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Obtiene todas las opciones disponibles en el filtro de estado
    /// </summary>
    public async Task<List<string>> ObtenerOpcionesEstadoAsync()
    {
        var opciones = await FiltroEstado.Locator("option").AllTextContentsAsync();
        return opciones.ToList();
    }

    /// <summary>
    /// Actualiza el listado
    /// </summary>
    public async Task ActualizarAsync()
    {
        await BotonActualizar.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(500);
    }

    /// <summary>
    /// Limpia los filtros aplicados
    /// </summary>
    public async Task LimpiarFiltrosAsync()
    {
        if (await BotonLimpiarFiltros.CountAsync() > 0 && await BotonLimpiarFiltros.IsVisibleAsync())
        {
            await BotonLimpiarFiltros.ClickAsync();
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Task.Delay(500);
        }
    }
}
