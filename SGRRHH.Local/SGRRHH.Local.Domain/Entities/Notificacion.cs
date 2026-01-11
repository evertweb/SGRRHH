namespace SGRRHH.Local.Domain.Entities;

/// <summary>
/// Entidad que representa una notificación del sistema
/// </summary>
public class Notificacion
{
    public int Id { get; set; }
    
    /// <summary>
    /// Usuario destinatario. NULL significa para todos los aprobadores
    /// </summary>
    public int? UsuarioDestinoId { get; set; }
    
    /// <summary>
    /// Título corto de la notificación
    /// </summary>
    public string Titulo { get; set; } = string.Empty;
    
    /// <summary>
    /// Mensaje descriptivo
    /// </summary>
    public string Mensaje { get; set; } = string.Empty;
    
    /// <summary>
    /// Icono emoji para mostrar
    /// </summary>
    public string Icono { get; set; } = "📌";
    
    /// <summary>
    /// Tipo de notificación (Info, Success, Warning, Error, etc.)
    /// </summary>
    public string Tipo { get; set; } = "Info";
    
    /// <summary>
    /// Categoría (Sistema, Permiso, Vacacion, Incapacidad, etc.)
    /// </summary>
    public string Categoria { get; set; } = "Sistema";
    
    /// <summary>
    /// Prioridad: 0=Normal, 1=Alta, 2=Urgente
    /// </summary>
    public int Prioridad { get; set; } = 0;
    
    /// <summary>
    /// Link de navegación al hacer clic
    /// </summary>
    public string? Link { get; set; }
    
    /// <summary>
    /// Tipo de entidad relacionada (Permiso, Vacacion, etc.)
    /// </summary>
    public string? EntidadTipo { get; set; }
    
    /// <summary>
    /// ID de la entidad relacionada
    /// </summary>
    public int? EntidadId { get; set; }
    
    /// <summary>
    /// Si la notificación ha sido leída
    /// </summary>
    public bool Leida { get; set; } = false;
    
    /// <summary>
    /// Fecha y hora en que fue leída
    /// </summary>
    public DateTime? FechaLectura { get; set; }
    
    /// <summary>
    /// Fecha de creación
    /// </summary>
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Usuario que creó la notificación
    /// </summary>
    public string? CreadoPor { get; set; }
    
    /// <summary>
    /// Fecha de expiración (después de esta fecha no se muestra)
    /// </summary>
    public DateTime? FechaExpiracion { get; set; }
    
    // Navegación
    public Usuario? UsuarioDestino { get; set; }
}
