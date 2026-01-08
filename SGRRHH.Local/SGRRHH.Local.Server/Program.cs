using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using QuestPDF.Infrastructure;
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
builder.Services.AddScoped<IActividadRepository, ActividadRepository>();
builder.Services.AddScoped<IProyectoEmpleadoRepository, ProyectoEmpleadoRepository>();
builder.Services.AddScoped<IRegistroDiarioRepository, RegistroDiarioRepository>();
builder.Services.AddScoped<IDetalleActividadRepository, DetalleActividadRepository>();
builder.Services.AddScoped<IDocumentoEmpleadoRepository, DocumentoEmpleadoRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IConfiguracionRepository, ConfiguracionRepository>();

// Authentication service
builder.Services.AddScoped<IAuthService, LocalAuthService>();

// Report service
builder.Services.AddScoped<IReportService, ReportService>();

// Advanced services
builder.Services.AddScoped<IBackupService, BackupService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IConfiguracionService, ConfiguracionService>();

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

app.UseStaticFiles();
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

