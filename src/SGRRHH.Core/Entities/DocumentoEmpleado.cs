using SGRRHH.Core.Enums;

namespace SGRRHH.Core.Entities;

/// <summary>
/// Entidad que representa un documento adjunto al expediente de un empleado
/// </summary>
public class DocumentoEmpleado : EntidadBase
{
    /// <summary>
    /// ID del empleado al que pertenece el documento
    /// </summary>
    public int EmpleadoId { get; set; }
    
    /// <summary>
    /// Navegación al empleado
    /// </summary>
    public Empleado Empleado { get; set; } = null!;
    
    /// <summary>
    /// Tipo de documento
    /// </summary>
    public TipoDocumentoEmpleado TipoDocumento { get; set; }
    
    /// <summary>
    /// Nombre descriptivo del documento
    /// </summary>
    public string Nombre { get; set; } = string.Empty;
    
    /// <summary>
    /// Descripción o notas adicionales
    /// </summary>
    public string? Descripcion { get; set; }
    
    /// <summary>
    /// Ruta del archivo en el sistema de archivos
    /// </summary>
    public string ArchivoPath { get; set; } = string.Empty;
    
    /// <summary>
    /// Nombre original del archivo
    /// </summary>
    public string NombreArchivoOriginal { get; set; } = string.Empty;
    
    /// <summary>
    /// Tamaño del archivo en bytes
    /// </summary>
    public long TamanoArchivo { get; set; }
    
    /// <summary>
    /// Tipo MIME del archivo
    /// </summary>
    public string TipoMime { get; set; } = string.Empty;
    
    /// <summary>
    /// Fecha de vencimiento del documento (si aplica, ej: exámenes médicos)
    /// </summary>
    public DateTime? FechaVencimiento { get; set; }
    
    /// <summary>
    /// Fecha de emisión del documento
    /// </summary>
    public DateTime? FechaEmision { get; set; }
    
    /// <summary>
    /// Indica si el documento está vigente
    /// </summary>
    public bool EstaVigente => !FechaVencimiento.HasValue || FechaVencimiento.Value >= DateTime.Today;
    
    /// <summary>
    /// ID del usuario que subió el documento
    /// </summary>
    public int? SubidoPorUsuarioId { get; set; }
    
    /// <summary>
    /// Nombre del usuario que subió el documento
    /// </summary>
    public string? SubidoPorNombre { get; set; }
}
