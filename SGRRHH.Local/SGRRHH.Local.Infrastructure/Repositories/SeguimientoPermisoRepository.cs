using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Infrastructure.Data;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Repositories;

public class SeguimientoPermisoRepository : ISeguimientoPermisoRepository
{
    private readonly DapperContext _context;
    private readonly ILogger<SeguimientoPermisoRepository> _logger;

    public SeguimientoPermisoRepository(DapperContext context, ILogger<SeguimientoPermisoRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<SeguimientoPermiso> AddAsync(SeguimientoPermiso entity)
    {
        entity.FechaCreacion = DateTime.Now;
        const string sql = @"
            INSERT INTO seguimiento_permisos (
                permiso_id, fecha_accion, tipo_accion, descripcion, 
                realizado_por_id, datos_adicionales, activo, fecha_creacion)
            VALUES (
                @PermisoId, @FechaAccion, @TipoAccion, @Descripcion, 
                @RealizadoPorId, @DatosAdicionales, @Activo, @FechaCreacion);
            SELECT last_insert_rowid();";

        using var connection = _context.CreateConnection();
        entity.Id = await connection.ExecuteScalarAsync<int>(sql, entity);
        return entity;
    }

    public async Task UpdateAsync(SeguimientoPermiso entity)
    {
        entity.FechaModificacion = DateTime.Now;
        const string sql = @"
            UPDATE seguimiento_permisos
            SET permiso_id = @PermisoId,
                fecha_accion = @FechaAccion,
                tipo_accion = @TipoAccion,
                descripcion = @Descripcion,
                realizado_por_id = @RealizadoPorId,
                datos_adicionales = @DatosAdicionales,
                fecha_modificacion = @FechaModificacion
            WHERE id = @Id";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "UPDATE seguimiento_permisos SET activo = 0, fecha_modificacion = @FechaMod WHERE id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id, FechaMod = DateTime.Now });
    }

    public async Task<SeguimientoPermiso?> GetByIdAsync(int id)
    {
        const string sql = "SELECT * FROM seguimiento_permisos WHERE id = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<SeguimientoPermiso>(sql, new { Id = id });
    }

    public async Task<IEnumerable<SeguimientoPermiso>> GetAllAsync()
    {
        const string sql = "SELECT * FROM seguimiento_permisos ORDER BY fecha_accion DESC";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<SeguimientoPermiso>(sql);
    }

    public async Task<IEnumerable<SeguimientoPermiso>> GetAllActiveAsync()
    {
        const string sql = "SELECT * FROM seguimiento_permisos WHERE activo = 1 ORDER BY fecha_accion DESC";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<SeguimientoPermiso>(sql);
    }

    public async Task<IEnumerable<SeguimientoPermiso>> GetByPermisoIdAsync(int permisoId)
    {
        const string sql = @"
            SELECT * FROM seguimiento_permisos 
            WHERE permiso_id = @PermisoId AND activo = 1 
            ORDER BY fecha_accion DESC";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<SeguimientoPermiso>(sql, new { PermisoId = permisoId });
    }

    public async Task<IEnumerable<SeguimientoPermiso>> GetByPermisoIdWithUsuarioAsync(int permisoId)
    {
        const string sql = @"
            SELECT s.*, u.nombre as RealizadoPorNombre
            FROM seguimiento_permisos s
            LEFT JOIN usuarios u ON s.realizado_por_id = u.id
            WHERE s.permiso_id = @PermisoId AND s.activo = 1 
            ORDER BY s.fecha_accion DESC";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<SeguimientoPermiso>(sql, new { PermisoId = permisoId });
    }

    public async Task RegistrarAccionAsync(int permisoId, TipoAccionSeguimiento tipoAccion, 
        string descripcion, int usuarioId, string? datosAdicionales = null)
    {
        var seguimiento = new SeguimientoPermiso
        {
            PermisoId = permisoId,
            FechaAccion = DateTime.Now,
            TipoAccion = tipoAccion,
            Descripcion = descripcion,
            RealizadoPorId = usuarioId,
            DatosAdicionales = datosAdicionales,
            Activo = true,
            FechaCreacion = DateTime.Now
        };

        await AddAsync(seguimiento);
        _logger.LogInformation("Seguimiento registrado: Permiso {PermisoId}, Acción {TipoAccion}, Usuario {UsuarioId}", 
            permisoId, tipoAccion, usuarioId);
    }

    public async Task<IEnumerable<SeguimientoPermiso>> GetByTipoAccionAsync(TipoAccionSeguimiento tipoAccion)
    {
        const string sql = @"
            SELECT * FROM seguimiento_permisos 
            WHERE tipo_accion = @TipoAccion AND activo = 1 
            ORDER BY fecha_accion DESC";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<SeguimientoPermiso>(sql, new { TipoAccion = tipoAccion });
    }

    public async Task<IEnumerable<SeguimientoPermiso>> GetRecientesAsync(int cantidad = 50)
    {
        const string sql = @"
            SELECT s.*, p.numero_acta as PermisoNumeroActa
            FROM seguimiento_permisos s
            LEFT JOIN permisos p ON s.permiso_id = p.id
            WHERE s.activo = 1 
            ORDER BY s.fecha_accion DESC
            LIMIT @Cantidad";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<SeguimientoPermiso>(sql, new { Cantidad = cantidad });
    }

    public Task<int> SaveChangesAsync() => Task.FromResult(0);
}
