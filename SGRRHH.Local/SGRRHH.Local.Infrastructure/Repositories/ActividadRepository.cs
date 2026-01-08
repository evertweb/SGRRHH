using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Repositories;

public class ActividadRepository : IActividadRepository
{
    private readonly Data.DapperContext _context;
    private readonly ILogger<ActividadRepository> _logger;

    public ActividadRepository(Data.DapperContext context, ILogger<ActividadRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Actividad> AddAsync(Actividad entity)
    {
        entity.FechaCreacion = DateTime.Now;
        const string sql = @"INSERT INTO actividades (codigo, nombre, descripcion, categoria, requiere_proyecto, orden, activo, fecha_creacion)
VALUES (@Codigo, @Nombre, @Descripcion, @Categoria, @RequiereProyecto, @Orden, 1, @FechaCreacion);
SELECT last_insert_rowid();";

        using var connection = _context.CreateConnection();
        entity.Id = await connection.ExecuteScalarAsync<int>(sql, entity);
        return entity;
    }

    public async Task UpdateAsync(Actividad entity)
    {
        entity.FechaModificacion = DateTime.Now;
        const string sql = @"UPDATE actividades
SET codigo = @Codigo,
    nombre = @Nombre,
    descripcion = @Descripcion,
    categoria = @Categoria,
    requiere_proyecto = @RequiereProyecto,
    orden = @Orden,
    fecha_modificacion = @FechaModificacion
WHERE id = @Id";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "UPDATE actividades SET activo = 0, fecha_modificacion = CURRENT_TIMESTAMP WHERE id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<Actividad?> GetByIdAsync(int id)
    {
        const string sql = "SELECT * FROM actividades WHERE id = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Actividad>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Actividad>> GetAllAsync()
    {
        const string sql = "SELECT * FROM actividades";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Actividad>(sql);
    }

    public async Task<IEnumerable<Actividad>> GetAllActiveAsync()
    {
        const string sql = "SELECT * FROM actividades WHERE activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Actividad>(sql);
    }

    public async Task<IEnumerable<Actividad>> GetByCategoriaAsync(string categoria)
    {
        const string sql = "SELECT * FROM actividades WHERE categoria = @Categoria AND activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Actividad>(sql, new { Categoria = categoria });
    }

    public async Task<IEnumerable<string>> GetCategoriasAsync()
    {
        const string sql = "SELECT DISTINCT categoria FROM actividades WHERE categoria IS NOT NULL AND categoria <> ''";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<string>(sql);
    }

    public async Task<IEnumerable<Actividad>> SearchAsync(string searchTerm)
    {
        var term = $"%{searchTerm}%";
        const string sql = @"SELECT * FROM actividades
WHERE lower(nombre) LIKE lower(@Term) OR lower(codigo) LIKE lower(@Term) OR lower(descripcion) LIKE lower(@Term)";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Actividad>(sql, new { Term = term });
    }

    public async Task<bool> ExistsCodigoAsync(string codigo, int? excludeId = null)
    {
        const string sql = "SELECT COUNT(1) FROM actividades WHERE codigo = @Codigo AND (@ExcludeId IS NULL OR id <> @ExcludeId)";
        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Codigo = codigo, ExcludeId = excludeId });
        return count > 0;
    }

    public async Task<string> GetNextCodigoAsync()
    {
        const string sql = "SELECT codigo FROM actividades ORDER BY id DESC LIMIT 1";
        using var connection = _context.CreateConnection();
        var last = await connection.QuerySingleOrDefaultAsync<string>(sql);
        if (string.IsNullOrWhiteSpace(last))
        {
            return "ACT-001";
        }

        var numeric = int.TryParse(last.Split('-').LastOrDefault(), out var number) ? number + 1 : 1;
        return $"ACT-{numeric:000}";
    }

    public Task<int> SaveChangesAsync() => Task.FromResult(0);
}
