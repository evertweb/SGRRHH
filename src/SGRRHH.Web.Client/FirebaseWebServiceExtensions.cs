using Microsoft.Extensions.DependencyInjection;

namespace SGRRHH.Web.Client;

/// <summary>
/// Extensiones para registrar servicios de Firebase Web en el contenedor de DI
/// </summary>
public static class FirebaseWebServiceExtensions
{
    /// <summary>
    /// Registra todos los servicios de Firebase para la aplicación web
    /// </summary>
    public static IServiceCollection AddFirebaseWebServices(
        this IServiceCollection services, 
        FirebaseWebConfig config)
    {
        // Registrar configuración como singleton
        services.AddSingleton(config);
        
        // Registrar el servicio de interoperabilidad con Firebase JS
        services.AddScoped<FirebaseJsInterop>();
        
        return services;
    }
}
