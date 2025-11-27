namespace SGRRHH.Core.Entities;

/// <summary>
/// Entidad que representa un departamento de la empresa
/// </summary>
public class Departamento : EntidadBase
{
    /// <summary>
    /// Código único del departamento
    /// </summary>
    public string Codigo { get; set; } = string.Empty;
    
    /// <summary>
    /// Nombre del departamento
    /// </summary>
    public string Nombre { get; set; } = string.Empty;
    
    /// <summary>
    /// Descripción del departamento
    /// </summary>
    public string? Descripcion { get; set; }
    
    /// <summary>
    /// ID del jefe del departamento (empleado)
    /// </summary>
    public int? JefeId { get; set; }
    
    /// <summary>
    /// Empleados que pertenecen a este departamento
    /// </summary>
    public ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();
    
    /// <summary>
    /// Cargos asociados a este departamento
    /// </summary>
    public ICollection<Cargo> Cargos { get; set; } = new List<Cargo>();
}
