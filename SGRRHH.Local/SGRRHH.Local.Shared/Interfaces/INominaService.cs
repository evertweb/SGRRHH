using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Shared;

namespace SGRRHH.Local.Shared.Interfaces;

/// <summary>
/// Servicio para generación y gestión de nómina según legislación colombiana
/// </summary>
public interface INominaService
{
    /// <summary>
    /// Calcula la nómina de un empleado para un período específico
    /// </summary>
    Task<Result<Nomina>> CalcularNominaAsync(int empleadoId, DateTime periodo);
    
    /// <summary>
    /// Genera la nómina masiva para múltiples empleados
    /// </summary>
    Task<Result<int>> GenerarNominaMasivaAsync(DateTime periodo, List<int> empleadosIds);
    
    /// <summary>
    /// Genera la nómina para todos los empleados activos
    /// </summary>
    Task<Result<int>> GenerarNominaTodosEmpleadosAsync(DateTime periodo);
    
    /// <summary>
    /// Aprueba una nómina específica
    /// </summary>
    Task<Result<Nomina>> AprobarNominaAsync(int nominaId, int aprobadorId);
    
    /// <summary>
    /// Aprueba todas las nóminas de un período
    /// </summary>
    Task<Result<int>> AprobarNominasPeriodoAsync(DateTime periodo, int aprobadorId);
    
    /// <summary>
    /// Genera el desprendible de pago en PDF
    /// </summary>
    Task<Result<byte[]>> GenerarDesprendiblePagoAsync(int nominaId);
    
    /// <summary>
    /// Obtiene las nóminas pendientes de aprobación
    /// </summary>
    Task<Result<List<Nomina>>> GetNominasPendientesAsync();
    
    /// <summary>
    /// Recalcula una nómina existente
    /// </summary>
    Task<Result<Nomina>> RecalcularNominaAsync(int nominaId);
    
    /// <summary>
    /// Calcula el valor de horas extras según recargos legales
    /// </summary>
    Task<Result<decimal>> CalcularHorasExtrasAsync(int empleadoId, DateTime periodo);
}
