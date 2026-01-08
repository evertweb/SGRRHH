using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Repositories;

public class RegistroDiarioRepository : IRegistroDiarioRepository
{
    private readonly Data.DapperContext _context;
    private readonly ILogger<RegistroDiarioRepository> _logger;

    public RegistroDiarioRepository(Data.DapperContext context, ILogger<RegistroDiarioRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<RegistroDiario> AddAsync(RegistroDiario entity)
    {
        entity.FechaCreacion = DateTime.Now;
        const string sql = @"INSERT INTO registros_diarios (fecha, empleado_id, hora_entrada, hora_salida, observaciones, estado, activo, fecha_creacion)
VALUES (@Fecha, @EmpleadoId, @HoraEntrada, @HoraSalida, @Observaciones, @Estado, 1, @FechaCreacion);
SELECT last_insert_rowid();";

        using var connection = _context.CreateConnection();
        entity.Id = await connection.ExecuteScalarAsync<int>(sql, entity);
        return entity;
    }

    public async Task UpdateAsync(RegistroDiario entity)
    {
        entity.FechaModificacion = DateTime.Now;
        const string sql = @"UPDATE registros_diarios
SET fecha = @Fecha,
    empleado_id = @EmpleadoId,
    hora_entrada = @HoraEntrada,
    hora_salida = @HoraSalida,
    observaciones = @Observaciones,
    estado = @Estado,
    fecha_modificacion = @FechaModificacion
WHERE id = @Id";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "UPDATE registros_diarios SET activo = 0, fecha_modificacion = CURRENT_TIMESTAMP WHERE id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<RegistroDiario?> GetByIdAsync(int id)
    {
        const string sql = "SELECT * FROM registros_diarios WHERE id = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<RegistroDiario>(sql, new { Id = id });
    }

    public async Task<IEnumerable<RegistroDiario>> GetAllAsync()
    {
        const string sql = "SELECT * FROM registros_diarios";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<RegistroDiario>(sql);
    }

    public async Task<IEnumerable<RegistroDiario>> GetAllActiveAsync()
    {
        const string sql = "SELECT * FROM registros_diarios WHERE activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<RegistroDiario>(sql);
    }

    public async Task<RegistroDiario?> GetByFechaEmpleadoAsync(DateTime fecha, int empleadoId)
    {
        const string sql = "SELECT * FROM registros_diarios WHERE fecha = @Fecha AND empleado_id = @EmpleadoId";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<RegistroDiario>(sql, new { Fecha = fecha, EmpleadoId = empleadoId });
    }

    public async Task<RegistroDiario?> GetByIdWithDetallesAsync(int id)
    {
        using var connection = _context.CreateConnection();
        var registro = await GetByIdAsync(id);
        if (registro is null)
        {
            return null;
        }

        const string detallesSql = "SELECT * FROM detalles_actividad WHERE registro_diario_id = @RegistroId AND activo = 1";
        var detalles = await connection.QueryAsync<DetalleActividad>(detallesSql, new { RegistroId = id });
        registro.DetallesActividades = detalles.ToList();
        return registro;
    }

    public async Task<IEnumerable<RegistroDiario>> GetByEmpleadoRangoFechasAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin)
    {
        const string sql = "SELECT * FROM registros_diarios WHERE empleado_id = @EmpleadoId AND fecha BETWEEN @FechaInicio AND @FechaFin";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<RegistroDiario>(sql, new { EmpleadoId = empleadoId, FechaInicio = fechaInicio, FechaFin = fechaFin });
    }

    public async Task<IEnumerable<RegistroDiario>> GetByFechaAsync(DateTime fecha)
    {
        const string sql = "SELECT * FROM registros_diarios WHERE fecha = @Fecha";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<RegistroDiario>(sql, new { Fecha = fecha });
    }

    public async Task<IEnumerable<RegistroDiario>> GetByRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        const string sql = "SELECT * FROM registros_diarios WHERE fecha BETWEEN @FechaInicio AND @FechaFin";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<RegistroDiario>(sql, new { FechaInicio = fechaInicio, FechaFin = fechaFin });
    }

    public async Task<IEnumerable<RegistroDiario>> GetByEmpleadoWithDetallesAsync(int empleadoId, int? cantidad = null)
    {
        var sql = "SELECT * FROM registros_diarios WHERE empleado_id = @EmpleadoId ORDER BY fecha DESC";
        if (cantidad.HasValue)
        {
            sql += " LIMIT @Cantidad";
        }

        using var connection = _context.CreateConnection();
        var registros = (await connection.QueryAsync<RegistroDiario>(sql, new { EmpleadoId = empleadoId, Cantidad = cantidad })).ToList();

        foreach (var registro in registros)
        {
            const string detallesSql = "SELECT * FROM detalles_actividad WHERE registro_diario_id = @RegistroId AND activo = 1";
            var detalles = await connection.QueryAsync<DetalleActividad>(detallesSql, new { RegistroId = registro.Id });
            registro.DetallesActividades = detalles.ToList();
        }

        return registros;
    }

    public async Task<IEnumerable<RegistroDiario>> GetByEmpleadoMesActualAsync(int empleadoId)
    {
        var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var finMes = inicioMes.AddMonths(1).AddDays(-1);
        return await GetByEmpleadoRangoFechasAsync(empleadoId, inicioMes, finMes);
    }

    public async Task<bool> ExistsByFechaEmpleadoAsync(DateTime fecha, int empleadoId)
    {
        const string sql = "SELECT COUNT(1) FROM registros_diarios WHERE fecha = @Fecha AND empleado_id = @EmpleadoId";
        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Fecha = fecha, EmpleadoId = empleadoId });
        return count > 0;
    }

    public async Task<decimal> GetTotalHorasByEmpleadoRangoAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin)
    {
        const string sql = @"SELECT COALESCE(SUM(horas), 0)
FROM detalles_actividad da
JOIN registros_diarios rd ON da.registro_diario_id = rd.id
WHERE rd.empleado_id = @EmpleadoId
  AND rd.fecha BETWEEN @FechaInicio AND @FechaFin
  AND da.activo = 1";

        using var connection = _context.CreateConnection();
        return await connection.ExecuteScalarAsync<decimal>(sql, new { EmpleadoId = empleadoId, FechaInicio = fechaInicio, FechaFin = fechaFin });
    }

    public async Task<DetalleActividad> AddDetalleAsync(int registroId, DetalleActividad detalle)
    {
        detalle.RegistroDiarioId = registroId;
        detalle.FechaCreacion = DateTime.Now;
        const string sql = @"INSERT INTO detalles_actividad (registro_diario_id, actividad_id, proyecto_id, descripcion, horas, hora_inicio, hora_fin, orden, activo, fecha_creacion)
VALUES (@RegistroDiarioId, @ActividadId, @ProyectoId, @Descripcion, @Horas, @HoraInicio, @HoraFin, @Orden, 1, @FechaCreacion);
SELECT last_insert_rowid();";

        using var connection = _context.CreateConnection();
        detalle.Id = await connection.ExecuteScalarAsync<int>(sql, detalle);
        return detalle;
    }

    public async Task UpdateDetalleAsync(int registroId, DetalleActividad detalle)
    {
        detalle.RegistroDiarioId = registroId;
        await UpdateDetalleAsync(detalle);
    }

    public async Task UpdateDetalleAsync(DetalleActividad detalle)
    {
        detalle.FechaModificacion = DateTime.Now;
        const string sql = @"UPDATE detalles_actividad
SET registro_diario_id = @RegistroDiarioId,
    actividad_id = @ActividadId,
    proyecto_id = @ProyectoId,
    descripcion = @Descripcion,
    horas = @Horas,
    hora_inicio = @HoraInicio,
    hora_fin = @HoraFin,
    orden = @Orden,
    fecha_modificacion = @FechaModificacion
WHERE id = @Id";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, detalle);
    }

    public async Task DeleteDetalleAsync(int registroId, int detalleId)
    {
        const string sql = "UPDATE detalles_actividad SET activo = 0, fecha_modificacion = CURRENT_TIMESTAMP WHERE id = @Id AND registro_diario_id = @RegistroId";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = detalleId, RegistroId = registroId });
    }

    public async Task<DetalleActividad?> GetDetalleByIdAsync(int registroId, int detalleId)
    {
        const string sql = "SELECT * FROM detalles_actividad WHERE id = @Id AND registro_diario_id = @RegistroId";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<DetalleActividad>(sql, new { Id = detalleId, RegistroId = registroId });
    }

    public async Task<DetalleActividad?> GetDetalleByIdAsync(int detalleId)
    {
        const string sql = "SELECT * FROM detalles_actividad WHERE id = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<DetalleActividad>(sql, new { Id = detalleId });
    }

    public async Task<IEnumerable<DetalleActividad>> GetDetallesByProyectoAsync(int proyectoId)
    {
        const string sql = "SELECT * FROM detalles_actividad WHERE proyecto_id = @ProyectoId AND activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<DetalleActividad>(sql, new { ProyectoId = proyectoId });
    }

    public async Task<IEnumerable<DetalleActividad>> GetDetallesByProyectoRangoFechasAsync(int proyectoId, DateTime fechaInicio, DateTime fechaFin)
    {
        const string sql = @"SELECT da.*
FROM detalles_actividad da
JOIN registros_diarios rd ON da.registro_diario_id = rd.id
WHERE da.proyecto_id = @ProyectoId
  AND rd.fecha BETWEEN @FechaInicio AND @FechaFin
  AND da.activo = 1";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<DetalleActividad>(sql, new { ProyectoId = proyectoId, FechaInicio = fechaInicio, FechaFin = fechaFin });
    }

    public async Task<decimal> GetTotalHorasByProyectoAsync(int proyectoId)
    {
        const string sql = "SELECT COALESCE(SUM(horas), 0) FROM detalles_actividad WHERE proyecto_id = @ProyectoId AND activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.ExecuteScalarAsync<decimal>(sql, new { ProyectoId = proyectoId });
    }

    public async Task<IEnumerable<ProyectoHorasEmpleado>> GetHorasPorEmpleadoProyectoAsync(int proyectoId)
    {
        const string sql = @"SELECT rd.empleado_id AS EmpleadoId,
       COALESCE(e.nombres || ' ' || e.apellidos, '') AS EmpleadoNombre,
       SUM(da.horas) AS TotalHoras,
       COUNT(da.id) AS CantidadActividades,
       MAX(rd.fecha) AS UltimaActividad
FROM detalles_actividad da
JOIN registros_diarios rd ON da.registro_diario_id = rd.id
LEFT JOIN empleados e ON rd.empleado_id = e.id
WHERE da.proyecto_id = @ProyectoId AND da.activo = 1
GROUP BY rd.empleado_id";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<ProyectoHorasEmpleado>(sql, new { ProyectoId = proyectoId });
    }

    public Task<int> SaveChangesAsync() => Task.FromResult(0);
}
