namespace SGRRHH.Local.Domain.Enums;

/// <summary>
/// Tipos de prestaciones sociales según legislación colombiana
/// </summary>
public enum TipoPrestacion
{
    /// <summary>
    /// Cesantías: 1 mes de salario por cada año trabajado
    /// </summary>
    Cesantias = 1,
    
    /// <summary>
    /// Intereses sobre cesantías: 12% anual sobre saldo de cesantías
    /// </summary>
    InteresesCesantias = 2,
    
    /// <summary>
    /// Prima de servicios: 30 días de salario por año (2 pagos: junio y diciembre)
    /// </summary>
    PrimaServicios = 3,
    
    /// <summary>
    /// Dotación: 3 veces al año (abril, agosto, diciembre) para salarios menores a 2 SMLMV
    /// </summary>
    Dotacion = 4,
    
    /// <summary>
    /// Auxilio de transporte: Mensual para salarios menores a 2 SMLMV
    /// </summary>
    AuxilioTransporte = 5,
    
    /// <summary>
    /// Bonificación no constitutiva de salario
    /// </summary>
    Bonificacion = 6
}
