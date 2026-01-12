using Microsoft.Extensions.Configuration;
using Microsoft.Playwright.NUnit;

namespace SGRRHH.Local.Tests.E2E;

/// <summary>
/// Configuración base para todos los tests de Playwright.
/// Hereda de PageTest para tener acceso a Page automáticamente.
/// </summary>
public class PlaywrightSetup : PageTest
{
    protected static IConfiguration Configuration { get; private set; } = null!;
    protected string BaseUrl => Configuration["TestSettings:BaseUrl"] ?? "https://localhost:5001";
    protected int DefaultTimeout => int.Parse(Configuration["TestSettings:DefaultTimeout"] ?? "30000");

    static PlaywrightSetup()
    {
        Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.test.json", optional: false)
            .Build();
    }

    public override BrowserNewContextOptions ContextOptions()
    {
        return new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true, // Para certificados de desarrollo
            Locale = "es-CO",
            TimezoneId = "America/Bogota",
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        };
    }

    [SetUp]
    public async Task BaseSetUp()
    {
        // Configurar timeout por defecto
        Page.SetDefaultTimeout(DefaultTimeout);
        Page.SetDefaultNavigationTimeout(DefaultTimeout);
    }

    [TearDown]
    public async Task BaseTearDown()
    {
        // Capturar screenshot en caso de fallo
        if (TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed)
        {
            var screenshotsEnabled = bool.Parse(Configuration["TestSettings:ScreenshotsOnFailure"] ?? "true");
            if (screenshotsEnabled)
            {
                var screenshotsPath = Configuration["TestSettings:ScreenshotsPath"] ?? "screenshots";
                Directory.CreateDirectory(screenshotsPath);
                
                var testName = TestContext.CurrentContext.Test.Name;
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileName = $"{screenshotsPath}/{testName}_{timestamp}.png";
                
                await Page.ScreenshotAsync(new PageScreenshotOptions { Path = fileName, FullPage = true });
                TestContext.WriteLine($"Screenshot guardado: {fileName}");
            }
        }
    }

    /// <summary>
    /// Navega a una ruta relativa de la aplicación
    /// </summary>
    protected async Task NavigateToAsync(string relativePath)
    {
        var url = $"{BaseUrl}{relativePath}";
        await Page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
    }

    /// <summary>
    /// Espera a que Blazor termine de renderizar (útil para componentes dinámicos)
    /// </summary>
    protected async Task WaitForBlazorAsync(int additionalMs = 500)
    {
        // Esperar a que no haya conexiones de red pendientes
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        // Espera adicional para renderizado de Blazor
        await Task.Delay(additionalMs);
    }
}
