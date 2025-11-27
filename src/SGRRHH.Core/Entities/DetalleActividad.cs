namespace SGRRHH.Core.Entities;

/// <summary>
/// Entidad que representa el detalle de una actividad realizada en un día
/// </summary>
public class DetalleActividad : EntidadBase
{
    /// <summary>
    /// ID del registro diario al que pertenece
    /// </summary>
    public int RegistroDiarioId { get; set; }
    
    /// <summary>
    /// Registro diario asociado
    /// </summary>
    public RegistroDiario? RegistroDiario { get; set; }
    
    /// <summary>
    /// ID de la actividad realizada
    /// </summary>
    public int ActividadId { get; set; }
    
    /// <summary>
    /// Actividad realizada
    /// </summary>
    public Actividad? Actividad { get; set; }
    
    /// <summary>
    /// ID del proyecto (opcional)
    /// </summary>
    public int? ProyectoId { get; set; }
    
    /// <summary>
    /// Proyecto asociado (si aplica)
    /// </summary>
    public Proyecto? Proyecto { get; set; }
    
    /// <summary>
    /// Horas dedicadas a esta actividad
    /// </summary>
    public decimal Horas { get; set; }
    
    /// <summary>
    /// Descripción específica de lo realizado
    /// </summary>
    public string? Descripcion { get; set; }
    
    /// <summary>
    /// Hora de inicio de la actividad (opcional para mayor detalle)
    /// </summary>
    public TimeSpan? HoraInicio { get; set; }
    
    /// <summary>
    /// Hora de fin de la actividad (opcional para mayor detalle)
    /// </summary>
    public TimeSpan? HoraFin { get; set; }
    
    /// <summary>
    /// Orden de la actividad en el día
    /// </summary>
    public int Orden { get; set; }
}
