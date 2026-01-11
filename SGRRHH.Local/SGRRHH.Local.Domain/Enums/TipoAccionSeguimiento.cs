namespace SGRRHH.Local.Domain.Enums;

/// <summary>
/// Tipos de acciones registradas en el seguimiento de permisos
/// </summary>
public enum TipoAccionSeguimiento
{
    /// <summary>Solicitud inicial del permiso</summary>
    Solicitud = 1,
    
    /// <summary>Aprobación del permiso</summary>
    Aprobacion = 2,
    
    /// <summary>Rechazo del permiso</summary>
    Rechazo = 3,
    
    /// <summary>Documento soporte entregado</summary>
    DocumentoEntregado = 4,
    
    /// <summary>Se inició el proceso de compensación de horas</summary>
    CompensacionIniciada = 5,
    
    /// <summary>Se registraron horas compensadas</summary>
    HorasCompensadas = 6,
    
    /// <summary>Permiso completado y cerrado</summary>
    Completado = 7,
    
    /// <summary>Observación o nota adicional</summary>
    Observacion = 8,
    
    /// <summary>Documento rechazado (no válido)</summary>
    DocumentoRechazado = 9,
    
    /// <summary>Compensación vencida sin completar</summary>
    CompensacionVencida = 10,
    
    /// <summary>Cambio de resolución (ej: de Remunerado a Descontado)</summary>
    CambioResolucion = 11,
    
    /// <summary>Documento vencido sin entregar</summary>
    DocumentoVencido = 12
}
