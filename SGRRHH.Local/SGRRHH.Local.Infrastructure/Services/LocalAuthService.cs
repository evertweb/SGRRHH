using BCrypt.Net;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Shared;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Services;

public class LocalAuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IAuditLogRepository _auditRepository;
    private readonly ILogger<LocalAuthService> _logger;
    
    private Usuario? _currentUser;
    
    public Usuario? CurrentUser => _currentUser;
    public int? CurrentUserId => _currentUser?.Id;
    public bool IsAuthenticated => _currentUser != null;
    public bool IsAdmin => _currentUser?.Rol == RolUsuario.Administrador;
    public bool IsAprobador => _currentUser?.Rol == RolUsuario.Aprobador || IsAdmin;
    public bool IsOperador => _currentUser?.Rol == RolUsuario.Operador || IsAdmin;
    public bool IsSupervisor => IsAprobador; // Supervisor es equivalente a Aprobador
    
    public event EventHandler<Usuario?>? OnAuthStateChanged;
    
    public LocalAuthService(
        IUsuarioRepository usuarioRepository,
        IAuditLogRepository auditRepository,
        ILogger<LocalAuthService> logger)
    {
        _usuarioRepository = usuarioRepository;
        _auditRepository = auditRepository;
        _logger = logger;
    }
    
    public async Task<Result<Usuario>> LoginAsync(string username, string password)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return Result<Usuario>.Fail("Usuario y contraseña son requeridos");
            }
            
            var usuario = await _usuarioRepository.GetByUsernameAsync(username);
            
            if (usuario == null)
            {
                _logger.LogWarning("Intento de login fallido: usuario no existe ({Username})", username);
                await RegistrarAuditoria("LOGIN_FALLIDO", "Usuario", null, $"Usuario no existe: {username}");
                return Result<Usuario>.Fail("Usuario o contraseña incorrectos");
            }
            
            if (!usuario.Activo)
            {
                _logger.LogWarning("Intento de login con usuario inactivo: {Username}", username);
                return Result<Usuario>.Fail("Usuario deshabilitado. Contacte al administrador.");
            }
            
            if (!VerifyPassword(password, usuario.PasswordHash))
            {
                _logger.LogWarning("Intento de login fallido: contraseña incorrecta ({Username})", username);
                await RegistrarAuditoria("LOGIN_FALLIDO", "Usuario", usuario.Id, "Contraseña incorrecta");
                return Result<Usuario>.Fail("Usuario o contraseña incorrectos");
            }
            
            // Login exitoso
            _currentUser = usuario;
            await _usuarioRepository.UpdateLastAccessAsync(usuario.Id);
            
            _logger.LogInformation("Login exitoso: {Username} (Rol: {Rol})", username, usuario.Rol);
            await RegistrarAuditoria("LOGIN_EXITOSO", "Usuario", usuario.Id, $"Login exitoso como {usuario.Rol}");
            
            OnAuthStateChanged?.Invoke(this, _currentUser);
            
            return Result<Usuario>.Ok(usuario, "Bienvenido, " + usuario.NombreCompleto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante login de {Username}", username);
            return Result<Usuario>.Fail("Error interno. Intente nuevamente.");
        }
    }
    
    public Task<Result> LogoutAsync()
    {
        if (_currentUser != null)
        {
            _logger.LogInformation("Logout: {Username}", _currentUser.Username);
            _ = RegistrarAuditoria("LOGOUT", "Usuario", _currentUser.Id, "Cierre de sesión");
        }
        
        _currentUser = null;
        OnAuthStateChanged?.Invoke(this, null);
        
        return Task.FromResult(Result.Ok("Sesión cerrada"));
    }
    
    public async Task<Result> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        try
        {
            var usuario = await _usuarioRepository.GetByIdAsync(userId);
            if (usuario == null)
                return Result.Fail("Usuario no encontrado");
            
            // Solo el propio usuario o admin puede cambiar contraseña
            if (_currentUser?.Id != userId && !IsAdmin)
                return Result.Fail("No tiene permisos para cambiar esta contraseña");
            
            // Verificar contraseña actual (excepto si es admin reseteando)
            if (_currentUser?.Id == userId)
            {
                if (!VerifyPassword(currentPassword, usuario.PasswordHash))
                    return Result.Fail("Contraseña actual incorrecta");
            }
            
            // Validar nueva contraseña
            var validacion = ValidarPassword(newPassword);
            if (!validacion.IsSuccess)
                return validacion;
            
            // Actualizar contraseña
            usuario.PasswordHash = HashPassword(newPassword);
            await _usuarioRepository.UpdateAsync(usuario);
            
            _logger.LogInformation("Contraseña cambiada para usuario {UserId}", userId);
            await RegistrarAuditoria("CAMBIO_PASSWORD", "Usuario", userId, "Contraseña actualizada");
            
            return Result.Ok("Contraseña actualizada correctamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cambiando contraseña de usuario {UserId}", userId);
            return Result.Fail("Error al cambiar contraseña");
        }
    }
    
    public async Task<Result> ResetPasswordAsync(int userId, string newPassword)
    {
        if (!IsAdmin)
            return Result.Fail("Solo administradores pueden resetear contraseñas");
        
        return await ChangePasswordAsync(userId, "", newPassword);
    }
    
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);
    }
    
    public bool VerifyPassword(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }
    
    private Result ValidarPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return Result.Fail("La contraseña es requerida");
        
        if (password.Length < 6)
            return Result.Fail("La contraseña debe tener al menos 6 caracteres");
        
        // Agregar más validaciones si se requiere (mayúsculas, números, etc.)
        
        return Result.Ok();
    }
    
    private async Task RegistrarAuditoria(string accion, string entidad, int? entidadId, string descripcion)
    {
        try
        {
            var log = new AuditLog
            {
                FechaHora = DateTime.Now,
                UsuarioId = _currentUser?.Id,
                UsuarioNombre = _currentUser?.NombreCompleto ?? "Sistema",
                Accion = accion,
                Entidad = entidad,
                EntidadId = entidadId,
                Descripcion = descripcion
            };
            
            await _auditRepository.AddAsync(log);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error registrando auditoría: {Accion}", accion);
        }
    }
}
