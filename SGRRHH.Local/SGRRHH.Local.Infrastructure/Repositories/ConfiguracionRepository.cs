using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Repositories;

public class ConfiguracionRepository : IConfiguracionRepository
{
    private readonly Data.DapperContext _context;
    private readonly ILogger<ConfiguracionRepository> _logger;

    public ConfiguracionRepository(Data.DapperContext context, ILogger<ConfiguracionRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ConfiguracionSistema> AddAsync(ConfiguracionSistema entity)
    {
        entity.FechaCreacion = DateTime.Now;
        const string sql = @"INSERT INTO configuracion_sistema (clave, valor, descripcion, categoria, activo, fecha_creacion)
VALUES (@Clave, @Valor, @Descripcion, @Categoria, 1, @FechaCreacion);
SELECT last_insert_rowid();";

        using var connection = _context.CreateConnection();
        entity.Id = await connection.ExecuteScalarAsync<int>(sql, entity);
        return entity;
    }

    public async Task UpdateAsync(ConfiguracionSistema entity)
    {
        entity.FechaModificacion = DateTime.Now;
        const string sql = @"UPDATE configuracion_sistema
SET clave = @Clave,
    valor = @Valor,
    descripcion = @Descripcion,
    categoria = @Categoria,
    fecha_modificacion = @FechaModificacion
WHERE id = @Id";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "DELETE FROM configuracion_sistema WHERE id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<ConfiguracionSistema?> GetByIdAsync(int id)
    {
        const string sql = "SELECT * FROM configuracion_sistema WHERE id = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<ConfiguracionSistema>(sql, new { Id = id });
    }

    public async Task<IEnumerable<ConfiguracionSistema>> GetAllAsync()
    {
        const string sql = "SELECT * FROM configuracion_sistema";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<ConfiguracionSistema>(sql);
    }

    public async Task<IEnumerable<ConfiguracionSistema>> GetAllActiveAsync()
    {
        const string sql = "SELECT * FROM configuracion_sistema WHERE activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<ConfiguracionSistema>(sql);
    }

    public async Task<ConfiguracionSistema?> GetByClaveAsync(string clave)
    {
        const string sql = "SELECT * FROM configuracion_sistema WHERE clave = @Clave";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<ConfiguracionSistema>(sql, new { Clave = clave });
    }

    public async Task<List<ConfiguracionSistema>> GetByCategoriaAsync(string categoria)
    {
        const string sql = "SELECT * FROM configuracion_sistema WHERE categoria = @Categoria";
        using var connection = _context.CreateConnection();
        var result = await connection.QueryAsync<ConfiguracionSistema>(sql, new { Categoria = categoria });
        return result.ToList();
    }

    public async Task<bool> ExistsClaveAsync(string clave)
    {
        const string sql = "SELECT COUNT(1) FROM configuracion_sistema WHERE clave = @Clave";
        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Clave = clave });
        return count > 0;
    }

    public Task<int> SaveChangesAsync() => Task.FromResult(0);
}
