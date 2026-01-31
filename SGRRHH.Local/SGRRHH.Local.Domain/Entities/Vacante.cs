using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Domain.Entities;

/// <summary>
/// Representa una vacante de trabajo en la empresa.
/// </summary>
public class Vacante : EntidadBase
{
    public int CargoId { get; set; }
    
    public Cargo? Cargo { get; set; }
    
    public int DepartamentoId { get; set; }
    
    public Departamento? Departamento { get; set; }
    
    public string Titulo { get; set; } = string.Empty;
    
    public string? Descripcion { get; set; }
    
    public string? Requisitos { get; set; }
    
    public decimal? SalarioMinimo { get; set; }
    
    public decimal? SalarioMaximo { get; set; }
    
    public DateTime FechaPublicacion { get; set; }
    
    public DateTime? FechaCierre { get; set; }
    
    public EstadoVacante Estado { get; set; } = EstadoVacante.Borrador;
    
    public int CantidadPosiciones { get; set; } = 1;
    
    /// <summary>
    /// Indica si el registro está activo (para soft delete en la tabla).
    /// </summary>
    public bool EsActivo { get; set; } = true;
    
    // ========== PROPIEDADES CALCULADAS ==========
    
    /// <summary>
    /// Indica si la vacante está abierta para recibir aspirantes.
    /// </summary>
    public bool EstaAbierta => Estado == EstadoVacante.Abierta && 
                               (FechaCierre == null || FechaCierre > DateTime.Today);
    
    /// <summary>
    /// Días transcurridos desde la publicación.
    /// </summary>
    public int DiasPublicada => (DateTime.Today - FechaPublicacion).Days;
    
    // ========== NAVEGACIÓN ==========
    
    public ICollection<Aspirante>? Aspirantes { get; set; }
}
