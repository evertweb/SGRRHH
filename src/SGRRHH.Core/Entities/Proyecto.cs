using System.Collections.Generic;

namespace SGRRHH.Core.Entities;

/// <summary>
/// Entidad que representa un proyecto o labor de la empresa
/// </summary>
public class Proyecto : EntidadBase
{
    /// <summary>
    /// Código único del proyecto
    /// </summary>
    public string Codigo { get; set; } = string.Empty;
    
    /// <summary>
    /// Nombre del proyecto
    /// </summary>
    public string Nombre { get; set; } = string.Empty;
    
    /// <summary>
    /// Descripción detallada del proyecto
    /// </summary>
    public string? Descripcion { get; set; }
    
    /// <summary>
    /// Cliente o entidad para la cual se realiza el proyecto
    /// </summary>
    public string? Cliente { get; set; }
    
    /// <summary>
    /// Fecha de inicio del proyecto
    /// </summary>
    public DateTime? FechaInicio { get; set; }
    
    /// <summary>
    /// Fecha estimada o real de finalización
    /// </summary>
    public DateTime? FechaFin { get; set; }
    
    /// <summary>
    /// Estado actual del proyecto
    /// </summary>
    public EstadoProyecto Estado { get; set; } = EstadoProyecto.Activo;
    
    /// <summary>
    /// Registros diarios asociados al proyecto
    /// </summary>
    public ICollection<DetalleActividad>? DetallesActividades { get; set; }
}

/// <summary>
/// Estados posibles de un proyecto
/// </summary>
public enum EstadoProyecto
{
    Activo = 0,
    Suspendido = 1,
    Finalizado = 2,
    Cancelado = 3
}
