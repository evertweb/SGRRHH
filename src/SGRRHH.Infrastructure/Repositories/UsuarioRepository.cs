using Microsoft.EntityFrameworkCore;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;
using SGRRHH.Infrastructure.Data;

namespace SGRRHH.Infrastructure.Repositories;

/// <summary>
/// Repositorio para la entidad Usuario
/// </summary>
public class UsuarioRepository : Repository<Usuario>, IUsuarioRepository
{
    private readonly AppDbContext _appContext;
    
    public UsuarioRepository(AppDbContext context) : base(context)
    {
        _appContext = context;
    }
    
    public async Task<Usuario?> GetByUsernameAsync(string username)
    {
        return await _appContext.Usuarios
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower() && u.Activo);
    }
    
    public async Task<bool> ExistsUsernameAsync(string username)
    {
        return await _appContext.Usuarios
            .AnyAsync(u => u.Username.ToLower() == username.ToLower());
    }
}
