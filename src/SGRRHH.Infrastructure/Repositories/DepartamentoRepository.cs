using Microsoft.EntityFrameworkCore;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;
using SGRRHH.Infrastructure.Data;

namespace SGRRHH.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de departamentos
/// </summary>
public class DepartamentoRepository : Repository<Departamento>, IDepartamentoRepository
{
    private readonly AppDbContext _appContext;
    
    public DepartamentoRepository(AppDbContext context) : base(context)
    {
        _appContext = context;
    }
    
    public async Task<Departamento?> GetByCodigoAsync(string codigo)
    {
        return await _dbSet.FirstOrDefaultAsync(d => d.Codigo == codigo);
    }
    
    public async Task<Departamento?> GetByIdWithEmpleadosAsync(int id)
    {
        return await _dbSet
            .Include(d => d.Empleados.Where(e => e.Activo))
            .FirstOrDefaultAsync(d => d.Id == id);
    }
    
    public async Task<Departamento?> GetByIdWithCargosAsync(int id)
    {
        return await _dbSet
            .Include(d => d.Cargos.Where(c => c.Activo))
            .FirstOrDefaultAsync(d => d.Id == id);
    }
    
    public async Task<IEnumerable<Departamento>> GetAllWithEmpleadosCountAsync()
    {
        return await _dbSet
            .Where(d => d.Activo)
            .Include(d => d.Empleados.Where(e => e.Activo))
            .OrderBy(d => d.Nombre)
            .ToListAsync();
    }
    
    public async Task<bool> ExistsCodigoAsync(string codigo, int? excludeId = null)
    {
        var query = _dbSet.Where(d => d.Codigo == codigo);
        
        if (excludeId.HasValue)
            query = query.Where(d => d.Id != excludeId.Value);
            
        return await query.AnyAsync();
    }
    
    public async Task<string> GetNextCodigoAsync()
    {
        var lastCodigo = await _dbSet
            .Where(d => d.Codigo.StartsWith("DEP"))
            .OrderByDescending(d => d.Codigo)
            .Select(d => d.Codigo)
            .FirstOrDefaultAsync();
            
        if (string.IsNullOrEmpty(lastCodigo))
            return "DEP001";
            
        if (int.TryParse(lastCodigo.Replace("DEP", ""), out int lastNumber))
        {
            return $"DEP{(lastNumber + 1):D3}";
        }
        
        return "DEP001";
    }
    
    public async Task<bool> HasEmpleadosAsync(int id)
    {
        return await _appContext.Empleados
            .AnyAsync(e => e.DepartamentoId == id && e.Activo);
    }
    
    public async Task<int> CountActiveAsync()
    {
        return await _dbSet.CountAsync(d => d.Activo);
    }
    
    public override async Task<IEnumerable<Departamento>> GetAllActiveAsync()
    {
        return await _dbSet
            .Where(d => d.Activo)
            .OrderBy(d => d.Nombre)
            .ToListAsync();
    }
}
