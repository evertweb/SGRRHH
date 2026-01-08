namespace SGRRHH.Local.Domain.Entities;

public class DetalleActividad : EntidadBase
{
    public int RegistroDiarioId { get; set; }
    
    public RegistroDiario? RegistroDiario { get; set; }
    
    public int ActividadId { get; set; }
    
    public Actividad? Actividad { get; set; }
    
    public int? ProyectoId { get; set; }
    
    public Proyecto? Proyecto { get; set; }
    
    public decimal Horas { get; set; }
    
    public string? Descripcion { get; set; }
    
    public TimeSpan? HoraInicio { get; set; }
    
    public TimeSpan? HoraFin { get; set; }
    
    public int Orden { get; set; }
}


