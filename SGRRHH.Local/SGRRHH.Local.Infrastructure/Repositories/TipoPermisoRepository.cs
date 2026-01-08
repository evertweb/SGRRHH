using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Repositories;

public class TipoPermisoRepository : ITipoPermisoRepository
{
    private readonly Data.DapperContext _context;
    private readonly ILogger<TipoPermisoRepository> _logger;

    public TipoPermisoRepository(Data.DapperContext context, ILogger<TipoPermisoRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TipoPermiso> AddAsync(TipoPermiso entity)
    {
        entity.FechaCreacion = DateTime.Now;
        const string sql = @"INSERT INTO tipos_permiso (nombre, descripcion, color, requiere_aprobacion, requiere_documento, dias_por_defecto, dias_maximos, es_compensable, activo, fecha_creacion)
VALUES (@Nombre, @Descripcion, @Color, @RequiereAprobacion, @RequiereDocumento, @DiasPorDefecto, @DiasMaximos, @EsCompensable, @Activo, @FechaCreacion);
SELECT last_insert_rowid();";

        using var connection = _context.CreateConnection();
        entity.Id = await connection.ExecuteScalarAsync<int>(sql, entity);
        return entity;
    }

    public async Task UpdateAsync(TipoPermiso entity)
    {
        entity.FechaModificacion = DateTime.Now;
        const string sql = @"UPDATE tipos_permiso
SET nombre = @Nombre,
    descripcion = @Descripcion,
    color = @Color,
    requiere_aprobacion = @RequiereAprobacion,
    requiere_documento = @RequiereDocumento,
    dias_por_defecto = @DiasPorDefecto,
    dias_maximos = @DiasMaximos,
    es_compensable = @EsCompensable,
    activo = @Activo,
    fecha_modificacion = @FechaModificacion
WHERE id = @Id";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "UPDATE tipos_permiso SET activo = 0, fecha_modificacion = CURRENT_TIMESTAMP WHERE id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<TipoPermiso?> GetByIdAsync(int id)
    {
        const string sql = "SELECT * FROM tipos_permiso WHERE id = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<TipoPermiso>(sql, new { Id = id });
    }

    public async Task<IEnumerable<TipoPermiso>> GetAllAsync()
    {
        const string sql = "SELECT * FROM tipos_permiso";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<TipoPermiso>(sql);
    }

    public async Task<IEnumerable<TipoPermiso>> GetAllActiveAsync()
    {
        const string sql = "SELECT * FROM tipos_permiso WHERE activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<TipoPermiso>(sql);
    }

    public async Task<IEnumerable<TipoPermiso>> GetActivosAsync()
    {
        return await GetAllActiveAsync();
    }

    public async Task<bool> ExisteNombreAsync(string nombre, int? excludeId = null)
    {
        const string sql = "SELECT COUNT(1) FROM tipos_permiso WHERE lower(nombre) = lower(@Nombre) AND (@ExcludeId IS NULL OR id <> @ExcludeId)";
        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Nombre = nombre, ExcludeId = excludeId });
        return count > 0;
    }

    public Task<int> SaveChangesAsync() => Task.FromResult(0);
}
