namespace SGRRHH.Core.Entities;

/// <summary>
/// Entidad que representa la presencia/estado de un usuario en el sistema.
/// Permite saber qué usuarios están activos/online en tiempo real.
/// </summary>
public class UserPresence : EntidadBase
{
    /// <summary>
    /// ID del usuario asociado (numérico interno)
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Firebase UID del usuario (identificador único en Firebase)
    /// </summary>
    public string? FirebaseUid { get; set; }
    
    /// <summary>
    /// Nombre de usuario para mostrar
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Nombre completo del usuario
    /// </summary>
    public string NombreCompleto { get; set; } = string.Empty;
    
    /// <summary>
    /// Indica si el usuario está actualmente online
    /// </summary>
    public bool IsOnline { get; set; }
    
    /// <summary>
    /// Última vez que el usuario estuvo activo
    /// </summary>
    public DateTime LastSeen { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Última vez que se actualizó el heartbeat
    /// </summary>
    public DateTime LastHeartbeat { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Nombre del dispositivo/PC desde donde se conectó
    /// </summary>
    public string? DeviceName { get; set; }
    
    /// <summary>
    /// Estado personalizado del usuario (ej: "Disponible", "Ocupado", "Ausente")
    /// </summary>
    public string Status { get; set; } = "Disponible";
    
    /// <summary>
    /// Mensaje de estado personalizado (opcional)
    /// </summary>
    public string? StatusMessage { get; set; }
}
