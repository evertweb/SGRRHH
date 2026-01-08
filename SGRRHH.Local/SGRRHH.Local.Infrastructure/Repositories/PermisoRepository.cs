using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Repositories;

public class PermisoRepository : IPermisoRepository
{
    private readonly Data.DapperContext _context;
    private readonly ILogger<PermisoRepository> _logger;

    public PermisoRepository(Data.DapperContext context, ILogger<PermisoRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Permiso> AddAsync(Permiso entity)
    {
        entity.FechaCreacion = DateTime.Now;
        const string sql = @"INSERT INTO permisos (
    numero_acta, empleado_id, tipo_permiso_id, motivo, fecha_solicitud, fecha_inicio, fecha_fin, total_dias,
    estado, observaciones, documento_soporte_path, dias_pendientes_compensacion, fecha_compensacion, solicitado_por_id,
    aprobado_por_id, fecha_aprobacion, motivo_rechazo, activo, fecha_creacion)
VALUES (
    @NumeroActa, @EmpleadoId, @TipoPermisoId, @Motivo, @FechaSolicitud, @FechaInicio, @FechaFin, @TotalDias,
    @Estado, @Observaciones, @DocumentoSoportePath, @DiasPendientesCompensacion, @FechaCompensacion, @SolicitadoPorId,
    @AprobadoPorId, @FechaAprobacion, @MotivoRechazo, @Activo, @FechaCreacion);
SELECT last_insert_rowid();";

        using var connection = _context.CreateConnection();
        entity.Id = await connection.ExecuteScalarAsync<int>(sql, entity);
        return entity;
    }

    public async Task UpdateAsync(Permiso entity)
    {
        entity.FechaModificacion = DateTime.Now;
        const string sql = @"UPDATE permisos
SET numero_acta = @NumeroActa,
    empleado_id = @EmpleadoId,
    tipo_permiso_id = @TipoPermisoId,
    motivo = @Motivo,
    fecha_solicitud = @FechaSolicitud,
    fecha_inicio = @FechaInicio,
    fecha_fin = @FechaFin,
    total_dias = @TotalDias,
    estado = @Estado,
    observaciones = @Observaciones,
    documento_soporte_path = @DocumentoSoportePath,
    dias_pendientes_compensacion = @DiasPendientesCompensacion,
    fecha_compensacion = @FechaCompensacion,
    solicitado_por_id = @SolicitadoPorId,
    aprobado_por_id = @AprobadoPorId,
    fecha_aprobacion = @FechaAprobacion,
    motivo_rechazo = @MotivoRechazo,
    fecha_modificacion = @FechaModificacion
WHERE id = @Id";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "UPDATE permisos SET activo = 0, fecha_modificacion = CURRENT_TIMESTAMP WHERE id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<Permiso?> GetByIdAsync(int id)
    {
        const string sql = "SELECT * FROM permisos WHERE id = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Permiso>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Permiso>> GetAllAsync()
    {
        const string sql = "SELECT * FROM permisos";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Permiso>(sql);
    }

    public async Task<IEnumerable<Permiso>> GetAllActiveAsync()
    {
        const string sql = "SELECT * FROM permisos WHERE activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Permiso>(sql);
    }

    public async Task<IEnumerable<Permiso>> GetPendientesAsync()
    {
        const string sql = "SELECT * FROM permisos WHERE estado = @Estado AND activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Permiso>(sql, new { Estado = EstadoPermiso.Pendiente });
    }

    public async Task<IEnumerable<Permiso>> GetByEmpleadoIdAsync(int empleadoId)
    {
        const string sql = "SELECT * FROM permisos WHERE empleado_id = @EmpleadoId AND activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Permiso>(sql, new { EmpleadoId = empleadoId });
    }

    public async Task<IEnumerable<Permiso>> GetByRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        const string sql = "SELECT * FROM permisos WHERE fecha_inicio >= @FechaInicio AND fecha_fin <= @FechaFin";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Permiso>(sql, new { FechaInicio = fechaInicio, FechaFin = fechaFin });
    }

    public async Task<IEnumerable<Permiso>> GetByEstadoAsync(EstadoPermiso estado)
    {
        const string sql = "SELECT * FROM permisos WHERE estado = @Estado";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Permiso>(sql, new { Estado = estado });
    }

    public async Task<string> GetProximoNumeroActaAsync()
    {
        var year = DateTime.Now.Year;
        const string sql = @"SELECT numero_acta FROM permisos WHERE substr(numero_acta, 6, 4) = @YearText ORDER BY numero_acta DESC LIMIT 1";
        using var connection = _context.CreateConnection();
        var last = await connection.QuerySingleOrDefaultAsync<string>(sql, new { YearText = year.ToString() });
        var nextSeq = 1;
        if (!string.IsNullOrWhiteSpace(last))
        {
            var parts = last.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out var current))
            {
                nextSeq = current + 1;
            }
        }
        return $"PERM-{year}-{nextSeq:0000}";
    }

    public async Task<bool> ExisteSolapamientoAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin, int? excludePermisoId = null)
    {
        const string sql = @"SELECT COUNT(1) FROM permisos
WHERE empleado_id = @EmpleadoId
  AND activo = 1
  AND fecha_inicio <= @FechaFin
  AND fecha_fin >= @FechaInicio
  AND (@ExcludeId IS NULL OR id <> @ExcludeId)";

        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { EmpleadoId = empleadoId, FechaInicio = fechaInicio, FechaFin = fechaFin, ExcludeId = excludePermisoId });
        return count > 0;
    }

    public async Task<IEnumerable<Permiso>> GetAllWithFiltersAsync(int? empleadoId = null, EstadoPermiso? estado = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null)
    {
        var sql = "SELECT * FROM permisos WHERE 1=1";
        if (empleadoId.HasValue) sql += " AND empleado_id = @EmpleadoId";
        if (estado.HasValue) sql += " AND estado = @Estado";
        if (fechaDesde.HasValue) sql += " AND fecha_inicio >= @FechaDesde";
        if (fechaHasta.HasValue) sql += " AND fecha_fin <= @FechaHasta";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Permiso>(sql, new
        {
            EmpleadoId = empleadoId,
            Estado = estado,
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta
        });
    }

    public Task<int> SaveChangesAsync() => Task.FromResult(0);
}
