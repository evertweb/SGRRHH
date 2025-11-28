using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SGRRHH.Core.Interfaces;
using SGRRHH.Infrastructure.Firebase.Repositories;
using SGRRHH.Infrastructure.Services;

namespace SGRRHH.Infrastructure.Firebase;

/// <summary>
/// Extensiones para registrar los servicios de Firebase en el contenedor de DI.
/// </summary>
public static class FirebaseServiceCollectionExtensions
{
    /// <summary>
    /// Registra todos los repositorios de Firestore para catálogos.
    /// Fase 2 de la migración: Departamentos, Cargos, Actividades, Proyectos, TiposPermiso, Configuracion.
    /// </summary>
    /// <param name="services">Colección de servicios</param>
    /// <param name="firebase">Inicializador de Firebase</param>
    /// <returns>La colección de servicios para encadenamiento</returns>
    public static IServiceCollection AddFirestoreCatalogRepositories(this IServiceCollection services, FirebaseInitializer firebase)
    {
        // Registrar repositorios de catálogos (Fase 2)
        services.AddScoped<IDepartamentoRepository>(sp => 
            new DepartamentoFirestoreRepository(firebase, sp.GetService<ILogger<DepartamentoFirestoreRepository>>()));
        
        services.AddScoped<ICargoRepository>(sp => 
            new CargoFirestoreRepository(firebase, sp.GetService<ILogger<CargoFirestoreRepository>>()));
        
        services.AddScoped<IActividadRepository>(sp => 
            new ActividadFirestoreRepository(firebase, sp.GetService<ILogger<ActividadFirestoreRepository>>()));
        
        services.AddScoped<IProyectoRepository>(sp => 
            new ProyectoFirestoreRepository(firebase, sp.GetService<ILogger<ProyectoFirestoreRepository>>()));
        
        services.AddScoped<ITipoPermisoRepository>(sp => 
            new TipoPermisoFirestoreRepository(firebase, sp.GetService<ILogger<TipoPermisoFirestoreRepository>>()));
        
        services.AddScoped<IConfiguracionRepository>(sp => 
            new ConfiguracionFirestoreRepository(firebase, sp.GetService<ILogger<ConfiguracionFirestoreRepository>>()));
        
        return services;
    }
    
    /// <summary>
    /// Registra el servicio de Firebase Storage.
    /// Fase 5 de la migración: Archivos y fotos en la nube.
    /// </summary>
    /// <param name="services">Colección de servicios</param>
    /// <param name="firebase">Inicializador de Firebase</param>
    /// <returns>La colección de servicios para encadenamiento</returns>
    public static IServiceCollection AddFirebaseStorageService(this IServiceCollection services, FirebaseInitializer firebase)
    {
        services.AddScoped<IFirebaseStorageService>(sp => 
            new FirebaseStorageService(firebase, sp.GetService<ILogger<FirebaseStorageService>>()));
        
        return services;
    }
    

    
    /// <summary>
    /// Registra todos los servicios de aplicación con soporte completo para Firebase.
    /// Incluye todos los servicios de negocio que usan repositorios Firestore.
    /// </summary>
    /// <param name="services">Colección de servicios</param>
    /// <returns>La colección de servicios para encadenamiento</returns>
    public static IServiceCollection AddFirebaseApplicationServices(this IServiceCollection services)
    {
        // Servicios de infraestructura transversales
        services.AddScoped<IDateCalculationService, DateCalculationService>();
        services.AddScoped<IAbsenceValidationService, AbsenceValidationService>();
        
        // Servicios de negocio
        services.AddScoped<IEmpleadoService, EmpleadoService>();
        services.AddScoped<IDepartamentoService, DepartamentoService>();
        services.AddScoped<ICargoService, CargoService>();
        services.AddScoped<IProyectoService, ProyectoService>();
        services.AddScoped<IActividadService, ActividadService>();
        services.AddScoped<IControlDiarioService, ControlDiarioService>();
        services.AddScoped<IPermisoService, PermisoService>();
        services.AddScoped<ITipoPermisoService, TipoPermisoService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IVacacionService, VacacionService>();
        services.AddScoped<IContratoService, ContratoService>();
        services.AddScoped<IConfiguracionService, ConfiguracionService>();
        services.AddScoped<IBackupService, BackupService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IDocumentoEmpleadoService, DocumentoEmpleadoService>();
        
        return services;
    }
    
    /// <summary>
    /// Registra los repositorios de Firestore para entidades principales.
    /// Fase 3 de la migración: Empleados, Usuarios, Permisos, Vacaciones, Contratos, DocumentosEmpleado.
    /// </summary>
    /// <param name="services">Colección de servicios</param>
    /// <param name="firebase">Inicializador de Firebase</param>
    /// <returns>La colección de servicios para encadenamiento</returns>
    public static IServiceCollection AddFirestoreMainEntityRepositories(this IServiceCollection services, FirebaseInitializer firebase)
    {
        // Registrar repositorios de entidades principales (Fase 3)
        services.AddScoped<IEmpleadoRepository>(sp => 
            new EmpleadoFirestoreRepository(firebase, sp.GetService<ILogger<EmpleadoFirestoreRepository>>()));
        
        services.AddScoped<IUsuarioRepository>(sp => 
            new UsuarioFirestoreRepository(firebase, sp.GetService<ILogger<UsuarioFirestoreRepository>>()));
        
        services.AddScoped<IPermisoRepository>(sp => 
            new PermisoFirestoreRepository(firebase, sp.GetService<ILogger<PermisoFirestoreRepository>>()));
        
        services.AddScoped<IVacacionRepository>(sp => 
            new VacacionFirestoreRepository(firebase, sp.GetService<ILogger<VacacionFirestoreRepository>>()));
        
        services.AddScoped<IContratoRepository>(sp => 
            new ContratoFirestoreRepository(firebase, sp.GetService<ILogger<ContratoFirestoreRepository>>()));
        
        services.AddScoped<IDocumentoEmpleadoRepository>(sp => 
            new DocumentoEmpleadoFirestoreRepository(firebase, sp.GetService<ILogger<DocumentoEmpleadoFirestoreRepository>>()));
        
        return services;
    }
    
    /// <summary>
    /// Registra los repositorios de Firestore para registros y auditoría.
    /// Fase 4 de la migración: RegistroDiario (con subcolección detalles), AuditLog.
    /// </summary>
    /// <param name="services">Colección de servicios</param>
    /// <param name="firebase">Inicializador de Firebase</param>
    /// <returns>La colección de servicios para encadenamiento</returns>
    public static IServiceCollection AddFirestoreRecordRepositories(this IServiceCollection services, FirebaseInitializer firebase)
    {
        // Registrar repositorios de registros y auditoría (Fase 4)
        services.AddScoped<IRegistroDiarioRepository>(sp => 
            new RegistroDiarioFirestoreRepository(firebase, sp.GetService<ILogger<RegistroDiarioFirestoreRepository>>()));
        
        services.AddScoped<IAuditLogRepository>(sp => 
            new AuditLogFirestoreRepository(firebase, sp.GetService<ILogger<AuditLogFirestoreRepository>>()));
        
        return services;
    }
    
    /// <summary>
    /// Registra los repositorios y servicios de Chat y Presencia.
    /// Fase 7: Sistema de chat y usuarios online.
    /// </summary>
    /// <param name="services">Colección de servicios</param>
    /// <param name="firebase">Inicializador de Firebase</param>
    /// <returns>La colección de servicios para encadenamiento</returns>
    public static IServiceCollection AddChatAndPresenceServices(this IServiceCollection services, FirebaseInitializer firebase)
    {
        // Registrar repositorios como Scoped
        services.AddScoped<PresenceFirestoreRepository>(sp => 
            new PresenceFirestoreRepository(firebase, sp.GetService<ILogger<PresenceFirestoreRepository>>()));
        
        services.AddScoped<ChatMessageFirestoreRepository>(sp => 
            new ChatMessageFirestoreRepository(firebase, sp.GetService<ILogger<ChatMessageFirestoreRepository>>()));
        
        // Registrar servicios como Transient para que cada vista tenga su instancia
        services.AddTransient<IPresenceService>(sp =>
        {
            var presenceRepo = new PresenceFirestoreRepository(firebase, sp.GetService<ILogger<PresenceFirestoreRepository>>());
            return new PresenceService(presenceRepo, sp.GetService<ILogger<PresenceService>>());
        });
        
        services.AddTransient<IChatService>(sp =>
        {
            var chatRepo = new ChatMessageFirestoreRepository(firebase, sp.GetService<ILogger<ChatMessageFirestoreRepository>>());
            return new ChatService(chatRepo, sp.GetService<ILogger<ChatService>>());
        });
        
        return services;
    }
    
    /// <summary>
    /// Registra todos los repositorios de Firestore disponibles.
    /// Incluye catálogos (Fase 2), entidades principales (Fase 3) y registros (Fase 4).
    /// </summary>
    /// <param name="services">Colección de servicios</param>
    /// <param name="firebase">Inicializador de Firebase</param>
    /// <returns>La colección de servicios para encadenamiento</returns>
    public static IServiceCollection AddFirestoreRepositories(this IServiceCollection services, FirebaseInitializer firebase)
    {
        // Registrar repositorios de catálogos (Fase 2)
        services.AddFirestoreCatalogRepositories(firebase);
        
        // Registrar repositorios de entidades principales (Fase 3)
        services.AddFirestoreMainEntityRepositories(firebase);
        
        // Registrar repositorios de registros y auditoría (Fase 4)
        services.AddFirestoreRecordRepositories(firebase);
        
        return services;
    }
    
    /// <summary>
    /// Registra todos los servicios de Firebase (autenticación, storage, actualizaciones).
    /// Incluye autenticación (Fase 1), storage (Fase 5) y actualizaciones (Fase 6).
    /// </summary>
    /// <param name="services">Colección de servicios</param>
    /// <param name="config">Configuración de Firebase</param>
    /// <param name="firebase">Inicializador de Firebase</param>
    /// <param name="currentVersion">Versión actual de la aplicación</param>
    /// <returns>La colección de servicios para encadenamiento</returns>
    public static IServiceCollection AddFirebaseServices(this IServiceCollection services, FirebaseConfig config, FirebaseInitializer firebase, string currentVersion)
    {
        // Registrar configuración y inicializador como singletons
        services.AddSingleton(config);
        services.AddSingleton(firebase);
        
        // Registrar servicio de autenticación Firebase (Fase 1)
        services.AddScoped<IFirebaseAuthService, FirebaseAuthService>();
        services.AddScoped<IAuthService>(sp => sp.GetRequiredService<IFirebaseAuthService>());
        
        // Registrar herramienta de migración de usuarios
        services.AddTransient<FirebaseUserMigration>();
        
        // Registrar servicio de Storage (Fase 5)
        services.AddFirebaseStorageService(firebase);
        

        
        // Registrar Unit of Work para transacciones de Firestore
        services.AddScoped<IUnitOfWork>(sp => 
            new FirestoreUnitOfWork(firebase, sp.GetService<ILogger<FirestoreUnitOfWork>>()));
        
        // Registrar todos los repositorios de Firestore (Fases 2, 3, 4)
        services.AddFirestoreRepositories(firebase);
        
        // Registrar servicios de Chat y Presencia (Fase 7)
        services.AddChatAndPresenceServices(firebase);
        
        // Registrar servicios de aplicación
        services.AddFirebaseApplicationServices();
        
        return services;
    }
    
    /// <summary>
    /// Configura todos los servicios de Firebase para la aplicación.
    /// Este método es el punto de entrada principal para configurar Firebase.
    /// Incluye todas las fases de migración (1-6).
    /// </summary>
    /// <param name="services">Colección de servicios</param>
    /// <param name="config">Configuración de Firebase</param>
    /// <param name="firebase">Inicializador de Firebase</param>
    /// <param name="currentVersion">Versión actual de la aplicación</param>
    /// <returns>La colección de servicios para encadenamiento</returns>
    public static IServiceCollection AddFullFirebaseSupport(this IServiceCollection services, FirebaseConfig config, FirebaseInitializer firebase, string currentVersion)
    {
        return services.AddFirebaseServices(config, firebase, currentVersion);
    }
}
