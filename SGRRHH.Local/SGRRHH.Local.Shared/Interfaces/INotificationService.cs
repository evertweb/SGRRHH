using SGRRHH.Local.Domain.DTOs;

namespace SGRRHH.Local.Shared.Interfaces;

/// <summary>
/// Interfaz para el servicio de notificaciones con persistencia
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Evento que se dispara cuando hay una nueva notificación
    /// </summary>
    event EventHandler<NotificationEventArgs>? OnNotification;
    
    /// <summary>
    /// Evento para actualizar el contador en la UI
    /// </summary>
    event EventHandler? OnCountChanged;
    
    /// <summary>
    /// Cantidad de notificaciones no leídas (en caché)
    /// </summary>
    int UnreadCount { get; }
    
    /// <summary>
    /// Resumen de notificaciones (en caché)
    /// </summary>
    NotificacionResumenDto? Resumen { get; }
    
    /// <summary>
    /// Inicializa el servicio cargando datos del usuario actual
    /// </summary>
    Task InitializeAsync();
    
    /// <summary>
    /// Obtiene las notificaciones no leídas del usuario actual
    /// </summary>
    Task<List<NotificacionDto>> GetNotificacionesAsync(int limite = 20);
    
    /// <summary>
    /// Obtiene el resumen actualizado
    /// </summary>
    Task<NotificacionResumenDto> GetResumenAsync();
    
    /// <summary>
    /// Envía una notificación (persiste en BD)
    /// </summary>
    Task<int> SendNotificationAsync(CrearNotificacionDto dto);
    
    /// <summary>
    /// Envía una notificación simple (compatible con versión anterior)
    /// </summary>
    Task SendNotificationAsync(string titulo, string mensaje, string tipo, string? link = null);
    
    /// <summary>
    /// Notifica sobre un nuevo permiso pendiente
    /// </summary>
    Task NotificarPermisoNuevoAsync(int permisoId, string empleadoNombre, string tipoPermiso);
    
    /// <summary>
    /// Notifica sobre una nueva solicitud de vacaciones
    /// </summary>
    Task NotificarVacacionNuevaAsync(int vacacionId, string empleadoNombre, int diasSolicitados);
    
    /// <summary>
    /// Notifica sobre una nueva incapacidad
    /// </summary>
    Task NotificarIncapacidadNuevaAsync(int incapacidadId, string empleadoNombre, string tipoIncapacidad);
    
    /// <summary>
    /// Marca una notificación como leída
    /// </summary>
    Task MarkAsReadAsync(int notificationId);
    
    /// <summary>
    /// Marca todas las notificaciones como leídas
    /// </summary>
    Task MarkAllAsReadAsync();
    
    /// <summary>
    /// Refresca el contador y resumen desde la BD
    /// </summary>
    Task RefreshAsync();
    
    /// <summary>
    /// Verifica si hay notificaciones nuevas (permisos pendientes, etc.)
    /// </summary>
    Task CheckForNewNotificationsAsync();
    
    /// <summary>
    /// Limpia notificaciones antiguas (mantenimiento)
    /// </summary>
    Task LimpiarAntiguasAsync(int diasAntiguedad = 30);
}

/// <summary>
/// Argumentos del evento de nueva notificación
/// </summary>
public class NotificationEventArgs : EventArgs
{
    public NotificacionDto Notificacion { get; set; } = new();
}
