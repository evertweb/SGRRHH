using SGRRHH.Local.Domain.Interfaces;

namespace SGRRHH.Local.Domain.Entities;

/// <summary>
/// Clase base para todas las entidades del dominio.
/// Implementa IConcurrencyAware para control de concurrencia optimista.
/// </summary>
public abstract class EntidadBase : IConcurrencyAware
{
    public int Id { get; set; }
    
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Token de concurrencia. Al actualizar, verificar que coincida con la DB.
    /// </summary>
    public DateTime? FechaModificacion { get; set; }
    
    public bool Activo { get; set; } = true;
}


