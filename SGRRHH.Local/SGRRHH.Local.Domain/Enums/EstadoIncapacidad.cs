namespace SGRRHH.Local.Domain.Enums;

/// <summary>
/// Estados del ciclo de vida de una incapacidad
/// </summary>
public enum EstadoIncapacidad
{
    /// <summary>Incapacidad activa, empleado en reposo</summary>
    Activa = 1,
    
    /// <summary>Empleado ya retornó, incapacidad terminada</summary>
    Finalizada = 2,
    
    /// <summary>Transcrita ante EPS/ARL</summary>
    Transcrita = 3,
    
    /// <summary>Cobrada a EPS/ARL</summary>
    Cobrada = 4,
    
    /// <summary>Cancelada (error de registro, etc.)</summary>
    Cancelada = 5
}
