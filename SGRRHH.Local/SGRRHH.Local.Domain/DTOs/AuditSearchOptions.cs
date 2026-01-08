namespace SGRRHH.Local.Domain.DTOs;

public class AuditSearchOptions
{
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public int? UsuarioId { get; set; }
    public string? Entidad { get; set; }
    public string? Accion { get; set; }
}
