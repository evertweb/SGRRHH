using Microsoft.EntityFrameworkCore;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;
using SGRRHH.Infrastructure.Data;

namespace SGRRHH.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de proyectos
/// </summary>
public class ProyectoRepository : Repository<Proyecto>, IProyectoRepository
{
    public ProyectoRepository(AppDbContext context) : base(context)
    {
    }
    
    public override async Task<IEnumerable<Proyecto>> GetAllActiveAsync()
    {
        return await _dbSet
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Proyecto>> GetByEstadoAsync(EstadoProyecto estado)
    {
        return await _dbSet
            .Where(p => p.Activo && p.Estado == estado)
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Proyecto>> SearchAsync(string searchTerm)
    {
        var term = searchTerm.ToLower().Trim();
        return await _dbSet
            .Where(p => p.Activo &&
                (p.Codigo.ToLower().Contains(term) ||
                 p.Nombre.ToLower().Contains(term) ||
                 (p.Cliente != null && p.Cliente.ToLower().Contains(term)) ||
                 (p.Descripcion != null && p.Descripcion.ToLower().Contains(term))))
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }
    
    public async Task<bool> ExistsCodigoAsync(string codigo, int? excludeId = null)
    {
        var query = _dbSet.Where(p => p.Codigo.ToLower() == codigo.ToLower());
        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }
        return await query.AnyAsync();
    }
    
    public async Task<string> GetNextCodigoAsync()
    {
        var lastProject = await _dbSet
            .OrderByDescending(p => p.Id)
            .FirstOrDefaultAsync();
            
        int nextNumber = 1;
        if (lastProject != null)
        {
            // Intentar extraer número del código
            var parts = lastProject.Codigo.Split('-');
            if (parts.Length > 1 && int.TryParse(parts[1], out int num))
            {
                nextNumber = num + 1;
            }
            else
            {
                nextNumber = lastProject.Id + 1;
            }
        }
        
        return $"PRY-{nextNumber:D4}";
    }
}
