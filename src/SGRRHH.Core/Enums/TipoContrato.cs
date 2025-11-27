namespace SGRRHH.Core.Enums;

/// <summary>
/// Tipos de contrato laboral según normativa colombiana
/// </summary>
public enum TipoContrato
{
    /// <summary>
    /// Contrato a término indefinido
    /// </summary>
    Indefinido = 1,
    
    /// <summary>
    /// Contrato a término fijo
    /// </summary>
    Fijo = 2,
    
    /// <summary>
    /// Contrato por obra o labor
    /// </summary>
    ObraLabor = 3,
    
    /// <summary>
    /// Contrato de prestación de servicios
    /// </summary>
    PrestacionServicios = 4,
    
    /// <summary>
    /// Contrato de aprendizaje (SENA)
    /// </summary>
    Aprendizaje = 5
}
