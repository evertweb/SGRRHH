using BCrypt.Net;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de autenticación usando BCrypt
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    
    public AuthService(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }
    
    public async Task<AuthResult> AuthenticateAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return new AuthResult
            {
                Success = false,
                Message = "Usuario y contraseña son requeridos"
            };
        }
        
        var usuario = await _usuarioRepository.GetByUsernameAsync(username);
        
        if (usuario == null)
        {
            return new AuthResult
            {
                Success = false,
                Message = "Usuario no encontrado"
            };
        }
        
        if (!usuario.Activo)
        {
            return new AuthResult
            {
                Success = false,
                Message = "Usuario desactivado"
            };
        }
        
        if (!VerifyPassword(password, usuario.PasswordHash))
        {
            return new AuthResult
            {
                Success = false,
                Message = "Contraseña incorrecta"
            };
        }
        
        // Actualizar último acceso
        usuario.UltimoAcceso = DateTime.Now;
        await _usuarioRepository.UpdateAsync(usuario);
        await _usuarioRepository.SaveChangesAsync();
        
        return new AuthResult
        {
            Success = true,
            Message = "Autenticación exitosa",
            Usuario = usuario
        };
    }
    
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
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
    
    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(userId);
        
        if (usuario == null)
            return false;
            
        if (!VerifyPassword(currentPassword, usuario.PasswordHash))
            return false;
            
        usuario.PasswordHash = HashPassword(newPassword);
        await _usuarioRepository.UpdateAsync(usuario);
        await _usuarioRepository.SaveChangesAsync();
        
        return true;
    }
}
