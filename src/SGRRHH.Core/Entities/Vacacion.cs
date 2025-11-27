using SGRRHH.Core.Enums;

namespace SGRRHH.Core.Entities;

public class Vacacion : EntidadBase
{
    public int EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;
    
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    
    public int DiasTomados { get; set; }
    
    /// <summary>
    /// Año al que corresponden estas vacaciones (ej: 2025)
    /// </summary>
    public int PeriodoCorrespondiente { get; set; }
    
    public EstadoVacacion Estado { get; set; }
    
    public string? Observaciones { get; set; }
}
