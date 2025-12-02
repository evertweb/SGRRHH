namespace SGRRHH.Core.Models;

/// <summary>
/// Información de una passkey de Windows Hello registrada
/// </summary>
public class PasskeyInfo
{
    /// <summary>
    /// ID único de la credencial (GUID)
    /// </summary>
    public string CredentialId { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del dispositivo donde está registrada la passkey
    /// </summary>
    public string DeviceName { get; set; } = string.Empty;

    /// <summary>
    /// Fecha y hora de creación de la passkey
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Fecha y hora del último uso de la passkey
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// Indica si este dispositivo es el actual
    /// </summary>
    public bool IsCurrent { get; set; }
}
