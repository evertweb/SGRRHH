using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly Data.DapperContext _context;
    private readonly ILogger<AuditLogRepository> _logger;

    public AuditLogRepository(Data.DapperContext context, ILogger<AuditLogRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AuditLog> AddAsync(AuditLog entity)
    {
        entity.FechaCreacion = DateTime.Now;
        const string sql = @"INSERT INTO audit_logs (fecha_hora, usuario_id, usuario_nombre, accion, entidad, entidad_id, descripcion, direccion_ip, datos_adicionales, activo, fecha_creacion)
VALUES (@FechaHora, @UsuarioId, @UsuarioNombre, @Accion, @Entidad, @EntidadId, @Descripcion, @DireccionIp, @DatosAdicionales, 1, @FechaCreacion);
SELECT last_insert_rowid();";

        using var connection = _context.CreateConnection();
        entity.Id = await connection.ExecuteScalarAsync<int>(sql, entity);
        return entity;
    }

    public async Task UpdateAsync(AuditLog entity)
    {
        entity.FechaModificacion = DateTime.Now;
        const string sql = @"UPDATE audit_logs
SET fecha_hora = @FechaHora,
    usuario_id = @UsuarioId,
    usuario_nombre = @UsuarioNombre,
    accion = @Accion,
    entidad = @Entidad,
    entidad_id = @EntidadId,
    descripcion = @Descripcion,
    direccion_ip = @DireccionIp,
    datos_adicionales = @DatosAdicionales,
    fecha_modificacion = @FechaModificacion
WHERE id = @Id";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "DELETE FROM audit_logs WHERE id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<AuditLog?> GetByIdAsync(int id)
    {
        const string sql = "SELECT * FROM audit_logs WHERE id = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<AuditLog>(sql, new { Id = id });
    }

    public async Task<IEnumerable<AuditLog>> GetAllAsync()
    {
        const string sql = "SELECT * FROM audit_logs ORDER BY fecha_hora DESC";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<AuditLog>(sql);
    }

    public async Task<IEnumerable<AuditLog>> GetAllActiveAsync()
    {
        const string sql = "SELECT * FROM audit_logs WHERE activo = 1 ORDER BY fecha_hora DESC";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<AuditLog>(sql);
    }

    public async Task<List<AuditLog>> GetFilteredAsync(DateTime? fechaDesde, DateTime? fechaHasta, string? entidad, int? usuarioId, int maxRegistros)
    {
        var sql = "SELECT * FROM audit_logs WHERE 1=1";
        if (fechaDesde.HasValue) sql += " AND fecha_hora >= @FechaDesde";
        if (fechaHasta.HasValue) sql += " AND fecha_hora <= @FechaHasta";
        if (!string.IsNullOrWhiteSpace(entidad)) sql += " AND entidad = @Entidad";
        if (usuarioId.HasValue) sql += " AND usuario_id = @UsuarioId";
        sql += " ORDER BY fecha_hora DESC LIMIT @MaxRegistros";

        using var connection = _context.CreateConnection();
        var result = await connection.QueryAsync<AuditLog>(sql, new { FechaDesde = fechaDesde, FechaHasta = fechaHasta, Entidad = entidad, UsuarioId = usuarioId, MaxRegistros = maxRegistros });
        return result.ToList();
    }

    public async Task<List<AuditLog>> GetByEntidadAsync(string entidad, int entidadId)
    {
        const string sql = "SELECT * FROM audit_logs WHERE entidad = @Entidad AND entidad_id = @EntidadId ORDER BY fecha_hora DESC";
        using var connection = _context.CreateConnection();
        var result = await connection.QueryAsync<AuditLog>(sql, new { Entidad = entidad, EntidadId = entidadId });
        return result.ToList();
    }

    public async Task<int> DeleteOlderThanAsync(DateTime fecha)
    {
        const string sql = "DELETE FROM audit_logs WHERE fecha_hora < @Fecha";
        using var connection = _context.CreateConnection();
        return await connection.ExecuteAsync(sql, new { Fecha = fecha });
    }

    public Task<int> SaveChangesAsync() => Task.FromResult(0);
}
