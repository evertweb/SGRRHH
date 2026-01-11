using SGRRHH.Local.Domain.Entities;

namespace SGRRHH.Local.Shared.Interfaces;

/// <summary>
/// Repositorio para gestión de festivos colombianos según Ley 51 de 1983 (Ley Emiliani)
/// </summary>
public interface IFestivoColombiaRepository : IRepository<FestivoColombia>
{
    /// <summary>
    /// Obtiene todos los festivos de un año específico
    /// </summary>
    Task<IEnumerable<FestivoColombia>> GetByAñoAsync(int año);
    
    /// <summary>
    /// Obtiene el festivo correspondiente a una fecha específica (si existe)
    /// </summary>
    Task<FestivoColombia?> GetByFechaAsync(DateTime fecha);
    
    /// <summary>
    /// Verifica si una fecha es festivo en Colombia
    /// </summary>
    Task<bool> EsFestivoAsync(DateTime fecha);
    
    /// <summary>
    /// Obtiene los festivos dentro de un rango de fechas
    /// </summary>
    Task<IEnumerable<FestivoColombia>> GetFestivosRangoAsync(DateTime inicio, DateTime fin);
    
    /// <summary>
    /// Cuenta el número de festivos en un período (para cálculo de días laborales)
    /// </summary>
    Task<int> ContarFestivosEnPeriodoAsync(DateTime inicio, DateTime fin);
    
    /// <summary>
    /// Verifica si existe configuración de festivos para un año
    /// </summary>
    Task<bool> ExisteFestivosAñoAsync(int año);
}
