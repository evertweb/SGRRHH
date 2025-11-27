using SGRRHH.Core.Entities;

namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Interfaz para el repositorio de usuarios
/// </summary>
public interface IUsuarioRepository : IRepository<Usuario>
{
    /// <summary>
    /// Obtiene un usuario por su nombre de usuario
    /// </summary>
    Task<Usuario?> GetByUsernameAsync(string username);
    
    /// <summary>
    /// Verifica si existe un usuario con el nombre de usuario especificado
    /// </summary>
    Task<bool> ExistsUsernameAsync(string username);
}
