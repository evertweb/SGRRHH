using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Domain.Entities;

public class DocumentoEmpleado : EntidadBase
{
    public int EmpleadoId { get; set; }
    
    public Empleado Empleado { get; set; } = null!;
    
    public TipoDocumentoEmpleado TipoDocumento { get; set; }
    
    public string Nombre { get; set; } = string.Empty;
    
    public string? Descripcion { get; set; }
    
    public string ArchivoPath { get; set; } = string.Empty;
    
    public string NombreArchivoOriginal { get; set; } = string.Empty;
    
    public long TamanoArchivo { get; set; }
    
    public string TipoMime { get; set; } = string.Empty;
    
    public DateTime? FechaVencimiento { get; set; }
    
    public DateTime? FechaEmision { get; set; }
    
    public bool EstaVigente => !FechaVencimiento.HasValue || FechaVencimiento.Value >= DateTime.Today;
    
    public int? SubidoPorUsuarioId { get; set; }
    
    public string? SubidoPorNombre { get; set; }
}


