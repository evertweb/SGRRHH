namespace SGRRHH.Local.Domain.Enums;

/// <summary>
/// Origen de una hoja de vida PDF en el sistema.
/// </summary>
public enum OrigenHojaVida
{
    /// <summary>
    /// PDF generado por el sistema con metadatos Forestech.
    /// </summary>
    Forestech = 0,
    
    /// <summary>
    /// PDF subido externamente sin metadatos Forestech.
    /// </summary>
    Externo = 1,
    
    /// <summary>
    /// Datos ingresados manualmente sin PDF asociado.
    /// </summary>
    Manual = 2
}
