using Microsoft.EntityFrameworkCore;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;
using SGRRHH.Infrastructure.Data;

namespace SGRRHH.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de vacaciones
/// </summary>
public class VacacionRepository : Repository<Vacacion>, IVacacionRepository
{
    private readonly AppDbContext _appContext;
    
    public VacacionRepository(AppDbContext context) : base(context)
    {
        _appContext = context;
    }
    
    public async Task<IEnumerable<Vacacion>> GetByEmpleadoIdAsync(int empleadoId)
    {
        return await _dbSet
            .Where(v => v.EmpleadoId == empleadoId && v.Activo)
            .Include(v => v.Empleado)
            .OrderByDescending(v => v.FechaInicio)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Vacacion>> GetByEmpleadoYPeriodoAsync(int empleadoId, int periodo)
    {
        return await _dbSet
            .Where(v => v.EmpleadoId == empleadoId && v.PeriodoCorrespondiente == periodo && v.Activo)
            .Include(v => v.Empleado)
            .OrderByDescending(v => v.FechaInicio)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Vacacion>> GetByRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        return await _dbSet
            .Where(v => v.Activo && 
                ((v.FechaInicio >= fechaInicio && v.FechaInicio <= fechaFin) ||
                 (v.FechaFin >= fechaInicio && v.FechaFin <= fechaFin) ||
                 (v.FechaInicio <= fechaInicio && v.FechaFin >= fechaFin)))
            .Include(v => v.Empleado)
            .OrderBy(v => v.FechaInicio)
            .ToListAsync();
    }
    
    public async Task<bool> ExisteTraslapeAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin, int? vacacionIdExcluir = null)
    {
        var query = _dbSet
            .Where(v => v.EmpleadoId == empleadoId && v.Activo && v.Estado != EstadoVacacion.Cancelada &&
                ((v.FechaInicio <= fechaFin && v.FechaFin >= fechaInicio)));
                
        if (vacacionIdExcluir.HasValue)
        {
            query = query.Where(v => v.Id != vacacionIdExcluir.Value);
        }
        
        return await query.AnyAsync();
    }
    
    /// <summary>
    /// Obtiene el total de días tomados por un empleado en un periodo
    /// </summary>
    public async Task<int> GetDiasTomadosEnPeriodoAsync(int empleadoId, int periodo)
    {
        var vacacionesDisfrutadas = await _dbSet
            .Where(v => v.EmpleadoId == empleadoId && 
                        v.PeriodoCorrespondiente == periodo && 
                        v.Activo &&
                        v.Estado == EstadoVacacion.Disfrutada)
            .ToListAsync();
            
        return vacacionesDisfrutadas.Sum(v => v.DiasTomados);
    }
    
    /// <summary>
    /// Obtiene vacaciones programadas por empleado
    /// </summary>
    public async Task<IEnumerable<Vacacion>> GetVacacionesProgramadasAsync(int empleadoId)
    {
        return await _dbSet
            .Where(v => v.EmpleadoId == empleadoId && 
                        v.Activo && 
                        v.Estado == EstadoVacacion.Programada &&
                        v.FechaInicio >= DateTime.Today)
            .Include(v => v.Empleado)
            .OrderBy(v => v.FechaInicio)
            .ToListAsync();
    }
    
    /// <summary>
    /// Obtiene todas las vacaciones programadas (próximas)
    /// </summary>
    public async Task<IEnumerable<Vacacion>> GetAllVacacionesProgramadasAsync()
    {
        return await _dbSet
            .Where(v => v.Activo && 
                        v.Estado == EstadoVacacion.Programada &&
                        v.FechaInicio >= DateTime.Today)
            .Include(v => v.Empleado)
            .OrderBy(v => v.FechaInicio)
            .ToListAsync();
    }
    
    /// <summary>
    /// Obtiene vacaciones con relaciones cargadas
    /// </summary>
    public async Task<Vacacion?> GetByIdWithRelationsAsync(int id)
    {
        return await _dbSet
            .Include(v => v.Empleado)
                .ThenInclude(e => e.Departamento)
            .Include(v => v.Empleado)
                .ThenInclude(e => e.Cargo)
            .FirstOrDefaultAsync(v => v.Id == id);
    }
    
    /// <summary>
    /// Obtiene todas las vacaciones activas con relaciones
    /// </summary>
    public async Task<IEnumerable<Vacacion>> GetAllWithRelationsAsync()
    {
        return await _dbSet
            .Where(v => v.Activo)
            .Include(v => v.Empleado)
                .ThenInclude(e => e.Departamento)
            .OrderByDescending(v => v.FechaInicio)
            .ToListAsync();
    }
}
