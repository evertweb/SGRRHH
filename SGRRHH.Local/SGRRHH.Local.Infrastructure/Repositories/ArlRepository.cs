using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Infrastructure.Data;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Repositories;

/// <summary>
/// Repositorio para Administradoras de Riesgos Laborales (ARL)
/// </summary>
public class ArlRepository : IArlRepository
{
    private readonly DapperContext _context;
    private readonly ILogger<ArlRepository> _logger;

    public ArlRepository(DapperContext context, ILogger<ArlRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Arl>> GetVigentesAsync()
    {
        using var connection = _context.CreateConnection();
        const string sql = "SELECT * FROM arl_colombia WHERE activo = 1 ORDER BY nombre";
        var result = await connection.QueryAsync<Arl>(sql);
        return result.ToList();
    }

    public async Task<Arl?> GetByCodigoAsync(string codigo)
    {
        using var connection = _context.CreateConnection();
        const string sql = "SELECT * FROM arl_colombia WHERE codigo = @Codigo AND activo = 1";
        return await connection.QueryFirstOrDefaultAsync<Arl>(sql, new { Codigo = codigo });
    }

    public async Task<Arl?> GetByIdAsync(int id)
    {
        using var connection = _context.CreateConnection();
        const string sql = "SELECT * FROM arl_colombia WHERE id = @Id";
        return await connection.QueryFirstOrDefaultAsync<Arl>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Arl>> GetAllAsync()
    {
        using var connection = _context.CreateConnection();
        const string sql = "SELECT * FROM arl_colombia ORDER BY nombre";
        return await connection.QueryAsync<Arl>(sql);
    }

    public async Task<IEnumerable<Arl>> GetAllActiveAsync()
    {
        return await GetVigentesAsync();
    }

    public async Task<Arl> AddAsync(Arl entity)
    {
        entity.FechaCreacion = DateTime.Now;
        using var connection = _context.CreateConnection();
        const string sql = @"
            INSERT INTO arl_colombia (codigo, nombre, activo, fecha_creacion)
            VALUES (@Codigo, @Nombre, 1, @FechaCreacion);
            SELECT last_insert_rowid();";
        entity.Id = await connection.ExecuteScalarAsync<int>(sql, entity);
        return entity;
    }

    public async Task UpdateAsync(Arl entity)
    {
        using var connection = _context.CreateConnection();
        const string sql = @"
            UPDATE arl_colombia SET 
                codigo = @Codigo,
                nombre = @Nombre
            WHERE id = @Id";
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = _context.CreateConnection();
        const string sql = "UPDATE arl_colombia SET activo = 0 WHERE id = @Id";
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public Task<int> SaveChangesAsync() => Task.FromResult(0);
}
