using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Repositories;

public class DetalleActividadRepository : IDetalleActividadRepository
{
    private readonly Data.DapperContext _context;
    private readonly ILogger<DetalleActividadRepository> _logger;

    public DetalleActividadRepository(Data.DapperContext context, ILogger<DetalleActividadRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DetalleActividad> AddAsync(DetalleActividad entity)
    {
        entity.FechaCreacion = DateTime.Now;
        const string sql = @"INSERT INTO detalles_actividad (registro_diario_id, actividad_id, proyecto_id, descripcion, horas, hora_inicio, hora_fin, orden, activo, fecha_creacion)
VALUES (@RegistroDiarioId, @ActividadId, @ProyectoId, @Descripcion, @Horas, @HoraInicio, @HoraFin, @Orden, 1, @FechaCreacion);
SELECT last_insert_rowid();";

        using var connection = _context.CreateConnection();
        entity.Id = await connection.ExecuteScalarAsync<int>(sql, entity);
        return entity;
    }

    public async Task UpdateAsync(DetalleActividad entity)
    {
        entity.FechaModificacion = DateTime.Now;
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
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(int id)
    {
        // Hard delete - elimina permanentemente el registro
        const string sql = "DELETE FROM detalles_actividad WHERE id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<DetalleActividad?> GetByIdAsync(int id)
    {
        const string sql = "SELECT * FROM detalles_actividad WHERE id = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<DetalleActividad>(sql, new { Id = id });
    }

    public async Task<IEnumerable<DetalleActividad>> GetAllAsync()
    {
        const string sql = "SELECT * FROM detalles_actividad";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<DetalleActividad>(sql);
    }

    public async Task<IEnumerable<DetalleActividad>> GetAllActiveAsync()
    {
        const string sql = "SELECT * FROM detalles_actividad WHERE activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<DetalleActividad>(sql);
    }

    public async Task<IEnumerable<DetalleActividad>> GetByRegistroAsync(int registroDiarioId)
    {
        const string sql = "SELECT * FROM detalles_actividad WHERE registro_diario_id = @RegistroId AND activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<DetalleActividad>(sql, new { RegistroId = registroDiarioId });
    }

    public async Task<IEnumerable<DetalleActividad>> GetByRegistroIdsAsync(IEnumerable<int> registroIds)
    {
        var ids = registroIds?.Distinct().ToList() ?? new List<int>();
        if (!ids.Any())
        {
            return new List<DetalleActividad>();
        }

        const string sql = "SELECT * FROM detalles_actividad WHERE registro_diario_id IN @RegistroIds AND activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<DetalleActividad>(sql, new { RegistroIds = ids });
    }

    public async Task<IEnumerable<DetalleActividad>> GetByProyectoAsync(int proyectoId)
    {
        const string sql = "SELECT * FROM detalles_actividad WHERE proyecto_id = @ProyectoId AND activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<DetalleActividad>(sql, new { ProyectoId = proyectoId });
    }

    public Task<int> SaveChangesAsync() => Task.FromResult(0);
}
