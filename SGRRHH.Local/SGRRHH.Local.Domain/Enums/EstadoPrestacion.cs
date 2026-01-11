namespace SGRRHH.Local.Domain.Enums;

public enum EstadoPrestacion
{
    /// <summary>
    /// Prestación calculada pero no pagada
    /// </summary>
    Calculada = 1,
    
    /// <summary>
    /// Prestación aprobada para pago
    /// </summary>
    Aprobada = 2,
    
    /// <summary>
    /// Prestación pagada y confirmada
    /// </summary>
    Pagada = 3,
    
    /// <summary>
    /// Prestación consignada en fondo (para cesantías)
    /// </summary>
    Consignada = 4,
    
    /// <summary>
    /// Prestación anulada o ajustada
    /// </summary>
    Anulada = 5
}
