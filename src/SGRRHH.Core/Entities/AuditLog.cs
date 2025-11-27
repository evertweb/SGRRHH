namespace SGRRHH.Core.Entities;

/// <summary>
/// Entidad para registrar auditoría de acciones importantes del sistema
/// </summary>
public class AuditLog : EntidadBase
{
    /// <summary>
    /// Fecha y hora de la acción
    /// </summary>
    public DateTime FechaHora { get; set; } = DateTime.Now;
    
    /// <summary>
    /// ID del usuario que realizó la acción
    /// </summary>
    public int? UsuarioId { get; set; }
    
    /// <summary>
    /// Nombre del usuario al momento de la acción (para persistencia)
    /// </summary>
    public string UsuarioNombre { get; set; } = string.Empty;
    
    /// <summary>
    /// Tipo de acción realizada
    /// </summary>
    public string Accion { get; set; } = string.Empty;
    
    /// <summary>
    /// Entidad afectada (ej: Empleado, Permiso, Usuario)
    /// </summary>
    public string Entidad { get; set; } = string.Empty;
    
    /// <summary>
    /// ID del registro afectado
    /// </summary>
    public int? EntidadId { get; set; }
    
    /// <summary>
    /// Descripción detallada de la acción
    /// </summary>
    public string Descripcion { get; set; } = string.Empty;
    
    /// <summary>
    /// Dirección IP desde donde se realizó la acción (opcional)
    /// </summary>
    public string? DireccionIp { get; set; }
    
    /// <summary>
    /// Datos adicionales en formato JSON (opcional)
    /// </summary>
    public string? DatosAdicionales { get; set; }
    
    // Navegación
    public Usuario? Usuario { get; set; }
}
