using SGRRHH.Core.Models;

namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Servicio para autenticación con Windows Hello (biometría/PIN)
/// </summary>
public interface IWindowsHelloService
{
    /// <summary>
    /// Verifica si Windows Hello está disponible en el dispositivo actual
    /// </summary>
    /// <returns>True si Windows Hello está disponible</returns>
    Task<bool> IsAvailableAsync();

    /// <summary>
    /// Obtiene un mensaje descriptivo sobre la disponibilidad de Windows Hello
    /// </summary>
    /// <returns>Mensaje de estado</returns>
    Task<string> GetAvailabilityMessageAsync();

    /// <summary>
    /// Solicita verificación de identidad con Windows Hello
    /// </summary>
    /// <param name="hwnd">Handle de la ventana</param>
    /// <param name="username">Nombre de usuario</param>
    /// <returns>Resultado de la verificación</returns>
    Task<WindowsHelloVerifyResult> VerifyAsync(IntPtr hwnd, string username);

    /// <summary>
    /// Registra una nueva passkey de Windows Hello
    /// </summary>
    /// <param name="hwnd">Handle de la ventana</param>
    /// <param name="username">Nombre de usuario</param>
    /// <param name="firebaseUid">UID del usuario en Firebase</param>
    /// <returns>Resultado del registro</returns>
    Task<PasskeyRegistrationResult> RegisterPasskeyAsync(IntPtr hwnd, string username, string firebaseUid);

    /// <summary>
    /// Verifica si el usuario tiene una passkey registrada localmente
    /// </summary>
    /// <param name="username">Nombre de usuario</param>
    /// <returns>True si tiene passkey registrada</returns>
    Task<bool> HasPasskeyAsync(string username);

    /// <summary>
    /// Obtiene el último nombre de usuario que usó passkey
    /// </summary>
    /// <returns>Nombre de usuario o null</returns>
    Task<string?> GetLastPasskeyUsernameAsync();

    /// <summary>
    /// Obtiene el credentialId almacenado localmente para un usuario
    /// </summary>
    /// <param name="username">Nombre de usuario</param>
    /// <returns>CredentialId o null</returns>
    Task<string?> GetStoredCredentialIdAsync(string username);

    /// <summary>
    /// Guarda el mapping username -> credentialId en el Credential Manager local
    /// </summary>
    /// <param name="username">Nombre de usuario</param>
    /// <param name="credentialId">ID de la credencial</param>
    Task SavePasskeyMappingAsync(string username, string credentialId);

    /// <summary>
    /// Elimina el mapping de passkey del Credential Manager local
    /// </summary>
    /// <param name="username">Nombre de usuario</param>
    Task RemovePasskeyMappingAsync(string username);

    /// <summary>
    /// Obtiene todas las passkeys registradas localmente en este dispositivo
    /// </summary>
    /// <returns>Lista de registros con username y credentialId</returns>
    Task<IReadOnlyList<LocalPasskeyInfo>> GetAllLocalPasskeysAsync();
}

/// <summary>
/// Información de una passkey registrada localmente
/// </summary>
public record LocalPasskeyInfo(string Username, string CredentialId);
