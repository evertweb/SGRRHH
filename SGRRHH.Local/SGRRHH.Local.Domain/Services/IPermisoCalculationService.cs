namespace SGRRHH.Local.Domain.Services;

public interface IPermisoCalculationService
{
    Task<int> CalcularDiasLaborablesAsync(DateTime fechaInicio, DateTime fechaFin);
    Task<decimal> CalcularMontoDescuentoAsync(int empleadoId, int diasPermiso);
    Task<bool> TieneSolapamientoAsync(int empleadoId, DateTime inicio, DateTime fin, int? permisoIdActual = null);
    Task<List<DateTime>> ObtenerDiasFestivosEnRangoAsync(DateTime inicio, DateTime fin);
    int ContarDiasSemanaEnRango(DateTime inicio, DateTime fin, DayOfWeek diaSemana);
}
