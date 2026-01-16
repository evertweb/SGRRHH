using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Domain.Entities;

public class EntregaDotacion : EntidadBase
{
    public int EmpleadoId { get; set; }
    
    public DateTime FechaEntrega { get; set; }
    public string Periodo { get; set; } = string.Empty; // "2024-1"
    public TipoEntregaDotacion TipoEntrega { get; set; }
    public int? NumeroEntregaAnual { get; set; } // 1, 2, 3
    
    public EstadoEntregaDotacion Estado { get; set; }
    public DateTime? FechaEntregaReal { get; set; }
    
    public int? DocumentoActaId { get; set; }
    public string? Observaciones { get; set; }
    
    public int? EntregadoPorUsuarioId { get; set; }
    public string? EntregadoPorNombre { get; set; }
    
    // Navegación
    public Empleado? Empleado { get; set; }
    public DocumentoEmpleado? DocumentoActa { get; set; }
    public List<DetalleEntregaDotacion> Detalles { get; set; } = new();
}
