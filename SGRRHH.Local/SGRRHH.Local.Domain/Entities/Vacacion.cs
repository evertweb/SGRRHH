using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Domain.Entities;

public class Vacacion : EntidadBase
{
    public int EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;
    
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    
    public int DiasTomados { get; set; }
    
    /// <summary>
    /// Días calendario (incluye fines de semana y festivos)
    /// </summary>
    public int DiasCalendario => (FechaFin - FechaInicio).Days + 1;
    
    /// <summary>
    /// Días hábiles reales (excluye sábados, domingos y festivos)
    /// Se calcula dinámicamente según legislación colombiana
    /// </summary>
    public int DiasHabiles 
    { 
        get 
        {
            int dias = 0;
            for (var date = FechaInicio; date <= FechaFin; date = date.AddDays(1))
            {
                // Excluir sábados y domingos
                if (date.DayOfWeek != DayOfWeek.Saturday && 
                    date.DayOfWeek != DayOfWeek.Sunday)
                {
                    dias++;
                }
            }
            return dias;
        }
    }
    
    public int PeriodoCorrespondiente { get; set; }
    
    /// <summary>
    /// Periodo de vacaciones en formato "YYYY"
    /// </summary>
    public string Periodo => PeriodoCorrespondiente.ToString();
    
    /// <summary>
    /// Días hábiles disponibles para el período según legislación colombiana (15 días hábiles)
    /// Nota: En Colombia son 15 días HÁBILES, no calendario
    /// </summary>
    public int DiasDisponibles { get; set; } = 15;
    
    public EstadoVacacion Estado { get; set; }
    
    public string? Observaciones { get; set; }
    
    // ===== Campos de trazabilidad (agregados para consistencia con Permiso) =====
    
    public DateTime FechaSolicitud { get; set; } = DateTime.Now;
    
    public int? SolicitadoPorId { get; set; }
    
    public Usuario? SolicitadoPor { get; set; }
    
    public int? AprobadoPorId { get; set; }
    
    public Usuario? AprobadoPor { get; set; }
    
    public DateTime? FechaAprobacion { get; set; }
    
    public string? MotivoRechazo { get; set; }
}


