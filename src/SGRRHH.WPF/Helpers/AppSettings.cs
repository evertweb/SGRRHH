using System.IO;
using System.Text.Json;

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
    /// Obtiene la ruta de la base de datos configurada
    /// </summary>
    public static string GetDatabasePath()
    {
        Load();
        
        try
        {
            if (_settings?.RootElement.TryGetProperty("Database", out var dbSection) == true)
            {
                if (dbSection.TryGetProperty("Path", out var pathElement))
                {
                    var configuredPath = pathElement.GetString();
                    if (!string.IsNullOrWhiteSpace(configuredPath))
                    {
                        // Si es una ruta relativa, combinar con el directorio base
                        if (!Path.IsPathRooted(configuredPath) && !configuredPath.StartsWith("\\\\"))
                        {
                            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configuredPath);
                        }
                        return configuredPath;
                    }
                }
            }
        }
        catch { }
        
        // Ruta por defecto
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "sgrrhh.db");
    }
    
    /// <summary>
    /// Indica si se debe usar el modo WAL para SQLite
    /// </summary>
    public static bool EnableWalMode()
    {
        Load();
        
        try
        {
            if (_settings?.RootElement.TryGetProperty("Database", out var dbSection) == true)
            {
                if (dbSection.TryGetProperty("EnableWalMode", out var walElement))
                {
                    return walElement.GetBoolean();
                }
            }
        }
        catch { }
        
        return true; // WAL habilitado por defecto
    }
    
    /// <summary>
    /// Obtiene el timeout de espera para operaciones de BD
    /// </summary>
    public static int GetBusyTimeout()
    {
        Load();
        
        try
        {
            if (_settings?.RootElement.TryGetProperty("Database", out var dbSection) == true)
            {
                if (dbSection.TryGetProperty("BusyTimeout", out var timeoutElement))
                {
                    return timeoutElement.GetInt32();
                }
            }
        }
        catch { }
        
        return 30000; // 30 segundos por defecto
    }
    
    /// <summary>
    /// Indica si está configurado para modo red
    /// </summary>
    public static bool IsNetworkMode()
    {
        Load();
        
        try
        {
            if (_settings?.RootElement.TryGetProperty("Network", out var netSection) == true)
            {
                if (netSection.TryGetProperty("IsNetworkMode", out var modeElement))
                {
                    return modeElement.GetBoolean();
                }
            }
        }
        catch { }
        
        // También detectar si la ruta es de red
        var dbPath = GetDatabasePath();
        return dbPath.StartsWith("\\\\");
    }
    
    /// <summary>
    /// Obtiene la cadena de conexión completa para SQLite
    /// </summary>
    public static string GetConnectionString()
    {
        var dbPath = GetDatabasePath();
        var busyTimeout = GetBusyTimeout();
        
        // Construir cadena de conexión con opciones optimizadas para concurrencia
        return $"Data Source={dbPath};Cache=Shared;Mode=ReadWriteCreate;Pooling=True;Default Timeout={busyTimeout / 1000}";
    }
}
