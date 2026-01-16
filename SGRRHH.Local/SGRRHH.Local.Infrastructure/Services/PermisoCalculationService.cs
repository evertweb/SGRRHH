using SGRRHH.Local.Domain.Services;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Services;

public class PermisoCalculationService : IPermisoCalculationService
{
    private readonly IFestivoColombiaRepository _festivoRepository;
    private readonly IPermisoRepository _permisoRepository;
    private readonly IEmpleadoRepository _empleadoRepository;

    public PermisoCalculationService(
        IFestivoColombiaRepository festivoRepository,
        IPermisoRepository permisoRepository,
        IEmpleadoRepository empleadoRepository)
    {
        _festivoRepository = festivoRepository;
        _permisoRepository = permisoRepository;
        _empleadoRepository = empleadoRepository;
    }

    public async Task<int> CalcularDiasLaborablesAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        if (fechaFin.Date < fechaInicio.Date)
            return 0;

        var festivos = await ObtenerDiasFestivosEnRangoAsync(fechaInicio, fechaFin);
        var festivosSet = new HashSet<DateTime>(festivos.Select(f => f.Date));

        var diasLaborables = 0;
        for (var fecha = fechaInicio.Date; fecha <= fechaFin.Date; fecha = fecha.AddDays(1))
        {
            if (fecha.DayOfWeek == DayOfWeek.Saturday || fecha.DayOfWeek == DayOfWeek.Sunday)
                continue;

            if (festivosSet.Contains(fecha.Date))
                continue;

            diasLaborables++;
        }

        return diasLaborables;
    }

    public async Task<decimal> CalcularMontoDescuentoAsync(int empleadoId, int diasPermiso)
    {
        if (diasPermiso <= 0)
            return 0m;

        var empleado = await _empleadoRepository.GetByIdAsync(empleadoId);
        if (empleado?.SalarioBase == null)
            return 0m;

        var salarioDiario = empleado.SalarioBase.Value / 30m;
        return salarioDiario * diasPermiso;
    }

    public async Task<bool> TieneSolapamientoAsync(int empleadoId, DateTime inicio, DateTime fin, int? permisoIdActual = null)
    {
        return await _permisoRepository.ExisteSolapamientoAsync(empleadoId, inicio, fin, permisoIdActual);
    }

    public async Task<List<DateTime>> ObtenerDiasFestivosEnRangoAsync(DateTime inicio, DateTime fin)
    {
        if (fin.Date < inicio.Date)
            return new List<DateTime>();

        var festivos = await _festivoRepository.GetFestivosRangoAsync(inicio.Date, fin.Date);
        return festivos.Select(f => f.Fecha.Date).ToList();
    }

    public int ContarDiasSemanaEnRango(DateTime inicio, DateTime fin, DayOfWeek diaSemana)
    {
        if (fin.Date < inicio.Date)
            return 0;

        var count = 0;
        for (var fecha = inicio.Date; fecha <= fin.Date; fecha = fecha.AddDays(1))
        {
            if (fecha.DayOfWeek == diaSemana)
                count++;
        }

        return count;
    }
}
