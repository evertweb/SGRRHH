using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.DTOs;
using SGRRHH.Local.Infrastructure.Data;
using SGRRHH.Local.Shared.Interfaces;
using System.Data;
using System.Globalization;

namespace SGRRHH.Local.Infrastructure.Services;

/// <summary>
/// Servicio de reportes de productividad silvicultural
/// </summary>
public class ReporteProductividadService : IReporteProductividadService
{
    private readonly DapperContext _context;
    private readonly ILogger<ReporteProductividadService> _logger;

    public ReporteProductividadService(DapperContext context, ILogger<ReporteProductividadService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // =========================================
    // DASHBOARD
    // =========================================

    public async Task<DashboardKPIs> GetDashboardKPIsAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null)
    {
        using var connection = _context.CreateConnection();
        
        var inicio = fechaInicio ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var fin = fechaFin ?? DateTime.Today;
        
        // Mes anterior para comparación
        var inicioAnterior = inicio.AddMonths(-1);
        var finAnterior = new DateTime(inicioAnterior.Year, inicioAnterior.Month, 
            DateTime.DaysInMonth(inicioAnterior.Year, inicioAnterior.Month));
        if (finAnterior > fin.AddMonths(-1)) finAnterior = fin.AddMonths(-1);

        var kpis = new DashboardKPIs();

        try
        {
            // Proyectos activos y área
            var proyectosSql = @"
                SELECT 
                    COUNT(*) as Total,
                    COALESCE(SUM(AreaHectareas), 0) as TotalArea
                FROM Proyectos 
                WHERE Estado = 0 AND Activo = 1";
            
            var proyectosData = await connection.QuerySingleOrDefaultAsync<dynamic>(proyectosSql);
            if (proyectosData != null)
            {
                kpis.TotalProyectosActivos = (int)(proyectosData.Total ?? 0);
                kpis.TotalHectareasActivas = (decimal)(proyectosData.TotalArea ?? 0m);
            }

            // Empleados asignados activos
            var empleadosSql = @"
                SELECT COUNT(DISTINCT EmpleadoId) 
                FROM ProyectosEmpleados 
                WHERE FechaDesasignacion IS NULL";
            kpis.TotalEmpleadosAsignados = await connection.ExecuteScalarAsync<int>(empleadosSql);

            // Métricas del periodo actual
            var periodoSql = @"
                SELECT 
                    COALESCE(SUM(da.Horas), 0) as TotalHoras,
                    COUNT(DISTINCT rd.Id) as TotalJornales
                FROM DetallesActividades da
                INNER JOIN RegistrosDiarios rd ON da.RegistroDiarioId = rd.Id
                WHERE rd.Fecha BETWEEN @Inicio AND @Fin";

            var periodoData = await connection.QuerySingleOrDefaultAsync<dynamic>(periodoSql, 
                new { Inicio = inicio.ToString("yyyy-MM-dd"), Fin = fin.ToString("yyyy-MM-dd") });
            if (periodoData != null)
            {
                kpis.HorasTrabajadasMes = (decimal)(periodoData.TotalHoras ?? 0m);
                kpis.JornalesMes = (int)(periodoData.TotalJornales ?? 0);
            }

            // Calcular costo de mano de obra aproximado
            var costoSql = @"
                SELECT COALESCE(SUM(sub.CostoDia), 0)
                FROM (
                    SELECT DISTINCT 
                        rd.EmpleadoId, 
                        rd.Fecha,
                        COALESCE(e.SalarioBase / 30.0, 0) as CostoDia
                    FROM RegistrosDiarios rd
                    INNER JOIN Empleados e ON rd.EmpleadoId = e.Id
                    INNER JOIN DetallesActividades da ON rd.Id = da.RegistroDiarioId
                    WHERE rd.Fecha BETWEEN @Inicio AND @Fin
                    AND e.SalarioBase IS NOT NULL
                ) sub";
            kpis.CostoManoObraMes = await connection.ExecuteScalarAsync<decimal>(costoSql, 
                new { Inicio = inicio.ToString("yyyy-MM-dd"), Fin = fin.ToString("yyyy-MM-dd") });

            // Rendimiento promedio (% del rendimiento esperado)
            var rendimientoSql = @"
                SELECT AVG(rendimiento)
                FROM (
                    SELECT 
                        CASE 
                            WHEN a.RendimientoEsperado > 0 AND da.Cantidad IS NOT NULL AND da.Horas > 0 
                            THEN ((da.Cantidad / da.Horas) / a.RendimientoEsperado) * 100
                            ELSE NULL 
                        END as rendimiento
                    FROM DetallesActividades da
                    INNER JOIN Actividades a ON da.ActividadId = a.Id
                    INNER JOIN RegistrosDiarios rd ON da.RegistroDiarioId = rd.Id
                    WHERE rd.Fecha BETWEEN @Inicio AND @Fin
                    AND da.Cantidad IS NOT NULL
                    AND a.RendimientoEsperado > 0
                ) sub
                WHERE rendimiento IS NOT NULL";
            
            var rendimiento = await connection.ExecuteScalarAsync<decimal?>(rendimientoSql, 
                new { Inicio = inicio.ToString("yyyy-MM-dd"), Fin = fin.ToString("yyyy-MM-dd") });
            kpis.RendimientoPromedioGeneral = Math.Round(rendimiento ?? 0, 1);

            // Promedios por hectárea
            if (kpis.TotalHectareasActivas > 0)
            {
                kpis.HorasPromedioHectarea = Math.Round(kpis.HorasTrabajadasMes / kpis.TotalHectareasActivas, 2);
                kpis.CostoPromedioHectarea = Math.Round(kpis.CostoManoObraMes / kpis.TotalHectareasActivas, 0);
            }

            // Variaciones vs mes anterior
            var periodoAnteriorData = await connection.QuerySingleOrDefaultAsync<dynamic>(periodoSql, 
                new { Inicio = inicioAnterior.ToString("yyyy-MM-dd"), Fin = finAnterior.ToString("yyyy-MM-dd") });
            
            if (periodoAnteriorData != null)
            {
                var horasAnterior = (decimal)(periodoAnteriorData.TotalHoras ?? 0m);
                if (horasAnterior > 0)
                {
                    kpis.VariacionHoras = Math.Round(((kpis.HorasTrabajadasMes - horasAnterior) / horasAnterior) * 100, 1);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener KPIs del dashboard");
        }

        return kpis;
    }

    public async Task<IEnumerable<ProyectoProductividadResumen>> GetProyectosResumenAsync(int top = 10)
    {
        using var connection = _context.CreateConnection();

        var sql = @"
            SELECT 
                p.Id as ProyectoId,
                p.Codigo,
                p.Nombre,
                ef.NombreComun as Especie,
                p.AreaHectareas,
                COALESCE(p.TotalHorasTrabajadas, 0) as HorasTrabajadas,
                COALESCE(p.CostoManoObraAcumulado, 0) as CostoManoObra,
                (SELECT COUNT(DISTINCT pe.EmpleadoId) 
                 FROM ProyectosEmpleados pe 
                 WHERE pe.ProyectoId = p.Id AND pe.FechaDesasignacion IS NULL) as EmpleadosActivos,
                CASE p.Estado 
                     WHEN 0 THEN 'Activo' 
                     WHEN 1 THEN 'Suspendido'
                     WHEN 2 THEN 'Finalizado'
                     ELSE 'Cancelado' END as Estado,
                CASE WHEN p.AreaHectareas > 0 
                     THEN ROUND(COALESCE(p.CostoManoObraAcumulado, 0) / p.AreaHectareas, 0) 
                     ELSE NULL END as CostoPorHectarea,
                0 as RendimientoPromedio
            FROM Proyectos p
            LEFT JOIN EspeciesForestales ef ON p.EspecieId = ef.Id
            WHERE p.Activo = 1
            ORDER BY COALESCE(p.TotalHorasTrabajadas, 0) DESC
            LIMIT @Top";

        try
        {
            return await connection.QueryAsync<ProyectoProductividadResumen>(sql, new { Top = top });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener resumen de proyectos");
            return Enumerable.Empty<ProyectoProductividadResumen>();
        }
    }

    // =========================================
    // REPORTE POR PROYECTO
    // =========================================

    public async Task<ReporteProyectoDetalle> GetReporteProyectoAsync(int proyectoId, DateTime fechaInicio, DateTime fechaFin)
    {
        using var connection = _context.CreateConnection();

        // Información del proyecto
        var proyectoSql = @"
            SELECT 
                p.Id as ProyectoId, 
                p.Codigo, 
                p.Nombre,
                CASE p.TipoProyecto 
                    WHEN 1 THEN 'Plantación Nueva'
                    WHEN 2 THEN 'Mantenimiento'
                    WHEN 3 THEN 'Raleo'
                    WHEN 4 THEN 'Cosecha'
                    WHEN 5 THEN 'Aprovechamiento'
                    ELSE 'Otro' END as TipoProyecto,
                ef.NombreComun as Especie,
                p.Predio, 
                p.Lote, 
                p.AreaHectareas
            FROM Proyectos p
            LEFT JOIN EspeciesForestales ef ON p.EspecieId = ef.Id
            WHERE p.Id = @ProyectoId";

        var reporte = await connection.QuerySingleOrDefaultAsync<ReporteProyectoDetalle>(proyectoSql, 
            new { ProyectoId = proyectoId });
        
        if (reporte == null) 
            return new ReporteProyectoDetalle();

        reporte.FechaInicio = fechaInicio;
        reporte.FechaFin = fechaFin;

        var fechaInicioStr = fechaInicio.ToString("yyyy-MM-dd");
        var fechaFinStr = fechaFin.ToString("yyyy-MM-dd");

        // Totales del periodo
        var totalesSql = @"
            SELECT 
                COALESCE(SUM(da.Horas), 0) as TotalHoras,
                COUNT(DISTINCT rd.Fecha) as TotalDiasTrabajados
            FROM DetallesActividades da
            INNER JOIN RegistrosDiarios rd ON da.RegistroDiarioId = rd.Id
            WHERE da.ProyectoId = @ProyectoId
            AND rd.Fecha BETWEEN @FechaInicio AND @FechaFin";

        var totales = await connection.QuerySingleOrDefaultAsync<dynamic>(totalesSql, 
            new { ProyectoId = proyectoId, FechaInicio = fechaInicioStr, FechaFin = fechaFinStr });
        
        if (totales != null)
        {
            reporte.TotalHoras = (decimal)(totales.TotalHoras ?? 0m);
            reporte.TotalDiasTrabajados = (int)(totales.TotalDiasTrabajados ?? 0);
            reporte.TotalJornales = (int)Math.Ceiling(reporte.TotalHoras / 8);
        }

        // Métricas por hectárea
        if (reporte.AreaHectareas > 0)
        {
            reporte.HorasPorHectarea = Math.Round(reporte.TotalHoras / reporte.AreaHectareas.Value, 2);
            reporte.JornalesPorHectarea = Math.Round((decimal)reporte.TotalJornales / reporte.AreaHectareas.Value, 2);
        }

        // Por categoría
        reporte.PorCategoria = (await GetActividadesPorCategoriaAsync(connection, proyectoId, fechaInicioStr, fechaFinStr)).ToList();

        // Por actividad
        reporte.PorActividad = (await GetActividadesDetalleAsync(connection, proyectoId, fechaInicioStr, fechaFinStr)).ToList();

        // Por empleado
        reporte.PorEmpleado = (await GetEmpleadosDetalleAsync(connection, proyectoId, fechaInicioStr, fechaFinStr)).ToList();

        // Por día
        reporte.PorDia = (await GetProductividadDiariaAsync(connection, proyectoId, fechaInicioStr, fechaFinStr)).ToList();

        return reporte;
    }

    private async Task<IEnumerable<ActividadCategoriaResumen>> GetActividadesPorCategoriaAsync(
        IDbConnection connection, int proyectoId, string fechaInicio, string fechaFin)
    {
        var sql = @"
            SELECT 
                COALESCE(ca.Nombre, a.CategoriaTexto, 'Sin Categoría') as Categoria,
                ca.ColorHex,
                SUM(da.Horas) as Horas,
                COUNT(DISTINCT da.ActividadId) as CantidadActividades
            FROM DetallesActividades da
            INNER JOIN Actividades a ON da.ActividadId = a.Id
            LEFT JOIN CategoriasActividades ca ON a.CategoriaId = ca.Id
            INNER JOIN RegistrosDiarios rd ON da.RegistroDiarioId = rd.Id
            WHERE da.ProyectoId = @ProyectoId
            AND rd.Fecha BETWEEN @FechaInicio AND @FechaFin
            GROUP BY COALESCE(ca.Nombre, a.CategoriaTexto, 'Sin Categoría'), ca.ColorHex
            ORDER BY Horas DESC";

        var categorias = (await connection.QueryAsync<ActividadCategoriaResumen>(sql, 
            new { ProyectoId = proyectoId, FechaInicio = fechaInicio, FechaFin = fechaFin })).ToList();

        // Calcular porcentajes
        var totalHoras = categorias.Sum(c => c.Horas);
        foreach (var cat in categorias)
        {
            cat.PorcentajeHoras = totalHoras > 0 ? Math.Round((cat.Horas / totalHoras) * 100, 1) : 0;
        }

        return categorias;
    }

    private async Task<IEnumerable<ActividadResumen>> GetActividadesDetalleAsync(
        IDbConnection connection, int proyectoId, string fechaInicio, string fechaFin)
    {
        var sql = @"
            SELECT 
                a.Id as ActividadId,
                a.Codigo,
                a.Nombre,
                COALESCE(ca.Nombre, a.CategoriaTexto) as Categoria,
                a.UnidadAbreviatura as UnidadMedida,
                SUM(da.Horas) as Horas,
                SUM(da.Cantidad) as Cantidad,
                a.RendimientoEsperado,
                COUNT(*) as Registros
            FROM DetallesActividades da
            INNER JOIN Actividades a ON da.ActividadId = a.Id
            LEFT JOIN CategoriasActividades ca ON a.CategoriaId = ca.Id
            INNER JOIN RegistrosDiarios rd ON da.RegistroDiarioId = rd.Id
            WHERE da.ProyectoId = @ProyectoId
            AND rd.Fecha BETWEEN @FechaInicio AND @FechaFin
            GROUP BY a.Id, a.Codigo, a.Nombre, ca.Nombre, a.CategoriaTexto, a.UnidadAbreviatura, a.RendimientoEsperado
            ORDER BY Horas DESC";

        var actividades = (await connection.QueryAsync<ActividadResumen>(sql, 
            new { ProyectoId = proyectoId, FechaInicio = fechaInicio, FechaFin = fechaFin })).ToList();

        // Calcular rendimiento y clasificación
        foreach (var act in actividades)
        {
            if (act.Horas > 0 && act.Cantidad.HasValue)
            {
                act.RendimientoLogrado = Math.Round(act.Cantidad.Value / act.Horas, 2);
            }
            
            if (act.RendimientoEsperado > 0 && act.RendimientoLogrado.HasValue)
            {
                act.PorcentajeRendimiento = Math.Round((act.RendimientoLogrado.Value / act.RendimientoEsperado.Value) * 100, 1);
                act.ClasificacionRendimiento = act.PorcentajeRendimiento switch
                {
                    >= 120 => "EXCELENTE",
                    >= 100 => "ÓPTIMO",
                    >= 80 => "ACEPTABLE",
                    >= 60 => "BAJO",
                    _ => "CRÍTICO"
                };
            }
        }

        return actividades;
    }

    private async Task<IEnumerable<EmpleadoResumen>> GetEmpleadosDetalleAsync(
        IDbConnection connection, int proyectoId, string fechaInicio, string fechaFin)
    {
        var sql = @"
            SELECT 
                e.Id as EmpleadoId,
                e.Codigo,
                e.Nombres || ' ' || e.Apellidos as Nombre,
                c.Nombre as Cargo,
                SUM(da.Horas) as Horas,
                COUNT(DISTINCT rd.Fecha) as DiasTrabajados
            FROM DetallesActividades da
            INNER JOIN RegistrosDiarios rd ON da.RegistroDiarioId = rd.Id
            INNER JOIN Empleados e ON rd.EmpleadoId = e.Id
            LEFT JOIN Cargos c ON e.CargoId = c.Id
            WHERE da.ProyectoId = @ProyectoId
            AND rd.Fecha BETWEEN @FechaInicio AND @FechaFin
            GROUP BY e.Id, e.Codigo, e.Nombres, e.Apellidos, c.Nombre
            ORDER BY Horas DESC";

        var empleados = (await connection.QueryAsync<EmpleadoResumen>(sql, 
            new { ProyectoId = proyectoId, FechaInicio = fechaInicio, FechaFin = fechaFin })).ToList();

        foreach (var emp in empleados)
        {
            emp.Jornales = (int)Math.Ceiling(emp.Horas / 8);
            emp.PromedioHorasDia = emp.DiasTrabajados > 0 ? Math.Round(emp.Horas / emp.DiasTrabajados, 1) : 0;
        }

        return empleados;
    }

    private async Task<IEnumerable<ProductividadDiaria>> GetProductividadDiariaAsync(
        IDbConnection connection, int proyectoId, string fechaInicio, string fechaFin)
    {
        var sql = @"
            SELECT 
                rd.Fecha,
                SUM(da.Horas) as Horas,
                COUNT(DISTINCT rd.EmpleadoId) as Empleados,
                SUM(da.Cantidad) as Cantidad
            FROM DetallesActividades da
            INNER JOIN RegistrosDiarios rd ON da.RegistroDiarioId = rd.Id
            WHERE da.ProyectoId = @ProyectoId
            AND rd.Fecha BETWEEN @FechaInicio AND @FechaFin
            GROUP BY rd.Fecha
            ORDER BY rd.Fecha";

        return await connection.QueryAsync<ProductividadDiaria>(sql, 
            new { ProyectoId = proyectoId, FechaInicio = fechaInicio, FechaFin = fechaFin });
    }

    public async Task<IEnumerable<ActividadResumen>> GetActividadesByProyectoAsync(int proyectoId, DateTime fechaInicio, DateTime fechaFin)
    {
        using var connection = _context.CreateConnection();
        return await GetActividadesDetalleAsync(connection, proyectoId, 
            fechaInicio.ToString("yyyy-MM-dd"), fechaFin.ToString("yyyy-MM-dd"));
    }

    public async Task<IEnumerable<EmpleadoResumen>> GetEmpleadosByProyectoAsync(int proyectoId, DateTime fechaInicio, DateTime fechaFin)
    {
        using var connection = _context.CreateConnection();
        return await GetEmpleadosDetalleAsync(connection, proyectoId, 
            fechaInicio.ToString("yyyy-MM-dd"), fechaFin.ToString("yyyy-MM-dd"));
    }

    // =========================================
    // REPORTE POR EMPLEADO
    // =========================================

    public async Task<ReporteEmpleadoProductividad> GetReporteEmpleadoAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin)
    {
        using var connection = _context.CreateConnection();

        var fechaInicioStr = fechaInicio.ToString("yyyy-MM-dd");
        var fechaFinStr = fechaFin.ToString("yyyy-MM-dd");

        // Información del empleado
        var empleadoSql = @"
            SELECT 
                e.Id as EmpleadoId,
                e.Codigo,
                e.Nombres || ' ' || e.Apellidos as Nombre,
                c.Nombre as Cargo,
                d.Nombre as Departamento
            FROM Empleados e
            LEFT JOIN Cargos c ON e.CargoId = c.Id
            LEFT JOIN Departamentos d ON e.DepartamentoId = d.Id
            WHERE e.Id = @EmpleadoId";

        var reporte = await connection.QuerySingleOrDefaultAsync<ReporteEmpleadoProductividad>(empleadoSql, 
            new { EmpleadoId = empleadoId });
        
        if (reporte == null) 
            return new ReporteEmpleadoProductividad();

        reporte.FechaInicio = fechaInicio;
        reporte.FechaFin = fechaFin;

        // Totales
        var totalesSql = @"
            SELECT 
                COALESCE(SUM(da.Horas), 0) as TotalHoras,
                COUNT(DISTINCT rd.Fecha) as DiasTrabajados,
                COUNT(DISTINCT da.ActividadId) as ActividadesRealizadas,
                COUNT(DISTINCT da.ProyectoId) as ProyectosParticipados
            FROM DetallesActividades da
            INNER JOIN RegistrosDiarios rd ON da.RegistroDiarioId = rd.Id
            WHERE rd.EmpleadoId = @EmpleadoId
            AND rd.Fecha BETWEEN @FechaInicio AND @FechaFin";

        var totales = await connection.QuerySingleOrDefaultAsync<dynamic>(totalesSql, 
            new { EmpleadoId = empleadoId, FechaInicio = fechaInicioStr, FechaFin = fechaFinStr });
        
        if (totales != null)
        {
            reporte.TotalHoras = (decimal)(totales.TotalHoras ?? 0m);
            reporte.DiasTrabajados = (int)(totales.DiasTrabajados ?? 0);
            reporte.ActividadesRealizadas = (int)(totales.ActividadesRealizadas ?? 0);
            reporte.ProyectosParticipados = (int)(totales.ProyectosParticipados ?? 0);
            reporte.TotalJornales = (int)Math.Ceiling(reporte.TotalHoras / 8);
            reporte.PromedioHorasDia = reporte.DiasTrabajados > 0 
                ? Math.Round(reporte.TotalHoras / reporte.DiasTrabajados, 1) 
                : 0;
        }

        // Por proyecto
        var porProyectoSql = @"
            SELECT 
                p.Id as ProyectoId,
                p.Codigo || ' - ' || p.Nombre as ProyectoNombre,
                SUM(da.Horas) as Horas,
                COUNT(DISTINCT rd.Fecha) as Dias
            FROM DetallesActividades da
            INNER JOIN RegistrosDiarios rd ON da.RegistroDiarioId = rd.Id
            INNER JOIN Proyectos p ON da.ProyectoId = p.Id
            WHERE rd.EmpleadoId = @EmpleadoId
            AND rd.Fecha BETWEEN @FechaInicio AND @FechaFin
            GROUP BY p.Id, p.Codigo, p.Nombre
            ORDER BY Horas DESC";

        reporte.PorProyecto = (await connection.QueryAsync<EmpleadoEnProyectoResumen>(porProyectoSql, 
            new { EmpleadoId = empleadoId, FechaInicio = fechaInicioStr, FechaFin = fechaFinStr })).ToList();

        // Por actividad
        var porActividadSql = @"
            SELECT 
                a.Nombre as Actividad,
                COALESCE(ca.Nombre, a.CategoriaTexto) as Categoria,
                SUM(da.Horas) as Horas,
                SUM(da.Cantidad) as Cantidad,
                a.UnidadAbreviatura as Unidad,
                COUNT(*) as Veces
            FROM DetallesActividades da
            INNER JOIN Actividades a ON da.ActividadId = a.Id
            LEFT JOIN CategoriasActividades ca ON a.CategoriaId = ca.Id
            INNER JOIN RegistrosDiarios rd ON da.RegistroDiarioId = rd.Id
            WHERE rd.EmpleadoId = @EmpleadoId
            AND rd.Fecha BETWEEN @FechaInicio AND @FechaFin
            GROUP BY a.Id, a.Nombre, ca.Nombre, a.CategoriaTexto, a.UnidadAbreviatura
            ORDER BY Horas DESC";

        reporte.PorActividad = (await connection.QueryAsync<EmpleadoActividadResumen>(porActividadSql, 
            new { EmpleadoId = empleadoId, FechaInicio = fechaInicioStr, FechaFin = fechaFinStr })).ToList();

        return reporte;
    }

    public async Task<IEnumerable<EmpleadoMensualResumen>> GetHistoricoEmpleadoAsync(int empleadoId, int meses = 12)
    {
        using var connection = _context.CreateConnection();

        var sql = @"
            SELECT 
                CAST(strftime('%Y', rd.Fecha) AS INTEGER) as Anio,
                CAST(strftime('%m', rd.Fecha) AS INTEGER) as Mes,
                SUM(da.Horas) as Horas
            FROM DetallesActividades da
            INNER JOIN RegistrosDiarios rd ON da.RegistroDiarioId = rd.Id
            WHERE rd.EmpleadoId = @EmpleadoId
            AND rd.Fecha >= date('now', '-' || @Meses || ' months')
            GROUP BY strftime('%Y', rd.Fecha), strftime('%m', rd.Fecha)
            ORDER BY Anio DESC, Mes DESC";

        var datos = (await connection.QueryAsync<EmpleadoMensualResumen>(sql, 
            new { EmpleadoId = empleadoId, Meses = meses })).ToList();

        // Agregar nombre del mes
        var culture = new CultureInfo("es-ES");
        foreach (var item in datos)
        {
            item.MesNombre = culture.DateTimeFormat.GetMonthName(item.Mes);
            item.Jornales = (int)Math.Ceiling(item.Horas / 8);
        }

        return datos;
    }

    // =========================================
    // REPORTE POR ACTIVIDAD
    // =========================================

    public async Task<IEnumerable<ActividadResumen>> GetRendimientosByActividadAsync(FiltrosReporte filtros)
    {
        using var connection = _context.CreateConnection();

        var fechaInicio = (filtros.FechaInicio ?? DateTime.Today.AddMonths(-1)).ToString("yyyy-MM-dd");
        var fechaFin = (filtros.FechaFin ?? DateTime.Today).ToString("yyyy-MM-dd");

        var sql = @"
            SELECT 
                a.Id as ActividadId,
                a.Codigo,
                a.Nombre,
                COALESCE(ca.Nombre, a.CategoriaTexto) as Categoria,
                a.UnidadAbreviatura as UnidadMedida,
                SUM(da.Horas) as Horas,
                SUM(da.Cantidad) as Cantidad,
                a.RendimientoEsperado,
                COUNT(*) as Registros
            FROM DetallesActividades da
            INNER JOIN Actividades a ON da.ActividadId = a.Id
            LEFT JOIN CategoriasActividades ca ON a.CategoriaId = ca.Id
            INNER JOIN RegistrosDiarios rd ON da.RegistroDiarioId = rd.Id
            WHERE rd.Fecha BETWEEN @FechaInicio AND @FechaFin
            AND (@ProyectoId IS NULL OR da.ProyectoId = @ProyectoId)
            AND (@CategoriaId IS NULL OR a.CategoriaId = @CategoriaId)
            GROUP BY a.Id, a.Codigo, a.Nombre, ca.Nombre, a.CategoriaTexto, a.UnidadAbreviatura, a.RendimientoEsperado
            ORDER BY Horas DESC";

        var actividades = (await connection.QueryAsync<ActividadResumen>(sql, new { 
            FechaInicio = fechaInicio, 
            FechaFin = fechaFin,
            ProyectoId = filtros.ProyectoId,
            CategoriaId = filtros.CategoriaActividadId
        })).ToList();

        // Calcular rendimiento y clasificación
        foreach (var act in actividades)
        {
            if (act.Horas > 0 && act.Cantidad.HasValue)
            {
                act.RendimientoLogrado = Math.Round(act.Cantidad.Value / act.Horas, 2);
            }
            
            if (act.RendimientoEsperado > 0 && act.RendimientoLogrado.HasValue)
            {
                act.PorcentajeRendimiento = Math.Round((act.RendimientoLogrado.Value / act.RendimientoEsperado.Value) * 100, 1);
                act.ClasificacionRendimiento = act.PorcentajeRendimiento switch
                {
                    >= 120 => "EXCELENTE",
                    >= 100 => "ÓPTIMO",
                    >= 80 => "ACEPTABLE",
                    >= 60 => "BAJO",
                    _ => "CRÍTICO"
                };
            }
        }

        return actividades;
    }

    // =========================================
    // REPORTE COMPARATIVO
    // =========================================

    public async Task<ComparativoProyectos> GetComparativoProyectosAsync(List<int> proyectoIds, DateTime fechaInicio, DateTime fechaFin)
    {
        using var connection = _context.CreateConnection();

        var fechaInicioStr = fechaInicio.ToString("yyyy-MM-dd");
        var fechaFinStr = fechaFin.ToString("yyyy-MM-dd");

        var comparativo = new ComparativoProyectos
        {
            FechaInicio = fechaInicio,
            FechaFin = fechaFin
        };

        var sql = @"
            SELECT 
                p.Id as ProyectoId,
                p.Codigo,
                p.Nombre,
                ef.NombreComun as Especie,
                p.AreaHectareas,
                COALESCE(SUM(da.Horas), 0) as HorasTotales,
                (SELECT COUNT(DISTINCT pe.EmpleadoId) 
                 FROM ProyectosEmpleados pe 
                 WHERE pe.ProyectoId = p.Id AND pe.FechaDesasignacion IS NULL) as EmpleadosPromedio
            FROM Proyectos p
            LEFT JOIN EspeciesForestales ef ON p.EspecieId = ef.Id
            LEFT JOIN DetallesActividades da ON da.ProyectoId = p.Id
            LEFT JOIN RegistrosDiarios rd ON da.RegistroDiarioId = rd.Id 
                AND rd.Fecha BETWEEN @FechaInicio AND @FechaFin
            WHERE p.Id IN @ProyectoIds
            GROUP BY p.Id, p.Codigo, p.Nombre, ef.NombreComun, p.AreaHectareas";

        var proyectos = (await connection.QueryAsync<ProyectoComparativo>(sql, new { 
            ProyectoIds = proyectoIds, 
            FechaInicio = fechaInicioStr, 
            FechaFin = fechaFinStr 
        })).ToList();

        // Calcular métricas derivadas
        foreach (var proy in proyectos)
        {
            if (proy.AreaHectareas > 0)
            {
                proy.HorasPorHectarea = Math.Round(proy.HorasTotales / proy.AreaHectareas.Value, 2);
            }
        }

        // Calcular rankings
        var ordenadosHorasHa = proyectos.OrderBy(p => p.HorasPorHectarea ?? decimal.MaxValue).ToList();
        var ordenadosRendimiento = proyectos.OrderByDescending(p => p.RendimientoPromedio).ToList();

        for (int i = 0; i < proyectos.Count; i++)
        {
            proyectos[i].RankingHorasHa = ordenadosHorasHa.IndexOf(proyectos[i]) + 1;
            proyectos[i].RankingRendimiento = ordenadosRendimiento.IndexOf(proyectos[i]) + 1;
        }

        comparativo.Proyectos = proyectos;
        return comparativo;
    }

    // =========================================
    // ACTUALIZACIÓN DE MÉTRICAS
    // =========================================

    public async Task ActualizarMetricasProyectoAsync(int proyectoId)
    {
        using var connection = _context.CreateConnection();

        var sql = @"
            UPDATE Proyectos 
            SET 
                TotalHorasTrabajadas = (
                    SELECT COALESCE(SUM(da.Horas), 0) 
                    FROM DetallesActividades da 
                    WHERE da.ProyectoId = @ProyectoId
                ),
                TotalJornales = (
                    SELECT COUNT(DISTINCT rd.Id) 
                    FROM RegistrosDiarios rd
                    INNER JOIN DetallesActividades da ON rd.Id = da.RegistroDiarioId
                    WHERE da.ProyectoId = @ProyectoId
                ),
                CostoManoObraAcumulado = (
                    SELECT COALESCE(SUM(sub.CostoDia), 0)
                    FROM (
                        SELECT DISTINCT 
                            rd.EmpleadoId, 
                            rd.Fecha,
                            COALESCE(e.SalarioBase / 30.0, 0) as CostoDia
                        FROM RegistrosDiarios rd
                        INNER JOIN Empleados e ON rd.EmpleadoId = e.Id
                        INNER JOIN DetallesActividades da ON rd.Id = da.RegistroDiarioId
                        WHERE da.ProyectoId = @ProyectoId
                        AND e.SalarioBase IS NOT NULL
                    ) sub
                ),
                FechaUltimaActualizacionMetricas = datetime('now', 'localtime'),
                FechaModificacion = datetime('now', 'localtime')
            WHERE Id = @ProyectoId";

        try
        {
            await connection.ExecuteAsync(sql, new { ProyectoId = proyectoId });
            _logger.LogInformation("Métricas actualizadas para proyecto {ProyectoId}", proyectoId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error actualizando métricas del proyecto {ProyectoId}", proyectoId);
            throw;
        }
    }

    public async Task ActualizarMetricasEmpleadoProyectoAsync(int proyectoId, int empleadoId)
    {
        using var connection = _context.CreateConnection();

        var sql = @"
            UPDATE ProyectosEmpleados 
            SET 
                HorasTrabajadasProyecto = (
                    SELECT COALESCE(SUM(da.Horas), 0) 
                    FROM DetallesActividades da
                    INNER JOIN RegistrosDiarios rd ON da.RegistroDiarioId = rd.Id
                    WHERE da.ProyectoId = @ProyectoId AND rd.EmpleadoId = @EmpleadoId
                ),
                UltimaActividadFecha = (
                    SELECT MAX(rd.Fecha)
                    FROM DetallesActividades da
                    INNER JOIN RegistrosDiarios rd ON da.RegistroDiarioId = rd.Id
                    WHERE da.ProyectoId = @ProyectoId AND rd.EmpleadoId = @EmpleadoId
                ),
                FechaModificacion = datetime('now', 'localtime')
            WHERE ProyectoId = @ProyectoId AND EmpleadoId = @EmpleadoId";

        try
        {
            await connection.ExecuteAsync(sql, new { ProyectoId = proyectoId, EmpleadoId = empleadoId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error actualizando métricas del empleado {EmpleadoId} en proyecto {ProyectoId}", 
                empleadoId, proyectoId);
        }
    }

    public async Task ActualizarTodasLasMetricasAsync()
    {
        using var connection = _context.CreateConnection();

        try
        {
            // Obtener todos los proyectos activos
            var proyectoIds = await connection.QueryAsync<int>(
                "SELECT Id FROM Proyectos WHERE Activo = 1");

            foreach (var proyectoId in proyectoIds)
            {
                await ActualizarMetricasProyectoAsync(proyectoId);
            }

            _logger.LogInformation("Todas las métricas de proyectos actualizadas");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error actualizando todas las métricas");
            throw;
        }
    }

    // =========================================
    // EXPORTACIÓN (Placeholder - implementar con QuestPDF/ClosedXML)
    // =========================================

    public Task<byte[]> ExportarPdfAsync(string tipoReporte, FiltrosReporte filtros)
    {
        // TODO: Implementar con QuestPDF
        _logger.LogWarning("Exportación PDF no implementada aún");
        return Task.FromResult(Array.Empty<byte>());
    }

    public Task<byte[]> ExportarExcelAsync(string tipoReporte, FiltrosReporte filtros)
    {
        // TODO: Implementar con ClosedXML
        _logger.LogWarning("Exportación Excel no implementada aún");
        return Task.FromResult(Array.Empty<byte>());
    }
}
