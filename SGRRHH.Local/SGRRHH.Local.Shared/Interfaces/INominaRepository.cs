using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Shared.Interfaces;

/// <summary>
/// Repositorio para gestión de nóminas mensuales
/// </summary>
public interface INominaRepository : IRepository<Nomina>
{
    /// <summary>
    /// Obtiene todas las nóminas de un empleado
    /// </summary>
    Task<IEnumerable<Nomina>> GetByEmpleadoIdAsync(int empleadoId);
    
    /// <summary>
    /// Obtiene las nóminas de un período específico (mes/año)
    /// </summary>
    Task<IEnumerable<Nomina>> GetByPeriodoAsync(DateTime periodo);
    
    /// <summary>
    /// Obtiene la nómina de un empleado para un período específico
    /// </summary>
    Task<Nomina?> GetByEmpleadoAndPeriodoAsync(int empleadoId, DateTime periodo);
    
    /// <summary>
    /// Obtiene nóminas por estado
    /// </summary>
    Task<IEnumerable<Nomina>> GetByEstadoAsync(EstadoNomina estado);
    
    /// <summary>
    /// Obtiene el total de la nómina de un período
    /// </summary>
    Task<decimal> GetTotalNominaPeriodoAsync(DateTime periodo);
    
    /// <summary>
    /// Obtiene las nóminas pendientes de aprobación
    /// </summary>
    Task<IEnumerable<Nomina>> GetPendientesAprobacionAsync();
    
    /// <summary>
    /// Obtiene nóminas con información del empleado
    /// </summary>
    Task<IEnumerable<Nomina>> GetByPeriodoWithEmpleadoAsync(DateTime periodo);
    
    /// <summary>
    /// Verifica si existe nómina para un empleado en un período
    /// </summary>
    Task<bool> ExisteNominaAsync(int empleadoId, DateTime periodo);
    
    /// <summary>
    /// Obtiene el resumen de nómina de un período (totales)
    /// </summary>
    Task<(decimal TotalDevengado, decimal TotalDeducciones, decimal TotalNeto, decimal TotalAportesPatronales)> GetResumenPeriodoAsync(DateTime periodo);
}
