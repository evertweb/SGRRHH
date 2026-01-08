namespace SGRRHH.Local.Domain.Entities;

public class TipoPermiso : EntidadBase
{
    public string Nombre { get; set; } = string.Empty;
    
    public string? Descripcion { get; set; }
    
    public string Color { get; set; } = "#1E88E5";
    
    public bool RequiereAprobacion { get; set; } = true;
    
    public bool RequiereDocumento { get; set; } = false;
    
    public int DiasPorDefecto { get; set; } = 1;
    
    public int DiasMaximos { get; set; } = 0;
    
    public bool EsCompensable { get; set; } = false;
    
    public new bool Activo { get; set; } = true;
}


