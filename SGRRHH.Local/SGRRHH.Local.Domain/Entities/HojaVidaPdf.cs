using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Domain.Entities;

/// <summary>
/// Metadatos de un PDF de hoja de vida procesado por el sistema.
/// </summary>
public class HojaVidaPdf : EntidadBase
{
    /// <summary>
    /// ID del aspirante si el PDF pertenece a un aspirante.
    /// </summary>
    public int? AspiranteId { get; set; }
    
    public Aspirante? Aspirante { get; set; }
    
    /// <summary>
    /// ID del empleado si el PDF pertenece a un empleado.
    /// </summary>
    public int? EmpleadoId { get; set; }
    
    public Empleado? Empleado { get; set; }
    
    /// <summary>
    /// ID del documento si está almacenado en documentos_empleado.
    /// </summary>
    public int? DocumentoEmpleadoId { get; set; }
    
    public DocumentoEmpleado? DocumentoEmpleado { get; set; }
    
    /// <summary>
    /// Número de versión del PDF (incrementa con cada actualización).
    /// </summary>
    public int Version { get; set; } = 1;
    
    /// <summary>
    /// Hash SHA256 del contenido del PDF para detectar cambios.
    /// </summary>
    public string HashContenido { get; set; } = string.Empty;
    
    /// <summary>
    /// Origen del PDF (Forestech, Externo, Manual).
    /// </summary>
    public OrigenHojaVida Origen { get; set; }
    
    /// <summary>
    /// Fecha en que se generó el PDF (solo para PDFs Forestech).
    /// </summary>
    public DateTime? FechaGeneracion { get; set; }
    
    /// <summary>
    /// Fecha en que se subió el PDF al sistema.
    /// </summary>
    public DateTime FechaSubida { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Datos extraídos del PDF en formato JSON.
    /// </summary>
    public string? DatosExtraidos { get; set; }
    
    /// <summary>
    /// Indica si el PDF tiene firma válida.
    /// </summary>
    public bool TieneFirma { get; set; }
    
    /// <summary>
    /// Indica si el PDF pasó todas las validaciones.
    /// </summary>
    public bool EsValido { get; set; } = true;
    
    /// <summary>
    /// Errores encontrados durante la validación en formato JSON.
    /// </summary>
    public string? ErroresValidacion { get; set; }
    
    /// <summary>
    /// Indica si el registro está activo (para soft delete en la tabla).
    /// </summary>
    public bool EsActivo { get; set; } = true;
    
    // ========== PROPIEDADES CALCULADAS ==========
    
    /// <summary>
    /// Indica si es un PDF generado por el sistema Forestech.
    /// </summary>
    public bool EsForestechPdf => Origen == OrigenHojaVida.Forestech;
    
    /// <summary>
    /// Indica si el PDF está listo para procesar (válido y con firma).
    /// </summary>
    public bool ListoParaProcesar => EsValido && TieneFirma;
}
