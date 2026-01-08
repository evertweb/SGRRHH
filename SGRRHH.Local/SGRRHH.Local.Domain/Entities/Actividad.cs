using System.Collections.Generic;

namespace SGRRHH.Local.Domain.Entities;

public class Actividad : EntidadBase
{
    public string Codigo { get; set; } = string.Empty;
    
    public string Nombre { get; set; } = string.Empty;
    
    public string? Descripcion { get; set; }
    
    public string? Categoria { get; set; }
    
    public bool RequiereProyecto { get; set; }
    
    public int Orden { get; set; }
    
    public ICollection<DetalleActividad>? DetallesActividades { get; set; }
}


