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
}


