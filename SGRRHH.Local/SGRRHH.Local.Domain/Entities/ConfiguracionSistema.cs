namespace SGRRHH.Local.Domain.Entities;

public class ConfiguracionSistema : EntidadBase
{
    public string Clave { get; set; } = string.Empty;
    
    public string Valor { get; set; } = string.Empty;
    
    public string? Descripcion { get; set; }
    
    public string Categoria { get; set; } = "General";
}


