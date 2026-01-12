using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;

namespace SGRRHH.Local.Tests.E2E.Helpers;

/// <summary>
/// Helper para manejo de autenticación en tests
/// </summary>
public class AuthHelper
{
    private readonly IPage _page;
    private readonly IConfiguration _configuration;
    private readonly string _baseUrl;

    public AuthHelper(IPage page, IConfiguration configuration, string baseUrl)
    {
        _page = page;
        _configuration = configuration;
        _baseUrl = baseUrl;
    }

    /// <summary>
    /// Realiza login como Operador (secretaria)
    /// </summary>
    public async Task LoginAsOperadorAsync()
    {
        var username = _configuration["TestUsers:Operador:Username"] ?? "secretaria";
        var password = _configuration["TestUsers:Operador:Password"] ?? "secretaria123";
        await LoginAsync(username, password);
    }

    /// <summary>
    /// Realiza login como Aprobador (ingeniera)
    /// </summary>
    public async Task LoginAsAprobadorAsync()
    {
        var username = _configuration["TestUsers:Aprobador:Username"] ?? "ingeniera";
        var password = _configuration["TestUsers:Aprobador:Password"] ?? "ingeniera123";
        await LoginAsync(username, password);
    }

    /// <summary>
    /// Realiza login como Admin
    /// </summary>
    public async Task LoginAsAdminAsync()
    {
        var username = _configuration["TestUsers:Admin:Username"] ?? "admin";
        var password = _configuration["TestUsers:Admin:Password"] ?? "admin";
        await LoginAsync(username, password);
    }

    /// <summary>
    /// Realiza login con credenciales específicas
    /// </summary>
    public async Task LoginAsync(string username, string password)
    {
        // Navegar a login
        await _page.GotoAsync($"{_baseUrl}/login", new PageGotoOptions 
        { 
            WaitUntil = WaitUntilState.NetworkIdle 
        });

        // Esperar a que cargue el formulario (usando selectores que coinciden con Login.razor)
        var usernameInput = _page.Locator(".login-form .form-field input[type='text']").First;
        var passwordInput = _page.Locator(".login-form input[type='password']");
        
        await usernameInput.WaitForAsync();

        // Llenar credenciales
        await usernameInput.FillAsync(username);
        await passwordInput.FillAsync(password);

        // Click en botón de login (texto real es "INICIAR SESION")
        await _page.ClickAsync("button.btn-login:has-text('INICIAR SESION')");

        // Esperar navegación post-login (debe salir de /login)
        await _page.WaitForURLAsync(url => !url.Contains("/login"), new PageWaitForURLOptions
        {
            Timeout = 10000
        });

        // Esperar a que cargue la página destino
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Realiza logout
    /// </summary>
    public async Task LogoutAsync()
    {
        // Buscar y clickear botón/link de logout
        var logoutButton = _page.Locator("button:has-text('SALIR'), a:has-text('SALIR'), .logout-btn");
        
        if (await logoutButton.CountAsync() > 0)
        {
            await logoutButton.First.ClickAsync();
            await _page.WaitForURLAsync(url => url.Contains("/login"));
        }
        else
        {
            // Si no hay botón visible, navegar directo a login
            await _page.GotoAsync($"{_baseUrl}/login");
        }
    }

    /// <summary>
    /// Verifica si el usuario está logueado
    /// </summary>
    public async Task<bool> IsLoggedInAsync()
    {
        var currentUrl = _page.Url;
        return !currentUrl.Contains("/login");
    }
}
