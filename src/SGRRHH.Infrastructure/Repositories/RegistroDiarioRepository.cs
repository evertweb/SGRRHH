using Microsoft.EntityFrameworkCore;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;
using SGRRHH.Infrastructure.Data;

namespace SGRRHH.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de registros diarios
/// </summary>
public class RegistroDiarioRepository : Repository<RegistroDiario>, IRegistroDiarioRepository
{
    private readonly AppDbContext _appContext;
    
    public RegistroDiarioRepository(AppDbContext context) : base(context)
    {
        _appContext = context;
    }
    
    public async Task<RegistroDiario?> GetByFechaEmpleadoAsync(DateTime fecha, int empleadoId)
    {
        return await _dbSet
            .Include(r => r.Empleado)
            .Include(r => r.DetallesActividades!)
                .ThenInclude(d => d.Actividad)
            .Include(r => r.DetallesActividades!)
                .ThenInclude(d => d.Proyecto)
            .FirstOrDefaultAsync(r => r.Fecha.Date == fecha.Date && r.EmpleadoId == empleadoId);
    }
    
    public async Task<RegistroDiario?> GetByIdWithDetallesAsync(int id)
    {
        return await _dbSet
            .Include(r => r.Empleado)
            .Include(r => r.DetallesActividades!)
                .ThenInclude(d => d.Actividad)
            .Include(r => r.DetallesActividades!)
                .ThenInclude(d => d.Proyecto)
            .FirstOrDefaultAsync(r => r.Id == id);
    }
    
    public async Task<IEnumerable<RegistroDiario>> GetByEmpleadoRangoFechasAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin)
    {
        return await _dbSet
            .Include(r => r.DetallesActividades!)
                .ThenInclude(d => d.Actividad)
            .Include(r => r.DetallesActividades!)
                .ThenInclude(d => d.Proyecto)
            .Where(r => r.EmpleadoId == empleadoId && 
                        r.Fecha.Date >= fechaInicio.Date && 
                        r.Fecha.Date <= fechaFin.Date)
            .OrderByDescending(r => r.Fecha)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<RegistroDiario>> GetByFechaAsync(DateTime fecha)
    {
        return await _dbSet
            .Include(r => r.Empleado)
            .Include(r => r.DetallesActividades!)
                .ThenInclude(d => d.Actividad)
            .Where(r => r.Fecha.Date == fecha.Date)
            .OrderBy(r => r.Empleado!.Apellidos)
            .ThenBy(r => r.Empleado!.Nombres)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<RegistroDiario>> GetByEmpleadoWithDetallesAsync(int empleadoId, int? cantidad = null)
    {
        var query = _dbSet
            .Include(r => r.DetallesActividades!)
                .ThenInclude(d => d.Actividad)
            .Include(r => r.DetallesActividades!)
                .ThenInclude(d => d.Proyecto)
            .Where(r => r.EmpleadoId == empleadoId)
            .OrderByDescending(r => r.Fecha);
            
        if (cantidad.HasValue)
        {
            return await query.Take(cantidad.Value).ToListAsync();
        }
        
        return await query.ToListAsync();
    }
    
    public async Task<IEnumerable<RegistroDiario>> GetByEmpleadoMesActualAsync(int empleadoId)
    {
        var inicioMes = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var finMes = inicioMes.AddMonths(1).AddDays(-1);
        
        return await GetByEmpleadoRangoFechasAsync(empleadoId, inicioMes, finMes);
    }
    
    public async Task<bool> ExistsByFechaEmpleadoAsync(DateTime fecha, int empleadoId)
    {
        return await _dbSet
            .AnyAsync(r => r.Fecha.Date == fecha.Date && r.EmpleadoId == empleadoId);
    }
    
    public async Task<decimal> GetTotalHorasByEmpleadoRangoAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin)
    {
        var registros = await _dbSet
            .Include(r => r.DetallesActividades)
            .Where(r => r.EmpleadoId == empleadoId && 
                        r.Fecha.Date >= fechaInicio.Date && 
                        r.Fecha.Date <= fechaFin.Date)
            .ToListAsync();
            
        return registros.Sum(r => r.TotalHoras);
    }
}
