using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SGRRHH.Web;
using SGRRHH.Web.Client;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;
using SGRRHH.Infrastructure.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Configuración de Firebase
var firebaseConfig = builder.Configuration.GetSection("Firebase").Get<FirebaseWebConfig>()
    ?? throw new InvalidOperationException("Firebase configuration section 'Firebase' not found.");
builder.Services.AddFirebaseWebServices(firebaseConfig);

// Registro de Repositorios Web (Implementaciones basadas en Firebase JS SDK)
builder.Services.AddScoped<IUsuarioRepository, WebUsuarioRepository>();
builder.Services.AddScoped<IEmpleadoRepository, WebEmpleadoRepository>();
builder.Services.AddScoped<IVacacionRepository, WebVacacionRepository>();
builder.Services.AddScoped<IPermisoRepository, WebPermisoRepository>();
builder.Services.AddScoped<ICargoRepository, WebCargoRepository>();
builder.Services.AddScoped<IDepartamentoRepository, WebDepartamentoRepository>();
builder.Services.AddScoped<ITipoPermisoRepository, WebTipoPermisoRepository>();
builder.Services.AddScoped<IRegistroDiarioRepository, WebRegistroDiarioRepository>();
builder.Services.AddScoped<IActividadRepository, WebActividadRepository>();
builder.Services.AddScoped<IProyectoRepository, WebProyectoRepository>();
builder.Services.AddScoped<IProyectoEmpleadoRepository, WebProyectoEmpleadoRepository>();
builder.Services.AddScoped<IContratoRepository, WebContratoRepository>();

// Repositorios genéricos para entidades que no tienen interfaz específica
builder.Services.AddScoped<IRepository<Departamento>, WebDepartamentoRepository>();
builder.Services.AddScoped<IRepository<Cargo>, WebCargoRepository>();
builder.Services.AddScoped<IRepository<TipoPermiso>, WebTipoPermisoRepository>();
builder.Services.AddScoped<IRepository<Proyecto>, WebProyectoRepository>();
builder.Services.AddScoped<IRepository<Actividad>, WebActividadRepository>();
builder.Services.AddScoped<IRepository<Empleado>, WebEmpleadoRepository>();
builder.Services.AddScoped<IRepository<Contrato>, WebContratoRepository>();
builder.Services.AddScoped<IRepository<RegistroDiario>, WebRegistroDiarioRepository>();
builder.Services.AddScoped<IRepository<Permiso>, WebPermisoRepository>();
builder.Services.AddScoped<IRepository<Vacacion>, WebVacacionRepository>();
builder.Services.AddScoped<IDocumentoEmpleadoRepository, WebDocumentoEmpleadoRepository>();
builder.Services.AddScoped<IRepository<DocumentoEmpleado>, WebDocumentoEmpleadoRepository>();
builder.Services.AddScoped<IRepository<ProyectoEmpleado>, WebProyectoEmpleadoRepository>();

// Registro de Servicios de Autenticación
// WebAuthService requires the AuthServer base URL (if configured in wwwroot/appsettings.json)
var authServerBaseUrl = builder.Configuration.GetValue<string>("AuthServer:BaseUrl") ?? string.Empty;
builder.Services.AddScoped<IFirebaseAuthService>(sp => new WebAuthService(
    sp.GetRequiredService<FirebaseJsInterop>(),
    sp.GetRequiredService<IUsuarioRepository>(),
    sp.GetRequiredService<HttpClient>(),
    authServerBaseUrl
));
builder.Services.AddScoped<IAuthService>(sp => sp.GetRequiredService<IFirebaseAuthService>());

// Registro de Servicios de Negocio (Reutilizando lógica de SGRRHH.Infrastructure)
builder.Services.AddScoped<IDateCalculationService, DateCalculationService>();
builder.Services.AddScoped<IAbsenceValidationService, AbsenceValidationService>();
builder.Services.AddScoped<IVacacionService, VacacionService>();
builder.Services.AddScoped<IEmpleadoService, EmpleadoService>();
builder.Services.AddScoped<ICargoService, CargoService>();
builder.Services.AddScoped<IDepartamentoService, DepartamentoService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<ITipoPermisoService, TipoPermisoService>();
builder.Services.AddScoped<IPermisoService, PermisoService>();
builder.Services.AddScoped<IContratoService, ContratoService>();
builder.Services.AddScoped<IControlDiarioService, ControlDiarioService>();
builder.Services.AddScoped<IProyectoService, ProyectoService>();
builder.Services.AddScoped<IActividadService, ActividadService>();

// Servicio de Exportación
builder.Services.AddScoped<SGRRHH.Web.Services.ExportService>();

// Estado Global de la aplicación
builder.Services.AddScoped<AppStateService>();
// Bridge para recibir eventos de auth desde JS
builder.Services.AddScoped<AuthJsBridge>();

await builder.Build().RunAsync();
