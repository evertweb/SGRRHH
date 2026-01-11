using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Shared;

namespace SGRRHH.Local.Shared.Interfaces;

/// <summary>
/// DTO para liquidación completa de un empleado
/// </summary>
public class LiquidacionCompleta
{
    public int EmpleadoId { get; set; }
    public string EmpleadoNombre { get; set; } = string.Empty;
    public int ContratoId { get; set; }
    public DateTime FechaIngreso { get; set; }
    public DateTime FechaRetiro { get; set; }
    public decimal SalarioBase { get; set; }
    public MotivoTerminacionContrato Motivo { get; set; }
    
    // Valores calculados
    public decimal Cesantias { get; set; }
    public decimal InteresesCesantias { get; set; }
    public decimal PrimaServicios { get; set; }
    public decimal VacacionesProporcionales { get; set; }
    public decimal Indemnizacion { get; set; }
    
    // Totales
    public decimal TotalLiquidacion => Cesantias + InteresesCesantias + PrimaServicios + VacacionesProporcionales + Indemnizacion;
    
    // Deducciones pendientes
    public decimal DeduccionesPendientes { get; set; }
    public decimal NetoAPagar => TotalLiquidacion - DeduccionesPendientes;
    
    // Detalles del cálculo
    public int DiasLaborados { get; set; }
    public string? Observaciones { get; set; }
    public DateTime FechaCalculo { get; set; } = DateTime.Now;
}

/// <summary>
/// Servicio para cálculo de liquidación de prestaciones sociales según legislación colombiana
/// </summary>
public interface ILiquidacionService
{
    /// <summary>
    /// Calcula las cesantías de un empleado según el CST colombiano.
    /// Fórmula: (Salario mensual × Días trabajados) / 360
    /// </summary>
    Task<Result<decimal>> CalcularCesantiasAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin);
    
    /// <summary>
    /// Calcula los intereses sobre cesantías (12% anual).
    /// Fórmula: (Cesantías × Días × 0.12) / 360
    /// </summary>
    Task<Result<decimal>> CalcularInteresesCesantiasAsync(int empleadoId, int año);
    
    /// <summary>
    /// Calcula la prima de servicios.
    /// Fórmula: (Salario mensual × Días trabajados) / 360
    /// Se paga en junio (primeros 6 meses) y diciembre (segundos 6 meses)
    /// </summary>
    Task<Result<decimal>> CalcularPrimaServiciosAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin);
    
    /// <summary>
    /// Calcula las vacaciones proporcionales.
    /// Fórmula: (Salario mensual × Días causados) / 720
    /// 15 días hábiles por año = factor 720
    /// </summary>
    Task<Result<decimal>> CalcularVacacionesProporcionalesAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin);
    
    /// <summary>
    /// Calcula la indemnización por despido sin justa causa según tipo de contrato.
    /// - Contrato indefinido < 1 año: 30 días de salario
    /// - Contrato indefinido > 1 año: 30 días + 20 días por cada año adicional
    /// - Contrato a término fijo: Días restantes del contrato
    /// </summary>
    Task<Result<decimal>> CalcularIndemnizacionAsync(int empleadoId, int contratoId, MotivoTerminacionContrato motivo);
    
    /// <summary>
    /// Genera la liquidación final completa de un empleado
    /// </summary>
    Task<Result<LiquidacionCompleta>> GenerarLiquidacionFinalAsync(int empleadoId, DateTime fechaRetiro, MotivoTerminacionContrato motivo);
    
    /// <summary>
    /// Calcula el salario base para prestaciones (incluye auxilio de transporte si aplica)
    /// </summary>
    Task<Result<decimal>> GetSalarioBaseParaPrestacionesAsync(int empleadoId);
}
