using System.IO;

namespace SGRRHH.WPF.Helpers;

/// <summary>
/// Proporciona rutas centralizadas para datos de la aplicación.
/// Usa LOCALAPPDATA para evitar problemas de permisos cuando la aplicación
/// está instalada en Program Files.
/// </summary>
public static class DataPaths
{
    /// <summary>
    /// Ruta base de datos de la aplicación en %LOCALAPPDATA%\SGRRHH
    /// </summary>
    public static string AppDataRoot { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "SGRRHH");
    
    /// <summary>
    /// Ruta para logs de la aplicación
    /// </summary>
    public static string Logs => Path.Combine(AppDataRoot, "logs");
    
    /// <summary>
    /// Ruta para documentos de contratos
    /// </summary>
    public static string Contratos => Path.Combine(AppDataRoot, "contratos");
    
    /// <summary>
    /// Ruta para documentos de permisos
    /// </summary>
    public static string Permisos => Path.Combine(AppDataRoot, "permisos");
    
    /// <summary>
    /// Ruta para fotos de empleados
    /// </summary>
    public static string Fotos => Path.Combine(AppDataRoot, "fotos");
    
    /// <summary>
    /// Ruta para documentos generales
    /// </summary>
    public static string Documentos => Path.Combine(AppDataRoot, "documentos");
    
    /// <summary>
    /// Ruta para backups locales
    /// </summary>
    public static string Backups => Path.Combine(AppDataRoot, "backups");
    
    /// <summary>
    /// Ruta para actualizaciones descargadas
    /// </summary>
    public static string Updates => Path.Combine(AppDataRoot, "updates");
    
    /// <summary>
    /// Asegura que un directorio exista, creándolo si es necesario.
    /// </summary>
    public static string EnsureDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        return path;
    }
    
    /// <summary>
    /// Obtiene la ruta para un archivo de log del día actual
    /// </summary>
    public static string GetLogFilePath(string prefix = "error")
    {
        EnsureDirectory(Logs);
        return Path.Combine(Logs, $"{prefix}_{DateTime.Now:yyyy-MM-dd}.log");
    }
}
