using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Infrastructure.Data;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Repositories;

/// <summary>
/// Repositorio para Entidades Promotoras de Salud (EPS)
/// </summary>
public class EpsRepository : IEpsRepository
{
    private readonly DapperContext _context;
    private readonly ILogger<EpsRepository> _logger;

    public EpsRepository(DapperContext context, ILogger<EpsRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Eps>> GetVigentesAsync()
    {
        using var connection = _context.CreateConnection();
        const string sql = "SELECT * FROM eps_colombia WHERE activo = 1 ORDER BY nombre";
        var result = await connection.QueryAsync<Eps>(sql);
        return result.ToList();
    }

    public async Task<Eps?> GetByCodigoAsync(string codigo)
    {
        using var connection = _context.CreateConnection();
        const string sql = "SELECT * FROM eps_colombia WHERE codigo = @Codigo AND activo = 1";
        return await connection.QueryFirstOrDefaultAsync<Eps>(sql, new { Codigo = codigo });
    }

    public async Task<Eps?> GetByIdAsync(int id)
    {
        using var connection = _context.CreateConnection();
        const string sql = "SELECT * FROM eps_colombia WHERE id = @Id";
        return await connection.QueryFirstOrDefaultAsync<Eps>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Eps>> GetAllAsync()
    {
        using var connection = _context.CreateConnection();
        const string sql = "SELECT * FROM eps_colombia ORDER BY nombre";
        return await connection.QueryAsync<Eps>(sql);
    }

    public async Task<IEnumerable<Eps>> GetAllActiveAsync()
    {
        return await GetVigentesAsync();
    }

    public async Task<Eps> AddAsync(Eps entity)
    {
        entity.FechaCreacion = DateTime.Now;
        using var connection = _context.CreateConnection();
        const string sql = @"
            INSERT INTO eps_colombia (codigo, nombre, activo, fecha_creacion)
            VALUES (@Codigo, @Nombre, 1, @FechaCreacion);
            SELECT last_insert_rowid();";
        entity.Id = await connection.ExecuteScalarAsync<int>(sql, entity);
        return entity;
    }

    public async Task UpdateAsync(Eps entity)
    {
        using var connection = _context.CreateConnection();
        const string sql = @"
            UPDATE eps_colombia SET 
                codigo = @Codigo,
                nombre = @Nombre
            WHERE id = @Id";
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = _context.CreateConnection();
        const string sql = "UPDATE eps_colombia SET activo = 0 WHERE id = @Id";
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public Task<int> SaveChangesAsync() => Task.FromResult(0);
}
