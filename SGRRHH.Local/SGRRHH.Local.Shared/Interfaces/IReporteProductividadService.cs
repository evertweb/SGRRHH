using SGRRHH.Local.Domain.DTOs;

namespace SGRRHH.Local.Shared.Interfaces;

/// <summary>
/// Servicio para reportes de productividad silvicultural
/// </summary>
public interface IReporteProductividadService
{
    // =========================================
    // DASHBOARD
    // =========================================
    
    /// <summary>
    /// Obtiene los KPIs principales del dashboard
    /// </summary>
    Task<DashboardKPIs> GetDashboardKPIsAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null);
    
    /// <summary>
    /// Obtiene resumen de proyectos ordenados por actividad
    /// </summary>
    Task<IEnumerable<ProyectoProductividadResumen>> GetProyectosResumenAsync(int top = 10);
    
    // =========================================
    // REPORTE POR PROYECTO
    // =========================================
    
    /// <summary>
    /// Obtiene reporte detallado de un proyecto
    /// </summary>
    Task<ReporteProyectoDetalle> GetReporteProyectoAsync(int proyectoId, DateTime fechaInicio, DateTime fechaFin);
    
    /// <summary>
    /// Obtiene actividades de un proyecto con sus rendimientos
    /// </summary>
    Task<IEnumerable<ActividadResumen>> GetActividadesByProyectoAsync(int proyectoId, DateTime fechaInicio, DateTime fechaFin);
    
    /// <summary>
    /// Obtiene empleados de un proyecto con sus métricas
    /// </summary>
    Task<IEnumerable<EmpleadoResumen>> GetEmpleadosByProyectoAsync(int proyectoId, DateTime fechaInicio, DateTime fechaFin);
    
    // =========================================
    // REPORTE POR EMPLEADO
    // =========================================
    
    /// <summary>
    /// Obtiene reporte de productividad de un empleado
    /// </summary>
    Task<ReporteEmpleadoProductividad> GetReporteEmpleadoAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin);
    
    /// <summary>
    /// Obtiene histórico mensual de productividad de un empleado
    /// </summary>
    Task<IEnumerable<EmpleadoMensualResumen>> GetHistoricoEmpleadoAsync(int empleadoId, int meses = 12);
    
    // =========================================
    // REPORTE POR ACTIVIDAD
    // =========================================
    
    /// <summary>
    /// Obtiene rendimientos agrupados por actividad
    /// </summary>
    Task<IEnumerable<ActividadResumen>> GetRendimientosByActividadAsync(FiltrosReporte filtros);
    
    // =========================================
    // REPORTE COMPARATIVO
    // =========================================
    
    /// <summary>
    /// Compara métricas entre proyectos
    /// </summary>
    Task<ComparativoProyectos> GetComparativoProyectosAsync(List<int> proyectoIds, DateTime fechaInicio, DateTime fechaFin);
    
    // =========================================
    // EXPORTACIÓN
    // =========================================
    
    /// <summary>
    /// Exporta reporte a PDF
    /// </summary>
    Task<byte[]> ExportarPdfAsync(string tipoReporte, FiltrosReporte filtros);
    
    /// <summary>
    /// Exporta reporte a Excel
    /// </summary>
    Task<byte[]> ExportarExcelAsync(string tipoReporte, FiltrosReporte filtros);
    
    // =========================================
    // ACTUALIZACIÓN DE MÉTRICAS
    // =========================================
    
    /// <summary>
    /// Actualiza métricas acumuladas de un proyecto
    /// </summary>
    Task ActualizarMetricasProyectoAsync(int proyectoId);
    
    /// <summary>
    /// Actualiza métricas de un empleado en un proyecto
    /// </summary>
    Task ActualizarMetricasEmpleadoProyectoAsync(int proyectoId, int empleadoId);
    
    /// <summary>
    /// Actualiza todas las métricas (job batch)
    /// </summary>
    Task ActualizarTodasLasMetricasAsync();
}
