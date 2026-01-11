using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Repositories;

public class CargoRepository : ICargoRepository
{
    private readonly Data.DapperContext _context;
    private readonly ILogger<CargoRepository> _logger;

    public CargoRepository(Data.DapperContext context, ILogger<CargoRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Cargo> AddAsync(Cargo entity)
    {
        entity.FechaCreacion = DateTime.Now;
        const string sql = @"INSERT INTO cargos (codigo, nombre, descripcion, nivel, departamento_id, salario_base, requisitos, competencias, cargo_superior_id, numero_plazas, activo, fecha_creacion)
VALUES (@Codigo, @Nombre, @Descripcion, @Nivel, @DepartamentoId, @SalarioBase, @Requisitos, @Competencias, @CargoSuperiorId, @NumeroPlazas, 1, @FechaCreacion);
SELECT last_insert_rowid();";

        using var connection = _context.CreateConnection();
        entity.Id = await connection.ExecuteScalarAsync<int>(sql, entity);
        return entity;
    }

    public async Task UpdateAsync(Cargo entity)
    {
        entity.FechaModificacion = DateTime.Now;
        const string sql = @"UPDATE cargos
SET codigo = @Codigo,
    nombre = @Nombre,
    descripcion = @Descripcion,
    nivel = @Nivel,
    departamento_id = @DepartamentoId,
    salario_base = @SalarioBase,
    requisitos = @Requisitos,
    competencias = @Competencias,
    cargo_superior_id = @CargoSuperiorId,
    numero_plazas = @NumeroPlazas,
    fecha_modificacion = @FechaModificacion
WHERE id = @Id";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(int id)
    {
        // Hard delete - elimina permanentemente el registro
        const string sql = "DELETE FROM cargos WHERE id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<Cargo?> GetByIdAsync(int id)
    {
        const string sql = "SELECT * FROM cargos WHERE id = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Cargo>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Cargo>> GetAllAsync()
    {
        const string sql = "SELECT * FROM cargos";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Cargo>(sql);
    }

    public async Task<IEnumerable<Cargo>> GetAllActiveAsync()
    {
        const string sql = "SELECT * FROM cargos WHERE activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Cargo>(sql);
    }

    public async Task<Cargo?> GetByCodigoAsync(string codigo)
    {
        const string sql = "SELECT * FROM cargos WHERE codigo = @Codigo";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Cargo>(sql, new { Codigo = codigo });
    }

    public async Task<Cargo?> GetByIdWithDepartamentoAsync(int id)
    {
        const string sql = @"SELECT c.*, d.*
FROM cargos c
LEFT JOIN departamentos d ON c.departamento_id = d.id
WHERE c.id = @Id";

        using var connection = _context.CreateConnection();
        Cargo? result = null;
        await connection.QueryAsync<Cargo, Departamento, Cargo>(sql, (cargo, departamento) =>
        {
            result ??= cargo;
            result.Departamento = departamento;
            return cargo;
        }, new { Id = id });
        return result;
    }

    public async Task<Cargo?> GetByIdWithEmpleadosAsync(int id)
    {
        using var connection = _context.CreateConnection();
        var cargo = await GetByIdAsync(id);
        if (cargo is null)
        {
            return null;
        }

        const string empleadosSql = "SELECT * FROM empleados WHERE cargo_id = @CargoId AND activo = 1";
        var empleados = await connection.QueryAsync<Empleado>(empleadosSql, new { CargoId = id });
        cargo.Empleados = empleados.ToList();
        return cargo;
    }

    public async Task<IEnumerable<Cargo>> GetAllWithDepartamentoAsync()
    {
        const string sql = @"SELECT c.*, d.*
FROM cargos c
LEFT JOIN departamentos d ON c.departamento_id = d.id";

        using var connection = _context.CreateConnection();
        var lookup = new Dictionary<int, Cargo>();
        await connection.QueryAsync<Cargo, Departamento, Cargo>(sql, (cargo, dept) =>
        {
            if (!lookup.TryGetValue(cargo.Id, out var existing))
            {
                existing = cargo;
                lookup[cargo.Id] = existing;
            }
            existing.Departamento = dept;
            return existing;
        });
        return lookup.Values;
    }

    public async Task<IEnumerable<Cargo>> GetAllActiveWithDepartamentoAsync()
    {
        const string sql = @"SELECT c.*, d.*
FROM cargos c
LEFT JOIN departamentos d ON c.departamento_id = d.id
WHERE c.activo = 1";

        using var connection = _context.CreateConnection();
        var lookup = new Dictionary<int, Cargo>();
        await connection.QueryAsync<Cargo, Departamento, Cargo>(sql, (cargo, dept) =>
        {
            if (!lookup.TryGetValue(cargo.Id, out var existing))
            {
                existing = cargo;
                lookup[cargo.Id] = existing;
            }
            existing.Departamento = dept;
            return existing;
        });
        return lookup.Values;
    }

    public async Task<IEnumerable<Cargo>> GetByDepartamentoAsync(int departamentoId)
    {
        const string sql = "SELECT * FROM cargos WHERE departamento_id = @DepartamentoId AND activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Cargo>(sql, new { DepartamentoId = departamentoId });
    }

    public async Task<bool> ExistsCodigoAsync(string codigo, int? excludeId = null)
    {
        const string sql = "SELECT COUNT(1) FROM cargos WHERE codigo = @Codigo AND (@ExcludeId IS NULL OR id <> @ExcludeId)";
        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Codigo = codigo, ExcludeId = excludeId });
        return count > 0;
    }

    public async Task<string> GetNextCodigoAsync()
    {
        const string sql = "SELECT codigo FROM cargos ORDER BY id DESC LIMIT 1";
        using var connection = _context.CreateConnection();
        var last = await connection.QuerySingleOrDefaultAsync<string>(sql);
        if (string.IsNullOrWhiteSpace(last))
        {
            return "CAR-001";
        }

        var numeric = int.TryParse(last.Split('-').LastOrDefault(), out var number) ? number + 1 : 1;
        return $"CAR-{numeric:000}";
    }

    public async Task<bool> HasEmpleadosAsync(int id)
    {
        const string sql = "SELECT COUNT(1) FROM empleados WHERE cargo_id = @Id AND activo = 1";
        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });
        return count > 0;
    }

    public async Task<int> CountActiveAsync()
    {
        const string sql = "SELECT COUNT(1) FROM cargos WHERE activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task<bool> ExistsNombreInDepartamentoAsync(string nombre, int? departamentoId, int? excludeId = null)
    {
        const string sql = @"SELECT COUNT(1)
FROM cargos
WHERE lower(nombre) = lower(@Nombre)
  AND (@DepartamentoId IS NULL OR departamento_id = @DepartamentoId)
  AND (@ExcludeId IS NULL OR id <> @ExcludeId)";

        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Nombre = nombre, DepartamentoId = departamentoId, ExcludeId = excludeId });
        return count > 0;
    }

    public void InvalidateCache()
    {
        // no caching layer yet
    }

    public Task<int> SaveChangesAsync() => Task.FromResult(0);
}
