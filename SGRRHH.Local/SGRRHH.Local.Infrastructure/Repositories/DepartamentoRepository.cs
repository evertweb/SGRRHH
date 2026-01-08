using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Repositories;

public class DepartamentoRepository : IDepartamentoRepository
{
    private readonly Data.DapperContext _context;
    private readonly ILogger<DepartamentoRepository> _logger;

    public DepartamentoRepository(Data.DapperContext context, ILogger<DepartamentoRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Departamento> AddAsync(Departamento entity)
    {
        entity.FechaCreacion = DateTime.Now;
        const string sql = @"INSERT INTO departamentos (codigo, nombre, descripcion, jefe_id, activo, fecha_creacion)
VALUES (@Codigo, @Nombre, @Descripcion, @JefeId, 1, @FechaCreacion);
SELECT last_insert_rowid();";

        using var connection = _context.CreateConnection();
        entity.Id = await connection.ExecuteScalarAsync<int>(sql, entity);
        return entity;
    }

    public async Task UpdateAsync(Departamento entity)
    {
        entity.FechaModificacion = DateTime.Now;
        const string sql = @"UPDATE departamentos
SET codigo = @Codigo,
    nombre = @Nombre,
    descripcion = @Descripcion,
    jefe_id = @JefeId,
    fecha_modificacion = @FechaModificacion
WHERE id = @Id";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "UPDATE departamentos SET activo = 0, fecha_modificacion = CURRENT_TIMESTAMP WHERE id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<Departamento?> GetByIdAsync(int id)
    {
        const string sql = "SELECT * FROM departamentos WHERE id = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Departamento>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Departamento>> GetAllAsync()
    {
        const string sql = "SELECT * FROM departamentos";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Departamento>(sql);
    }

    public async Task<IEnumerable<Departamento>> GetAllActiveAsync()
    {
        const string sql = "SELECT * FROM departamentos WHERE activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Departamento>(sql);
    }

    public async Task<Departamento?> GetByCodigoAsync(string codigo)
    {
        const string sql = "SELECT * FROM departamentos WHERE codigo = @Codigo";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Departamento>(sql, new { Codigo = codigo });
    }

    public async Task<Departamento?> GetByIdWithEmpleadosAsync(int id)
    {
        using var connection = _context.CreateConnection();
        var departamento = await GetByIdAsync(id);
        if (departamento is null)
        {
            return null;
        }

        const string empleadosSql = "SELECT * FROM empleados WHERE departamento_id = @DepartamentoId AND activo = 1";
        var empleados = await connection.QueryAsync<Empleado>(empleadosSql, new { DepartamentoId = id });
        departamento.Empleados = empleados.ToList();
        return departamento;
    }

    public async Task<Departamento?> GetByIdWithCargosAsync(int id)
    {
        using var connection = _context.CreateConnection();
        var departamento = await GetByIdAsync(id);
        if (departamento is null)
        {
            return null;
        }

        const string cargosSql = "SELECT * FROM cargos WHERE departamento_id = @DepartamentoId AND activo = 1";
        var cargos = await connection.QueryAsync<Cargo>(cargosSql, new { DepartamentoId = id });
        departamento.Cargos = cargos.ToList();
        return departamento;
    }

    public async Task<IEnumerable<Departamento>> GetAllWithEmpleadosCountAsync()
    {
        const string sql = @"SELECT d.* FROM departamentos d WHERE d.activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Departamento>(sql);
    }

    public async Task<bool> ExistsCodigoAsync(string codigo, int? excludeId = null)
    {
        const string sql = "SELECT COUNT(1) FROM departamentos WHERE codigo = @Codigo AND (@ExcludeId IS NULL OR id <> @ExcludeId)";
        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Codigo = codigo, ExcludeId = excludeId });
        return count > 0;
    }

    public async Task<bool> ExistsByNameAsync(string nombre, int? excludeId = null)
    {
        const string sql = "SELECT COUNT(1) FROM departamentos WHERE lower(nombre) = lower(@Nombre) AND (@ExcludeId IS NULL OR id <> @ExcludeId)";
        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Nombre = nombre, ExcludeId = excludeId });
        return count > 0;
    }

    public async Task<string> GetNextCodigoAsync()
    {
        const string sql = "SELECT codigo FROM departamentos ORDER BY id DESC LIMIT 1";
        using var connection = _context.CreateConnection();
        var last = await connection.QuerySingleOrDefaultAsync<string>(sql);
        if (string.IsNullOrWhiteSpace(last))
        {
            return "DEP-001";
        }

        var numeric = int.TryParse(last.Split('-').LastOrDefault(), out var number) ? number + 1 : 1;
        return $"DEP-{numeric:000}";
    }

    public async Task<bool> HasEmpleadosAsync(int id)
    {
        const string sql = "SELECT COUNT(1) FROM empleados WHERE departamento_id = @Id AND activo = 1";
        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });
        return count > 0;
    }

    public async Task<int> CountActiveAsync()
    {
        const string sql = "SELECT COUNT(1) FROM departamentos WHERE activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public void InvalidateCache()
    {
        // no caching layer yet
    }

    public async Task<(IEnumerable<Departamento> Items, int TotalCount)> GetAllActivePagedAsync(int pageNumber = 1, int pageSize = 50)
    {
        const string dataSql = @"SELECT * FROM departamentos WHERE activo = 1 ORDER BY nombre LIMIT @PageSize OFFSET @Offset";
        const string countSql = "SELECT COUNT(1) FROM departamentos WHERE activo = 1";
        using var connection = _context.CreateConnection();
        var items = await connection.QueryAsync<Departamento>(dataSql, new { PageSize = pageSize, Offset = (pageNumber - 1) * pageSize });
        var total = await connection.ExecuteScalarAsync<int>(countSql);
        return (items, total);
    }

    public Task<int> SaveChangesAsync() => Task.FromResult(0);
}
