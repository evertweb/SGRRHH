using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Shared.Interfaces;

/// <summary>
/// Repositorio para gestión de prestaciones sociales según legislación colombiana
/// </summary>
public interface IPrestacionRepository : IRepository<Prestacion>
{
    /// <summary>
    /// Obtiene todas las prestaciones de un empleado
    /// </summary>
    Task<IEnumerable<Prestacion>> GetByEmpleadoIdAsync(int empleadoId);
    
    /// <summary>
    /// Obtiene prestaciones de un período específico (año)
    /// </summary>
    Task<IEnumerable<Prestacion>> GetByPeriodoAsync(int periodo);
    
    /// <summary>
    /// Obtiene prestaciones por tipo (Cesantías, Prima, etc.)
    /// </summary>
    Task<IEnumerable<Prestacion>> GetByTipoAsync(TipoPrestacion tipo);
    
    /// <summary>
    /// Obtiene una prestación específica de un empleado por período y tipo
    /// </summary>
    Task<Prestacion?> GetByEmpleadoAndPeriodoAsync(int empleadoId, int periodo, TipoPrestacion tipo);
    
    /// <summary>
    /// Obtiene el total pagado a un empleado en un período
    /// </summary>
    Task<decimal> GetTotalPagadoByEmpleadoAsync(int empleadoId, int periodo);
    
    /// <summary>
    /// Obtiene prestaciones pendientes de pago
    /// </summary>
    Task<IEnumerable<Prestacion>> GetPendientesPagoAsync();
    
    /// <summary>
    /// Obtiene prestaciones de un empleado por estado
    /// </summary>
    Task<IEnumerable<Prestacion>> GetByEmpleadoAndEstadoAsync(int empleadoId, EstadoPrestacion estado);
}
