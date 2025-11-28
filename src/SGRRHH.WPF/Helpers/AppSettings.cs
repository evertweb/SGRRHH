using System.IO;
using System.Text.Json;
using SGRRHH.Infrastructure.Firebase;

namespace SGRRHH.WPF.Helpers;

/// <summary>
/// Clase para gestionar la configuración de la aplicación desde appsettings.json
/// </summary>
public static class AppSettings
{
    private static readonly string SettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
    private static JsonDocument? _settings;
    
    /// <summary>
    /// Carga la configuración desde el archivo JSON
    /// </summary>
    public static void Load()
    {
        if (File.Exists(SettingsPath))
        {
            try
            {
                var json = File.ReadAllText(SettingsPath);
                _settings = JsonDocument.Parse(json);
            }
            catch
            {
                _settings = null;
            }
        }
    }
    
    /// <summary>
    /// Indica si las actualizaciones automáticas están habilitadas
    /// </summary>
    public static bool UpdatesEnabled()
    {
        Load();
        
        try
        {
            if (_settings?.RootElement.TryGetProperty("Updates", out var updateSection) == true)
            {
                if (updateSection.TryGetProperty("Enabled", out var enabledElement))
                {
                    return enabledElement.GetBoolean();
                }
            }
        }
        catch { }
        
        return true; // Habilitado por defecto
    }
    
    /// <summary>
    /// Indica si se debe verificar actualizaciones al iniciar
    /// </summary>
    public static bool CheckUpdatesOnStartup()
    {
        Load();
        
        try
        {
            if (_settings?.RootElement.TryGetProperty("Updates", out var updateSection) == true)
            {
                if (updateSection.TryGetProperty("CheckOnStartup", out var checkElement))
                {
                    return checkElement.GetBoolean();
                }
            }
        }
        catch { }
        
        return true; // Verificar por defecto
    }
    
    /// <summary>
    /// Obtiene la versión actual de la aplicación
    /// </summary>
    public static string GetAppVersion()
    {
        Load();
        
        try
        {
            if (_settings?.RootElement.TryGetProperty("Application", out var appSection) == true)
            {
                if (appSection.TryGetProperty("Version", out var versionElement))
                {
                    return versionElement.GetString() ?? "1.0.0";
                }
            }
        }
        catch { }
        
        // Intentar obtener de la asamblea
        try
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            if (version != null)
            {
                return $"{version.Major}.{version.Minor}.{version.Build}";
            }
        }
        catch { }
        
        return "1.0.0";
    }
    
    // ========================
    // Configuración de Firebase
    // ========================
    
    /// <summary>
    /// Obtiene la configuración de Firebase
    /// </summary>
    public static FirebaseConfig GetFirebaseConfig()
    {
        Load();
        
        var config = new FirebaseConfig();
        
        try
        {
            if (_settings?.RootElement.TryGetProperty("Firebase", out var fbSection) == true)
            {
                if (fbSection.TryGetProperty("Enabled", out var enabledElement))
                    config.Enabled = enabledElement.GetBoolean();
                    
                if (fbSection.TryGetProperty("ProjectId", out var projectIdElement))
                    config.ProjectId = projectIdElement.GetString() ?? "";
                    
                if (fbSection.TryGetProperty("DatabaseId", out var databaseIdElement))
                    config.DatabaseId = databaseIdElement.GetString() ?? "(default)";
                    
                if (fbSection.TryGetProperty("StorageBucket", out var bucketElement))
                    config.StorageBucket = bucketElement.GetString() ?? "";
                    
                if (fbSection.TryGetProperty("ApiKey", out var apiKeyElement))
                    config.ApiKey = apiKeyElement.GetString() ?? "";
                    
                if (fbSection.TryGetProperty("AuthDomain", out var authDomainElement))
                    config.AuthDomain = authDomainElement.GetString() ?? "";
                    
                if (fbSection.TryGetProperty("CredentialsPath", out var credentialsElement))
                    config.CredentialsPath = credentialsElement.GetString() ?? "";
            }
        }
        catch { }
        
        return config;
    }
}
