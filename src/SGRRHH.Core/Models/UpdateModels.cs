namespace SGRRHH.Core.Models;

/// <summary>
/// Información sobre una versión de la aplicación
/// </summary>
public class VersionInfo
{
    /// <summary>
    /// Número de versión (ej: "1.1.0")
    /// </summary>
    public string Version { get; set; } = "1.0.0";
    
    /// <summary>
    /// Fecha de publicación de la versión
    /// </summary>
    public DateTime ReleaseDate { get; set; }
    
    /// <summary>
    /// Indica si la actualización es obligatoria
    /// </summary>
    public bool Mandatory { get; set; }
    
    /// <summary>
    /// Versión mínima requerida para actualizar (si aplica)
    /// </summary>
    public string? MinimumVersion { get; set; }
    
    /// <summary>
    /// Notas de la versión (changelog)
    /// </summary>
    public string? ReleaseNotes { get; set; }
    
    /// <summary>
    /// Checksum SHA256 del paquete completo
    /// </summary>
    public string? Checksum { get; set; }
    
    /// <summary>
    /// Tamaño total de la descarga en bytes
    /// </summary>
    public long DownloadSize { get; set; }
    
    /// <summary>
    /// Lista de archivos incluidos en la actualización
    /// </summary>
    public List<UpdateFile> Files { get; set; } = new();
}

/// <summary>
/// Información de un archivo en la actualización
/// </summary>
public class UpdateFile
{
    /// <summary>
    /// Nombre del archivo (relativo a la carpeta de instalación)
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Checksum SHA256 del archivo individual
    /// </summary>
    public string? Checksum { get; set; }
    
    /// <summary>
    /// Tamaño del archivo en bytes
    /// </summary>
    public long Size { get; set; }
}

/// <summary>
/// Resultado de la verificación de actualizaciones
/// </summary>
public class UpdateCheckResult
{
    /// <summary>
    /// Indica si hay una actualización disponible
    /// </summary>
    public bool UpdateAvailable { get; set; }
    
    /// <summary>
    /// Versión actual instalada
    /// </summary>
    public string CurrentVersion { get; set; } = "1.0.0";
    
    /// <summary>
    /// Información de la nueva versión (si hay actualización)
    /// </summary>
    public VersionInfo? NewVersion { get; set; }
    
    /// <summary>
    /// Mensaje de error si falló la verificación
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Indica si la verificación fue exitosa
    /// </summary>
    public bool Success { get; set; }
}

/// <summary>
/// Estado del progreso de la actualización
/// </summary>
public class UpdateProgress
{
    /// <summary>
    /// Fase actual de la actualización
    /// </summary>
    public UpdatePhase Phase { get; set; }
    
    /// <summary>
    /// Porcentaje de progreso (0-100)
    /// </summary>
    public int Percentage { get; set; }
    
    /// <summary>
    /// Mensaje descriptivo del estado actual
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Archivo actual siendo procesado
    /// </summary>
    public string? CurrentFile { get; set; }
    
    /// <summary>
    /// Bytes transferidos
    /// </summary>
    public long BytesTransferred { get; set; }
    
    /// <summary>
    /// Bytes totales
    /// </summary>
    public long TotalBytes { get; set; }
}

/// <summary>
/// Fases del proceso de actualización
/// </summary>
public enum UpdatePhase
{
    /// <summary>
    /// Verificando si hay actualizaciones
    /// </summary>
    Checking,
    
    /// <summary>
    /// Descargando archivos
    /// </summary>
    Downloading,
    
    /// <summary>
    /// Verificando integridad de archivos
    /// </summary>
    Verifying,
    
    /// <summary>
    /// Preparando la instalación
    /// </summary>
    Preparing,
    
    /// <summary>
    /// Instalando archivos
    /// </summary>
    Installing,
    
    /// <summary>
    /// Actualización completada
    /// </summary>
    Completed,
    
    /// <summary>
    /// Error durante la actualización
    /// </summary>
    Error
}
