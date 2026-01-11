using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de especies forestales
/// </summary>
public class EspecieForestalRepository : IEspecieForestalRepository
{
    private readonly Data.DapperContext _context;
    private readonly ILogger<EspecieForestalRepository> _logger;

    public EspecieForestalRepository(Data.DapperContext context, ILogger<EspecieForestalRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<EspecieForestal> AddAsync(EspecieForestal entity)
    {
        entity.FechaCreacion = DateTime.Now;
        const string sql = @"
            INSERT INTO especies_forestales (codigo, nombre_comun, nombre_cientifico, familia, turno_promedio, densidad_recomendada, observaciones, activo, fecha_creacion)
            VALUES (@Codigo, @NombreComun, @NombreCientifico, @Familia, @TurnoPromedio, @DensidadRecomendada, @Observaciones, 1, @FechaCreacion);
            SELECT last_insert_rowid();";

        using var connection = _context.CreateConnection();
        entity.Id = await connection.ExecuteScalarAsync<int>(sql, entity);
        return entity;
    }

    public async Task UpdateAsync(EspecieForestal entity)
    {
        entity.FechaModificacion = DateTime.Now;
        const string sql = @"
            UPDATE especies_forestales SET
                codigo = @Codigo,
                nombre_comun = @NombreComun,
                nombre_cientifico = @NombreCientifico,
                familia = @Familia,
                turno_promedio = @TurnoPromedio,
                densidad_recomendada = @DensidadRecomendada,
                observaciones = @Observaciones,
                fecha_modificacion = @FechaModificacion
            WHERE id = @Id";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(int id)
    {
        // Soft delete
        const string sql = "UPDATE especies_forestales SET activo = 0, fecha_modificacion = @FechaMod WHERE id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id, FechaMod = DateTime.Now });
    }

    public async Task<EspecieForestal?> GetByIdAsync(int id)
    {
        const string sql = "SELECT * FROM especies_forestales WHERE id = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<EspecieForestal>(sql, new { Id = id });
    }

    public async Task<IEnumerable<EspecieForestal>> GetAllAsync()
    {
        const string sql = "SELECT * FROM especies_forestales ORDER BY nombre_comun";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<EspecieForestal>(sql);
    }

    public async Task<IEnumerable<EspecieForestal>> GetAllActiveAsync()
    {
        const string sql = "SELECT * FROM especies_forestales WHERE activo = 1 ORDER BY nombre_comun";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<EspecieForestal>(sql);
    }

    public async Task<IEnumerable<EspecieForestal>> SearchAsync(string searchTerm)
    {
        const string sql = @"
            SELECT * FROM especies_forestales 
            WHERE activo = 1 AND (
                lower(codigo) LIKE lower(@Term) OR 
                lower(nombre_comun) LIKE lower(@Term) OR 
                lower(nombre_cientifico) LIKE lower(@Term) OR
                lower(familia) LIKE lower(@Term)
            )
            ORDER BY nombre_comun";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<EspecieForestal>(sql, new { Term = $"%{searchTerm}%" });
    }

    public async Task<bool> ExistsCodigoAsync(string codigo, int? excludeId = null)
    {
        const string sql = "SELECT COUNT(1) FROM especies_forestales WHERE codigo = @Codigo AND (@ExcludeId IS NULL OR id <> @ExcludeId)";
        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Codigo = codigo, ExcludeId = excludeId });
        return count > 0;
    }

    public async Task<EspecieForestal?> GetByCodigoAsync(string codigo)
    {
        const string sql = "SELECT * FROM especies_forestales WHERE codigo = @Codigo";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<EspecieForestal>(sql, new { Codigo = codigo });
    }

    public async Task<IEnumerable<EspecieForestal>> GetMasUtilizadasAsync(int top = 10)
    {
        const string sql = @"
            SELECT ef.*, COUNT(p.id) as cantidad
            FROM especies_forestales ef
            LEFT JOIN proyectos p ON p.especie_id = ef.id
            WHERE ef.activo = 1
            GROUP BY ef.id
            ORDER BY cantidad DESC, ef.nombre_comun
            LIMIT @Top";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<EspecieForestal>(sql, new { Top = top });
    }

    public Task<int> SaveChangesAsync() => Task.FromResult(0);
}
