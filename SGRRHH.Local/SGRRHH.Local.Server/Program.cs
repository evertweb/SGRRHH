using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using QuestPDF.Infrastructure;
using SGRRHH.Local.Domain.Interfaces;
using SGRRHH.Local.Infrastructure.Data;
using SGRRHH.Local.Infrastructure.Repositories;
using SGRRHH.Local.Infrastructure.Services;
using SGRRHH.Local.Server.Components;
using SGRRHH.Local.Shared.Interfaces;

var builder = WebApplication.CreateBuilder(args);

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

// Servicio de Reportes de Productividad Silvicultural
builder.Services.AddScoped<IReporteProductividadService, ReporteProductividadService>();

// Servicio de Escaneo de Documentos (NAPS2.Sdk)
builder.Services.AddScoped<IScannerService, ScannerService>();

// Servicio de Impresión de Documentos
builder.Services.AddScoped<IPrintService, PrintService>();

// Cache y Session services
builder.Services.AddScoped<ICatalogCacheService, CatalogCacheService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IKeyboardShortcutService, KeyboardShortcutService>();

// Database Maintenance service
builder.Services.AddScoped<IDatabaseMaintenanceService, DatabaseMaintenanceService>();

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
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(storagePath, "Fotos")),
    RequestPath = "/fotos"
});
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

