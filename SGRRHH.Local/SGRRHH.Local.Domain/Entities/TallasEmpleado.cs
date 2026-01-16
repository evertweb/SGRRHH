namespace SGRRHH.Local.Domain.Entities;

public class TallasEmpleado : EntidadBase
{
    public int EmpleadoId { get; set; }
    
    // Tallas Ropa
    public string? TallaCamisa { get; set; } // S, M, L, XL, XXL, XXXL
    public string? TallaPantalon { get; set; } // 28, 30, 32, 34, 36, 38, 40, 42, 44
    public string? TallaOverall { get; set; }
    public string? TallaChaqueta { get; set; }
    
    // Tallas Calzado
    public int? TallaCalzadoNumero { get; set; } // 36-46
    public string? AnchoCalzado { get; set; } // Normal, Ancho
    public string? TipoCalzadoPreferido { get; set; } // Bota, Zapato, Tenis
    
    // Tallas Protección
    public string? TallaGuantes { get; set; } // S, M, L, XL
    public string? TallaCasco { get; set; } // Ajustable, S, M, L
    public string? TallaGafas { get; set; } // Universal, Graduadas
    
    public string? Observaciones { get; set; }
    
    // Navegación
    public Empleado? Empleado { get; set; }
}
