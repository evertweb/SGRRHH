using BCrypt.Net;
using SGRRHH.Core.Common;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de gestión de usuarios
/// </summary>
public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IAuditService? _auditService;
    
    public UsuarioService(IUsuarioRepository usuarioRepository, IAuditService? auditService = null)
    {
        _usuarioRepository = usuarioRepository;
        _auditService = auditService;
    }
    
    public async Task<List<Usuario>> GetAllAsync()
    {
        var usuarios = await _usuarioRepository.GetAllAsync();
        return usuarios.ToList();
    }
    
    public async Task<Usuario?> GetByIdAsync(int id)
    {
        return await _usuarioRepository.GetByIdAsync(id);
    }
    
    public async Task<Usuario?> GetByUsernameAsync(string username)
    {
        return await _usuarioRepository.GetByUsernameAsync(username);
    }
    
    public async Task<ServiceResult<Usuario>> CreateAsync(string username, string password, string nombreCompleto, RolUsuario rol, string? email = null, int? empleadoId = null)
    {
        try
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(username))
                return ServiceResult<Usuario>.Fail("El nombre de usuario es requerido");
                
            if (string.IsNullOrWhiteSpace(password))
                return ServiceResult<Usuario>.Fail("La contraseña es requerida");
                
            if (password.Length < 6)
                return ServiceResult<Usuario>.Fail("La contraseña debe tener al menos 6 caracteres");
                
            if (string.IsNullOrWhiteSpace(nombreCompleto))
                return ServiceResult<Usuario>.Fail("El nombre completo es requerido");
            
            // Verificar que el username no exista
            if (await _usuarioRepository.ExistsUsernameAsync(username))
                return ServiceResult<Usuario>.Fail($"Ya existe un usuario con el nombre '{username}'");
            
            var usuario = new Usuario
            {
                Username = username.ToLower().Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12)),
                NombreCompleto = nombreCompleto.Trim(),
                Email = email?.Trim(),
                Rol = rol,
                EmpleadoId = empleadoId,
                Activo = true,
                FechaCreacion = DateTime.Now
            };
            
            await _usuarioRepository.AddAsync(usuario);
            await _usuarioRepository.SaveChangesAsync();
            
            // Registrar en auditoría
            if (_auditService != null)
            {
                await _auditService.RegistrarAsync("Crear", "Usuario", usuario.Id, $"Usuario creado: {usuario.Username} ({usuario.Rol})");
            }
            
            return ServiceResult<Usuario>.Ok(usuario, "Usuario creado exitosamente");
        }
        catch (Exception ex)
        {
            return ServiceResult<Usuario>.Fail($"Error al crear usuario: {ex.Message}");
        }
    }
    
    public async Task<ServiceResult<Usuario>> UpdateAsync(int id, string nombreCompleto, RolUsuario rol, string? email = null, int? empleadoId = null, bool activo = true)
    {
        try
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id);
            
            if (usuario == null)
                return ServiceResult<Usuario>.Fail("Usuario no encontrado");
            
            // Validaciones
            if (string.IsNullOrWhiteSpace(nombreCompleto))
                return ServiceResult<Usuario>.Fail("El nombre completo es requerido");
            
            usuario.NombreCompleto = nombreCompleto.Trim();
            usuario.Rol = rol;
            usuario.Email = email?.Trim();
            usuario.EmpleadoId = empleadoId;
            usuario.Activo = activo;
            usuario.FechaModificacion = DateTime.Now;
            
            await _usuarioRepository.UpdateAsync(usuario);
            await _usuarioRepository.SaveChangesAsync();
            
            // Registrar en auditoría
            if (_auditService != null)
            {
                await _auditService.RegistrarAsync("Actualizar", "Usuario", usuario.Id, $"Usuario actualizado: {usuario.Username}");
            }
            
            return ServiceResult<Usuario>.Ok(usuario, "Usuario actualizado exitosamente");
        }
        catch (Exception ex)
        {
            return ServiceResult<Usuario>.Fail($"Error al actualizar usuario: {ex.Message}");
        }
    }
    
    public async Task<ServiceResult> ResetPasswordAsync(int id, string newPassword)
    {
        try
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id);
            
            if (usuario == null)
                return ServiceResult.Fail("Usuario no encontrado");
            
            if (string.IsNullOrWhiteSpace(newPassword))
                return ServiceResult.Fail("La nueva contraseña es requerida");
                
            if (newPassword.Length < 6)
                return ServiceResult.Fail("La contraseña debe tener al menos 6 caracteres");
            
            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword, BCrypt.Net.BCrypt.GenerateSalt(12));
            usuario.FechaModificacion = DateTime.Now;
            
            await _usuarioRepository.UpdateAsync(usuario);
            await _usuarioRepository.SaveChangesAsync();
            
            // Registrar en auditoría
            if (_auditService != null)
            {
                await _auditService.RegistrarAsync("ResetPassword", "Usuario", usuario.Id, $"Contraseña restablecida para: {usuario.Username}");
            }
            
            return ServiceResult.Ok("Contraseña restablecida exitosamente");
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail($"Error al restablecer contraseña: {ex.Message}");
        }
    }
    
    public async Task<ServiceResult> ChangePasswordAsync(int id, string currentPassword, string newPassword)
    {
        try
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id);
            
            if (usuario == null)
                return ServiceResult.Fail("Usuario no encontrado");
            
            // Verificar contraseña actual
            if (!BCrypt.Net.BCrypt.Verify(currentPassword, usuario.PasswordHash))
                return ServiceResult.Fail("La contraseña actual es incorrecta");
            
            if (string.IsNullOrWhiteSpace(newPassword))
                return ServiceResult.Fail("La nueva contraseña es requerida");
                
            if (newPassword.Length < 6)
                return ServiceResult.Fail("La nueva contraseña debe tener al menos 6 caracteres");
            
            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword, BCrypt.Net.BCrypt.GenerateSalt(12));
            usuario.FechaModificacion = DateTime.Now;
            
            await _usuarioRepository.UpdateAsync(usuario);
            await _usuarioRepository.SaveChangesAsync();
            
            // Registrar en auditoría
            if (_auditService != null)
            {
                await _auditService.RegistrarAsync("ChangePassword", "Usuario", usuario.Id, $"Usuario cambió su contraseña: {usuario.Username}");
            }
            
            return ServiceResult.Ok("Contraseña cambiada exitosamente");
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail($"Error al cambiar contraseña: {ex.Message}");
        }
    }
    
    public async Task<ServiceResult> SetActivoAsync(int id, bool activo)
    {
        try
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id);
            
            if (usuario == null)
                return ServiceResult.Fail("Usuario no encontrado");
            
            usuario.Activo = activo;
            usuario.FechaModificacion = DateTime.Now;
            
            await _usuarioRepository.UpdateAsync(usuario);
            await _usuarioRepository.SaveChangesAsync();
            
            // Registrar en auditoría
            if (_auditService != null)
            {
                var accion = activo ? "Activar" : "Desactivar";
                await _auditService.RegistrarAsync(accion, "Usuario", usuario.Id, $"Usuario {(activo ? "activado" : "desactivado")}: {usuario.Username}");
            }
            
            return ServiceResult.Ok($"Usuario {(activo ? "activado" : "desactivado")} exitosamente");
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail($"Error al cambiar estado del usuario: {ex.Message}");
        }
    }
    
    public async Task<ServiceResult> DeleteAsync(int id)
    {
        try
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id);
            
            if (usuario == null)
                return ServiceResult.Fail("Usuario no encontrado");
            
            var username = usuario.Username;
            
            await _usuarioRepository.DeleteAsync(id);
            await _usuarioRepository.SaveChangesAsync();
            
            // Registrar en auditoría
            if (_auditService != null)
            {
                await _auditService.RegistrarAsync("Eliminar", "Usuario", id, $"Usuario eliminado: {username}");
            }
            
            return ServiceResult.Ok("Usuario eliminado exitosamente");
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail($"Error al eliminar usuario: {ex.Message}");
        }
    }
    
    public async Task<bool> ExistsUsernameAsync(string username, int? excludeId = null)
    {
        var usuario = await _usuarioRepository.GetByUsernameAsync(username.ToLower().Trim());
        
        if (usuario == null)
            return false;
            
        // Si se proporciona un ID a excluir y coincide, no cuenta como duplicado
        if (excludeId.HasValue && usuario.Id == excludeId.Value)
            return false;
            
        return true;
    }
    
    public async Task<int> CountActivosAsync()
    {
        var todos = await _usuarioRepository.GetAllAsync();
        return todos.Count(u => u.Activo);
    }
}
