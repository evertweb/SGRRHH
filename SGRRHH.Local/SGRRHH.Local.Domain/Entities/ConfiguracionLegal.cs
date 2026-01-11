using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Domain.Entities;

/// <summary>
/// Entidad para configuración del sistema y valores legales para Colombia
/// </summary>
public class ConfiguracionLegal : EntidadBase
{
    /// <summary>
    /// Año de vigencia de la configuración
    /// </summary>
    public int Año { get; set; }
    
    // ===== SALARIO MÍNIMO LEGAL =====
    
    /// <summary>
    /// Salario Mínimo Legal Mensual Vigente (SMLMV)
    /// </summary>
    public decimal SalarioMinimoMensual { get; set; }
    
    /// <summary>
    /// Salario Mínimo Diario
    /// </summary>
    public decimal SalarioMinimoDiario => SalarioMinimoMensual / 30;
    
    /// <summary>
    /// Salario Mínimo por Hora
    /// </summary>
    public decimal SalarioMinimoHora => SalarioMinimoMensual / 240; // 30 días x 8 horas
    
    // ===== AUXILIO DE TRANSPORTE =====
    
    /// <summary>
    /// Auxilio de transporte mensual para salarios menores a 2 SMLMV
    /// </summary>
    public decimal AuxilioTransporte { get; set; }
    
    // ===== PORCENTAJES DE SEGURIDAD SOCIAL =====
    
    /// <summary>
    /// Porcentaje aporte salud empleado (4%)
    /// </summary>
    public decimal PorcentajeSaludEmpleado { get; set; } = 4.0m;
    
    /// <summary>
    /// Porcentaje aporte salud empleador (8.5%)
    /// </summary>
    public decimal PorcentajeSaludEmpleador { get; set; } = 8.5m;
    
    /// <summary>
    /// Porcentaje aporte pensión empleado (4%)
    /// </summary>
    public decimal PorcentajePensionEmpleado { get; set; } = 4.0m;
    
    /// <summary>
    /// Porcentaje aporte pensión empleador (12%)
    /// </summary>
    public decimal PorcentajePensionEmpleador { get; set; } = 12.0m;
    
    // ===== PARAFISCALES =====
    
    /// <summary>
    /// Porcentaje aporte Caja de Compensación Familiar (4%)
    /// </summary>
    public decimal PorcentajeCajaCompensacion { get; set; } = 4.0m;
    
    /// <summary>
    /// Porcentaje aporte ICBF (3%)
    /// </summary>
    public decimal PorcentajeICBF { get; set; } = 3.0m;
    
    /// <summary>
    /// Porcentaje aporte SENA (2%)
    /// </summary>
    public decimal PorcentajeSENA { get; set; } = 2.0m;
    
    // ===== ARL - RIESGOS LABORALES =====
    
    /// <summary>
    /// Porcentaje mínimo ARL Clase I (0.522%)
    /// </summary>
    public decimal ARLClase1Min { get; set; } = 0.522m;
    
    /// <summary>
    /// Porcentaje máximo ARL Clase V (6.96%)
    /// </summary>
    public decimal ARLClase5Max { get; set; } = 6.96m;
    
    // ===== INTERESES SOBRE CESANTÍAS =====
    
    /// <summary>
    /// Porcentaje de intereses sobre cesantías (12% anual)
    /// </summary>
    public decimal PorcentajeInteresesCesantias { get; set; } = 12.0m;
    
    // ===== VACACIONES =====
    
    /// <summary>
    /// Días hábiles de vacaciones por año (15 días)
    /// </summary>
    public int DiasVacacionesAño { get; set; } = 15;
    
    // ===== JORNADA LABORAL =====
    
    /// <summary>
    /// Horas máximas de trabajo semanal (48 horas)
    /// </summary>
    public int HorasMaximasSemanales { get; set; } = 48;
    
    /// <summary>
    /// Horas ordinarias diarias (8 horas)
    /// </summary>
    public int HorasOrdinariasDiarias { get; set; } = 8;
    
    // ===== RECARGOS =====
    
    /// <summary>
    /// Recargo hora extra diurna (25%)
    /// </summary>
    public decimal RecargoHoraExtraDiurna { get; set; } = 25.0m;
    
    /// <summary>
    /// Recargo hora extra nocturna (75%)
    /// </summary>
    public decimal RecargoHoraExtraNocturna { get; set; } = 75.0m;
    
    /// <summary>
    /// Recargo hora nocturna ordinaria (35%)
    /// </summary>
    public decimal RecargoHoraNocturna { get; set; } = 35.0m;
    
    /// <summary>
    /// Recargo hora dominical o festivo (75%)
    /// </summary>
    public decimal RecargoHoraDominicalFestivo { get; set; } = 75.0m;
    
    // ===== EDAD MÍNIMA =====
    
    /// <summary>
    /// Edad mínima para trabajar (18 años, 14-17 con autorización)
    /// </summary>
    public int EdadMinimaLaboral { get; set; } = 18;
    
    public string? Observaciones { get; set; }
    
    /// <summary>
    /// Indica si esta es la configuración activa actualmente
    /// </summary>
    public bool EsVigente { get; set; }
}
