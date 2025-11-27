using Microsoft.EntityFrameworkCore;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;
using SGRRHH.Infrastructure.Data;

namespace SGRRHH.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de cargos
/// </summary>
public class CargoRepository : Repository<Cargo>, ICargoRepository
{
    private readonly AppDbContext _appContext;
    
    public CargoRepository(AppDbContext context) : base(context)
    {
        _appContext = context;
    }
    
    public async Task<Cargo?> GetByCodigoAsync(string codigo)
    {
        return await _dbSet
            .Include(c => c.Departamento)
            .FirstOrDefaultAsync(c => c.Codigo == codigo);
    }
    
    public async Task<Cargo?> GetByIdWithDepartamentoAsync(int id)
    {
        return await _dbSet
            .Include(c => c.Departamento)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
    
    public async Task<Cargo?> GetByIdWithEmpleadosAsync(int id)
    {
        return await _dbSet
            .Include(c => c.Empleados.Where(e => e.Activo))
            .Include(c => c.Departamento)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
    
    public async Task<IEnumerable<Cargo>> GetAllWithDepartamentoAsync()
    {
        return await _dbSet
            .Include(c => c.Departamento)
            .OrderBy(c => c.Departamento!.Nombre)
            .ThenBy(c => c.Nivel)
            .ThenBy(c => c.Nombre)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Cargo>> GetAllActiveWithDepartamentoAsync()
    {
        return await _dbSet
            .Where(c => c.Activo)
            .Include(c => c.Departamento)
            .OrderBy(c => c.Departamento!.Nombre)
            .ThenBy(c => c.Nivel)
            .ThenBy(c => c.Nombre)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Cargo>> GetByDepartamentoAsync(int departamentoId)
    {
        return await _dbSet
            .Where(c => c.DepartamentoId == departamentoId && c.Activo)
            .OrderBy(c => c.Nivel)
            .ThenBy(c => c.Nombre)
            .ToListAsync();
    }
    
    public async Task<bool> ExistsCodigoAsync(string codigo, int? excludeId = null)
    {
        var query = _dbSet.Where(c => c.Codigo == codigo);
        
        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);
            
        return await query.AnyAsync();
    }
    
    public async Task<string> GetNextCodigoAsync()
    {
        var lastCodigo = await _dbSet
            .Where(c => c.Codigo.StartsWith("CAR"))
            .OrderByDescending(c => c.Codigo)
            .Select(c => c.Codigo)
            .FirstOrDefaultAsync();
            
        if (string.IsNullOrEmpty(lastCodigo))
            return "CAR001";
            
        if (int.TryParse(lastCodigo.Replace("CAR", ""), out int lastNumber))
        {
            return $"CAR{(lastNumber + 1):D3}";
        }
        
        return "CAR001";
    }
    
    public async Task<bool> HasEmpleadosAsync(int id)
    {
        return await _appContext.Empleados
            .AnyAsync(e => e.CargoId == id && e.Activo);
    }
    
    public async Task<int> CountActiveAsync()
    {
        return await _dbSet.CountAsync(c => c.Activo);
    }
    
    public override async Task<IEnumerable<Cargo>> GetAllActiveAsync()
    {
        return await _dbSet
            .Where(c => c.Activo)
            .Include(c => c.Departamento)
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }
}
