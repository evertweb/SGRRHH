using SGRRHH.Local.Domain.DTOs;
using SGRRHH.Local.Domain.Entities;

namespace SGRRHH.Local.Domain.Interfaces;

public interface IAuditService
{
    /// <summary>
    /// Registra una acción de auditoría
    /// </summary>
    Task LogAsync(string accion, string entidad, int? entidadId, string descripcion, object? datos = null);
    
    /// <summary>
    /// Obtiene logs de auditoría para una entidad específica
    /// </summary>
    Task<IEnumerable<AuditLog>> GetByEntidadAsync(string entidad, int entidadId);
    
    /// <summary>
    /// Obtiene los logs más recientes
    /// </summary>
    Task<IEnumerable<AuditLog>> GetRecentAsync(int count = 100);
    
    /// <summary>
    /// Busca logs con criterios específicos
    /// </summary>
    Task<IEnumerable<AuditLog>> SearchAsync(AuditSearchOptions options);
}
