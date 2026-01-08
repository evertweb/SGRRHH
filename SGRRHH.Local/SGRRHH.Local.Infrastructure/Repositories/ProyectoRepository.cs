using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Repositories;

public class ProyectoRepository : IProyectoRepository
{
    private readonly Data.DapperContext _context;
    private readonly ILogger<ProyectoRepository> _logger;

    public ProyectoRepository(Data.DapperContext context, ILogger<ProyectoRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Proyecto> AddAsync(Proyecto entity)
    {
        entity.FechaCreacion = DateTime.Now;
        const string sql = @"INSERT INTO proyectos (codigo, nombre, descripcion, cliente, ubicacion, presupuesto, progreso, fecha_inicio, fecha_fin, estado, responsable_id, activo, fecha_creacion)
VALUES (@Codigo, @Nombre, @Descripcion, @Cliente, @Ubicacion, @Presupuesto, @Progreso, @FechaInicio, @FechaFin, @Estado, @ResponsableId, 1, @FechaCreacion);
SELECT last_insert_rowid();";

        using var connection = _context.CreateConnection();
        entity.Id = await connection.ExecuteScalarAsync<int>(sql, entity);
        return entity;
    }

    public async Task UpdateAsync(Proyecto entity)
    {
        entity.FechaModificacion = DateTime.Now;
        const string sql = @"UPDATE proyectos
SET codigo = @Codigo,
    nombre = @Nombre,
    descripcion = @Descripcion,
    cliente = @Cliente,
    ubicacion = @Ubicacion,
    presupuesto = @Presupuesto,
    progreso = @Progreso,
    fecha_inicio = @FechaInicio,
    fecha_fin = @FechaFin,
    estado = @Estado,
    responsable_id = @ResponsableId,
    fecha_modificacion = @FechaModificacion
WHERE id = @Id";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "UPDATE proyectos SET activo = 0, fecha_modificacion = CURRENT_TIMESTAMP WHERE id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<Proyecto?> GetByIdAsync(int id)
    {
        const string sql = "SELECT * FROM proyectos WHERE id = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Proyecto>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Proyecto>> GetAllAsync()
    {
        const string sql = "SELECT * FROM proyectos";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Proyecto>(sql);
    }

    public async Task<IEnumerable<Proyecto>> GetAllActiveAsync()
    {
        const string sql = "SELECT * FROM proyectos WHERE activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Proyecto>(sql);
    }

    public async Task<IEnumerable<Proyecto>> GetByEstadoAsync(EstadoProyecto estado)
    {
        const string sql = "SELECT * FROM proyectos WHERE estado = @Estado";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Proyecto>(sql, new { Estado = estado });
    }

    public async Task<IEnumerable<Proyecto>> SearchAsync(string searchTerm)
    {
        return await SearchAsync(searchTerm, null);
    }

    public async Task<IEnumerable<Proyecto>> SearchAsync(string? searchTerm, EstadoProyecto? estado)
    {
        var sql = "SELECT * FROM proyectos WHERE 1=1";
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            sql += " AND (lower(nombre) LIKE lower(@Term) OR lower(codigo) LIKE lower(@Term) OR lower(descripcion) LIKE lower(@Term))";
        }
        if (estado.HasValue)
        {
            sql += " AND estado = @Estado";
        }

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Proyecto>(sql, new { Term = $"%{searchTerm}%", Estado = estado });
    }

    public async Task<bool> ExistsCodigoAsync(string codigo, int? excludeId = null)
    {
        const string sql = "SELECT COUNT(1) FROM proyectos WHERE codigo = @Codigo AND (@ExcludeId IS NULL OR id <> @ExcludeId)";
        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Codigo = codigo, ExcludeId = excludeId });
        return count > 0;
    }

    public async Task<string> GetNextCodigoAsync()
    {
        const string sql = "SELECT codigo FROM proyectos ORDER BY id DESC LIMIT 1";
        using var connection = _context.CreateConnection();
        var last = await connection.QuerySingleOrDefaultAsync<string>(sql);
        if (string.IsNullOrWhiteSpace(last))
        {
            return "PROY-001";
        }

        var numeric = int.TryParse(last.Split('-').LastOrDefault(), out var number) ? number + 1 : 1;
        return $"PROY-{numeric:000}";
    }

    public async Task<IEnumerable<Proyecto>> GetProximosAVencerAsync(int diasAnticipacion = 7)
    {
        const string sql = @"SELECT * FROM proyectos
WHERE fecha_fin IS NOT NULL
  AND fecha_fin <= date('now', '+' || @Dias || ' day')
  AND fecha_fin >= date('now')
  AND estado = @Estado";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Proyecto>(sql, new { Dias = diasAnticipacion, Estado = EstadoProyecto.Activo });
    }

    public async Task<IEnumerable<Proyecto>> GetVencidosAsync()
    {
        const string sql = "SELECT * FROM proyectos WHERE fecha_fin < date('now') AND estado = @Estado";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Proyecto>(sql, new { Estado = EstadoProyecto.Activo });
    }

    public async Task<IEnumerable<Proyecto>> GetByResponsableAsync(int empleadoId)
    {
        const string sql = "SELECT * FROM proyectos WHERE responsable_id = @EmpleadoId";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Proyecto>(sql, new { EmpleadoId = empleadoId });
    }

    public async Task<ProyectoEstadisticas> GetEstadisticasAsync()
    {
        const string sql = @"SELECT
    COUNT(1) AS TotalProyectos,
    SUM(CASE WHEN estado = 0 THEN 1 ELSE 0 END) AS Activos,
    SUM(CASE WHEN estado = 1 THEN 1 ELSE 0 END) AS Suspendidos,
    SUM(CASE WHEN estado = 2 THEN 1 ELSE 0 END) AS Finalizados,
    SUM(CASE WHEN estado = 3 THEN 1 ELSE 0 END) AS Cancelados,
    SUM(CASE WHEN estado = 0 AND fecha_fin IS NOT NULL AND fecha_fin <= date('now', '+7 day') AND fecha_fin >= date('now') THEN 1 ELSE 0 END) AS ProximosAVencer,
    SUM(CASE WHEN estado = 0 AND fecha_fin IS NOT NULL AND fecha_fin < date('now') THEN 1 ELSE 0 END) AS Vencidos,
    SUM(COALESCE(presupuesto, 0)) AS PresupuestoTotal
FROM proyectos";

        using var connection = _context.CreateConnection();
        return await connection.QuerySingleAsync<ProyectoEstadisticas>(sql);
    }

    public Task<int> SaveChangesAsync() => Task.FromResult(0);
}
