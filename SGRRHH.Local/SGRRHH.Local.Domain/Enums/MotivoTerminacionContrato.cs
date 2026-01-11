namespace SGRRHH.Local.Domain.Enums;

/// <summary>
/// Motivos de terminación de contrato según legislación colombiana
/// Basado en el Código Sustantivo del Trabajo
/// </summary>
public enum MotivoTerminacionContrato
{
    /// <summary>
    /// Terminación del contrato a término fijo por vencimiento del plazo
    /// </summary>
    VencimientoTerminoFijo = 1,
    
    /// <summary>
    /// Terminación de contrato de obra o labor por finalización de la obra
    /// </summary>
    FinalizacionObraLabor = 2,
    
    /// <summary>
    /// Renuncia voluntaria del trabajador (Art. 62 CST)
    /// </summary>
    RenunciaVoluntaria = 3,
    
    /// <summary>
    /// Despido con justa causa por parte del empleador (Art. 62 CST)
    /// </summary>
    DespidoJustaCausa = 4,
    
    /// <summary>
    /// Despido sin justa causa - Genera indemnización (Art. 64 CST)
    /// </summary>
    DespidoSinJustaCausa = 5,
    
    /// <summary>
    /// Terminación unilateral por parte del trabajador con justa causa (Art. 62 CST)
    /// Genera indemnización a favor del trabajador
    /// </summary>
    TerminacionTrabajadorJustaCausa = 6,
    
    /// <summary>
    /// Mutuo acuerdo entre las partes
    /// </summary>
    MutuoAcuerdo = 7,
    
    /// <summary>
    /// Muerte del trabajador
    /// </summary>
    MuerteTrabajador = 8,
    
    /// <summary>
    /// Liquidación o cierre definitivo de la empresa
    /// </summary>
    LiquidacionEmpresa = 9,
    
    /// <summary>
    /// Pensión de vejez o invalidez del trabajador
    /// </summary>
    Pension = 10,
    
    /// <summary>
    /// Terminación durante período de prueba
    /// </summary>
    PeriodoPrueba = 11
}
