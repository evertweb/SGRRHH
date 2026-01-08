using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
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
        const string sql = @"INSERT INTO proyectos_empleados (proyecto_id, empleado_id, fecha_asignacion, fecha_desasignacion, rol, observaciones, activo, fecha_creacion)
VALUES (@ProyectoId, @EmpleadoId, @FechaAsignacion, @FechaDesasignacion, @Rol, @Observaciones, 1, @FechaCreacion);
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
    observaciones = @Observaciones,
    fecha_modificacion = @FechaModificacion
WHERE id = @Id";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "UPDATE proyectos_empleados SET activo = 0, fecha_modificacion = CURRENT_TIMESTAMP WHERE id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<ProyectoEmpleado?> GetByIdAsync(int id)
    {
        const string sql = "SELECT * FROM proyectos_empleados WHERE id = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<ProyectoEmpleado>(sql, new { Id = id });
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
        const string sql = "SELECT * FROM proyectos_empleados WHERE proyecto_id = @ProyectoId AND activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<ProyectoEmpleado>(sql, new { ProyectoId = proyectoId });
    }

    public async Task<IEnumerable<ProyectoEmpleado>> GetByEmpleadoAsync(int empleadoId)
    {
        const string sql = "SELECT * FROM proyectos_empleados WHERE empleado_id = @EmpleadoId";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<ProyectoEmpleado>(sql, new { EmpleadoId = empleadoId });
    }

    public async Task<IEnumerable<ProyectoEmpleado>> GetActiveByEmpleadoAsync(int empleadoId)
    {
        const string sql = "SELECT * FROM proyectos_empleados WHERE empleado_id = @EmpleadoId AND activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<ProyectoEmpleado>(sql, new { EmpleadoId = empleadoId });
    }

    public async Task<bool> ExistsAsignacionAsync(int proyectoId, int empleadoId)
    {
        const string sql = "SELECT COUNT(1) FROM proyectos_empleados WHERE proyecto_id = @ProyectoId AND empleado_id = @EmpleadoId AND activo = 1";
        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { ProyectoId = proyectoId, EmpleadoId = empleadoId });
        return count > 0;
    }

    public async Task<ProyectoEmpleado?> GetAsignacionAsync(int proyectoId, int empleadoId)
    {
        const string sql = "SELECT * FROM proyectos_empleados WHERE proyecto_id = @ProyectoId AND empleado_id = @EmpleadoId";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<ProyectoEmpleado>(sql, new { ProyectoId = proyectoId, EmpleadoId = empleadoId });
    }

    public async Task DesasignarAsync(int proyectoId, int empleadoId)
    {
        const string sql = @"UPDATE proyectos_empleados
SET activo = 0,
    fecha_desasignacion = @FechaDesasignacion,
    fecha_modificacion = CURRENT_TIMESTAMP
WHERE proyecto_id = @ProyectoId AND empleado_id = @EmpleadoId";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { ProyectoId = proyectoId, EmpleadoId = empleadoId, FechaDesasignacion = DateTime.Now });
    }

    public async Task<int> GetCountByProyectoAsync(int proyectoId)
    {
        const string sql = "SELECT COUNT(1) FROM proyectos_empleados WHERE proyecto_id = @ProyectoId AND activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, new { ProyectoId = proyectoId });
    }

    public Task<int> SaveChangesAsync() => Task.FromResult(0);
}
