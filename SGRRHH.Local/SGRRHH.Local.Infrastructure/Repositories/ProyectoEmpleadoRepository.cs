using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Repositories;

public class ProyectoEmpleadoRepository : IProyectoEmpleadoRepository
{
    private readonly Data.DapperContext _context;
    private readonly ILogger<ProyectoEmpleadoRepository> _logger;

    public ProyectoEmpleadoRepository(Data.DapperContext context, ILogger<ProyectoEmpleadoRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ProyectoEmpleado> AddAsync(ProyectoEmpleado entity)
    {
        entity.FechaCreacion = DateTime.Now;
        const string sql = @"INSERT INTO proyectos_empleados (
            proyecto_id, empleado_id, fecha_asignacion, fecha_desasignacion, 
            rol, rol_enum, es_lider_cuadrilla, porcentaje_dedicacion, tipo_vinculacion,
            motivo_desasignacion, total_horas_trabajadas, total_jornales, 
            ultima_fecha_trabajo, dias_trabajados, observaciones, activo, fecha_creacion)
        VALUES (
            @ProyectoId, @EmpleadoId, @FechaAsignacion, @FechaDesasignacion,
            @Rol, @RolEnum, @EsLiderCuadrilla, @PorcentajeDedicacion, @TipoVinculacion,
            @MotivoDesasignacion, @TotalHorasTrabajadas, @TotalJornales,
            @UltimaFechaTrabajo, @DiasTrabajados, @Observaciones, 1, @FechaCreacion);
        SELECT last_insert_rowid();";

        using var connection = _context.CreateConnection();
        entity.Id = await connection.ExecuteScalarAsync<int>(sql, entity);
        return entity;
    }

    public async Task UpdateAsync(ProyectoEmpleado entity)
    {
        entity.FechaModificacion = DateTime.Now;
        const string sql = @"UPDATE proyectos_empleados
        SET proyecto_id = @ProyectoId,
            empleado_id = @EmpleadoId,
            fecha_asignacion = @FechaAsignacion,
            fecha_desasignacion = @FechaDesasignacion,
            rol = @Rol,
            rol_enum = @RolEnum,
            es_lider_cuadrilla = @EsLiderCuadrilla,
            porcentaje_dedicacion = @PorcentajeDedicacion,
            tipo_vinculacion = @TipoVinculacion,
            motivo_desasignacion = @MotivoDesasignacion,
            total_horas_trabajadas = @TotalHorasTrabajadas,
            total_jornales = @TotalJornales,
            ultima_fecha_trabajo = @UltimaFechaTrabajo,
            dias_trabajados = @DiasTrabajados,
            observaciones = @Observaciones,
            fecha_modificacion = @FechaModificacion
        WHERE id = @Id";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "DELETE FROM proyectos_empleados WHERE id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<ProyectoEmpleado?> GetByIdAsync(int id)
    {
        const string sql = @"SELECT pe.*, 
            e.id as EmpleadoId, e.codigo, e.nombres, e.apellidos, e.cedula
        FROM proyectos_empleados pe
        LEFT JOIN empleados e ON pe.empleado_id = e.id
        WHERE pe.id = @Id";
        
        using var connection = _context.CreateConnection();
        var result = await connection.QueryAsync<ProyectoEmpleado, Empleado, ProyectoEmpleado>(
            sql,
            (pe, emp) =>
            {
                pe.Empleado = emp;
                return pe;
            },
            new { Id = id },
            splitOn: "EmpleadoId"
        );
        return result.FirstOrDefault();
    }

    public async Task<IEnumerable<ProyectoEmpleado>> GetAllAsync()
    {
        const string sql = "SELECT * FROM proyectos_empleados";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<ProyectoEmpleado>(sql);
    }

    public async Task<IEnumerable<ProyectoEmpleado>> GetAllActiveAsync()
    {
        const string sql = "SELECT * FROM proyectos_empleados WHERE activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<ProyectoEmpleado>(sql);
    }

    public async Task<IEnumerable<ProyectoEmpleado>> GetByProyectoAsync(int proyectoId)
    {
        const string sql = "SELECT * FROM proyectos_empleados WHERE proyecto_id = @ProyectoId";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<ProyectoEmpleado>(sql, new { ProyectoId = proyectoId });
    }

    public async Task<IEnumerable<ProyectoEmpleado>> GetActiveByProyectoAsync(int proyectoId)
    {
        const string sql = @"SELECT * FROM proyectos_empleados 
            WHERE proyecto_id = @ProyectoId AND activo = 1 AND fecha_desasignacion IS NULL";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<ProyectoEmpleado>(sql, new { ProyectoId = proyectoId });
    }

    public async Task<IEnumerable<ProyectoEmpleado>> GetActiveByProyectoWithEmpleadoAsync(int proyectoId)
    {
        const string sql = @"SELECT pe.*, 
            e.id as EmpleadoId, e.codigo, e.nombres, e.apellidos, e.cedula, e.cargo_id
        FROM proyectos_empleados pe
        LEFT JOIN empleados e ON pe.empleado_id = e.id
        WHERE pe.proyecto_id = @ProyectoId AND pe.activo = 1 AND pe.fecha_desasignacion IS NULL
        ORDER BY pe.es_lider_cuadrilla DESC, e.apellidos, e.nombres";
        
        using var connection = _context.CreateConnection();
        var result = await connection.QueryAsync<ProyectoEmpleado, Empleado, ProyectoEmpleado>(
            sql,
            (pe, emp) =>
            {
                pe.Empleado = emp;
                return pe;
            },
            new { ProyectoId = proyectoId },
            splitOn: "EmpleadoId"
        );
        return result;
    }

    public async Task<IEnumerable<ProyectoEmpleado>> GetByEmpleadoAsync(int empleadoId)
    {
        const string sql = "SELECT * FROM proyectos_empleados WHERE empleado_id = @EmpleadoId";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<ProyectoEmpleado>(sql, new { EmpleadoId = empleadoId });
    }

    public async Task<IEnumerable<ProyectoEmpleado>> GetActiveByEmpleadoAsync(int empleadoId)
    {
        const string sql = @"SELECT * FROM proyectos_empleados 
            WHERE empleado_id = @EmpleadoId AND activo = 1 AND fecha_desasignacion IS NULL";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<ProyectoEmpleado>(sql, new { EmpleadoId = empleadoId });
    }

    public async Task<IEnumerable<Proyecto>> GetProyectosActivosByEmpleadoAsync(int empleadoId)
    {
        const string sql = @"SELECT p.* FROM proyectos p
            INNER JOIN proyectos_empleados pe ON p.id = pe.proyecto_id
            WHERE pe.empleado_id = @EmpleadoId AND pe.activo = 1 AND pe.fecha_desasignacion IS NULL
            AND p.activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Proyecto>(sql, new { EmpleadoId = empleadoId });
    }

    public async Task<bool> ExistsAsignacionAsync(int proyectoId, int empleadoId)
    {
        const string sql = @"SELECT COUNT(1) FROM proyectos_empleados 
            WHERE proyecto_id = @ProyectoId AND empleado_id = @EmpleadoId";
        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { ProyectoId = proyectoId, EmpleadoId = empleadoId });
        return count > 0;
    }

    public async Task<bool> ExistsAsignacionActivaAsync(int proyectoId, int empleadoId)
    {
        const string sql = @"SELECT COUNT(1) FROM proyectos_empleados 
            WHERE proyecto_id = @ProyectoId AND empleado_id = @EmpleadoId 
            AND activo = 1 AND fecha_desasignacion IS NULL";
        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { ProyectoId = proyectoId, EmpleadoId = empleadoId });
        return count > 0;
    }

    public async Task<ProyectoEmpleado?> GetAsignacionAsync(int proyectoId, int empleadoId)
    {
        const string sql = @"SELECT * FROM proyectos_empleados 
            WHERE proyecto_id = @ProyectoId AND empleado_id = @EmpleadoId";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<ProyectoEmpleado>(sql, new { ProyectoId = proyectoId, EmpleadoId = empleadoId });
    }

    public async Task<ProyectoEmpleado?> GetAsignacionActivaAsync(int proyectoId, int empleadoId)
    {
        const string sql = @"SELECT * FROM proyectos_empleados 
            WHERE proyecto_id = @ProyectoId AND empleado_id = @EmpleadoId 
            AND activo = 1 AND fecha_desasignacion IS NULL";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<ProyectoEmpleado>(sql, new { ProyectoId = proyectoId, EmpleadoId = empleadoId });
    }

    public async Task DesasignarAsync(int proyectoId, int empleadoId, string? motivo = null)
    {
        const string sql = @"UPDATE proyectos_empleados 
            SET fecha_desasignacion = @FechaDesasignacion,
                motivo_desasignacion = @Motivo,
                fecha_modificacion = @FechaModificacion
            WHERE proyecto_id = @ProyectoId AND empleado_id = @EmpleadoId 
            AND fecha_desasignacion IS NULL";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new 
        { 
            ProyectoId = proyectoId, 
            EmpleadoId = empleadoId,
            Motivo = motivo,
            FechaDesasignacion = DateTime.Now,
            FechaModificacion = DateTime.Now
        });
    }

    public async Task<int> GetCountByProyectoAsync(int proyectoId)
    {
        const string sql = "SELECT COUNT(1) FROM proyectos_empleados WHERE proyecto_id = @ProyectoId";
        using var connection = _context.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, new { ProyectoId = proyectoId });
    }

    public async Task<int> GetActiveCountByProyectoAsync(int proyectoId)
    {
        const string sql = @"SELECT COUNT(1) FROM proyectos_empleados 
            WHERE proyecto_id = @ProyectoId AND activo = 1 AND fecha_desasignacion IS NULL";
        using var connection = _context.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, new { ProyectoId = proyectoId });
    }

    // ===== Asignación Masiva =====

    public async Task AsignarMultiplesAsync(int proyectoId, IEnumerable<int> empleadoIds, RolProyectoForestal? rol = null)
    {
        var ahora = DateTime.Now;
        foreach (var empleadoId in empleadoIds)
        {
            // Verificar si ya existe asignación activa
            if (await ExistsAsignacionActivaAsync(proyectoId, empleadoId))
                continue;

            var asignacion = new ProyectoEmpleado
            {
                ProyectoId = proyectoId,
                EmpleadoId = empleadoId,
                RolEnum = rol,
                Rol = rol?.ToString(),
                FechaAsignacion = ahora,
                PorcentajeDedicacion = 100,
                FechaCreacion = ahora
            };

            await AddAsync(asignacion);
        }
    }

    public async Task DesasignarMultiplesAsync(int proyectoId, IEnumerable<int> empleadoIds, string? motivo = null)
    {
        foreach (var empleadoId in empleadoIds)
        {
            await DesasignarAsync(proyectoId, empleadoId, motivo);
        }
    }

    // ===== Consultas por Rol =====

    public async Task<IEnumerable<ProyectoEmpleado>> GetByRolAsync(int proyectoId, RolProyectoForestal rol)
    {
        const string sql = @"SELECT pe.*, e.* 
            FROM proyectos_empleados pe
            LEFT JOIN empleados e ON pe.empleado_id = e.id
            WHERE pe.proyecto_id = @ProyectoId AND pe.rol_enum = @Rol 
            AND pe.activo = 1 AND pe.fecha_desasignacion IS NULL";
        
        using var connection = _context.CreateConnection();
        var result = await connection.QueryAsync<ProyectoEmpleado, Empleado, ProyectoEmpleado>(
            sql,
            (pe, emp) =>
            {
                pe.Empleado = emp;
                return pe;
            },
            new { ProyectoId = proyectoId, Rol = (int)rol },
            splitOn: "id"
        );
        return result;
    }

    public async Task<ProyectoEmpleado?> GetLiderCuadrillaAsync(int proyectoId)
    {
        const string sql = @"SELECT pe.*, 
            e.id as EmpleadoId, e.codigo, e.nombres, e.apellidos
            FROM proyectos_empleados pe
            LEFT JOIN empleados e ON pe.empleado_id = e.id
            WHERE pe.proyecto_id = @ProyectoId AND pe.es_lider_cuadrilla = 1 
            AND pe.activo = 1 AND pe.fecha_desasignacion IS NULL
            LIMIT 1";
        
        using var connection = _context.CreateConnection();
        var result = await connection.QueryAsync<ProyectoEmpleado, Empleado, ProyectoEmpleado>(
            sql,
            (pe, emp) =>
            {
                pe.Empleado = emp;
                return pe;
            },
            new { ProyectoId = proyectoId },
            splitOn: "EmpleadoId"
        );
        return result.FirstOrDefault();
    }

    // ===== Métricas =====

    public async Task ActualizarMetricasAsync(int proyectoEmpleadoId)
    {
        // Obtener asignación
        var asignacion = await GetByIdAsync(proyectoEmpleadoId);
        if (asignacion == null) return;

        const string sql = @"SELECT 
            COALESCE(SUM(da.horas_trabajadas), 0) as TotalHoras,
            COUNT(DISTINCT DATE(da.fecha)) as DiasTrabajados,
            MAX(da.fecha) as UltimaFecha
        FROM detalles_actividades da
        WHERE da.empleado_id = @EmpleadoId AND da.proyecto_id = @ProyectoId
        AND da.activo = 1";

        using var connection = _context.CreateConnection();
        var metricas = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, 
            new { EmpleadoId = asignacion.EmpleadoId, ProyectoId = asignacion.ProyectoId });

        if (metricas != null)
        {
            const string updateSql = @"UPDATE proyectos_empleados 
                SET total_horas_trabajadas = @TotalHoras,
                    dias_trabajados = @DiasTrabajados,
                    ultima_fecha_trabajo = @UltimaFecha,
                    total_jornales = @DiasTrabajados,
                    fecha_modificacion = @FechaModificacion
                WHERE id = @Id";

            await connection.ExecuteAsync(updateSql, new
            {
                Id = proyectoEmpleadoId,
                TotalHoras = (decimal)(metricas.TotalHoras ?? 0),
                DiasTrabajados = (int)(metricas.DiasTrabajados ?? 0),
                UltimaFecha = metricas.UltimaFecha as DateTime?,
                FechaModificacion = DateTime.Now
            });
        }
    }

    public async Task ActualizarMetricasProyectoAsync(int proyectoId)
    {
        var asignaciones = await GetActiveByProyectoAsync(proyectoId);
        foreach (var asignacion in asignaciones)
        {
            await ActualizarMetricasAsync(asignacion.Id);
        }
    }

    // ===== Reportes =====

    public async Task<IEnumerable<EmpleadoProyectoResumen>> GetResumenPorEmpleadoAsync(int proyectoId)
    {
        const string sql = @"SELECT 
            pe.empleado_id as EmpleadoId,
            e.nombres || ' ' || e.apellidos as NombreEmpleado,
            COALESCE(pe.rol, pe.rol_enum, 'Sin rol') as Rol,
            pe.total_horas_trabajadas as TotalHoras,
            pe.total_jornales as TotalJornales,
            pe.dias_trabajados as DiasTrabajados,
            pe.ultima_fecha_trabajo as UltimaActividad,
            CASE WHEN pe.fecha_desasignacion IS NULL THEN 1 ELSE 0 END as Activo
        FROM proyectos_empleados pe
        LEFT JOIN empleados e ON pe.empleado_id = e.id
        WHERE pe.proyecto_id = @ProyectoId AND pe.activo = 1
        ORDER BY pe.total_horas_trabajadas DESC";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<EmpleadoProyectoResumen>(sql, new { ProyectoId = proyectoId });
    }

    public async Task<IEnumerable<ProyectoEmpleadoHistorial>> GetHistorialAsync(int proyectoId)
    {
        const string sql = @"SELECT 
            pe.empleado_id as EmpleadoId,
            e.nombres || ' ' || e.apellidos as NombreEmpleado,
            COALESCE(pe.rol, CASE WHEN pe.rol_enum IS NOT NULL THEN CAST(pe.rol_enum AS TEXT) ELSE 'Sin rol' END) as Rol,
            pe.fecha_asignacion as FechaAsignacion,
            pe.fecha_desasignacion as FechaDesasignacion,
            pe.motivo_desasignacion as MotivoDesasignacion,
            CAST(JULIANDAY(COALESCE(pe.fecha_desasignacion, DATE('now'))) - JULIANDAY(pe.fecha_asignacion) AS INTEGER) as DiasEnProyecto,
            pe.total_horas_trabajadas as HorasTrabajadas
        FROM proyectos_empleados pe
        LEFT JOIN empleados e ON pe.empleado_id = e.id
        WHERE pe.proyecto_id = @ProyectoId
        ORDER BY pe.fecha_asignacion DESC";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<ProyectoEmpleadoHistorial>(sql, new { ProyectoId = proyectoId });
    }

    public Task<int> SaveChangesAsync() => Task.FromResult(0);
}
