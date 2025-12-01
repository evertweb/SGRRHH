namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Resultado de envío de código SMS
/// </summary>
public class SmsVerificationResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? VerificationSid { get; set; }
}

/// <summary>
/// Resultado de verificación de código
/// </summary>
public class SmsVerifyResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public bool IsValid { get; set; }
}

/// <summary>
/// Servicio para verificación SMS (MFA)
/// </summary>
public interface ISmsVerificationService
{
    /// <summary>
    /// Envía un código de verificación al número de teléfono
    /// </summary>
    /// <param name="phoneNumber">Número de teléfono en formato E.164 (ej: +573236019907)</param>
    /// <returns>Resultado del envío</returns>
    Task<SmsVerificationResult> SendVerificationCodeAsync(string phoneNumber);
    
    /// <summary>
    /// Verifica el código ingresado por el usuario
    /// </summary>
    /// <param name="phoneNumber">Número de teléfono</param>
    /// <param name="code">Código de 6 dígitos</param>
    /// <returns>Resultado de la verificación</returns>
    Task<SmsVerifyResult> VerifyCodeAsync(string phoneNumber, string code);
    
    /// <summary>
    /// Verifica si el dispositivo actual está registrado como confiable
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <returns>True si el dispositivo es confiable</returns>
    Task<bool> IsDeviceTrustedAsync(string userId);
    
    /// <summary>
    /// Registra el dispositivo actual como confiable
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="durationDays">Días que el dispositivo será confiable (default: 30)</param>
    Task TrustDeviceAsync(string userId, int durationDays = 30);
    
    /// <summary>
    /// Elimina la confianza del dispositivo actual
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    Task RemoveDeviceTrustAsync(string userId);
}
