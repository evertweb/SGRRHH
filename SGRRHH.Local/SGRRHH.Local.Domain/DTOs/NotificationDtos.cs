namespace SGRRHH.Local.Domain.DTOs;

/// <summary>
/// DTO para crear una nueva notificación
/// </summary>
public class CreateNotificationDto
{
    public int? TargetUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "Info";
    public string Category { get; set; } = "Sistema";
    public int Priority { get; set; } = 0;
    public string? Link { get; set; }
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public DateTime? ExpirationDate { get; set; }
}

/// <summary>
/// DTO para mostrar notificación en la UI
/// </summary>
public class NotificationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Icon { get; set; } = "📌";
    public string Type { get; set; } = "Info";
    public string Category { get; set; } = "Sistema";
    public int Priority { get; set; }
    public string? Link { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public string RelativeTime { get; set; } = string.Empty;
    
    /// <summary>
    /// Clase CSS según el tipo
    /// </summary>
    public string CssClass => Type.ToLower() switch
    {
        "success" or "permisoaprobado" or "vacacionaprobada" => "notification-success",
        "error" or "permisorechazado" => "notification-error",
        "warning" => "notification-warning",
        "permisonuevo" or "vacacionnueva" or "incapacidadnueva" => "notification-pending",
        "urgente" => "notification-urgent",
        _ => "notification-info"
    };
    
    /// <summary>
    /// Determina si es urgente
    /// </summary>
    public bool IsUrgent => Priority >= 2;
}

/// <summary>
/// DTO para resumen de notificaciones
/// </summary>
public class NotificationSummaryDto
{
    public int TotalUnread { get; set; }
    public int Urgent { get; set; }
    public int Leaves { get; set; }
    public int Vacations { get; set; }
    public int System { get; set; }
    public List<NotificationDto> LatestNotifications { get; set; } = new();
}

/// <summary>
/// Filtro para consultar notificaciones
/// </summary>
public class NotificationFilterDto
{
    public int? UserId { get; set; }
    public bool? UnreadOnly { get; set; } = true;
    public string? Category { get; set; }
    public string? Type { get; set; }
    public int? Limit { get; set; } = 20;
    public bool IncludeExpired { get; set; } = false;
}
