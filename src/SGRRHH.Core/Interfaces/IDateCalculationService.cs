namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Servicio centralizado para cálculos de fechas
/// Implementa reglas de negocio colombianas para días hábiles
/// </summary>
public interface IDateCalculationService
{
    /// <summary>
    /// Calcula días calendario entre dos fechas (incluye fines de semana)
    /// </summary>
    /// <param name="fechaInicio">Fecha de inicio</param>
    /// <param name="fechaFin">Fecha de fin</param>
    /// <returns>Número de días calendario</returns>
    int CalcularDiasCalendario(DateTime fechaInicio, DateTime fechaFin);
    
    /// <summary>
    /// Calcula días hábiles entre dos fechas (excluye sábados y domingos)
    /// </summary>
    /// <param name="fechaInicio">Fecha de inicio</param>
    /// <param name="fechaFin">Fecha de fin</param>
    /// <returns>Número de días hábiles</returns>
    int CalcularDiasHabiles(DateTime fechaInicio, DateTime fechaFin);
    
    /// <summary>
    /// Calcula días hábiles excluyendo festivos colombianos
    /// </summary>
    /// <param name="fechaInicio">Fecha de inicio</param>
    /// <param name="fechaFin">Fecha de fin</param>
    /// <param name="anio">Año para calcular festivos</param>
    /// <returns>Número de días hábiles (sin festivos)</returns>
    int CalcularDiasHabilesSinFestivos(DateTime fechaInicio, DateTime fechaFin, int? anio = null);
    
    /// <summary>
    /// Obtiene los festivos colombianos para un año
    /// </summary>
    /// <param name="anio">Año</param>
    /// <returns>Lista de fechas festivas</returns>
    IEnumerable<DateTime> GetFestivosColombia(int anio);
    
    /// <summary>
    /// Determina si una fecha es día hábil
    /// </summary>
    /// <param name="fecha">Fecha a verificar</param>
    /// <returns>True si es día hábil</returns>
    bool EsDiaHabil(DateTime fecha);
    
    /// <summary>
    /// Determina si una fecha es festivo colombiano
    /// </summary>
    /// <param name="fecha">Fecha a verificar</param>
    /// <returns>True si es festivo</returns>
    bool EsFestivo(DateTime fecha);
    
    /// <summary>
    /// Calcula el próximo cumpleaños de una fecha
    /// </summary>
    /// <param name="fechaNacimiento">Fecha de nacimiento</param>
    /// <returns>Próxima fecha de cumpleaños</returns>
    DateTime GetProximoCumpleanos(DateTime fechaNacimiento);
    
    /// <summary>
    /// Calcula el próximo aniversario laboral
    /// </summary>
    /// <param name="fechaIngreso">Fecha de ingreso</param>
    /// <returns>Próxima fecha de aniversario</returns>
    DateTime GetProximoAniversario(DateTime fechaIngreso);
    
    /// <summary>
    /// Calcula años completos de servicio
    /// </summary>
    /// <param name="fechaIngreso">Fecha de ingreso</param>
    /// <returns>Años de servicio</returns>
    int CalcularAnosServicio(DateTime fechaIngreso);
    
    /// <summary>
    /// Calcula meses trabajados en un periodo (proporcional)
    /// </summary>
    /// <param name="fechaIngreso">Fecha de ingreso del empleado</param>
    /// <param name="periodo">Año del periodo</param>
    /// <returns>Meses trabajados (con decimales para proporcionalidad)</returns>
    decimal CalcularMesesTrabajadosEnPeriodo(DateTime fechaIngreso, int periodo);
    
    /// <summary>
    /// Calcula días de vacaciones ganados según ley colombiana
    /// 15 días por año, proporcional al tiempo trabajado
    /// </summary>
    /// <param name="fechaIngreso">Fecha de ingreso</param>
    /// <param name="periodo">Año del periodo</param>
    /// <returns>Días de vacaciones ganados</returns>
    int CalcularDiasVacacionesGanados(DateTime fechaIngreso, int periodo);
}
