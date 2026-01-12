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
    
    // ========== MODO CORPORATIVO ==========
    
    /// <summary>
    /// Indica si el Modo Corporativo está activado.
    /// Cuando está activo, se aplican las restricciones de roles.
    /// Cuando está inactivo, todos tienen acceso completo.
    /// </summary>
    bool ModoCorporativoActivo { get; }
    
    /// <summary>
    /// Activa o desactiva el Modo Corporativo. Solo disponible para Administradores.
    /// </summary>
    Task<Result> SetModoCorporativoAsync(bool activar);
    
    /// <summary>
    /// Verifica si el usuario actual tiene un permiso específico en un módulo.
    /// Considera el estado del Modo Corporativo.
    /// </summary>
    bool TienePermiso(string modulo, PermisosModulo permiso);
    
    /// <summary>
    /// Obtiene todos los permisos del usuario actual para un módulo.
    /// </summary>
    PermisosModulo ObtenerPermisosModulo(string modulo);
    
    // Gestión de contraseñas
    Task<Result> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    Task<Result> ResetPasswordAsync(int userId, string newPassword); // Solo admin
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
    
    // =========================================================================
    // AUTENTICACIÓN POR DISPOSITIVO (sin contraseña, estilo SSH)
    // =========================================================================
    
    /// <summary>
    /// Intenta autenticar usando un token de dispositivo almacenado.
    /// </summary>
    /// <param name="deviceToken">Token del dispositivo desde localStorage</param>
    /// <returns>Usuario si el token es válido, null si no</returns>
    Task<Result<Usuario>> LoginConTokenDispositivoAsync(string deviceToken);
    
    /// <summary>
    /// Autoriza el dispositivo actual para login sin contraseña.
    /// Genera un token único que se guarda en el navegador.
    /// </summary>
    /// <param name="nombreDispositivo">Nombre amigable (ej: "PC Oficina")</param>
    /// <param name="huellaNavegador">User-Agent del navegador</param>
    /// <param name="ipCliente">IP del cliente (opcional)</param>
    /// <param name="diasExpiracion">Días hasta expirar (null = no expira)</param>
    /// <returns>Token generado para guardar en localStorage</returns>
    Task<Result<string>> AutorizarDispositivoAsync(
        string nombreDispositivo, 
        string? huellaNavegador = null, 
        string? ipCliente = null,
        int? diasExpiracion = null);
    
    /// <summary>
    /// Revoca (desactiva) un dispositivo autorizado.
    /// </summary>
    Task<Result> RevocarDispositivoAsync(int dispositivoId);
    
    /// <summary>
    /// Revoca todos los dispositivos de un usuario.
    /// </summary>
    Task<Result> RevocarTodosDispositivosAsync(int usuarioId);
    
    /// <summary>
    /// Obtiene los dispositivos autorizados del usuario actual.
    /// </summary>
    Task<IEnumerable<DispositivoAutorizado>> ObtenerMisDispositivosAsync();
    
    /// <summary>
    /// Obtiene los dispositivos autorizados de un usuario específico (solo admin).
    /// </summary>
    Task<IEnumerable<DispositivoAutorizado>> ObtenerDispositivosDeUsuarioAsync(int usuarioId);
    
    /// <summary>
    /// Obtiene un usuario por su ID (para restaurar sesión desde token).
    /// </summary>
    Task<Usuario?> GetUserByIdAsync(int userId);
    
    // Eventos
    event EventHandler<Usuario?>? OnAuthStateChanged;
}
