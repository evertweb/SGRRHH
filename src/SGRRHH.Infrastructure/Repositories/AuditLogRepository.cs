using Microsoft.EntityFrameworkCore;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;
using SGRRHH.Infrastructure.Data;

namespace SGRRHH.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de logs de auditoría
/// </summary>
public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(AppDbContext context) : base(context)
    {
    }
    
    public async Task<List<AuditLog>> GetFilteredAsync(DateTime? fechaDesde, DateTime? fechaHasta, string? entidad, int? usuarioId, int maxRegistros)
    {
        var query = _dbSet.Include(a => a.Usuario).AsQueryable();
        
        if (fechaDesde.HasValue)
            query = query.Where(a => a.FechaHora >= fechaDesde.Value);
            
        if (fechaHasta.HasValue)
            query = query.Where(a => a.FechaHora <= fechaHasta.Value);
            
        if (!string.IsNullOrWhiteSpace(entidad))
            query = query.Where(a => a.Entidad == entidad);
            
        if (usuarioId.HasValue)
            query = query.Where(a => a.UsuarioId == usuarioId.Value);
        
        return await query
            .OrderByDescending(a => a.FechaHora)
            .Take(maxRegistros)
            .ToListAsync();
    }
    
    public async Task<List<AuditLog>> GetByEntidadAsync(string entidad, int entidadId)
    {
        return await _dbSet
            .Include(a => a.Usuario)
            .Where(a => a.Entidad == entidad && a.EntidadId == entidadId)
            .OrderByDescending(a => a.FechaHora)
            .ToListAsync();
    }
    
    public async Task<int> DeleteOlderThanAsync(DateTime fecha)
    {
        var registrosAntiguos = await _dbSet
            .Where(a => a.FechaHora < fecha)
            .ToListAsync();
            
        _dbSet.RemoveRange(registrosAntiguos);
        return registrosAntiguos.Count;
    }
}
