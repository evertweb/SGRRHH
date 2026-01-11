namespace SGRRHH.Local.Domain.DTOs;

// =========================================
// DTOs PARA DASHBOARD DE PRODUCTIVIDAD
// =========================================

/// <summary>
/// KPIs principales del dashboard de productividad
/// </summary>
public class DashboardKPIs
{
    // Proyectos
    public int TotalProyectosActivos { get; set; }
    public decimal TotalHectareasActivas { get; set; }
    public int TotalEmpleadosAsignados { get; set; }
    
    // Periodo actual (mes)
    public decimal HorasTrabajadasMes { get; set; }
    public int JornalesMes { get; set; }
    public decimal CostoManoObraMes { get; set; }
    
    // Productividad
    public decimal RendimientoPromedioGeneral { get; set; } // % vs esperado
    public decimal CostoPromedioHectarea { get; set; }
    public decimal HorasPromedioHectarea { get; set; }
    
    // Tendencias (vs mes anterior)
    public decimal VariacionHoras { get; set; } // %
    public decimal VariacionCosto { get; set; } // %
    public decimal VariacionRendimiento { get; set; } // %
}

/// <summary>
/// Resumen de productividad por proyecto para dashboard
/// </summary>
public class ProyectoProductividadResumen
{
    public int ProyectoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Especie { get; set; }
    public decimal? AreaHectareas { get; set; }
    public decimal HorasTrabajadas { get; set; }
    public decimal CostoManoObra { get; set; }
    public int EmpleadosActivos { get; set; }
    public decimal RendimientoPromedio { get; set; } // % vs esperado
    public decimal? CostoPorHectarea { get; set; }
    public string Estado { get; set; } = string.Empty;
}

// =========================================
// DTOs PARA REPORTE POR PROYECTO
// =========================================

/// <summary>
/// Reporte detallado de un proyecto
/// </summary>
public class ReporteProyectoDetalle
{
    // Información del proyecto
    public int ProyectoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? TipoProyecto { get; set; }
    public string? Especie { get; set; }
    public string? Predio { get; set; }
    public string? Lote { get; set; }
    public decimal? AreaHectareas { get; set; }
    public int? EdadCultivo { get; set; }
    
    // Periodo del reporte
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    
    // Totales
    public decimal TotalHoras { get; set; }
    public int TotalJornales { get; set; }
    public decimal TotalCosto { get; set; }
    public int TotalDiasTrabajados { get; set; }
    
    // Métricas por hectárea
    public decimal? HorasPorHectarea { get; set; }
    public decimal? CostoPorHectarea { get; set; }
    public decimal? JornalesPorHectarea { get; set; }
    
    // Desglose por categoría de actividad
    public List<ActividadCategoriaResumen> PorCategoria { get; set; } = new();
    
    // Desglose por actividad
    public List<ActividadResumen> PorActividad { get; set; } = new();
    
    // Desglose por empleado
    public List<EmpleadoResumen> PorEmpleado { get; set; } = new();
    
    // Distribución temporal
    public List<ProductividadDiaria> PorDia { get; set; } = new();
}

/// <summary>
/// Resumen por categoría de actividad
/// </summary>
public class ActividadCategoriaResumen
{
    public string Categoria { get; set; } = string.Empty;
    public string? ColorHex { get; set; }
    public decimal Horas { get; set; }
    public decimal PorcentajeHoras { get; set; }
    public decimal Costo { get; set; }
    public int CantidadActividades { get; set; }
}

/// <summary>
/// Resumen por actividad específica
/// </summary>
public class ActividadResumen
{
    public int ActividadId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Categoria { get; set; }
    public string? UnidadMedida { get; set; }
    public decimal Horas { get; set; }
    public decimal? Cantidad { get; set; }
    public decimal? RendimientoLogrado { get; set; } // cantidad/hora
    public decimal? RendimientoEsperado { get; set; }
    public decimal? PorcentajeRendimiento { get; set; } // % del esperado
    public string? ClasificacionRendimiento { get; set; } // Excelente, Óptimo, etc.
    public int Registros { get; set; } // Número de veces realizada
}

/// <summary>
/// Resumen por empleado
/// </summary>
public class EmpleadoResumen
{
    public int EmpleadoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Cargo { get; set; }
    public string? Rol { get; set; }
    public decimal Horas { get; set; }
    public int Jornales { get; set; }
    public int DiasTrabajados { get; set; }
    public decimal PromedioHorasDia { get; set; }
    public decimal RendimientoPromedio { get; set; } // % vs esperado
    public decimal Costo { get; set; }
}

/// <summary>
/// Productividad por día
/// </summary>
public class ProductividadDiaria
{
    public DateTime Fecha { get; set; }
    public decimal Horas { get; set; }
    public int Empleados { get; set; }
    public decimal? Cantidad { get; set; }
    public string? ActividadPrincipal { get; set; }
}

// =========================================
// DTOs PARA REPORTE POR EMPLEADO
// =========================================

/// <summary>
/// Reporte de productividad individual de un empleado
/// </summary>
public class ReporteEmpleadoProductividad
{
    public int EmpleadoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Cargo { get; set; }
    public string? Departamento { get; set; }
    
    // Periodo
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    
    // Totales
    public decimal TotalHoras { get; set; }
    public int TotalJornales { get; set; }
    public int DiasTrabajados { get; set; }
    public decimal PromedioHorasDia { get; set; }
    
    // Rendimiento
    public decimal RendimientoPromedioGeneral { get; set; }
    public int ActividadesRealizadas { get; set; }
    public int ProyectosParticipados { get; set; }
    
    // Por proyecto
    public List<EmpleadoEnProyectoResumen> PorProyecto { get; set; } = new();
    
    // Por actividad
    public List<EmpleadoActividadResumen> PorActividad { get; set; } = new();
    
    // Histórico mensual
    public List<EmpleadoMensualResumen> PorMes { get; set; } = new();
}

public class EmpleadoEnProyectoResumen
{
    public int ProyectoId { get; set; }
    public string ProyectoNombre { get; set; } = string.Empty;
    public decimal Horas { get; set; }
    public int Dias { get; set; }
    public decimal Rendimiento { get; set; }
}

public class EmpleadoActividadResumen
{
    public string Actividad { get; set; } = string.Empty;
    public string? Categoria { get; set; }
    public decimal Horas { get; set; }
    public decimal? Cantidad { get; set; }
    public string? Unidad { get; set; }
    public decimal? Rendimiento { get; set; }
    public int Veces { get; set; }
}

public class EmpleadoMensualResumen
{
    public int Anio { get; set; }
    public int Mes { get; set; }
    public string MesNombre { get; set; } = string.Empty;
    public decimal Horas { get; set; }
    public int Jornales { get; set; }
    public decimal Rendimiento { get; set; }
}

// =========================================
// DTOs PARA REPORTE COMPARATIVO
// =========================================

/// <summary>
/// Datos para comparación entre proyectos
/// </summary>
public class ComparativoProyectos
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public List<ProyectoComparativo> Proyectos { get; set; } = new();
}

public class ProyectoComparativo
{
    public int ProyectoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Especie { get; set; }
    public decimal? AreaHectareas { get; set; }
    
    // Métricas para comparar
    public decimal HorasTotales { get; set; }
    public decimal? HorasPorHectarea { get; set; }
    public decimal CostoTotal { get; set; }
    public decimal? CostoPorHectarea { get; set; }
    public decimal RendimientoPromedio { get; set; }
    public int EmpleadosPromedio { get; set; }
    
    // Ranking (posición relativa)
    public int RankingHorasHa { get; set; }
    public int RankingCostoHa { get; set; }
    public int RankingRendimiento { get; set; }
}

// =========================================
// FILTROS PARA REPORTES
// =========================================

/// <summary>
/// Filtros comunes para reportes
/// </summary>
public class FiltrosReporte
{
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public int? ProyectoId { get; set; }
    public int? EmpleadoId { get; set; }
    public int? CategoriaActividadId { get; set; }
    public int? ActividadId { get; set; }
    public int? EspecieId { get; set; }
    public string? Departamento { get; set; }
    public string? Municipio { get; set; }
    public string? Predio { get; set; }
    public List<int>? ProyectoIds { get; set; }
    public List<int>? EmpleadoIds { get; set; }
}
