using SGRRHH.Local.Domain.Entities;

namespace SGRRHH.Local.Shared.Interfaces;

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<Usuario?> GetByUsernameAsync(string username);
    
    Task<bool> ExistsUsernameAsync(string username);
    
    Task UpdateLastAccessAsync(int userId);
}


