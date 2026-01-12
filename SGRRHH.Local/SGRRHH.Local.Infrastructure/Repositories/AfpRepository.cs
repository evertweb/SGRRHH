using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Infrastructure.Data;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Repositories;

/// <summary>
/// Repositorio para Administradoras de Fondos de Pensiones (AFP)
/// </summary>
public class AfpRepository : IAfpRepository
{
    private readonly DapperContext _context;
    private readonly ILogger<AfpRepository> _logger;

    public AfpRepository(DapperContext context, ILogger<AfpRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Afp>> GetVigentesAsync()
    {
        using var connection = _context.CreateConnection();
        const string sql = "SELECT * FROM afp_colombia WHERE activo = 1 ORDER BY nombre";
        var result = await connection.QueryAsync<Afp>(sql);
        return result.ToList();
    }

    public async Task<Afp?> GetByCodigoAsync(string codigo)
    {
        using var connection = _context.CreateConnection();
        const string sql = "SELECT * FROM afp_colombia WHERE codigo = @Codigo AND activo = 1";
        return await connection.QueryFirstOrDefaultAsync<Afp>(sql, new { Codigo = codigo });
    }

    public async Task<Afp?> GetByIdAsync(int id)
    {
        using var connection = _context.CreateConnection();
        const string sql = "SELECT * FROM afp_colombia WHERE id = @Id";
        return await connection.QueryFirstOrDefaultAsync<Afp>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Afp>> GetAllAsync()
    {
        using var connection = _context.CreateConnection();
        const string sql = "SELECT * FROM afp_colombia ORDER BY nombre";
        return await connection.QueryAsync<Afp>(sql);
    }

    public async Task<IEnumerable<Afp>> GetAllActiveAsync()
    {
        return await GetVigentesAsync();
    }

    public async Task<Afp> AddAsync(Afp entity)
    {
        entity.FechaCreacion = DateTime.Now;
        using var connection = _context.CreateConnection();
        const string sql = @"
            INSERT INTO afp_colombia (codigo, nombre, activo, fecha_creacion)
            VALUES (@Codigo, @Nombre, 1, @FechaCreacion);
            SELECT last_insert_rowid();";
        entity.Id = await connection.ExecuteScalarAsync<int>(sql, entity);
        return entity;
    }

    public async Task UpdateAsync(Afp entity)
    {
        using var connection = _context.CreateConnection();
        const string sql = @"
            UPDATE afp_colombia SET 
                codigo = @Codigo,
                nombre = @Nombre
            WHERE id = @Id";
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = _context.CreateConnection();
        const string sql = "UPDATE afp_colombia SET activo = 0 WHERE id = @Id";
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public Task<int> SaveChangesAsync() => Task.FromResult(0);
}
