using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Infrastructure.Data;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Repositories;

/// <summary>
/// Repositorio para Cajas de Compensación Familiar
/// </summary>
public class CajaCompensacionRepository : ICajaCompensacionRepository
{
    private readonly DapperContext _context;
    private readonly ILogger<CajaCompensacionRepository> _logger;

    public CajaCompensacionRepository(DapperContext context, ILogger<CajaCompensacionRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<CajaCompensacion>> GetVigentesAsync()
    {
        using var connection = _context.CreateConnection();
        const string sql = "SELECT * FROM cajas_compensacion WHERE activo = 1 ORDER BY nombre";
        var result = await connection.QueryAsync<CajaCompensacion>(sql);
        return result.ToList();
    }

    public async Task<List<CajaCompensacion>> GetByDepartamentoAsync(string departamento)
    {
        using var connection = _context.CreateConnection();
        const string sql = @"
            SELECT * FROM cajas_compensacion 
            WHERE (UPPER(region) = UPPER(@Departamento) OR region IS NULL)
                AND activo = 1 
            ORDER BY nombre";
        var result = await connection.QueryAsync<CajaCompensacion>(sql, new { Departamento = departamento });
        return result.ToList();
    }

    public async Task<CajaCompensacion?> GetByCodigoAsync(string codigo)
    {
        using var connection = _context.CreateConnection();
        const string sql = "SELECT * FROM cajas_compensacion WHERE codigo = @Codigo AND activo = 1";
        return await connection.QueryFirstOrDefaultAsync<CajaCompensacion>(sql, new { Codigo = codigo });
    }

    public async Task<CajaCompensacion?> GetByIdAsync(int id)
    {
        using var connection = _context.CreateConnection();
        const string sql = "SELECT * FROM cajas_compensacion WHERE id = @Id";
        return await connection.QueryFirstOrDefaultAsync<CajaCompensacion>(sql, new { Id = id });
    }

    public async Task<IEnumerable<CajaCompensacion>> GetAllAsync()
    {
        using var connection = _context.CreateConnection();
        const string sql = "SELECT * FROM cajas_compensacion ORDER BY nombre";
        return await connection.QueryAsync<CajaCompensacion>(sql);
    }

    public async Task<IEnumerable<CajaCompensacion>> GetAllActiveAsync()
    {
        return await GetVigentesAsync();
    }

    public async Task<CajaCompensacion> AddAsync(CajaCompensacion entity)
    {
        entity.FechaCreacion = DateTime.Now;
        using var connection = _context.CreateConnection();
        const string sql = @"
            INSERT INTO cajas_compensacion (codigo, nombre, region, observaciones, activo, fecha_creacion)
            VALUES (@Codigo, @Nombre, @Region, @Observaciones, 1, @FechaCreacion);
            SELECT last_insert_rowid();";
        entity.Id = await connection.ExecuteScalarAsync<int>(sql, entity);
        return entity;
    }

    public async Task UpdateAsync(CajaCompensacion entity)
    {
        using var connection = _context.CreateConnection();
        const string sql = @"
            UPDATE cajas_compensacion SET 
                codigo = @Codigo,
                nombre = @Nombre,
                region = @Region,
                observaciones = @Observaciones
            WHERE id = @Id";
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = _context.CreateConnection();
        const string sql = "UPDATE cajas_compensacion SET activo = 0 WHERE id = @Id";
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public Task<int> SaveChangesAsync() => Task.FromResult(0);
}
