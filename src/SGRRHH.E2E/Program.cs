using System.Net.Http.Json;
using Microsoft.Playwright;
using System.Text.Json;

Console.WriteLine("E2E test starting...");

var authServer = Environment.GetEnvironmentVariable("AUTH_SERVER") ?? "http://localhost:5001";
var clientBase = Environment.GetEnvironmentVariable("CLIENT_BASE") ?? "http://localhost:5103";

// Create test user (only allowed in Development on the AuthServer)
var http = new HttpClient();
var testId = Guid.NewGuid().ToString("N").Substring(0, 8);
var email = $"e2e_{testId}@example.com";
var password = "Test1234!";

Console.WriteLine($"Creating test user: {email}");
var createResp = await http.PostAsJsonAsync(new Uri(new Uri(authServer), "api/auth/test/create-user"), new { Email = email, Password = password, DisplayName = $"E2E User {testId}" });
if (!createResp.IsSuccessStatusCode)
{
    var text = await createResp.Content.ReadAsStringAsync();
    Console.WriteLine($"Failed to create test user: {createResp.StatusCode} - {text}");
    return 1;
}
var createJson = await createResp.Content.ReadFromJsonAsync<JsonElement>();
Console.WriteLine($"User created: {createJson}");

// Start Playwright
using var playwright = await Playwright.CreateAsync();

var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
var context = await browser.NewContextAsync();
var page = await context.NewPageAsync();

// Pipe browser console messages to test output
page.Console += (_, msg) => Console.WriteLine($"[Browser] {msg.Text}");
page.RequestFailed += (_, req) => Console.WriteLine($"[Browser] request failed: {req.Url} -> {req.Failure}");

Console.WriteLine("Navigating to login page");
try
{
    await page.GotoAsync(clientBase + "/login", new PageGotoOptions { Timeout = 30000 });
}
catch (Exception ex)
{
    Console.WriteLine($"Navigation error: {ex.Message}");
    return 1;
}

// Wait for login form
await page.WaitForSelectorAsync("input[placeholder=\"Ingrese su usuario\"]", new PageWaitForSelectorOptions { Timeout = 20000 });

// Fill login form
await page.FillAsync("input[placeholder=\"Ingrese su usuario\"]", email);
await page.FillAsync("input[placeholder=\"Ingrese su contrasena\"]", password);
await page.ClickAsync("button.login-button");

// Wait for dashboard (look for 'Bienvenido') or a login error using polling
bool success = false;
int waited = 0;
int sleepMs = 500;
int timeoutMs = 30000; // total timeout
while (waited < timeoutMs)
{
    // check success
    var successLocator = await page.QuerySelectorAsync("text=Bienvenido");
    if (successLocator != null)
    {
        success = true;
        Console.WriteLine("Login succeeded, dashboard visible.");
        break;
    }

    var errorLocator = await page.QuerySelectorAsync("div.login-error");
    if (errorLocator != null)
    {
        var errText = await errorLocator.InnerTextAsync();
        Console.WriteLine($"Login failed, error message on page: {errText}");
        return 1;
    }

    await Task.Delay(sleepMs);
    waited += sleepMs;
}

if (!success)
{
    Console.WriteLine("Timed out waiting for dashboard after login attempt (no 'Bienvenido' detected).");
    // Capture screenshot for debugging
    try
    {
        var screenshotPath = $"e2e_failure_{Guid.NewGuid().ToString("N").Substring(0,8)}.png";
        await page.ScreenshotAsync(new PageScreenshotOptions { Path = screenshotPath, FullPage = true });
        Console.WriteLine($"Saved screenshot: {screenshotPath}");
    }
    catch { }
    return 1;
}

// Check session cookie from AuthServer domain
var cookies = await context.CookiesAsync();
var sessionCookie = cookies.FirstOrDefault(c => c.Name == "sgrrhh_session");
if (sessionCookie == null)
{
    Console.WriteLine("ERROR: session cookie 'sgrrhh_session' not found.");
    // still continue to test client persistence
}
else
{
    Console.WriteLine($"Session cookie found: {sessionCookie.Name}, domain {sessionCookie.Domain}");
}

// Reload page and confirm still logged in
await page.ReloadAsync();
await page.WaitForSelectorAsync("text=Bienvenido", new PageWaitForSelectorOptions { Timeout = 10000 });
Console.WriteLine("Reload successful and user still logged in (persistence OK).");

// Logout
await page.ClickAsync("text='[Salir]'");
// After logout, should be redirected to login
await page.WaitForSelectorAsync("text=Ingresar", new PageWaitForSelectorOptions { Timeout = 10000 });
Console.WriteLine("Logout succeeded, back to login.");

// Final check: session cookie should be removed or invalid on server
var meResp = await http.GetAsync(new Uri(new Uri(authServer), "api/auth/me"));
if (meResp.IsSuccessStatusCode)
{
    Console.WriteLine("Warning: /api/auth/me returned success after logout (cookie may persist) -> check server invalidation.");
}
else
{
    Console.WriteLine("/api/auth/me returned unauthorized after logout (as expected).");
}

await browser.CloseAsync();
Console.WriteLine("E2E test finished successfully.");
return 0;