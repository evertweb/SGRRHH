using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Domain.Entities;

public class DetalleEntregaDotacion : EntidadBase
{
    public int EntregaId { get; set; }
    
    public CategoriaElementoDotacion CategoriaElemento { get; set; }
    public string NombreElemento { get; set; } = string.Empty;
    public int Cantidad { get; set; } = 1;
    public string? Talla { get; set; }
    
    public bool EsDotacionLegal { get; set; }
    public bool EsEPP { get; set; }
    
    public string? Marca { get; set; }
    public string? Referencia { get; set; }
    public decimal? ValorUnitario { get; set; }
    
    public string? Observaciones { get; set; }
    
    // Navegación
    public EntregaDotacion? Entrega { get; set; }
}
