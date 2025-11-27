using SGRRHH.Core.Common;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;

namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Interfaz para el servicio de gestión de usuarios
/// </summary>
public interface IUsuarioService
{
    /// <summary>
    /// Obtiene todos los usuarios
    /// </summary>
    Task<List<Usuario>> GetAllAsync();
    
    /// <summary>
    /// Obtiene un usuario por su ID
    /// </summary>
    Task<Usuario?> GetByIdAsync(int id);
    
    /// <summary>
    /// Obtiene un usuario por su nombre de usuario
    /// </summary>
    Task<Usuario?> GetByUsernameAsync(string username);
    
    /// <summary>
    /// Crea un nuevo usuario
    /// </summary>
    Task<ServiceResult<Usuario>> CreateAsync(string username, string password, string nombreCompleto, RolUsuario rol, string? email = null, int? empleadoId = null);
    
    /// <summary>
    /// Actualiza un usuario existente (sin cambiar contraseña)
    /// </summary>
    Task<ServiceResult<Usuario>> UpdateAsync(int id, string nombreCompleto, RolUsuario rol, string? email = null, int? empleadoId = null, bool activo = true);
    
    /// <summary>
    /// Cambia la contraseña de un usuario (como admin, sin verificar contraseña actual)
    /// </summary>
    Task<ServiceResult> ResetPasswordAsync(int id, string newPassword);
    
    /// <summary>
    /// Cambia la contraseña del usuario actual (verificando contraseña actual)
    /// </summary>
    Task<ServiceResult> ChangePasswordAsync(int id, string currentPassword, string newPassword);
    
    /// <summary>
    /// Activa o desactiva un usuario
    /// </summary>
    Task<ServiceResult> SetActivoAsync(int id, bool activo);
    
    /// <summary>
    /// Elimina un usuario
    /// </summary>
    Task<ServiceResult> DeleteAsync(int id);
    
    /// <summary>
    /// Verifica si existe un username
    /// </summary>
    Task<bool> ExistsUsernameAsync(string username, int? excludeId = null);
    
    /// <summary>
    /// Obtiene la cantidad de usuarios activos
    /// </summary>
    Task<int> CountActivosAsync();
}
