using Microsoft.EntityFrameworkCore;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;
using SGRRHH.Infrastructure.Data;

namespace SGRRHH.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de empleados
/// </summary>
public class EmpleadoRepository : Repository<Empleado>, IEmpleadoRepository
{
    private readonly AppDbContext _appContext;
    
    public EmpleadoRepository(AppDbContext context) : base(context)
    {
        _appContext = context;
    }
    
    public async Task<Empleado?> GetByCodigoAsync(string codigo)
    {
        return await _dbSet
            .Include(e => e.Cargo)
            .Include(e => e.Departamento)
            .FirstOrDefaultAsync(e => e.Codigo == codigo);
    }
    
    public async Task<Empleado?> GetByCedulaAsync(string cedula)
    {
        return await _dbSet
            .Include(e => e.Cargo)
            .Include(e => e.Departamento)
            .FirstOrDefaultAsync(e => e.Cedula == cedula);
    }
    
    public async Task<Empleado?> GetByIdWithRelationsAsync(int id)
    {
        return await _dbSet
            .Include(e => e.Cargo)
                .ThenInclude(c => c!.Departamento)
            .Include(e => e.Departamento)
            .Include(e => e.Supervisor)
            .FirstOrDefaultAsync(e => e.Id == id);
    }
    
    public async Task<IEnumerable<Empleado>> GetAllWithRelationsAsync()
    {
        return await _dbSet
            .Include(e => e.Cargo)
            .Include(e => e.Departamento)
            .OrderBy(e => e.Apellidos)
            .ThenBy(e => e.Nombres)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Empleado>> GetAllActiveWithRelationsAsync()
    {
        return await _dbSet
            .Where(e => e.Activo && e.Estado == EstadoEmpleado.Activo)
            .Include(e => e.Cargo)
            .Include(e => e.Departamento)
            .OrderBy(e => e.Apellidos)
            .ThenBy(e => e.Nombres)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Empleado>> GetByDepartamentoAsync(int departamentoId)
    {
        return await _dbSet
            .Where(e => e.DepartamentoId == departamentoId && e.Activo)
            .Include(e => e.Cargo)
            .OrderBy(e => e.Apellidos)
            .ThenBy(e => e.Nombres)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Empleado>> GetByCargoAsync(int cargoId)
    {
        return await _dbSet
            .Where(e => e.CargoId == cargoId && e.Activo)
            .Include(e => e.Departamento)
            .OrderBy(e => e.Apellidos)
            .ThenBy(e => e.Nombres)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Empleado>> GetByEstadoAsync(EstadoEmpleado estado)
    {
        return await _dbSet
            .Where(e => e.Estado == estado && e.Activo)
            .Include(e => e.Cargo)
            .Include(e => e.Departamento)
            .OrderBy(e => e.Apellidos)
            .ThenBy(e => e.Nombres)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Empleado>> SearchAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllActiveWithRelationsAsync();
            
        var term = searchTerm.ToLower();
        
        return await _dbSet
            .Where(e => e.Activo && (
                e.Nombres.ToLower().Contains(term) ||
                e.Apellidos.ToLower().Contains(term) ||
                e.Cedula.Contains(term) ||
                e.Codigo.ToLower().Contains(term) ||
                (e.Email != null && e.Email.ToLower().Contains(term))
            ))
            .Include(e => e.Cargo)
            .Include(e => e.Departamento)
            .OrderBy(e => e.Apellidos)
            .ThenBy(e => e.Nombres)
            .ToListAsync();
    }
    
    public async Task<bool> ExistsCodigoAsync(string codigo, int? excludeId = null)
    {
        var query = _dbSet.Where(e => e.Codigo == codigo);
        
        if (excludeId.HasValue)
            query = query.Where(e => e.Id != excludeId.Value);
            
        return await query.AnyAsync();
    }
    
    public async Task<bool> ExistsCedulaAsync(string cedula, int? excludeId = null)
    {
        var query = _dbSet.Where(e => e.Cedula == cedula);
        
        if (excludeId.HasValue)
            query = query.Where(e => e.Id != excludeId.Value);
            
        return await query.AnyAsync();
    }
    
    public async Task<string> GetNextCodigoAsync()
    {
        // Verificar si hay algún empleado en la base de datos (activo o inactivo)
        var hayEmpleados = await _dbSet.AnyAsync();
        
        if (!hayEmpleados)
            return "EMP001";
        
        var lastCodigo = await _dbSet
            .Where(e => e.Codigo.StartsWith("EMP"))
            .OrderByDescending(e => e.Codigo)
            .Select(e => e.Codigo)
            .FirstOrDefaultAsync();
            
        if (string.IsNullOrEmpty(lastCodigo))
            return "EMP001";
            
        // Extraer el número del código (EMP001 -> 1)
        if (int.TryParse(lastCodigo.Replace("EMP", ""), out int lastNumber))
        {
            return $"EMP{(lastNumber + 1):D3}";
        }
        
        return "EMP001";
    }
    
    public async Task<int> CountActiveAsync()
    {
        return await _dbSet.CountAsync(e => e.Activo && e.Estado == EstadoEmpleado.Activo);
    }
}
