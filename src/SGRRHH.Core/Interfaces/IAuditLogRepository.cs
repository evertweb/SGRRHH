using SGRRHH.Core.Entities;

namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Interfaz para el repositorio de logs de auditoría
/// </summary>
public interface IAuditLogRepository : IRepository<AuditLog>
{
    /// <summary>
    /// Obtiene registros de auditoría filtrados
    /// </summary>
    Task<List<AuditLog>> GetFilteredAsync(DateTime? fechaDesde, DateTime? fechaHasta, string? entidad, int? usuarioId, int maxRegistros);
    
    /// <summary>
    /// Obtiene registros de auditoría de una entidad específica
    /// </summary>
    Task<List<AuditLog>> GetByEntidadAsync(string entidad, int entidadId);
    
    /// <summary>
    /// Elimina registros anteriores a una fecha
    /// </summary>
    Task<int> DeleteOlderThanAsync(DateTime fecha);
}
