namespace SGRRHH.Local.Domain.Entities;

/// <summary>
/// Especie forestal utilizada en los proyectos de silvicultura.
/// Catálogo de especies plantadas en Colombia.
/// </summary>
public class EspecieForestal : EntidadBase
{
    /// <summary>
    /// Código único de la especie (ej: PIN-PAT, EUC-GRA)
    /// </summary>
    public string Codigo { get; set; } = string.Empty;

    /// <summary>
    /// Nombre común de la especie (ej: Pino Pátula)
    /// </summary>
    public string NombreComun { get; set; } = string.Empty;

    /// <summary>
    /// Nombre científico de la especie (ej: Pinus patula)
    /// </summary>
    public string? NombreCientifico { get; set; }

    /// <summary>
    /// Familia botánica (ej: Pinaceae, Myrtaceae)
    /// </summary>
    public string? Familia { get; set; }

    /// <summary>
    /// Turno promedio de cosecha en años
    /// </summary>
    public int? TurnoPromedio { get; set; }

    /// <summary>
    /// Densidad de siembra recomendada (árboles/hectárea)
    /// </summary>
    public int? DensidadRecomendada { get; set; }

    /// <summary>
    /// Observaciones adicionales sobre la especie
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// Descripción completa incluyendo código y nombre científico
    /// </summary>
    public string DescripcionCompleta => string.IsNullOrEmpty(NombreCientifico) 
        ? $"{Codigo} - {NombreComun}" 
        : $"{Codigo} - {NombreComun} ({NombreCientifico})";
}
