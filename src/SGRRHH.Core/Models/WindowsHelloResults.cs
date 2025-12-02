namespace SGRRHH.Core.Models;

/// <summary>
/// Resultado de la verificación de Windows Hello
/// </summary>
public class WindowsHelloVerifyResult
{
    /// <summary>
    /// Indica si la verificación fue exitosa
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Mensaje descriptivo del resultado
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// ID de la credencial verificada (si fue exitosa)
    /// </summary>
    public string? CredentialId { get; set; }

    /// <summary>
    /// Crea un resultado exitoso
    /// </summary>
    public static WindowsHelloVerifyResult Ok(string credentialId)
    {
        return new WindowsHelloVerifyResult
        {
            Success = true,
            CredentialId = credentialId
        };
    }

    /// <summary>
    /// Crea un resultado fallido
    /// </summary>
    public static WindowsHelloVerifyResult Fail(string message)
    {
        return new WindowsHelloVerifyResult
        {
            Success = false,
            Message = message
        };
    }
}

/// <summary>
/// Resultado del registro de una passkey
/// </summary>
public class PasskeyRegistrationResult
{
    /// <summary>
    /// Indica si el registro fue exitoso
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Mensaje descriptivo del resultado
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// ID de la credencial registrada (si fue exitosa)
    /// </summary>
    public string? CredentialId { get; set; }

    /// <summary>
    /// Crea un resultado exitoso
    /// </summary>
    public static PasskeyRegistrationResult Ok(string credentialId)
    {
        return new PasskeyRegistrationResult
        {
            Success = true,
            CredentialId = credentialId,
            Message = "Passkey registrada exitosamente"
        };
    }

    /// <summary>
    /// Crea un resultado fallido
    /// </summary>
    public static PasskeyRegistrationResult Fail(string message)
    {
        return new PasskeyRegistrationResult
        {
            Success = false,
            Message = message
        };
    }
}
