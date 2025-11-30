namespace SGRRHH.Core.Models;

/// <summary>
/// Configuración para integración con Sendbird Chat
/// </summary>
public class SendbirdSettings
{
    /// <summary>
    /// Indica si Sendbird está habilitado
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Application ID de Sendbird
    /// </summary>
    public string ApplicationId { get; set; } = string.Empty;

    /// <summary>
    /// API Token para operaciones de administración (opcional)
    /// </summary>
    public string? ApiToken { get; set; }

    /// <summary>
    /// Región del servidor Sendbird (opcional, ej: "us-1", "ap-2")
    /// </summary>
    public string? Region { get; set; }
}
