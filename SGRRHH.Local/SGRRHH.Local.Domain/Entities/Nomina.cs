using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Domain.Entities;

/// <summary>
/// Entidad para gestionar la nómina mensual según legislación colombiana
/// </summary>
public class Nomina : EntidadBase
{
    public int EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;
    
    /// <summary>
    /// Período de la nómina (Mes/Año)
    /// </summary>
    public DateTime Periodo { get; set; }
    
    /// <summary>
    /// Fecha de pago de la nómina
    /// </summary>
    public DateTime? FechaPago { get; set; }
    
    // ===== DEVENGOS =====
    
    /// <summary>
    /// Salario básico mensual
    /// </summary>
    public decimal SalarioBase { get; set; }
    
    /// <summary>
    /// Auxilio de transporte (si aplica, salarios < 2 SMLMV)
    /// </summary>
    public decimal AuxilioTransporte { get; set; }
    
    /// <summary>
    /// Valor de horas extras diurnas trabajadas
    /// </summary>
    public decimal HorasExtrasDiurnas { get; set; }
    
    /// <summary>
    /// Valor de horas extras nocturnas trabajadas
    /// </summary>
    public decimal HorasExtrasNocturnas { get; set; }
    
    /// <summary>
    /// Valor de horas nocturnas ordinarias
    /// </summary>
    public decimal HorasNocturnas { get; set; }
    
    /// <summary>
    /// Valor de horas dominicales y festivos
    /// </summary>
    public decimal HorasDominicalesFestivos { get; set; }
    
    /// <summary>
    /// Comisiones del período
    /// </summary>
    public decimal Comisiones { get; set; }
    
    /// <summary>
    /// Bonificaciones no constitutivas de salario
    /// </summary>
    public decimal Bonificaciones { get; set; }
    
    /// <summary>
    /// Otros devengos
    /// </summary>
    public decimal OtrosDevengos { get; set; }
    
    /// <summary>
    /// Total devengado antes de deducciones
    /// </summary>
    public decimal TotalDevengado => SalarioBase + AuxilioTransporte + HorasExtrasDiurnas + 
                                     HorasExtrasNocturnas + HorasNocturnas + HorasDominicalesFestivos +
                                     Comisiones + Bonificaciones + OtrosDevengos;
    
    // ===== DEDUCCIONES =====
    
    /// <summary>
    /// Aporte a salud del empleado (4% del salario base)
    /// </summary>
    public decimal DeduccionSalud { get; set; }
    
    /// <summary>
    /// Aporte a pensión del empleado (4% del salario base)
    /// </summary>
    public decimal DeduccionPension { get; set; }
    
    /// <summary>
    /// Retención en la fuente (si aplica)
    /// </summary>
    public decimal RetencionFuente { get; set; }
    
    /// <summary>
    /// Préstamos o anticipos
    /// </summary>
    public decimal Prestamos { get; set; }
    
    /// <summary>
    /// Embargos judiciales
    /// </summary>
    public decimal Embargos { get; set; }
    
    /// <summary>
    /// Aportes a fondos de empleados
    /// </summary>
    public decimal FondoEmpleados { get; set; }
    
    /// <summary>
    /// Otras deducciones autorizadas
    /// </summary>
    public decimal OtrasDeducciones { get; set; }
    
    /// <summary>
    /// Total de deducciones
    /// </summary>
    public decimal TotalDeducciones => DeduccionSalud + DeduccionPension + RetencionFuente + 
                                       Prestamos + Embargos + FondoEmpleados + OtrasDeducciones;
    
    // ===== APORTES PATRONALES (No se descuentan al empleado) =====
    
    /// <summary>
    /// Aporte a salud del empleador (8.5%)
    /// </summary>
    public decimal AporteSaludEmpleador { get; set; }
    
    /// <summary>
    /// Aporte a pensión del empleador (12%)
    /// </summary>
    public decimal AportePensionEmpleador { get; set; }
    
    /// <summary>
    /// Aporte a ARL según clase de riesgo (0.522% - 6.96%)
    /// </summary>
    public decimal AporteARL { get; set; }
    
    /// <summary>
    /// Aporte a Caja de Compensación Familiar (4%)
    /// </summary>
    public decimal AporteCajaCompensacion { get; set; }
    
    /// <summary>
    /// Aporte ICBF (3%)
    /// </summary>
    public decimal AporteICBF { get; set; }
    
    /// <summary>
    /// Aporte SENA (2%)
    /// </summary>
    public decimal AporteSENA { get; set; }
    
    /// <summary>
    /// Total aportes patronales (costo para la empresa)
    /// </summary>
    public decimal TotalAportesPatronales => AporteSaludEmpleador + AportePensionEmpleador + 
                                             AporteARL + AporteCajaCompensacion + AporteICBF + AporteSENA;
    
    // ===== NETO A PAGAR =====
    
    /// <summary>
    /// Valor neto a pagar al empleado
    /// </summary>
    public decimal NetoPagar => TotalDevengado - TotalDeducciones;
    
    /// <summary>
    /// Costo total para la empresa (neto + aportes patronales)
    /// </summary>
    public decimal CostoTotalEmpresa => NetoPagar + TotalAportesPatronales;
    
    // ===== CONTROL =====
    
    public EstadoNomina Estado { get; set; } = EstadoNomina.Borrador;
    
    /// <summary>
    /// Usuario que aprobó la nómina
    /// </summary>
    public int? AprobadoPorId { get; set; }
    public Usuario? AprobadoPor { get; set; }
    
    /// <summary>
    /// Fecha de aprobación
    /// </summary>
    public DateTime? FechaAprobacion { get; set; }
    
    /// <summary>
    /// Número de comprobante de pago
    /// </summary>
    public string? ComprobanteNumero { get; set; }
    
    public string? Observaciones { get; set; }
}
