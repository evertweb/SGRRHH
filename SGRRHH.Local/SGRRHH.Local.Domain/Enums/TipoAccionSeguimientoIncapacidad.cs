namespace SGRRHH.Local.Domain.Enums;

/// <summary>
/// Tipos de acción para el seguimiento de incapacidades
/// </summary>
public enum TipoAccionSeguimientoIncapacidad
{
    /// <summary>Registro inicial de la incapacidad</summary>
    Registro = 1,
    
    /// <summary>Transcripción ante EPS/ARL</summary>
    Transcripcion = 2,
    
    /// <summary>Radicado ante EPS/ARL</summary>
    RadicadoEPS = 3,
    
    /// <summary>Cobro realizado</summary>
    Cobro = 4,
    
    /// <summary>Prórroga registrada</summary>
    Prorroga = 5,
    
    /// <summary>Finalización de incapacidad</summary>
    Finalizacion = 6,
    
    /// <summary>Observación o nota</summary>
    Observacion = 7,
    
    /// <summary>Documento agregado</summary>
    DocumentoAgregado = 8,
    
    /// <summary>Conversión desde permiso</summary>
    ConversionDesdePermiso = 9
}
