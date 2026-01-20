namespace SGRRHH.Local.Domain.Enums;

/// <summary>
/// Tipos de acción para el seguimiento de incapacidades
/// </summary>
public enum SickLeaveTrackingActionType
{
    /// <summary>Registro inicial de la incapacidad</summary>
    Registration = 1,
    
    /// <summary>Transcripción ante EPS/ARL</summary>
    Transcription = 2,
    
    /// <summary>Radicado ante EPS/ARL</summary>
    FiledWithEPS = 3,
    
    /// <summary>Cobro realizado</summary>
    Collection = 4,
    
    /// <summary>Prórroga registrada</summary>
    Extension = 5,
    
    /// <summary>Finalización de incapacidad</summary>
    Completion = 6,
    
    /// <summary>Observación o nota</summary>
    Observation = 7,
    
    /// <summary>Documento agregado</summary>
    DocumentAdded = 8,
    
    /// <summary>Conversión desde permiso</summary>
    ConvertedFromLeave = 9
}
