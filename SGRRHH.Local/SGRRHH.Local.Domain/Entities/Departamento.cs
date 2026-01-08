namespace SGRRHH.Local.Domain.Entities;

public class Departamento : EntidadBase
{
    public string Codigo { get; set; } = string.Empty;
    
    public string Nombre { get; set; } = string.Empty;
    
    public string? Descripcion { get; set; }
    
    public int? JefeId { get; set; }
    
    public ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();
    
    public ICollection<Cargo> Cargos { get; set; } = new List<Cargo>();
}


