namespace SGRRHH.Core.Models;

/// <summary>
/// DTO para datos de gráficos de barras sencillos
/// </summary>
public class EstadisticaItemDTO
{
    /// <summary>
    /// Etiqueta del item (ej: nombre del departamento)
    /// </summary>
    public string Etiqueta { get; set; } = string.Empty;
    
    /// <summary>
    /// Valor numérico
    /// </summary>
    public int Cantidad { get; set; }
    
    /// <summary>
    /// Porcentaje del total (0-100)
    /// </summary>
    public double Porcentaje { get; set; }
    
    /// <summary>
    /// Color para la barra (formato #RRGGBB)
    /// </summary>
    public string Color { get; set; } = "#1E88E5";
}