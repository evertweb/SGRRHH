namespace SGRRHH.Local.Domain.Interfaces;

/// <summary>
/// Servicio para mantenimiento de la base de datos.
/// Incluye operaciones de limpieza y reset para distribución.
/// </summary>
public interface IDatabaseMaintenanceService
{
    /// <summary>
    /// Limpia todas las tablas excepto usuarios.
    /// Preserva los usuarios actuales del sistema.
    /// </summary>
    /// <returns>Resultado con el número de registros eliminados</returns>
    Task<DatabaseCleanupResult> LimpiarBaseDatosAsync();
    
    /// <summary>
    /// Resetea completamente la base de datos para distribución.
    /// Elimina TODO incluyendo usuarios y crea un usuario admin por defecto.
    /// Usuario por defecto: admin / Admin123!
    /// </summary>
    /// <returns>Resultado de la operación</returns>
    Task<DatabaseCleanupResult> ResetearParaDistribucionAsync();
    
    /// <summary>
    /// Obtiene estadísticas de la base de datos actual.
    /// </summary>
    Task<DatabaseStats> ObtenerEstadisticasAsync();
}

/// <summary>
/// Resultado de operaciones de limpieza de base de datos.
/// </summary>
public class DatabaseCleanupResult
{
    public bool Exitoso { get; set; }
    public int TotalRegistrosEliminados { get; set; }
    public int TablasLimpiadas { get; set; }
    public int UsuariosPreservados { get; set; }
    public string? MensajeError { get; set; }
    public List<string> TablasAfectadas { get; set; } = new();
    public DateTime FechaOperacion { get; set; } = DateTime.Now;
}

/// <summary>
/// Estadísticas de la base de datos.
/// </summary>
public class DatabaseStats
{
    public long TamanoBytes { get; set; }
    public int TotalUsuarios { get; set; }
    public int TotalEmpleados { get; set; }
    public int TotalRegistros { get; set; }
    public DateTime? UltimoBackup { get; set; }
    public Dictionary<string, int> RegistrosPorTabla { get; set; } = new();
}
