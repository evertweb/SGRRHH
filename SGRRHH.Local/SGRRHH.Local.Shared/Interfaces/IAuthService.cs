using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Shared.Interfaces;

public interface IAuthService
{
    // Autenticación
    Task<Result<Usuario>> LoginAsync(string username, string password);
    Task<Result> LogoutAsync();
    
    /// <summary>
    /// Restaura la sesión desde almacenamiento persistente (localStorage).
    /// Debe llamarse al inicio de la aplicación para recuperar sesión tras F5/recarga.
    /// </summary>
    Task<bool> TryRestoreSessionAsync();
    
    /// <summary>
    /// Establece el usuario actual (usado internamente para restaurar sesión).
    /// </summary>
    void SetCurrentUser(Usuario? user);
    
    // Estado de sesión
    Usuario? CurrentUser { get; }
    int? CurrentUserId { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
    bool IsAprobador { get; }
    bool IsOperador { get; }
    bool IsSupervisor { get; }
    
    // Gestión de contraseñas
    Task<Result> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    Task<Result> ResetPasswordAsync(int userId, string newPassword); // Solo admin
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
    
    // Eventos
    event EventHandler<Usuario?>? OnAuthStateChanged;
}
