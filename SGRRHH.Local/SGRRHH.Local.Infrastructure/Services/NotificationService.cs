using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Shared.Interfaces;
using Microsoft.Extensions.Logging;

namespace SGRRHH.Local.Infrastructure.Services;

/// <summary>
/// Servicio de notificaciones en tiempo real
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IPermisoRepository _permisoRepository;
    private readonly IVacacionRepository _vacacionRepository;
    private readonly IAuthService _authService;
    private readonly ILogger<NotificationService> _logger;
    
    private readonly List<AppNotification> _notifications = new();
    private int _nextId = 1;
    private DateTime _lastCheck = DateTime.MinValue;
    
    public event EventHandler<NotificationEventArgs>? OnNotification;
    
    public List<AppNotification> UnreadNotifications => 
        _notifications.Where(n => !n.IsRead).OrderByDescending(n => n.CreatedAt).ToList();
    
    public int UnreadCount => _notifications.Count(n => !n.IsRead);
    
    public NotificationService(
        IPermisoRepository permisoRepository,
        IVacacionRepository vacacionRepository,
        IAuthService authService,
        ILogger<NotificationService> logger)
    {
        _permisoRepository = permisoRepository;
        _vacacionRepository = vacacionRepository;
        _authService = authService;
        _logger = logger;
    }
    
    public async Task SendNotificationAsync(string title, string message, NotificationType type, string? link = null)
    {
        var notification = new AppNotification
        {
            Id = _nextId++,
            Title = title,
            Message = message,
            Type = type,
            CreatedAt = DateTime.Now,
            IsRead = false,
            Link = link
        };
        
        _notifications.Add(notification);
        
        // Limitar a las últimas 50 notificaciones
        while (_notifications.Count > 50)
        {
            _notifications.RemoveAt(0);
        }
        
        _logger.LogInformation("Nueva notificación: {Title}", title);
        
        OnNotification?.Invoke(this, new NotificationEventArgs { Notification = notification });
        
        await Task.CompletedTask;
    }
    
    public void MarkAsRead(int notificationId)
    {
        var notification = _notifications.FirstOrDefault(n => n.Id == notificationId);
        if (notification != null)
        {
            notification.IsRead = true;
        }
    }
    
    public void MarkAllAsRead()
    {
        foreach (var notification in _notifications)
        {
            notification.IsRead = true;
        }
    }
    
    public async Task CheckForNewNotificationsAsync()
    {
        if (!_authService.IsAuthenticated) return;
        
        try
        {
            // Solo verificar si es aprobador/admin
            if (_authService.IsAprobador)
            {
                await CheckPendingPermisosAsync();
                await CheckPendingVacacionesAsync();
            }
            
            _lastCheck = DateTime.Now;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al verificar notificaciones");
        }
    }
    
    private async Task CheckPendingPermisosAsync()
    {
        try
        {
            var permisos = await _permisoRepository.GetAllAsync();
            var pendientes = permisos.Where(p => 
                p.Estado == EstadoPermiso.Pendiente && 
                p.FechaSolicitud > _lastCheck).ToList();
            
            foreach (var permiso in pendientes)
            {
                await SendNotificationAsync(
                    "Nuevo Permiso Pendiente",
                    $"{permiso.Empleado?.NombreCompleto ?? "Empleado"} ha solicitado un permiso de {permiso.TipoPermiso?.Nombre ?? "N/A"}",
                    NotificationType.PermisoNuevo,
                    $"/permisos/{permiso.Id}"
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al verificar permisos pendientes");
        }
    }
    
    private async Task CheckPendingVacacionesAsync()
    {
        try
        {
            var vacaciones = await _vacacionRepository.GetAllAsync();
            var pendientes = vacaciones.Where(v => 
                v.Estado == EstadoVacacion.Pendiente && 
                v.FechaSolicitud > _lastCheck).ToList();
            
            foreach (var vacacion in pendientes)
            {
                await SendNotificationAsync(
                    "Nueva Solicitud de Vacaciones",
                    $"{vacacion.Empleado?.NombreCompleto ?? "Empleado"} ha solicitado vacaciones ({vacacion.DiasTomados} días)",
                    NotificationType.VacacionNueva,
                    $"/vacaciones/{vacacion.Id}"
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al verificar vacaciones pendientes");
        }
    }
}
