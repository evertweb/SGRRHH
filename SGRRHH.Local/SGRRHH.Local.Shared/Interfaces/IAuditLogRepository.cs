using SGRRHH.Local.Domain.Entities;

namespace SGRRHH.Local.Shared.Interfaces;

public interface IAuditLogRepository : IRepository<AuditLog>
{
    Task<List<AuditLog>> GetFilteredAsync(DateTime? fechaDesde, DateTime? fechaHasta, string? entidad, int? usuarioId, int maxRegistros);
    
    Task<List<AuditLog>> GetByEntidadAsync(string entidad, int entidadId);
    
    Task<int> DeleteOlderThanAsync(DateTime fecha);
}


