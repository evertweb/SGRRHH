namespace SGRRHH.Core.Entities;

/// <summary>
/// Entidad para almacenar configuraciones del sistema como pares clave-valor
/// </summary>
public class ConfiguracionSistema : EntidadBase
{
    /// <summary>
    /// Clave única de la configuración
    /// </summary>
    public string Clave { get; set; } = string.Empty;
    
    /// <summary>
    /// Valor de la configuración (almacenado como string)
    /// </summary>
    public string Valor { get; set; } = string.Empty;
    
    /// <summary>
    /// Descripción de para qué sirve esta configuración
    /// </summary>
    public string? Descripcion { get; set; }
    
    /// <summary>
    /// Categoría de la configuración (Empresa, Sistema, Backup, etc.)
    /// </summary>
    public string Categoria { get; set; } = "General";
}
