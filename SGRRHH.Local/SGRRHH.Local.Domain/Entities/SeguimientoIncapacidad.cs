using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Domain.Entities;

/// <summary>
/// Registra el historial de acciones sobre una incapacidad
/// </summary>
public class SeguimientoIncapacidad : EntidadBase
{
    /// <summary>ID de la incapacidad</summary>
    public int IncapacidadId { get; set; }
    
    /// <summary>Incapacidad relacionada</summary>
    public Incapacidad Incapacidad { get; set; } = null!;
    
    /// <summary>Fecha y hora de la acción</summary>
    public DateTime FechaAccion { get; set; } = DateTime.Now;
    
    /// <summary>Tipo de acción realizada</summary>
    public TipoAccionSeguimientoIncapacidad TipoAccion { get; set; }
    
    /// <summary>Descripción de la acción</summary>
    public string? Descripcion { get; set; }
    
    /// <summary>ID del usuario que realizó la acción</summary>
    public int RealizadoPorId { get; set; }
    
    /// <summary>Usuario que realizó la acción</summary>
    public Usuario RealizadoPor { get; set; } = null!;
    
    /// <summary>Datos adicionales en JSON</summary>
    public string? DatosAdicionales { get; set; }
    
    // ===== PROPIEDADES CALCULADAS =====
    
    /// <summary>Nombre descriptivo del tipo de acción</summary>
    public string TipoAccionNombre => TipoAccion switch
    {
        TipoAccionSeguimientoIncapacidad.Registro => "Registro",
        TipoAccionSeguimientoIncapacidad.Transcripcion => "Transcripción",
        TipoAccionSeguimientoIncapacidad.RadicadoEPS => "Radicado EPS",
        TipoAccionSeguimientoIncapacidad.Cobro => "Cobro",
        TipoAccionSeguimientoIncapacidad.Prorroga => "Prórroga",
        TipoAccionSeguimientoIncapacidad.Finalizacion => "Finalización",
        TipoAccionSeguimientoIncapacidad.Observacion => "Observación",
        TipoAccionSeguimientoIncapacidad.DocumentoAgregado => "Documento Agregado",
        TipoAccionSeguimientoIncapacidad.ConversionDesdePermiso => "Conversión desde Permiso",
        _ => "Desconocido"
    };
}
