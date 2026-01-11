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
    
    // ===== HORAS EXTRAS Y RECARGOS (Colombia) =====
    
    /// <summary>
    /// Horas extras diurnas (6:00 AM - 9:00 PM, excede jornada ordinaria) - Recargo 25%
    /// </summary>
    public decimal HorasExtrasDiurnas { get; set; }
    
    /// <summary>
    /// Horas extras nocturnas (9:00 PM - 6:00 AM, excede jornada ordinaria) - Recargo 75%
    /// </summary>
    public decimal HorasExtrasNocturnas { get; set; }
    
    /// <summary>
    /// Horas nocturnas ordinarias (9:00 PM - 6:00 AM, dentro de jornada) - Recargo 35%
    /// </summary>
    public decimal HorasNocturnas { get; set; }
    
    /// <summary>
    /// Horas dominicales o festivos (cualquier hora) - Recargo 75%
    /// </summary>
    public decimal HorasDominicalesFestivos { get; set; }
    
    /// <summary>
    /// Horas extras dominicales/festivos nocturnas - Recargo 110% (75% + 35%)
    /// </summary>
    public decimal HorasExtrasDominicalesNocturnas { get; set; }
    
    /// <summary>
    /// Indica si el día corresponde a un domingo o festivo
    /// </summary>
    public bool EsDominicalOFestivo { get; set; }
    
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


