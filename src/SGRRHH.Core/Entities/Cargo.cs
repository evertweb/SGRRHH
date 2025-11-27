namespace SGRRHH.Core.Entities;

/// <summary>
/// Entidad que representa un cargo dentro de la empresa
/// </summary>
public class Cargo : EntidadBase
{
    /// <summary>
    /// Código único del cargo
    /// </summary>
    public string Codigo { get; set; } = string.Empty;
    
    /// <summary>
    /// Nombre del cargo
    /// </summary>
    public string Nombre { get; set; } = string.Empty;
    
    /// <summary>
    /// Descripción de las funciones del cargo
    /// </summary>
    public string? Descripcion { get; set; }
    
    /// <summary>
    /// Nivel jerárquico del cargo (1=más alto)
    /// </summary>
    public int Nivel { get; set; } = 1;
    
    /// <summary>
    /// ID del departamento al que pertenece el cargo
    /// </summary>
    public int? DepartamentoId { get; set; }
    
    /// <summary>
    /// Departamento al que pertenece el cargo
    /// </summary>
    public Departamento? Departamento { get; set; }
    
    /// <summary>
    /// Empleados que tienen este cargo
    /// </summary>
    public ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();
}
