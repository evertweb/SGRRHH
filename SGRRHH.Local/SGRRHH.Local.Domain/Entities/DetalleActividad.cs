namespace SGRRHH.Local.Domain.Entities;

/// <summary>
/// Detalle de una actividad realizada en un registro diario
/// </summary>
public class DetalleActividad : EntidadBase
{
    public int RegistroDiarioId { get; set; }
    public RegistroDiario? RegistroDiario { get; set; }
    
    public int ActividadId { get; set; }
    public Actividad? Actividad { get; set; }
    
    public int? ProyectoId { get; set; }
    public Proyecto? Proyecto { get; set; }
    
    /// <summary>
    /// Horas dedicadas a esta actividad
    /// </summary>
    public decimal Horas { get; set; }
    
    // ===== NUEVOS CAMPOS =====
    
    /// <summary>
    /// Cantidad realizada en la unidad de medida de la actividad
    /// Ejemplo: 2.5 hectáreas, 150 árboles, etc.
    /// </summary>
    public decimal? Cantidad { get; set; }
    
    /// <summary>
    /// Unidad de medida usada (copia de la actividad al momento del registro)
    /// </summary>
    public string? UnidadMedida { get; set; }
    
    /// <summary>
    /// Rendimiento logrado (Cantidad / Horas)
    /// </summary>
    public decimal? RendimientoLogrado => (Horas > 0 && Cantidad.HasValue) 
        ? Math.Round(Cantidad.Value / Horas, 2) 
        : null;
    
    /// <summary>
    /// Porcentaje del rendimiento esperado logrado
    /// </summary>
    public decimal? PorcentajeRendimiento => (Actividad?.RendimientoEsperado > 0 && RendimientoLogrado.HasValue)
        ? Math.Round((RendimientoLogrado.Value / Actividad.RendimientoEsperado.Value) * 100, 1)
        : null;
    
    /// <summary>
    /// Lote o sector específico donde se realizó (opcional)
    /// </summary>
    public string? LoteEspecifico { get; set; }
    
    // ===== Campos existentes =====
    
    public string? Descripcion { get; set; }
    
    public TimeSpan? HoraInicio { get; set; }
    
    public TimeSpan? HoraFin { get; set; }
    
    public int Orden { get; set; }
    
    // ===== Estado de productividad =====
    
    /// <summary>
    /// Clasificación de productividad basada en rendimiento
    /// </summary>
    public string ClasificacionProductividad
    {
        get
        {
            if (!PorcentajeRendimiento.HasValue)
                return "N/A";
            
            return PorcentajeRendimiento.Value switch
            {
                >= 120 => "EXCELENTE",
                >= 100 => "ÓPTIMO",
                >= 80 => "ACEPTABLE",
                >= 60 => "BAJO",
                _ => "CRÍTICO"
            };
        }
    }
}


