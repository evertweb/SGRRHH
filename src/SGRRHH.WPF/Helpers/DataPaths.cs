using SGRRHH.Core.Common;

namespace SGRRHH.WPF.Helpers;

/// <summary>
/// Proporciona rutas centralizadas para datos de la aplicación.
/// Esta clase es un wrapper alrededor de AppDataPaths en Core para compatibilidad.
/// </summary>
public static class DataPaths
{
    /// <summary>
    /// Ruta base de datos de la aplicación en %LOCALAPPDATA%\SGRRHH
    /// </summary>
    public static string AppDataRoot => AppDataPaths.AppDataRoot;
    
    /// <summary>
    /// Ruta para logs de la aplicación
    /// </summary>
    public static string Logs => AppDataPaths.Logs;
    
    /// <summary>
    /// Ruta para documentos de contratos
    /// </summary>
    public static string Contratos => AppDataPaths.Contratos;
    
    /// <summary>
    /// Ruta para documentos de permisos
    /// </summary>
    public static string Permisos => AppDataPaths.Permisos;
    
    /// <summary>
    /// Ruta para fotos de empleados
    /// </summary>
    public static string Fotos => AppDataPaths.Fotos;
    
    /// <summary>
    /// Ruta para documentos generales
    /// </summary>
    public static string Documentos => AppDataPaths.Documentos;
    
    /// <summary>
    /// Ruta para backups locales
    /// </summary>
    public static string Backups => AppDataPaths.Backups;
    
    /// <summary>
    /// Ruta para actualizaciones descargadas
    /// </summary>
    public static string Updates => AppDataPaths.Updates;
    
    /// <summary>
    /// Asegura que un directorio exista, creándolo si es necesario.
    /// </summary>
    public static string EnsureDirectory(string path) => AppDataPaths.EnsureDirectory(path);
    
    /// <summary>
    /// Obtiene la ruta para un archivo de log del día actual
    /// </summary>
    public static string GetLogFilePath(string prefix = "error") => AppDataPaths.GetLogFilePath(prefix);
}

