using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Infrastructure.Data;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Repositories;

public class CompensacionHorasRepository : ICompensacionHorasRepository
{
    private readonly DapperContext _context;
    private readonly ILogger<CompensacionHorasRepository> _logger;

    public CompensacionHorasRepository(DapperContext context, ILogger<CompensacionHorasRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CompensacionHoras> AddAsync(CompensacionHoras entity)
    {
        entity.FechaCreacion = DateTime.Now;
        const string sql = @"
            INSERT INTO compensaciones_horas (
                permiso_id, fecha_compensacion, horas_compensadas, descripcion, 
                aprobado_por_id, fecha_aprobacion, activo, fecha_creacion)
            VALUES (
                @PermisoId, @FechaCompensacion, @HorasCompensadas, @Descripcion, 
                @AprobadoPorId, @FechaAprobacion, @Activo, @FechaCreacion);
            SELECT last_insert_rowid();";

        using var connection = _context.CreateConnection();
        entity.Id = await connection.ExecuteScalarAsync<int>(sql, entity);
        return entity;
    }

    public async Task UpdateAsync(CompensacionHoras entity)
    {
        entity.FechaModificacion = DateTime.Now;
        const string sql = @"
            UPDATE compensaciones_horas
            SET permiso_id = @PermisoId,
                fecha_compensacion = @FechaCompensacion,
                horas_compensadas = @HorasCompensadas,
                descripcion = @Descripcion,
                aprobado_por_id = @AprobadoPorId,
                fecha_aprobacion = @FechaAprobacion,
                fecha_modificacion = @FechaModificacion
            WHERE id = @Id";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "UPDATE compensaciones_horas SET activo = 0, fecha_modificacion = @FechaMod WHERE id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id, FechaMod = DateTime.Now });
    }

    public async Task<CompensacionHoras?> GetByIdAsync(int id)
    {
        const string sql = "SELECT * FROM compensaciones_horas WHERE id = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<CompensacionHoras>(sql, new { Id = id });
    }

    public async Task<IEnumerable<CompensacionHoras>> GetAllAsync()
    {
        const string sql = "SELECT * FROM compensaciones_horas ORDER BY fecha_compensacion DESC";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<CompensacionHoras>(sql);
    }

    public async Task<IEnumerable<CompensacionHoras>> GetAllActiveAsync()
    {
        const string sql = "SELECT * FROM compensaciones_horas WHERE activo = 1 ORDER BY fecha_compensacion DESC";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<CompensacionHoras>(sql);
    }

    public async Task<IEnumerable<CompensacionHoras>> GetByPermisoIdAsync(int permisoId)
    {
        const string sql = @"
            SELECT * FROM compensaciones_horas 
            WHERE permiso_id = @PermisoId AND activo = 1 
            ORDER BY fecha_compensacion DESC";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<CompensacionHoras>(sql, new { PermisoId = permisoId });
    }

    public async Task<IEnumerable<CompensacionHoras>> GetByPermisoIdWithAprobadorAsync(int permisoId)
    {
        const string sql = @"
            SELECT c.*, u.nombre as AprobadorNombre
            FROM compensaciones_horas c
            LEFT JOIN usuarios u ON c.aprobado_por_id = u.id
            WHERE c.permiso_id = @PermisoId AND c.activo = 1 
            ORDER BY c.fecha_compensacion DESC";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<CompensacionHoras>(sql, new { PermisoId = permisoId });
    }

    public async Task<int> GetTotalHorasCompensadasAsync(int permisoId)
    {
        const string sql = @"
            SELECT COALESCE(SUM(horas_compensadas), 0) 
            FROM compensaciones_horas 
            WHERE permiso_id = @PermisoId AND activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, new { PermisoId = permisoId });
    }

    public async Task RegistrarCompensacionAsync(int permisoId, DateTime fecha, int horas, 
        string descripcion, int aprobadoPorId)
    {
        var compensacion = new CompensacionHoras
        {
            PermisoId = permisoId,
            FechaCompensacion = fecha,
            HorasCompensadas = horas,
            Descripcion = descripcion,
            AprobadoPorId = aprobadoPorId,
            FechaAprobacion = DateTime.Now,
            Activo = true,
            FechaCreacion = DateTime.Now
        };

        await AddAsync(compensacion);
        _logger.LogInformation("Compensación registrada: Permiso {PermisoId}, {Horas} horas, Aprobado por {AprobadorId}", 
            permisoId, horas, aprobadoPorId);
    }

    public async Task<IEnumerable<CompensacionHoras>> GetPendientesAprobacionAsync()
    {
        const string sql = @"
            SELECT * FROM compensaciones_horas 
            WHERE aprobado_por_id IS NULL AND activo = 1 
            ORDER BY fecha_compensacion ASC";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<CompensacionHoras>(sql);
    }

    public Task<int> SaveChangesAsync() => Task.FromResult(0);
}
