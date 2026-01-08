using SGRRHH.Local.Domain.Entities;

namespace SGRRHH.Local.Shared.Interfaces;

public interface IAuditService
{
    Task LogAsync(string accion, string entidad, int? entidadId, string descripcion, object? datos = null);
    Task<IEnumerable<AuditLog>> GetByEntidadAsync(string entidad, int entidadId);
    Task<IEnumerable<AuditLog>> GetRecentAsync(int count = 100);
    Task<IEnumerable<AuditLog>> SearchAsync(AuditSearchOptions options);
}

public class AuditSearchOptions
{
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public int? UsuarioId { get; set; }
    public string? Entidad { get; set; }
    public string? Accion { get; set; }
}
