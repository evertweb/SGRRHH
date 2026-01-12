using Microsoft.Playwright;

namespace SGRRHH.Local.Tests.E2E.PageObjects;

/// <summary>
/// Page Object para la página de login (/login)
/// </summary>
public class LoginPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    // Selectores
    private ILocator UsernameInput => _page.Locator("input[name='username']");
    private ILocator PasswordInput => _page.Locator("input[name='password']");
    private ILocator LoginButton => _page.Locator("button:has-text('INGRESAR')");
    private ILocator ErrorMessage => _page.Locator(".error-block, .error-message, [class*='error']");

    public LoginPage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl;
    }

    /// <summary>
    /// Navega a la página de login
    /// </summary>
    public async Task NavigateAsync()
    {
        await _page.GotoAsync($"{_baseUrl}/login", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });
        await _page.WaitForSelectorAsync("input[name='username']");
    }

    /// <summary>
    /// Realiza el login con las credenciales proporcionadas
    /// </summary>
    public async Task LoginAsync(string username, string password)
    {
        await UsernameInput.FillAsync(username);
        await PasswordInput.FillAsync(password);
        await LoginButton.ClickAsync();
    }

    /// <summary>
    /// Login y espera navegación exitosa
    /// </summary>
    public async Task LoginAndWaitAsync(string username, string password)
    {
        await LoginAsync(username, password);
        await _page.WaitForURLAsync(url => !url.Contains("/login"), new PageWaitForURLOptions
        {
            Timeout = 10000
        });
    }

    /// <summary>
    /// Verifica si hay un mensaje de error visible
    /// </summary>
    public async Task<bool> HasErrorAsync()
    {
        return await ErrorMessage.IsVisibleAsync();
    }

    /// <summary>
    /// Obtiene el texto del mensaje de error
    /// </summary>
    public async Task<string> GetErrorMessageAsync()
    {
        if (await HasErrorAsync())
        {
            return await ErrorMessage.TextContentAsync() ?? string.Empty;
        }
        return string.Empty;
    }

    /// <summary>
    /// Verifica si estamos en la página de login
    /// </summary>
    public bool IsOnLoginPage()
    {
        return _page.Url.Contains("/login");
    }
}
