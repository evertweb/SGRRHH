using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;

namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Resultado extendido de autenticación para Firebase
/// </summary>
public class FirebaseAuthResult : AuthResult
{
    /// <summary>
    /// UID de Firebase del usuario autenticado
    /// </summary>
    public string? FirebaseUid { get; set; }
    
    /// <summary>
    /// Token de ID de Firebase (JWT)
    /// </summary>
    public string? IdToken { get; set; }
    
    /// <summary>
    /// Token de refresh de Firebase
    /// </summary>
    public string? RefreshToken { get; set; }
    
    /// <summary>
    /// Tiempo de expiración del token en segundos
    /// </summary>
    public int? ExpiresIn { get; set; }
}

/// <summary>
/// Interfaz extendida para autenticación con Firebase
/// </summary>
public interface IFirebaseAuthService : IAuthService
{
    /// <summary>
    /// Autenticación con Firebase Auth usando email y contraseña
    /// </summary>
    /// <param name="email">Email del usuario</param>
    /// <param name="password">Contraseña</param>
    /// <returns>Resultado con tokens de Firebase</returns>
    Task<FirebaseAuthResult> AuthenticateWithFirebaseAsync(string email, string password);
    
    /// <summary>
    /// Crea un nuevo usuario en Firebase Auth
    /// </summary>
    /// <param name="email">Email del usuario</param>
    /// <param name="password">Contraseña</param>
    /// <param name="displayName">Nombre para mostrar</param>
    /// <returns>UID del usuario creado</returns>
    Task<string?> CreateFirebaseUserAsync(string email, string password, string displayName);
    
    /// <summary>
    /// Elimina un usuario de Firebase Auth
    /// </summary>
    /// <param name="uid">UID del usuario</param>
    Task<bool> DeleteFirebaseUserAsync(string uid);
    
    /// <summary>
    /// Actualiza la contraseña de un usuario en Firebase Auth
    /// </summary>
    /// <param name="uid">UID del usuario</param>
    /// <param name="newPassword">Nueva contraseña</param>
    Task<bool> UpdatePasswordAsync(string uid, string newPassword);
    
    /// <summary>
    /// Migra un usuario local (SQLite) a Firebase Auth
    /// </summary>
    /// <param name="username">Username original</param>
    /// <param name="password">Contraseña (si se conoce)</param>
    /// <param name="nombreCompleto">Nombre completo</param>
    /// <param name="rol">Rol del usuario</param>
    /// <returns>UID de Firebase del usuario migrado</returns>
    Task<string?> MigrateUserToFirebaseAsync(string username, string password, string nombreCompleto, RolUsuario rol);
    
    /// <summary>
    /// Establece claims personalizados (rol) para un usuario
    /// </summary>
    /// <param name="uid">UID del usuario</param>
    /// <param name="rol">Rol a establecer</param>
    Task<bool> SetUserRoleClaimAsync(string uid, RolUsuario rol);
    
    /// <summary>
    /// Obtiene el usuario actual de Firestore por UID
    /// </summary>
    /// <param name="uid">UID de Firebase</param>
    Task<Usuario?> GetUserByFirebaseUidAsync(string uid);
    
    /// <summary>
    /// Obtiene el usuario por email
    /// </summary>
    /// <param name="email">Email del usuario</param>
    Task<Usuario?> GetUserByEmailAsync(string email);
    
    /// <summary>
    /// Cierra la sesión actual (revoca tokens)
    /// </summary>
    Task SignOutAsync();
    
    /// <summary>
    /// Verifica si el servicio de Firebase Auth está disponible
    /// </summary>
    Task<bool> IsFirebaseAvailableAsync();
}
