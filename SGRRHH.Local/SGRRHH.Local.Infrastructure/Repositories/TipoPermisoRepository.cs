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
        const string sql = @"INSERT INTO tipos_permiso (
            nombre, descripcion, color, requiere_aprobacion, requiere_documento, 
            dias_por_defecto, dias_maximos, es_compensable, activo, fecha_creacion,
            tipo_resolucion_por_defecto, dias_limite_documento, dias_limite_compensacion,
            horas_compensar_por_dia, genera_descuento, porcentaje_descuento)
        VALUES (
            @Nombre, @Descripcion, @Color, @RequiereAprobacion, @RequiereDocumento, 
            @DiasPorDefecto, @DiasMaximos, @EsCompensable, @Activo, @FechaCreacion,
            @TipoResolucionPorDefecto, @DiasLimiteDocumento, @DiasLimiteCompensacion,
            @HorasCompensarPorDia, @GeneraDescuento, @PorcentajeDescuento);
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
    fecha_modificacion = @FechaModificacion,
    tipo_resolucion_por_defecto = @TipoResolucionPorDefecto,
    dias_limite_documento = @DiasLimiteDocumento,
    dias_limite_compensacion = @DiasLimiteCompensacion,
    horas_compensar_por_dia = @HorasCompensarPorDia,
    genera_descuento = @GeneraDescuento,
    porcentaje_descuento = @PorcentajeDescuento
WHERE id = @Id";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(int id)
    {
        // Hard delete - elimina permanentemente el registro
        const string sql = "DELETE FROM tipos_permiso WHERE id = @Id";
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
