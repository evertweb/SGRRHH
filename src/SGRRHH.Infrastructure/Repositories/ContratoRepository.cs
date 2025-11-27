using Microsoft.EntityFrameworkCore;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;
using SGRRHH.Infrastructure.Data;

namespace SGRRHH.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de contratos
/// </summary>
public class ContratoRepository : Repository<Contrato>, IContratoRepository
{
    private readonly AppDbContext _appContext;
    
    public ContratoRepository(AppDbContext context) : base(context)
    {
        _appContext = context;
    }
    
    public async Task<IEnumerable<Contrato>> GetByEmpleadoIdAsync(int empleadoId)
    {
        return await _dbSet
            .Where(c => c.EmpleadoId == empleadoId && c.Activo)
            .Include(c => c.Empleado)
            .Include(c => c.Cargo)
                .ThenInclude(ca => ca.Departamento)
            .OrderByDescending(c => c.FechaInicio)
            .ToListAsync();
    }
    
    public async Task<Contrato?> GetContratoActivoByEmpleadoIdAsync(int empleadoId)
    {
        return await _dbSet
            .Where(c => c.EmpleadoId == empleadoId && 
                        c.Activo && 
                        c.Estado == EstadoContrato.Activo)
            .Include(c => c.Empleado)
            .Include(c => c.Cargo)
                .ThenInclude(ca => ca.Departamento)
            .FirstOrDefaultAsync();
    }
    
    public async Task<IEnumerable<Contrato>> GetContratosProximosAVencerAsync(int diasAnticipacion)
    {
        var fechaLimite = DateTime.Today.AddDays(diasAnticipacion);
        
        return await _dbSet
            .Where(c => c.Activo && 
                        c.Estado == EstadoContrato.Activo &&
                        c.FechaFin.HasValue &&
                        c.FechaFin.Value <= fechaLimite &&
                        c.FechaFin.Value >= DateTime.Today)
            .Include(c => c.Empleado)
            .Include(c => c.Cargo)
            .OrderBy(c => c.FechaFin)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Contrato>> GetContratosPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        return await _dbSet
            .Where(c => c.Activo &&
                ((c.FechaInicio >= fechaInicio && c.FechaInicio <= fechaFin) ||
                 (c.FechaFin.HasValue && c.FechaFin.Value >= fechaInicio && c.FechaFin.Value <= fechaFin)))
            .Include(c => c.Empleado)
            .Include(c => c.Cargo)
            .OrderByDescending(c => c.FechaInicio)
            .ToListAsync();
    }
    
    /// <summary>
    /// Obtiene contrato con todas las relaciones cargadas
    /// </summary>
    public async Task<Contrato?> GetByIdWithRelationsAsync(int id)
    {
        return await _dbSet
            .Include(c => c.Empleado)
                .ThenInclude(e => e.Departamento)
            .Include(c => c.Cargo)
                .ThenInclude(ca => ca.Departamento)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
    
    /// <summary>
    /// Obtiene todos los contratos activos con relaciones
    /// </summary>
    public async Task<IEnumerable<Contrato>> GetAllWithRelationsAsync()
    {
        return await _dbSet
            .Where(c => c.Activo)
            .Include(c => c.Empleado)
                .ThenInclude(e => e.Departamento)
            .Include(c => c.Cargo)
            .OrderByDescending(c => c.FechaInicio)
            .ToListAsync();
    }
    
    /// <summary>
    /// Obtiene contratos por tipo
    /// </summary>
    public async Task<IEnumerable<Contrato>> GetByTipoContratoAsync(TipoContrato tipo)
    {
        return await _dbSet
            .Where(c => c.Activo && c.TipoContrato == tipo)
            .Include(c => c.Empleado)
            .Include(c => c.Cargo)
            .OrderByDescending(c => c.FechaInicio)
            .ToListAsync();
    }
    
    /// <summary>
    /// Obtiene contratos por estado
    /// </summary>
    public async Task<IEnumerable<Contrato>> GetByEstadoAsync(EstadoContrato estado)
    {
        return await _dbSet
            .Where(c => c.Activo && c.Estado == estado)
            .Include(c => c.Empleado)
            .Include(c => c.Cargo)
            .OrderByDescending(c => c.FechaInicio)
            .ToListAsync();
    }
    
    /// <summary>
    /// Cuenta contratos activos por vencer en los próximos N días
    /// </summary>
    public async Task<int> CountContratosProximosAVencerAsync(int diasAnticipacion)
    {
        var fechaLimite = DateTime.Today.AddDays(diasAnticipacion);
        
        return await _dbSet
            .CountAsync(c => c.Activo && 
                        c.Estado == EstadoContrato.Activo &&
                        c.FechaFin.HasValue &&
                        c.FechaFin.Value <= fechaLimite &&
                        c.FechaFin.Value >= DateTime.Today);
    }
}
