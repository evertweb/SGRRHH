using System.Security.Cryptography;
using BCrypt.Net;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Configuration;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Shared;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Services;

public class LocalAuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IAuditLogRepository _auditRepository;
    private readonly IDispositivoAutorizadoRepository _dispositivoRepository;
    private readonly IConfiguracionService _configuracionService;
    private readonly ILogger<LocalAuthService> _logger;
    
    private Usuario? _currentUser;
    private bool _modoCorporativoActivo = false;
    
    public Usuario? CurrentUser => _currentUser;
    public int? CurrentUserId => _currentUser?.Id;
    public bool IsAuthenticated => _currentUser != null;
    
    // =====================================================================
    // SISTEMA DE ROLES CON MODO CORPORATIVO
    // Si Modo Corporativo está DESACTIVADO: todos tienen acceso completo
    // Si Modo Corporativo está ACTIVADO: se aplican restricciones de rol
    // =====================================================================
    public bool ModoCorporativoActivo => _modoCorporativoActivo;
    
    public bool IsAdmin => IsAuthenticated && 
        (!_modoCorporativoActivo || _currentUser?.Rol == RolUsuario.Administrador);
    
    public bool IsAprobador => IsAuthenticated && 
        (!_modoCorporativoActivo || _currentUser?.Rol == RolUsuario.Administrador || 
         _currentUser?.Rol == RolUsuario.Aprobador);
    
    public bool IsOperador => IsAuthenticated; // Todos los autenticados pueden ver
    
    public bool IsSupervisor => IsAprobador; // Equivalente a Aprobador
    
    public event EventHandler<Usuario?>? OnAuthStateChanged;
    
    public LocalAuthService(
        IUsuarioRepository usuarioRepository,
        IAuditLogRepository auditRepository,
        IDispositivoAutorizadoRepository dispositivoRepository,
        IConfiguracionService configuracionService,
        ILogger<LocalAuthService> logger)
    {
        _usuarioRepository = usuarioRepository;
        _auditRepository = auditRepository;
        _dispositivoRepository = dispositivoRepository;
        _configuracionService = configuracionService;
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
            
            // Cargar estado del Modo Corporativo
            var modoCorp = await _configuracionService.GetAsync("modo_corporativo");
            _modoCorporativoActivo = modoCorp == "1";
            
            _logger.LogInformation("Login exitoso: {Username} (Rol: {Rol}, ModoCorp: {ModoCorp})", 
                username, usuario.Rol, _modoCorporativoActivo);
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
    
    /// <summary>
    /// Intenta restaurar la sesión buscando el usuario por ID.
    /// El ID se obtiene desde localStorage por el componente que llama este método.
    /// </summary>
    public async Task<bool> TryRestoreSessionAsync()
    {
        // Este método es llamado por el componente AuthPersistence que lee el userId de localStorage
        // Si ya hay un usuario autenticado, no hacer nada
        if (_currentUser != null)
            return true;
            
        return false;
    }
    
    /// <summary>
    /// Establece el usuario actual (usado para restaurar sesión desde localStorage).
    /// </summary>
    public void SetCurrentUser(Usuario? user)
    {
        _currentUser = user;
        if (user != null)
        {
            _logger.LogInformation("Sesión restaurada para: {Username}", user.Username);
        }
        OnAuthStateChanged?.Invoke(this, _currentUser);
    }
    
    /// <summary>
    /// Obtiene un usuario por ID para restaurar sesión.
    /// </summary>
    public async Task<Usuario?> GetUserByIdAsync(int userId)
    {
        try
        {
            var usuario = await _usuarioRepository.GetByIdAsync(userId);
            if (usuario != null && usuario.Activo)
            {
                return usuario;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error obteniendo usuario {UserId} para restaurar sesión", userId);
        }
        return null;
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
    
    // =========================================================================
    // AUTENTICACIÓN POR DISPOSITIVO (sin contraseña, estilo SSH)
    // =========================================================================
    
    /// <summary>
    /// Genera un token único para identificar un dispositivo.
    /// Combina un UUID aleatorio con un hash para mayor seguridad.
    /// </summary>
    private static string GenerarTokenDispositivo()
    {
        // Generar 32 bytes aleatorios (256 bits)
        var randomBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        
        // Combinar UUID + bytes aleatorios en base64
        var uuid = Guid.NewGuid().ToString("N");
        var randomPart = Convert.ToBase64String(randomBytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
        
        return $"{uuid}_{randomPart}";
    }
    
    public async Task<Result<Usuario>> LoginConTokenDispositivoAsync(string deviceToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(deviceToken))
            {
                return Result<Usuario>.Fail("Token de dispositivo requerido");
            }
            
            var dispositivo = await _dispositivoRepository.GetByTokenAsync(deviceToken);
            
            if (dispositivo == null)
            {
                _logger.LogWarning("Login con token fallido: token no encontrado o expirado");
                return Result<Usuario>.Fail("Dispositivo no autorizado o expirado");
            }
            
            var usuario = await _usuarioRepository.GetByIdAsync(dispositivo.UsuarioId);
            
            if (usuario == null || !usuario.Activo)
            {
                _logger.LogWarning("Login con token fallido: usuario {UsuarioId} no existe o inactivo", dispositivo.UsuarioId);
                await _dispositivoRepository.RevocarAsync(dispositivo.Id);
                return Result<Usuario>.Fail("Usuario no disponible");
            }
            
            // Login exitoso con token de dispositivo
            _currentUser = usuario;
            await _usuarioRepository.UpdateLastAccessAsync(usuario.Id);
            await _dispositivoRepository.ActualizarUltimoUsoAsync(dispositivo.Id);
            
            // Cargar estado del Modo Corporativo
            var modoCorp = await _configuracionService.GetAsync("modo_corporativo");
            _modoCorporativoActivo = modoCorp == "1";
            
            _logger.LogInformation("Login con dispositivo autorizado: {Username} desde '{Dispositivo}' (ModoCorp: {ModoCorp})", 
                usuario.Username, dispositivo.NombreDispositivo, _modoCorporativoActivo);
            await RegistrarAuditoria("LOGIN_DISPOSITIVO", "DispositivoAutorizado", dispositivo.Id, 
                $"Login automático desde dispositivo: {dispositivo.NombreDispositivo}");
            
            OnAuthStateChanged?.Invoke(this, _currentUser);
            
            return Result<Usuario>.Ok(usuario, "Bienvenido, " + usuario.NombreCompleto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante login con token de dispositivo");
            return Result<Usuario>.Fail("Error interno. Intente nuevamente.");
        }
    }
    
    public async Task<Result<string>> AutorizarDispositivoAsync(
        string nombreDispositivo, 
        string? huellaNavegador = null, 
        string? ipCliente = null,
        int? diasExpiracion = null)
    {
        try
        {
            if (!IsAuthenticated)
            {
                return Result<string>.Fail("Debe iniciar sesión primero");
            }
            
            if (string.IsNullOrWhiteSpace(nombreDispositivo))
            {
                return Result<string>.Fail("Nombre del dispositivo requerido");
            }
            
            // Limitar cantidad de dispositivos por usuario (máximo 5 activos)
            var cantidadActual = await _dispositivoRepository.ContarDispositivosActivosAsync(_currentUser!.Id);
            if (cantidadActual >= 5)
            {
                return Result<string>.Fail("Límite de dispositivos alcanzado (máx. 5). Revoque alguno primero.");
            }
            
            var token = GenerarTokenDispositivo();
            
            var dispositivo = new DispositivoAutorizado
            {
                UsuarioId = _currentUser.Id,
                DeviceToken = token,
                NombreDispositivo = nombreDispositivo.Trim(),
                HuellaNavegador = huellaNavegador,
                IpAutorizacion = ipCliente,
                FechaExpiracion = diasExpiracion.HasValue 
                    ? DateTime.Now.AddDays(diasExpiracion.Value) 
                    : null,
                Activo = true
            };
            
            await _dispositivoRepository.AddAsync(dispositivo);
            
            _logger.LogInformation("Dispositivo autorizado: '{Nombre}' para usuario {Username}", 
                nombreDispositivo, _currentUser.Username);
            await RegistrarAuditoria("DISPOSITIVO_AUTORIZADO", "DispositivoAutorizado", dispositivo.Id, 
                $"Dispositivo '{nombreDispositivo}' autorizado para login automático");
            
            return Result<string>.Ok(token, "Dispositivo autorizado correctamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error autorizando dispositivo");
            return Result<string>.Fail("Error al autorizar dispositivo");
        }
    }
    
    public async Task<Result> RevocarDispositivoAsync(int dispositivoId)
    {
        try
        {
            var dispositivo = await _dispositivoRepository.GetByIdAsync(dispositivoId);
            
            if (dispositivo == null)
            {
                return Result.Fail("Dispositivo no encontrado");
            }
            
            // Solo el propietario o admin puede revocar
            if (_currentUser?.Id != dispositivo.UsuarioId && !IsAdmin)
            {
                return Result.Fail("No tiene permisos para revocar este dispositivo");
            }
            
            await _dispositivoRepository.RevocarAsync(dispositivoId);
            
            _logger.LogInformation("Dispositivo revocado: {Id} '{Nombre}'", dispositivoId, dispositivo.NombreDispositivo);
            await RegistrarAuditoria("DISPOSITIVO_REVOCADO", "DispositivoAutorizado", dispositivoId, 
                $"Dispositivo '{dispositivo.NombreDispositivo}' revocado");
            
            return Result.Ok("Dispositivo revocado correctamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revocando dispositivo {Id}", dispositivoId);
            return Result.Fail("Error al revocar dispositivo");
        }
    }
    
    public async Task<Result> RevocarTodosDispositivosAsync(int usuarioId)
    {
        try
        {
            // Solo el propietario o admin puede revocar
            if (_currentUser?.Id != usuarioId && !IsAdmin)
            {
                return Result.Fail("No tiene permisos para revocar estos dispositivos");
            }
            
            await _dispositivoRepository.RevocarTodosDeUsuarioAsync(usuarioId);
            
            _logger.LogInformation("Todos los dispositivos revocados para usuario {UsuarioId}", usuarioId);
            await RegistrarAuditoria("DISPOSITIVOS_REVOCADOS_TODOS", "Usuario", usuarioId, 
                "Todos los dispositivos autorizados revocados");
            
            return Result.Ok("Todos los dispositivos revocados");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revocando dispositivos de usuario {UsuarioId}", usuarioId);
            return Result.Fail("Error al revocar dispositivos");
        }
    }
    
    public async Task<IEnumerable<DispositivoAutorizado>> ObtenerMisDispositivosAsync()
    {
        if (!IsAuthenticated)
            return Enumerable.Empty<DispositivoAutorizado>();
        
        return await _dispositivoRepository.GetByUsuarioIdAsync(_currentUser!.Id);
    }
    
    public async Task<IEnumerable<DispositivoAutorizado>> ObtenerDispositivosDeUsuarioAsync(int usuarioId)
    {
        // Solo el propietario o admin puede ver
        if (_currentUser?.Id != usuarioId && !IsAdmin)
            return Enumerable.Empty<DispositivoAutorizado>();
        
        return await _dispositivoRepository.GetByUsuarioIdAsync(usuarioId);
    }
    
    // =========================================================================
    // MODO CORPORATIVO - Control de Acceso basado en Roles
    // =========================================================================
    
    /// <summary>
    /// Activa o desactiva el Modo Corporativo. Solo disponible para Administradores.
    /// </summary>
    public async Task<Result> SetModoCorporativoAsync(bool activar)
    {
        if (!IsAuthenticated)
            return Result.Fail("Debe iniciar sesión");
        
        // Solo admin puede cambiar el modo (verificación REAL, no afectada por el toggle)
        if (_currentUser?.Rol != RolUsuario.Administrador)
            return Result.Fail("Solo administradores pueden cambiar el Modo Corporativo");
        
        _modoCorporativoActivo = activar;
        
        // Persistir en configuración
        await _configuracionService.SetAsync("modo_corporativo", activar ? "1" : "0");
        
        _logger.LogWarning("Modo Corporativo {Estado} por {Usuario}", 
            activar ? "ACTIVADO" : "DESACTIVADO", _currentUser.Username);
        
        await RegistrarAuditoria(
            activar ? "MODO_CORPORATIVO_ACTIVADO" : "MODO_CORPORATIVO_DESACTIVADO",
            "Sistema", null,
            $"Modo Corporativo {(activar ? "activado" : "desactivado")} por {_currentUser.NombreCompleto}");
        
        OnAuthStateChanged?.Invoke(this, _currentUser);
        
        return Result.Ok($"Modo Corporativo {(activar ? "activado" : "desactivado")}");
    }
    
    /// <summary>
    /// Verifica si el usuario actual tiene un permiso específico en un módulo.
    /// Considera el estado del Modo Corporativo.
    /// </summary>
    public bool TienePermiso(string modulo, PermisosModulo permiso)
    {
        if (!IsAuthenticated)
            return false;
        
        // Si Modo Corporativo está DESACTIVADO, todos tienen acceso completo
        if (!_modoCorporativoActivo)
            return true;
        
        // Si está ACTIVADO, verificar según la matriz de permisos
        return ConfiguracionRoles.TienePermiso(_currentUser!.Rol, modulo, permiso);
    }
    
    /// <summary>
    /// Obtiene todos los permisos del usuario actual para un módulo.
    /// </summary>
    public PermisosModulo ObtenerPermisosModulo(string modulo)
    {
        if (!IsAuthenticated)
            return PermisosModulo.Ninguno;
        
        // Si Modo Corporativo está DESACTIVADO, todos tienen acceso completo
        if (!_modoCorporativoActivo)
            return PermisosModulo.Completo;
        
        return ConfiguracionRoles.ObtenerPermisos(_currentUser!.Rol, modulo);
    }
}
