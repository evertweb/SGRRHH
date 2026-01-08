namespace SGRRHH.Local.Domain.Entities;

public class AuditLog : EntidadBase
{
    public DateTime FechaHora { get; set; } = DateTime.Now;
    
    public int? UsuarioId { get; set; }
    
    public string UsuarioNombre { get; set; } = string.Empty;
    
    public string Accion { get; set; } = string.Empty;
    
    public string Entidad { get; set; } = string.Empty;
    
    public int? EntidadId { get; set; }
    
    public string Descripcion { get; set; } = string.Empty;
    
    public string? DireccionIp { get; set; }
    
    public string? DatosAdicionales { get; set; }
    
    // Navegacia³n
    public Usuario? Usuario { get; set; }
}


