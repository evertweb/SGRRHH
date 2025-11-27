namespace SGRRHH.Core.Models;

/// <summary>
/// Clase para el resumen de vacaciones de un empleado
/// </summary>
public class ResumenVacaciones
{
    public int EmpleadoId { get; set; }
    public string NombreEmpleado { get; set; } = string.Empty;
    public DateTime FechaIngreso { get; set; }
    public int AntiguedadAnos { get; set; }
    public int PeriodoActual { get; set; }
    public int DiasGanadosPeriodo { get; set; }
    public int DiasTomadosPeriodo { get; set; }
    public int DiasDisponiblesPeriodo { get; set; }
    public int DiasAcumuladosAnteriores { get; set; }
    public int TotalDiasDisponibles { get; set; }
}
