using System.Collections.Generic;

namespace SGRRHH.Core.Entities;

/// <summary>
/// Entidad que representa el registro diario de un empleado
/// </summary>
public class RegistroDiario : EntidadBase
{
    /// <summary>
    /// Fecha del registro
    /// </summary>
    public DateTime Fecha { get; set; }
    
    /// <summary>
    /// ID del empleado
    /// </summary>
    public int EmpleadoId { get; set; }
    
    /// <summary>
    /// Empleado asociado
    /// </summary>
    public Empleado? Empleado { get; set; }
    
    /// <summary>
    /// Hora de entrada
    /// </summary>
    public TimeSpan? HoraEntrada { get; set; }
    
    /// <summary>
    /// Hora de salida
    /// </summary>
    public TimeSpan? HoraSalida { get; set; }
    
    /// <summary>
    /// Observaciones generales del día
    /// </summary>
    public string? Observaciones { get; set; }
    
    /// <summary>
    /// Estado del registro
    /// </summary>
    public EstadoRegistroDiario Estado { get; set; } = EstadoRegistroDiario.Borrador;
    
    /// <summary>
    /// Detalle de actividades realizadas
    /// </summary>
    public ICollection<DetalleActividad>? DetallesActividades { get; set; } = new List<DetalleActividad>();
    
    /// <summary>
    /// Total de horas trabajadas (calculado de las actividades)
    /// </summary>
    public decimal TotalHoras => DetallesActividades?.Sum(d => d.Horas) ?? 0;
    
    /// <summary>
    /// Indica si el registro está completo (tiene entrada, salida y actividades)
    /// </summary>
    public bool EstaCompleto => HoraEntrada.HasValue && HoraSalida.HasValue && (DetallesActividades?.Any() ?? false);
}

/// <summary>
/// Estados posibles de un registro diario
/// </summary>
public enum EstadoRegistroDiario
{
    /// <summary>
    /// Registro en edición, no finalizado
    /// </summary>
    Borrador = 0,
    
    /// <summary>
    /// Registro completado y cerrado
    /// </summary>
    Completado = 1,
    
    /// <summary>
    /// Registro revisado/aprobado por supervisor
    /// </summary>
    Aprobado = 2
}
