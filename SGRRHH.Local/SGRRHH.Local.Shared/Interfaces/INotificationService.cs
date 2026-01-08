using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Shared.Interfaces;

/// <summary>
/// Interfaz para el servicio de notificaciones en tiempo real
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Evento que se dispara cuando hay una nueva notificación
    /// </summary>
    event EventHandler<NotificationEventArgs>? OnNotification;
    
    /// <summary>
    /// Lista de notificaciones no leídas
    /// </summary>
    List<AppNotification> UnreadNotifications { get; }
    
    /// <summary>
    /// Cantidad de notificaciones no leídas
    /// </summary>
    int UnreadCount { get; }
    
    /// <summary>
    /// Envía una notificación
    /// </summary>
    Task SendNotificationAsync(string title, string message, NotificationType type, string? link = null);
    
    /// <summary>
    /// Marca una notificación como leída
    /// </summary>
    void MarkAsRead(int notificationId);
    
    /// <summary>
    /// Marca todas las notificaciones como leídas
    /// </summary>
    void MarkAllAsRead();
    
    /// <summary>
    /// Verifica si hay notificaciones nuevas (permisos pendientes, etc.)
    /// </summary>
    Task CheckForNewNotificationsAsync();
}

public class NotificationEventArgs : EventArgs
{
    public AppNotification Notification { get; set; } = new();
}

public class AppNotification
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public NotificationType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
    public string? Link { get; set; }
}

public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error,
    PermisoNuevo,
    PermisoAprobado,
    PermisoRechazado,
    VacacionNueva,
    VacacionAprobada,
    Sistema
}
