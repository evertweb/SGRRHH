using System.Collections.Generic;

namespace SGRRHH.Local.Domain.Entities;

public class RegistroDiario : EntidadBase
{
    public DateTime Fecha { get; set; }
    
    public int EmpleadoId { get; set; }
    
    public Empleado? Empleado { get; set; }
    
    public TimeSpan? HoraEntrada { get; set; }
    
    public TimeSpan? HoraSalida { get; set; }
    
    public string? Observaciones { get; set; }
    
    public EstadoRegistroDiario Estado { get; set; } = EstadoRegistroDiario.Borrador;
    
    public int? CreadoPorId { get; set; }
    
    public ICollection<DetalleActividad>? DetallesActividades { get; set; } = new List<DetalleActividad>();
    
    public decimal TotalHoras => DetallesActividades?.Sum(d => d.Horas) ?? 0;
    
    public bool EstaCompleto => HoraEntrada.HasValue && HoraSalida.HasValue && (DetallesActividades?.Any() ?? false);
}

public enum EstadoRegistroDiario
{
    Borrador = 0,
    
    Completado = 1,
    
    Aprobado = 2
}


