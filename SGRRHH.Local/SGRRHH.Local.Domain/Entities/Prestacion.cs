using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Domain.Entities;

/// <summary>
/// Entidad para gestionar prestaciones sociales según legislación colombiana
/// </summary>
public class Prestacion : EntidadBase
{
    public int EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;
    
    public int Periodo { get; set; } // Año
    
    public TipoPrestacion Tipo { get; set; }
    
    /// <summary>
    /// Fecha inicial del período de cálculo
    /// </summary>
    public DateTime FechaInicio { get; set; }
    
    /// <summary>
    /// Fecha final del período de cálculo
    /// </summary>
    public DateTime FechaFin { get; set; }
    
    /// <summary>
    /// Base salarial para el cálculo (incluye salario + auxilio de transporte si aplica)
    /// </summary>
    public decimal SalarioBase { get; set; }
    
    /// <summary>
    /// Valor calculado de la prestación
    /// </summary>
    public decimal ValorCalculado { get; set; }
    
    /// <summary>
    /// Valor pagado (puede ser diferente al calculado por ajustes)
    /// </summary>
    public decimal ValorPagado { get; set; }
    
    /// <summary>
    /// Fecha de pago efectivo
    /// </summary>
    public DateTime? FechaPago { get; set; }
    
    public EstadoPrestacion Estado { get; set; } = EstadoPrestacion.Calculada;
    
    /// <summary>
    /// Método de pago: Consignación, Nómina, Fondo Cesantías
    /// </summary>
    public string? MetodoPago { get; set; }
    
    /// <summary>
    /// Referencia de comprobante de pago
    /// </summary>
    public string? ComprobanteReferencia { get; set; }
    
    public string? Observaciones { get; set; }
    
    // ===== Campos específicos según tipo =====
    
    /// <summary>
    /// Para intereses sobre cesantías: valor de cesantías sobre el cual se calculan
    /// </summary>
    public decimal? ValorBase { get; set; }
    
    /// <summary>
    /// Porcentaje aplicado (ej: 12% para intereses sobre cesantías)
    /// </summary>
    public decimal? PorcentajeAplicado { get; set; }
    
    /// <summary>
    /// Días proporcionales para cálculos parciales
    /// </summary>
    public int? DiasProporcionales { get; set; }
}
