namespace SGRRHH.Local.Domain.Enums;

public enum EstadoPermiso
{
    /// <summary>Esperando aprobación de ingeniería</summary>
    Pendiente = 1,
    
    /// <summary>Aprobado y completamente cerrado</summary>
    Aprobado = 2,
    
    /// <summary>Rechazado por ingeniería</summary>
    Rechazado = 3,
    
    /// <summary>Cancelado por el solicitante</summary>
    Cancelado = 4,
    
    /// <summary>Aprobado pero falta entregar documento</summary>
    AprobadoPendienteDocumento = 5,
    
    /// <summary>Aprobado, en proceso de compensación de horas</summary>
    AprobadoEnCompensacion = 6,
    
    /// <summary>Completado y cerrado (documento entregado o compensado)</summary>
    Completado = 7
}


