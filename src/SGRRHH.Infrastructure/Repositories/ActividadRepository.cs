using Microsoft.EntityFrameworkCore;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;
using SGRRHH.Infrastructure.Data;

namespace SGRRHH.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de actividades
/// </summary>
public class ActividadRepository : Repository<Actividad>, IActividadRepository
{
    public ActividadRepository(AppDbContext context) : base(context)
    {
    }
    
    public new async Task<IEnumerable<Actividad>> GetAllActiveAsync()
    {
        return await _dbSet
            .Where(a => a.Activo)
            .OrderBy(a => a.Categoria)
            .ThenBy(a => a.Orden)
            .ThenBy(a => a.Nombre)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Actividad>> GetByCategoriaAsync(string categoria)
    {
        return await _dbSet
            .Where(a => a.Activo && a.Categoria == categoria)
            .OrderBy(a => a.Orden)
            .ThenBy(a => a.Nombre)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<string>> GetCategoriasAsync()
    {
        return await _dbSet
            .Where(a => a.Activo && !string.IsNullOrEmpty(a.Categoria))
            .Select(a => a.Categoria!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Actividad>> SearchAsync(string searchTerm)
    {
        var term = searchTerm.ToLower().Trim();
        return await _dbSet
            .Where(a => a.Activo &&
                (a.Codigo.ToLower().Contains(term) ||
                 a.Nombre.ToLower().Contains(term) ||
                 (a.Categoria != null && a.Categoria.ToLower().Contains(term)) ||
                 (a.Descripcion != null && a.Descripcion.ToLower().Contains(term))))
            .OrderBy(a => a.Categoria)
            .ThenBy(a => a.Orden)
            .ThenBy(a => a.Nombre)
            .ToListAsync();
    }
    
    public async Task<bool> ExistsCodigoAsync(string codigo, int? excludeId = null)
    {
        var query = _dbSet.Where(a => a.Codigo.ToLower() == codigo.ToLower());
        if (excludeId.HasValue)
        {
            query = query.Where(a => a.Id != excludeId.Value);
        }
        return await query.AnyAsync();
    }
    
    public async Task<string> GetNextCodigoAsync()
    {
        var lastActividad = await _dbSet
            .OrderByDescending(a => a.Id)
            .FirstOrDefaultAsync();
            
        int nextNumber = 1;
        if (lastActividad != null)
        {
            var parts = lastActividad.Codigo.Split('-');
            if (parts.Length > 1 && int.TryParse(parts[1], out int num))
            {
                nextNumber = num + 1;
            }
            else
            {
                nextNumber = lastActividad.Id + 1;
            }
        }
        
        return $"ACT-{nextNumber:D4}";
    }
}
