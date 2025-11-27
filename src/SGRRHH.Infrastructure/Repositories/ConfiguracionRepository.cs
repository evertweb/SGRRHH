using Microsoft.EntityFrameworkCore;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;
using SGRRHH.Infrastructure.Data;

namespace SGRRHH.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de configuraciones del sistema
/// </summary>
public class ConfiguracionRepository : Repository<ConfiguracionSistema>, IConfiguracionRepository
{
    public ConfiguracionRepository(AppDbContext context) : base(context)
    {
    }
    
    public async Task<ConfiguracionSistema?> GetByClaveAsync(string clave)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.Clave == clave);
    }
    
    public async Task<List<ConfiguracionSistema>> GetByCategoriaAsync(string categoria)
    {
        return await _dbSet
            .Where(c => c.Categoria == categoria && c.Activo)
            .OrderBy(c => c.Clave)
            .ToListAsync();
    }
    
    public async Task<bool> ExistsClaveAsync(string clave)
    {
        return await _dbSet.AnyAsync(c => c.Clave == clave);
    }
}
