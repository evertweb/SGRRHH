using System.Collections.Generic;
using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Domain.Entities;

/// <summary>
/// Catálogo de actividades realizables en proyectos forestales
/// </summary>
public class Actividad : EntidadBase
{
    /// <summary>
    /// Código único de la actividad (ej: SIEM-001)
    /// </summary>
    public string Codigo { get; set; } = string.Empty;
    
    /// <summary>
    /// Nombre de la actividad
    /// </summary>
    public string Nombre { get; set; } = string.Empty;
    
    /// <summary>
    /// Descripción detallada
    /// </summary>
    public string? Descripcion { get; set; }
    
    // ===== Relación con Categoría =====
    
    /// <summary>
    /// ID de la categoría a la que pertenece
    /// </summary>
    public int? CategoriaId { get; set; }
    
    /// <summary>
    /// Categoría de la actividad
    /// </summary>
    public CategoriaActividad? Categoria { get; set; }
    
    /// <summary>
    /// Categoría como texto (legacy, para compatibilidad)
    /// </summary>
    public string? CategoriaTexto { get; set; }
    
    // ===== Unidad de Medida y Rendimientos =====
    
    /// <summary>
    /// Unidad de medida para cuantificar la actividad
    /// </summary>
    public UnidadMedidaActividad UnidadMedida { get; set; } = UnidadMedidaActividad.SoloHoras;
    
    /// <summary>
    /// Abreviatura de la unidad (ej: "ha", "árb", "m³")
    /// </summary>
    public string? UnidadAbreviatura { get; set; }
    
    /// <summary>
    /// Rendimiento esperado por hora/hombre
    /// Ejemplo: 0.5 ha/hora para plateo, 50 árboles/hora para poda
    /// </summary>
    public decimal? RendimientoEsperado { get; set; }
    
    /// <summary>
    /// Rendimiento mínimo aceptable
    /// </summary>
    public decimal? RendimientoMinimo { get; set; }
    
    /// <summary>
    /// Costo estándar por unidad (opcional, para presupuestos)
    /// </summary>
    public decimal? CostoUnitario { get; set; }
    
    // ===== Restricciones =====
    
    /// <summary>
    /// Indica si esta actividad requiere asociarse a un proyecto
    /// </summary>
    public bool RequiereProyecto { get; set; }
    
    /// <summary>
    /// Indica si requiere registrar cantidad (además de horas)
    /// </summary>
    public bool RequiereCantidad { get; set; }
    
    /// <summary>
    /// Tipos de proyecto donde aplica esta actividad (CSV de IDs de enum)
    /// Ej: "1,2,3" para Plantación, Mantenimiento, Raleo
    /// Null = aplica a todos
    /// </summary>
    public string? TiposProyectoAplicables { get; set; }
    
    /// <summary>
    /// Especies donde aplica esta actividad (CSV de IDs)
    /// Null = aplica a todas
    /// </summary>
    public string? EspeciesAplicables { get; set; }
    
    // ===== Orden y visualización =====
    
    /// <summary>
    /// Orden de visualización en listas
    /// </summary>
    public int Orden { get; set; }
    
    /// <summary>
    /// Si es una actividad destacada/frecuente
    /// </summary>
    public bool EsDestacada { get; set; }
    
    // ===== Navegación =====
    
    public ICollection<DetalleActividad>? DetallesActividades { get; set; }
    
    // ===== Métodos Helper =====
    
    /// <summary>
    /// Obtiene los tipos de proyecto como lista de enum
    /// </summary>
    public List<TipoProyectoForestal> GetTiposProyecto()
    {
        if (string.IsNullOrEmpty(TiposProyectoAplicables))
            return new List<TipoProyectoForestal>();
            
        return TiposProyectoAplicables
            .Split(',')
            .Select(s => (TipoProyectoForestal)int.Parse(s.Trim()))
            .ToList();
    }
    
    /// <summary>
    /// Verifica si aplica para un tipo de proyecto específico
    /// </summary>
    public bool AplicaParaTipoProyecto(TipoProyectoForestal? tipo)
    {
        if (string.IsNullOrEmpty(TiposProyectoAplicables))
            return true; // Aplica a todos
            
        if (!tipo.HasValue)
            return true;
            
        return GetTiposProyecto().Contains(tipo.Value);
    }
}


