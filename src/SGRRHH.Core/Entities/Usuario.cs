using SGRRHH.Core.Enums;

namespace SGRRHH.Core.Entities;

/// <summary>
/// Entidad que representa un usuario del sistema
/// </summary>
public class Usuario : EntidadBase
{
    /// <summary>
    /// Nombre de usuario para iniciar sesión
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Hash de la contraseña (BCrypt)
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;
    
    /// <summary>
    /// Nombre completo del usuario
    /// </summary>
    public string NombreCompleto { get; set; } = string.Empty;
    
    /// <summary>
    /// Correo electrónico del usuario
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// Rol del usuario en el sistema
    /// </summary>
    public RolUsuario Rol { get; set; }
    
    /// <summary>
    /// Fecha del último inicio de sesión
    /// </summary>
    public DateTime? UltimoAcceso { get; set; }
    
    /// <summary>
    /// ID del empleado asociado (opcional)
    /// </summary>
    public int? EmpleadoId { get; set; }
}
