namespace SGRRHH.Local.Domain.Entities;

public class Cargo : EntidadBase
{
    public string Codigo { get; set; } = string.Empty;
    
    public string Nombre { get; set; } = string.Empty;
    
    public string? Descripcion { get; set; }
    
    public int Nivel { get; set; } = 1;
    
    public int? DepartamentoId { get; set; }
    
    public Departamento? Departamento { get; set; }
    
    // ========== NUEVOS CAMPOS ==========
    
    public decimal? SalarioBase { get; set; }
    
    public string? Requisitos { get; set; }
    
    public string? Competencias { get; set; }
    
    public int? CargoSuperiorId { get; set; }
    
    public Cargo? CargoSuperior { get; set; }
    
    public int NumeroPlazas { get; set; } = 1;
    
    // ========== COLECCIONES ==========
    
    public ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();
    
    public ICollection<Cargo> CargosSubordinados { get; set; } = new List<Cargo>();
}


