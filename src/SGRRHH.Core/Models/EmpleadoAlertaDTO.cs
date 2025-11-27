namespace SGRRHH.Core.Models;

/// <summary>
/// DTO para mostrar alertas de cumpleaños en el dashboard
/// </summary>
public class CumpleaniosDTO
{
    public int EmpleadoId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public DateTime FechaCumple { get; set; }
    public int DiasRestantes { get; set; }
    public int EdadCumplir { get; set; }
    
    /// <summary>
    /// Texto descriptivo (Ej: "Hoy", "Mañana", "En 3 días")
    /// </summary>
    public string DiasTexto => DiasRestantes switch
    {
        0 => "🎉 ¡Hoy!",
        1 => "Mañana",
        _ => $"En {DiasRestantes} días"
    };
}

/// <summary>
/// DTO para mostrar alertas de aniversarios laborales en el dashboard
/// </summary>
public class AniversarioDTO
{
    public int EmpleadoId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public DateTime FechaAniversario { get; set; }
    public int DiasRestantes { get; set; }
    public int AnosCumplir { get; set; }
    
    /// <summary>
    /// Texto descriptivo (Ej: "Hoy", "Mañana", "En 3 días")
    /// </summary>
    public string DiasTexto => DiasRestantes switch
    {
        0 => "🎉 ¡Hoy!",
        1 => "Mañana",
        _ => $"En {DiasRestantes} días"
    };
    
    /// <summary>
    /// Texto de años de servicio
    /// </summary>
    public string AnosTexto => AnosCumplir == 1 ? "1 año" : $"{AnosCumplir} años";
}