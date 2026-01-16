using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using QuestPDF.Infrastructure;
using SGRRHH.Local.Domain.Interfaces;
using SGRRHH.Local.Domain.Services;
using SGRRHH.Local.Infrastructure.Data;
using SGRRHH.Local.Infrastructure.Repositories;
using SGRRHH.Local.Infrastructure.Services;
using SGRRHH.Local.Server.Components;
using SGRRHH.Local.Shared.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// Configuracion de puertos automaticos (evita conflictos si ya esta corriendo)
// ============================================================================
int httpPort = FindAvailablePort(5002);
int httpsPort = FindAvailablePort(5003);

// Solo configurar puertos automaticos si NO hay configuracion de Kestrel en appsettings
// (en desarrollo usa appsettings.Development.json con URLs especificas)
if (!builder.Environment.IsDevelopment())
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{httpPort}", $"https://0.0.0.0:{httpsPort}");
}

// QuestPDF requires selecting a license type.
QuestPDF.Settings.License = LicenseType.Community;

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Memory Cache (requerido para CatalogCacheService)
builder.Services.AddMemoryCache();

// Data access registration
builder.Services.AddSingleton<DatabasePathResolver>();
builder.Services.AddSingleton<DapperContext>();
builder.Services.AddSingleton<ILocalStorageService, LocalStorageService>();

builder.Services.AddScoped<IEmpleadoRepository, EmpleadoRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IPermisoRepository, PermisoRepository>();
builder.Services.AddScoped<IVacacionRepository, VacacionRepository>();
builder.Services.AddScoped<IContratoRepository, ContratoRepository>();
builder.Services.AddScoped<IDepartamentoRepository, DepartamentoRepository>();
builder.Services.AddScoped<ICargoRepository, CargoRepository>();
builder.Services.AddScoped<ITipoPermisoRepository, TipoPermisoRepository>();
builder.Services.AddScoped<IProyectoRepository, ProyectoRepository>();
builder.Services.AddScoped<IEspecieForestalRepository, EspecieForestalRepository>();
builder.Services.AddScoped<IActividadRepository, ActividadRepository>();
builder.Services.AddScoped<ICategoriaActividadRepository, CategoriaActividadRepository>();
builder.Services.AddScoped<IProyectoEmpleadoRepository, ProyectoEmpleadoRepository>();
builder.Services.AddScoped<IRegistroDiarioRepository, RegistroDiarioRepository>();
builder.Services.AddScoped<IDetalleActividadRepository, DetalleActividadRepository>();
builder.Services.AddScoped<IDocumentoEmpleadoRepository, DocumentoEmpleadoRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IConfiguracionRepository, ConfiguracionRepository>();
builder.Services.AddScoped<IRegistroDiarioService, RegistroDiarioService>();

// Repositorios Fase 2 - Prestaciones, Nómina, Configuración Legal
builder.Services.AddScoped<IPrestacionRepository, PrestacionRepository>();
builder.Services.AddScoped<IFestivoColombiaRepository, FestivoColombiaRepository>();
builder.Services.AddScoped<IConfiguracionLegalRepository, ConfiguracionLegalRepository>();
builder.Services.AddScoped<INominaRepository, NominaRepository>();

// Repositorios Sistema de Seguimiento de Permisos
builder.Services.AddScoped<ISeguimientoPermisoRepository, SeguimientoPermisoRepository>();
builder.Services.AddScoped<ICompensacionHorasRepository, CompensacionHorasRepository>();

// Repositorios Sistema de Incapacidades
builder.Services.AddScoped<ISeguimientoIncapacidadRepository, SeguimientoIncapacidadRepository>();
builder.Services.AddScoped<IIncapacidadRepository, IncapacidadRepository>();

// Repositorio de Notificaciones
builder.Services.AddScoped<INotificacionRepository, NotificacionRepository>();

// Repositorios de Seguridad Social (EPS, AFP, ARL, Cajas de Compensación)
builder.Services.AddScoped<IEpsRepository, EpsRepository>();
builder.Services.AddScoped<IAfpRepository, AfpRepository>();
builder.Services.AddScoped<IArlRepository, ArlRepository>();
builder.Services.AddScoped<ICajaCompensacionRepository, CajaCompensacionRepository>();

// Repositorio de Dispositivos Autorizados (login sin contraseña)
builder.Services.AddScoped<IDispositivoAutorizadoRepository, DispositivoAutorizadoRepository>();

// Repositorio de Cuentas Bancarias
builder.Services.AddScoped<ICuentaBancariaRepository, CuentaBancariaRepository>();

// Repositorios de Dotación y EPP
builder.Services.AddScoped<ITallasEmpleadoRepository, TallasEmpleadoRepository>();
builder.Services.AddScoped<IEntregaDotacionRepository, EntregaDotacionRepository>();
builder.Services.AddScoped<IDetalleEntregaDotacionRepository, DetalleEntregaDotacionRepository>();

// Authentication service
builder.Services.AddScoped<IAuthService, LocalAuthService>();

// Report service
builder.Services.AddScoped<IReportService, ReportService>();

// Advanced services
builder.Services.AddScoped<IBackupService, BackupService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IConfiguracionService, ConfiguracionService>();

// Servicios Fase 2 - Liquidación, Nómina, Validación (CST Colombia)
builder.Services.AddScoped<ILiquidacionService, LiquidacionService>();
builder.Services.AddScoped<INominaService, NominaService>();
builder.Services.AddScoped<IValidacionService, ValidacionService>();

// Servicio de Alertas de Permisos
builder.Services.AddScoped<IAlertaPermisoService, AlertaPermisoService>();
builder.Services.AddScoped<IPermisoCalculationService, PermisoCalculationService>();

// Servicio de Reportes de Productividad Silvicultural
builder.Services.AddScoped<IReporteProductividadService, ReporteProductividadService>();

// Servicio de Escaneo de Documentos (NAPS2.Sdk)
builder.Services.AddScoped<IScannerService, ScannerService>();
builder.Services.AddScoped<IScannerModalStateService, ScannerModalStateService>();
builder.Services.AddScoped<IScannerWorkflowService, ScannerWorkflowService>();

// Servicio de Procesamiento de Imágenes (corrección, rotación, recorte)
builder.Services.AddScoped<IImageProcessingService, ImageProcessingService>();
builder.Services.AddScoped<IImageTransformationService, ImageTransformationService>();

// Servicio de OCR (reconocimiento óptico de caracteres)
builder.Services.AddScoped<IOcrService, OcrService>();

// Repositorio de Perfiles de Escaneo
builder.Services.AddScoped<IScanProfileRepository, ScanProfileRepository>();

// Servicio de Impresión de Documentos
builder.Services.AddScoped<IPrintService, PrintService>();

// Cache y Session services
builder.Services.AddScoped<ICatalogCacheService, CatalogCacheService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IKeyboardShortcutService, KeyboardShortcutService>();

// Database Maintenance service
builder.Services.AddScoped<IDatabaseMaintenanceService, DatabaseMaintenanceService>();

// Servicio de almacenamiento de documentos centralizado
builder.Services.AddScoped<IDocumentoStorageService, DocumentoStorageService>();

// Background services
builder.Services.AddHostedService<BackupSchedulerService>();

var app = builder.Build();

// Ensure SQLite database is created and up to date
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    var context = scope.ServiceProvider.GetRequiredService<DapperContext>();
    var pathResolver = scope.ServiceProvider.GetRequiredService<DatabasePathResolver>();

    try
    {
        await context.RunMigrationsAsync();
        logger.LogInformation("SQLite ready at {DbPath}", pathResolver.GetDatabasePath());
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error running SQLite migrations");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Configuración de archivos estáticos SIN caché para desarrollo (CSS siempre actualizado)
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Deshabilitar caché para CSS y JS durante desarrollo
        if (ctx.File.Name.EndsWith(".css") || ctx.File.Name.EndsWith(".js"))
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
            ctx.Context.Response.Headers.Append("Pragma", "no-cache");
            ctx.Context.Response.Headers.Append("Expires", "0");
        }
    }
});
var storagePath = app.Services.GetRequiredService<DatabasePathResolver>().GetStoragePath();

// Configurar MIME types para archivos estáticos de fotos
var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
provider.Mappings[".png"] = "image/png";
provider.Mappings[".jpg"] = "image/jpeg";
provider.Mappings[".jpeg"] = "image/jpeg";

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(storagePath, "Fotos")),
    RequestPath = "/fotos",
    ContentTypeProvider = provider
});
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// ============================================================================
// Banner informativo y apertura de navegador
// ============================================================================
var urls = app.Urls.Any() ? app.Urls : new[] { $"https://localhost:{httpsPort}" };
var primaryUrl = urls.FirstOrDefault(u => u.StartsWith("https")) ?? urls.First();

app.Lifetime.ApplicationStarted.Register(() =>
{
    Console.WriteLine();
    Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
    Console.WriteLine("║                    SGRRHH Local - Iniciado                     ║");
    Console.WriteLine("╠════════════════════════════════════════════════════════════════╣");
    Console.WriteLine($"║  HTTP:  http://localhost:{httpPort,-5}                                  ║");
    Console.WriteLine($"║  HTTPS: https://localhost:{httpsPort,-5}                                 ║");
    Console.WriteLine("╠════════════════════════════════════════════════════════════════╣");
    Console.WriteLine("║  Presione Ctrl+C para detener el servidor                      ║");
    Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
    Console.WriteLine();

    // Abrir navegador automaticamente (solo en produccion/self-contained)
    if (!builder.Environment.IsDevelopment())
    {
        OpenBrowser(primaryUrl);
    }
});

app.Run();

// ============================================================================
// Funciones auxiliares
// ============================================================================

/// <summary>
/// Busca un puerto disponible empezando desde el puerto especificado.
/// Si el puerto esta ocupado, busca el siguiente disponible.
/// </summary>
static int FindAvailablePort(int startPort)
{
    for (int port = startPort; port < startPort + 100; port++)
    {
        if (IsPortAvailable(port))
        {
            return port;
        }
    }
    return startPort; // Fallback al puerto original
}

/// <summary>
/// Verifica si un puerto TCP esta disponible.
/// </summary>
static bool IsPortAvailable(int port)
{
    try
    {
        using var listener = new TcpListener(IPAddress.Loopback, port);
        listener.Start();
        listener.Stop();
        return true;
    }
    catch (SocketException)
    {
        return false;
    }
}

/// <summary>
/// Abre el navegador por defecto con la URL especificada.
/// </summary>
static void OpenBrowser(string url)
{
    try
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }
    catch
    {
        // Ignorar errores al abrir navegador
    }
}

