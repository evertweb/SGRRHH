using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Domain.Entities;

public class Contrato : EntidadBase
{
    public int EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;
    
    public TipoContrato TipoContrato { get; set; }
    
    public DateTime FechaInicio { get; set; }
    
    public DateTime? FechaFin { get; set; }
    
    public decimal Salario { get; set; }
    
    public int CargoId { get; set; }
    public Cargo Cargo { get; set; } = null!;
    
    public EstadoContrato Estado { get; set; }
    
    public string? ArchivoAdjuntoPath { get; set; }
    
    public string? Observaciones { get; set; }
    
    // ===== LIQUIDACIÓN Y TERMINACIÓN =====
    
    /// <summary>
    /// Motivo de terminación del contrato según legislación colombiana
    /// </summary>
    public MotivoTerminacionContrato? MotivoTerminacion { get; set; }
    
    /// <summary>
    /// Fecha efectiva de terminación del contrato
    /// </summary>
    public DateTime? FechaTerminacion { get; set; }
    
    /// <summary>
    /// Indica si se pagó indemnización por terminación sin justa causa
    /// </summary>
    public bool PagoIndemnizacion { get; set; }
    
    /// <summary>
    /// Valor de la indemnización pagada (si aplica)
    /// </summary>
    public decimal? ValorIndemnizacion { get; set; }
    
    /// <summary>
    /// Referencia a la liquidación final del contrato
    /// </summary>
    public int? LiquidacionId { get; set; }
    
    /// <summary>
    /// Observaciones adicionales sobre la terminación
    /// </summary>
    public string? ObservacionesTerminacion { get; set; }
}


