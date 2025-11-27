using SGRRHH.Core.Entities;

namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Resultado de autenticación
/// </summary>
public class AuthResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Usuario? Usuario { get; set; }
}

/// <summary>
/// Interfaz para el servicio de autenticación
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Intenta autenticar un usuario con las credenciales proporcionadas
    /// </summary>
    /// <param name="username">Nombre de usuario</param>
    /// <param name="password">Contraseña en texto plano</param>
    /// <returns>Resultado de la autenticación</returns>
    Task<AuthResult> AuthenticateAsync(string username, string password);
    
    /// <summary>
    /// Genera el hash de una contraseña
    /// </summary>
    /// <param name="password">Contraseña en texto plano</param>
    /// <returns>Hash de la contraseña</returns>
    string HashPassword(string password);
    
    /// <summary>
    /// Verifica si una contraseña coincide con un hash
    /// </summary>
    /// <param name="password">Contraseña en texto plano</param>
    /// <param name="hash">Hash almacenado</param>
    /// <returns>True si coincide, False si no</returns>
    bool VerifyPassword(string password, string hash);
    
    /// <summary>
    /// Cambia la contraseña de un usuario
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="currentPassword">Contraseña actual</param>
    /// <param name="newPassword">Nueva contraseña</param>
    /// <returns>True si se cambió exitosamente</returns>
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
}
