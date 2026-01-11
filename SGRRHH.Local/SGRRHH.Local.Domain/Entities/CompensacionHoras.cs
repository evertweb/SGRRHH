namespace SGRRHH.Local.Domain.Entities;

/// <summary>
/// Registra las compensaciones de horas realizadas para un permiso
/// </summary>
public class CompensacionHoras : EntidadBase
{
    /// <summary>ID del permiso al que pertenece esta compensación</summary>
    public int PermisoId { get; set; }
    
    /// <summary>Permiso relacionado</summary>
    public Permiso Permiso { get; set; } = null!;
    
    /// <summary>Fecha en que se realizó la compensación</summary>
    public DateTime FechaCompensacion { get; set; }
    
    /// <summary>Cantidad de horas compensadas</summary>
    public int HorasCompensadas { get; set; }
    
    /// <summary>Descripción de la actividad realizada durante la compensación</summary>
    public string? Descripcion { get; set; }
    
    /// <summary>ID del usuario que aprobó la compensación</summary>
    public int? AprobadoPorId { get; set; }
    
    /// <summary>Usuario que aprobó la compensación</summary>
    public Usuario? AprobadoPor { get; set; }
    
    /// <summary>Fecha en que se aprobó la compensación</summary>
    public DateTime? FechaAprobacion { get; set; }
}
