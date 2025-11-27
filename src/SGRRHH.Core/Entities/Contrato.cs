using SGRRHH.Core.Enums;

namespace SGRRHH.Core.Entities;

public class Contrato : EntidadBase
{
    public int EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;
    
    public TipoContrato TipoContrato { get; set; }
    
    public DateTime FechaInicio { get; set; }
    
    /// <summary>
    /// Fecha de terminación del contrato. Null para contratos indefinidos activos.
    /// </summary>
    public DateTime? FechaFin { get; set; }
    
    public decimal Salario { get; set; }
    
    public int CargoId { get; set; }
    public Cargo Cargo { get; set; } = null!;
    
    public EstadoContrato Estado { get; set; }
    
    /// <summary>
    /// Ruta del archivo del contrato firmado (opcional)
    /// </summary>
    public string? ArchivoAdjuntoPath { get; set; }
    
    public string? Observaciones { get; set; }
}
