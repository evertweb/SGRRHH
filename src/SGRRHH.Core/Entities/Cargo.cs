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
    
    // ========== NUEVOS CAMPOS ==========
    
    /// <summary>
    /// Salario base del cargo (puede ser null si no se define)
    /// </summary>
    public decimal? SalarioBase { get; set; }
    
    /// <summary>
    /// Requisitos mínimos para ocupar el cargo (experiencia, educación, etc.)
    /// </summary>
    public string? Requisitos { get; set; }
    
    /// <summary>
    /// Competencias y habilidades requeridas para el cargo
    /// </summary>
    public string? Competencias { get; set; }
    
    /// <summary>
    /// ID del cargo inmediato superior en la jerarquía
    /// </summary>
    public int? CargoSuperiorId { get; set; }
    
    /// <summary>
    /// Cargo inmediato superior
    /// </summary>
    public Cargo? CargoSuperior { get; set; }
    
    /// <summary>
    /// Número de plazas/posiciones disponibles para este cargo
    /// </summary>
    public int NumeroPlazas { get; set; } = 1;
    
    // ========== COLECCIONES ==========
    
    /// <summary>
    /// Empleados que tienen este cargo
    /// </summary>
    public ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();
    
    /// <summary>
    /// Cargos que reportan directamente a este cargo
    /// </summary>
    public ICollection<Cargo> CargosSubordinados { get; set; } = new List<Cargo>();
}
