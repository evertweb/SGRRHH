namespace SGRRHH.Local.Domain.DTOs;

/// <summary>
/// DTO para crear una nueva notificación
/// </summary>
public class CrearNotificacionDto
{
    public int? UsuarioDestinoId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string Tipo { get; set; } = "Info";
    public string Categoria { get; set; } = "Sistema";
    public int Prioridad { get; set; } = 0;
    public string? Link { get; set; }
    public string? EntidadTipo { get; set; }
    public int? EntidadId { get; set; }
    public DateTime? FechaExpiracion { get; set; }
}

/// <summary>
/// DTO para mostrar notificación en la UI
/// </summary>
public class NotificacionDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string Icono { get; set; } = "📌";
    public string Tipo { get; set; } = "Info";
    public string Categoria { get; set; } = "Sistema";
    public int Prioridad { get; set; }
    public string? Link { get; set; }
    public bool Leida { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string TiempoRelativo { get; set; } = string.Empty;
    
    /// <summary>
    /// Clase CSS según el tipo
    /// </summary>
    public string CssClass => Tipo.ToLower() switch
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
    public bool EsUrgente => Prioridad >= 2;
}

/// <summary>
/// DTO para resumen de notificaciones
/// </summary>
public class NotificacionResumenDto
{
    public int TotalNoLeidas { get; set; }
    public int Urgentes { get; set; }
    public int Permisos { get; set; }
    public int Vacaciones { get; set; }
    public int Sistema { get; set; }
    public List<NotificacionDto> UltimasNotificaciones { get; set; } = new();
}

/// <summary>
/// Filtro para consultar notificaciones
/// </summary>
public class NotificacionFiltroDto
{
    public int? UsuarioId { get; set; }
    public bool? SoloNoLeidas { get; set; } = true;
    public string? Categoria { get; set; }
    public string? Tipo { get; set; }
    public int? Limite { get; set; } = 20;
    public bool IncluirExpiradas { get; set; } = false;
}
