using SGRRHH.Core.Interfaces;

namespace SGRRHH.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de cálculo de fechas
/// Aplica reglas colombianas para días hábiles y festivos
/// </summary>
public class DateCalculationService : IDateCalculationService
{
    /// <summary>
    /// Días de vacaciones por año según ley colombiana (Art. 186 CST)
    /// </summary>
    private const int DIAS_VACACIONES_POR_ANO = 15;
    
    /// <summary>
    /// Cache de festivos por año para evitar recálculos
    /// </summary>
    private readonly Dictionary<int, List<DateTime>> _festivosCache = new();

    public int CalcularDiasCalendario(DateTime fechaInicio, DateTime fechaFin)
    {
        if (fechaFin < fechaInicio)
            return 0;
            
        return (fechaFin.Date - fechaInicio.Date).Days + 1;
    }

    public int CalcularDiasHabiles(DateTime fechaInicio, DateTime fechaFin)
    {
        if (fechaFin < fechaInicio)
            return 0;
            
        int diasHabiles = 0;
        var fecha = fechaInicio.Date;
        var fechaFinDate = fechaFin.Date;
        
        while (fecha <= fechaFinDate)
        {
            if (fecha.DayOfWeek != DayOfWeek.Saturday && fecha.DayOfWeek != DayOfWeek.Sunday)
            {
                diasHabiles++;
            }
            fecha = fecha.AddDays(1);
        }
        
        return Math.Max(1, diasHabiles);
    }

    public int CalcularDiasHabilesSinFestivos(DateTime fechaInicio, DateTime fechaFin, int? anio = null)
    {
        if (fechaFin < fechaInicio)
            return 0;
            
        var anioCalculo = anio ?? fechaInicio.Year;
        var festivos = GetFestivosColombia(anioCalculo).ToHashSet();
        
        // Si el rango cruza años, agregar festivos del siguiente año
        if (fechaFin.Year != fechaInicio.Year)
        {
            foreach (var festivo in GetFestivosColombia(fechaFin.Year))
            {
                festivos.Add(festivo);
            }
        }
        
        int diasHabiles = 0;
        var fecha = fechaInicio.Date;
        var fechaFinDate = fechaFin.Date;
        
        while (fecha <= fechaFinDate)
        {
            if (fecha.DayOfWeek != DayOfWeek.Saturday && 
                fecha.DayOfWeek != DayOfWeek.Sunday &&
                !festivos.Contains(fecha))
            {
                diasHabiles++;
            }
            fecha = fecha.AddDays(1);
        }
        
        return Math.Max(1, diasHabiles);
    }

    public IEnumerable<DateTime> GetFestivosColombia(int anio)
    {
        if (_festivosCache.TryGetValue(anio, out var cachedFestivos))
        {
            return cachedFestivos;
        }
        
        var festivos = new List<DateTime>();
        
        // Festivos fijos
        festivos.Add(new DateTime(anio, 1, 1));   // Año Nuevo
        festivos.Add(new DateTime(anio, 5, 1));   // Día del Trabajo
        festivos.Add(new DateTime(anio, 7, 20));  // Día de la Independencia
        festivos.Add(new DateTime(anio, 8, 7));   // Batalla de Boyacá
        festivos.Add(new DateTime(anio, 12, 8));  // Inmaculada Concepción
        festivos.Add(new DateTime(anio, 12, 25)); // Navidad
        
        // Festivos que se trasladan al lunes (Ley Emiliani)
        festivos.Add(TrasladarALunes(new DateTime(anio, 1, 6)));   // Reyes Magos
        festivos.Add(TrasladarALunes(new DateTime(anio, 3, 19)));  // San José
        festivos.Add(TrasladarALunes(new DateTime(anio, 6, 29)));  // San Pedro y San Pablo
        festivos.Add(TrasladarALunes(new DateTime(anio, 8, 15)));  // Asunción de la Virgen
        festivos.Add(TrasladarALunes(new DateTime(anio, 10, 12))); // Día de la Raza
        festivos.Add(TrasladarALunes(new DateTime(anio, 11, 1)));  // Todos los Santos
        festivos.Add(TrasladarALunes(new DateTime(anio, 11, 11))); // Independencia de Cartagena
        
        // Festivos móviles basados en Pascua
        var pascua = CalcularDomingoPascua(anio);
        
        festivos.Add(pascua.AddDays(-3));  // Jueves Santo
        festivos.Add(pascua.AddDays(-2));  // Viernes Santo
        festivos.Add(TrasladarALunes(pascua.AddDays(43)));  // Ascensión del Señor (+39 días desde Pascua, trasladado)
        festivos.Add(TrasladarALunes(pascua.AddDays(64)));  // Corpus Christi (+60 días, trasladado)
        festivos.Add(TrasladarALunes(pascua.AddDays(71)));  // Sagrado Corazón (+68 días, trasladado)
        
        _festivosCache[anio] = festivos;
        return festivos;
    }

    public bool EsDiaHabil(DateTime fecha)
    {
        return fecha.DayOfWeek != DayOfWeek.Saturday && 
               fecha.DayOfWeek != DayOfWeek.Sunday &&
               !EsFestivo(fecha);
    }

    public bool EsFestivo(DateTime fecha)
    {
        var festivos = GetFestivosColombia(fecha.Year);
        return festivos.Contains(fecha.Date);
    }

    public DateTime GetProximoCumpleanos(DateTime fechaNacimiento)
    {
        var hoy = DateTime.Today;
        
        try
        {
            var cumpleEsteAnio = new DateTime(hoy.Year, fechaNacimiento.Month, fechaNacimiento.Day);
            return cumpleEsteAnio < hoy ? cumpleEsteAnio.AddYears(1) : cumpleEsteAnio;
        }
        catch (ArgumentOutOfRangeException)
        {
            // Manejo para 29 de febrero en años no bisiestos
            var cumpleEsteAnio = new DateTime(hoy.Year, fechaNacimiento.Month, 28);
            return cumpleEsteAnio < hoy ? cumpleEsteAnio.AddYears(1) : cumpleEsteAnio;
        }
    }

    public DateTime GetProximoAniversario(DateTime fechaIngreso)
    {
        var hoy = DateTime.Today;
        
        try
        {
            var aniversarioEsteAnio = new DateTime(hoy.Year, fechaIngreso.Month, fechaIngreso.Day);
            return aniversarioEsteAnio < hoy ? aniversarioEsteAnio.AddYears(1) : aniversarioEsteAnio;
        }
        catch (ArgumentOutOfRangeException)
        {
            // Manejo para 29 de febrero en años no bisiestos
            var aniversarioEsteAnio = new DateTime(hoy.Year, fechaIngreso.Month, 28);
            return aniversarioEsteAnio < hoy ? aniversarioEsteAnio.AddYears(1) : aniversarioEsteAnio;
        }
    }

    public int CalcularAnosServicio(DateTime fechaIngreso)
    {
        var hoy = DateTime.Today;
        var anos = hoy.Year - fechaIngreso.Year;
        
        if (hoy < fechaIngreso.AddYears(anos))
            anos--;
            
        return Math.Max(0, anos);
    }

    public decimal CalcularMesesTrabajadosEnPeriodo(DateTime fechaIngreso, int periodo)
    {
        var inicioPeriodo = new DateTime(periodo, 1, 1);
        var finPeriodo = new DateTime(periodo, 12, 31);
        
        // Si el empleado ingresó después del inicio del periodo
        if (fechaIngreso > inicioPeriodo)
            inicioPeriodo = fechaIngreso;
            
        // Si la fecha actual es antes del fin del periodo
        if (DateTime.Today < finPeriodo)
            finPeriodo = DateTime.Today;
            
        if (inicioPeriodo > finPeriodo)
            return 0;
        
        // Calcular meses con proporcionalidad por días
        var mesesCompletos = ((finPeriodo.Year - inicioPeriodo.Year) * 12) + 
                            finPeriodo.Month - inicioPeriodo.Month;
        
        // Ajuste proporcional por días dentro del mes
        var diasEnMesInicio = DateTime.DaysInMonth(inicioPeriodo.Year, inicioPeriodo.Month);
        var diasTrabajadosMesInicio = (decimal)(diasEnMesInicio - inicioPeriodo.Day + 1) / diasEnMesInicio;
        
        var diasEnMesFin = DateTime.DaysInMonth(finPeriodo.Year, finPeriodo.Month);
        var diasTrabajadosMesFin = (decimal)finPeriodo.Day / diasEnMesFin;
        
        // Si es el mismo mes
        if (inicioPeriodo.Year == finPeriodo.Year && inicioPeriodo.Month == finPeriodo.Month)
        {
            return (decimal)(finPeriodo.Day - inicioPeriodo.Day + 1) / diasEnMesInicio;
        }
        
        // Meses completos (sin el primero ni el último) + proporcional del primero y último
        return Math.Max(0, mesesCompletos - 1) + diasTrabajadosMesInicio + diasTrabajadosMesFin;
    }

    public int CalcularDiasVacacionesGanados(DateTime fechaIngreso, int periodo)
    {
        var mesesTrabajados = CalcularMesesTrabajadosEnPeriodo(fechaIngreso, periodo);
        
        // 15 días por 12 meses = 1.25 días por mes
        var diasGanados = (int)Math.Floor(mesesTrabajados * ((decimal)DIAS_VACACIONES_POR_ANO / 12));
        
        return Math.Min(diasGanados, DIAS_VACACIONES_POR_ANO);
    }

    #region Métodos privados

    /// <summary>
    /// Traslada una fecha al siguiente lunes si no cae en lunes (Ley Emiliani)
    /// </summary>
    private static DateTime TrasladarALunes(DateTime fecha)
    {
        if (fecha.DayOfWeek == DayOfWeek.Monday)
            return fecha;
            
        // Calcular días hasta el próximo lunes
        int diasHastaLunes = ((int)DayOfWeek.Monday - (int)fecha.DayOfWeek + 7) % 7;
        if (diasHastaLunes == 0)
            diasHastaLunes = 7;
            
        return fecha.AddDays(diasHastaLunes);
    }

    /// <summary>
    /// Calcula el Domingo de Pascua usando el algoritmo de Computus
    /// </summary>
    private static DateTime CalcularDomingoPascua(int anio)
    {
        int a = anio % 19;
        int b = anio / 100;
        int c = anio % 100;
        int d = b / 4;
        int e = b % 4;
        int f = (b + 8) / 25;
        int g = (b - f + 1) / 3;
        int h = (19 * a + b - d - g + 15) % 30;
        int i = c / 4;
        int k = c % 4;
        int l = (32 + 2 * e + 2 * i - h - k) % 7;
        int m = (a + 11 * h + 22 * l) / 451;
        int mes = (h + l - 7 * m + 114) / 31;
        int dia = ((h + l - 7 * m + 114) % 31) + 1;
        
        return new DateTime(anio, mes, dia);
    }

    #endregion
}
