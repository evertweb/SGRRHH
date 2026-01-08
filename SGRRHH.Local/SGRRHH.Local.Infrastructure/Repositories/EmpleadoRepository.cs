using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Domain.Interfaces;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Repositories;

public class EmpleadoRepository : IEmpleadoRepository
{
    private readonly Data.DapperContext _context;
    private readonly ILogger<EmpleadoRepository> _logger;

    public EmpleadoRepository(Data.DapperContext context, ILogger<EmpleadoRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Empleado> AddAsync(Empleado entity)
    {
        entity.FechaCreacion = DateTime.Now;
        const string sql = @"INSERT INTO empleados (
    codigo, cedula, nombres, apellidos, fecha_nacimiento, genero, estado_civil, direccion, telefono, telefono_emergencia,
    contacto_emergencia, email, foto_path, fecha_ingreso, fecha_retiro, estado, tipo_contrato, cargo_id, departamento_id,
    supervisor_id, observaciones, numero_cuenta, banco, nivel_educacion, eps, arl, afp, salario_base, creado_por_id,
    fecha_solicitud, aprobado_por_id, fecha_aprobacion, motivo_rechazo, activo, fecha_creacion)
VALUES (
    @Codigo, @Cedula, @Nombres, @Apellidos, @FechaNacimiento, @Genero, @EstadoCivil, @Direccion, @Telefono, @TelefonoEmergencia,
    @ContactoEmergencia, @Email, @FotoPath, @FechaIngreso, @FechaRetiro, @Estado, @TipoContrato, @CargoId, @DepartamentoId,
    @SupervisorId, @Observaciones, @NumeroCuenta, @Banco, @NivelEducacion, @EPS, @ARL, @AFP, @SalarioBase, @CreadoPorId,
    @FechaSolicitud, @AprobadoPorId, @FechaAprobacion, @MotivoRechazo, @Activo, @FechaCreacion);
SELECT last_insert_rowid();";

        using var connection = _context.CreateConnection();
        entity.Id = await connection.ExecuteScalarAsync<int>(sql, entity);
        return entity;
    }

    public async Task UpdateAsync(Empleado entity)
    {
        var versionAnterior = entity.FechaModificacion;
        entity.FechaModificacion = DateTime.Now;
        
        // SQL con control de concurrencia optimista:
        // Solo actualiza si fecha_modificacion no cambió desde que se cargó el registro
        const string sql = @"UPDATE empleados
SET codigo = @Codigo,
    cedula = @Cedula,
    nombres = @Nombres,
    apellidos = @Apellidos,
    fecha_nacimiento = @FechaNacimiento,
    genero = @Genero,
    estado_civil = @EstadoCivil,
    direccion = @Direccion,
    telefono = @Telefono,
    telefono_emergencia = @TelefonoEmergencia,
    contacto_emergencia = @ContactoEmergencia,
    email = @Email,
    foto_path = @FotoPath,
    fecha_ingreso = @FechaIngreso,
    fecha_retiro = @FechaRetiro,
    estado = @Estado,
    tipo_contrato = @TipoContrato,
    cargo_id = @CargoId,
    departamento_id = @DepartamentoId,
    supervisor_id = @SupervisorId,
    observaciones = @Observaciones,
    numero_cuenta = @NumeroCuenta,
    banco = @Banco,
    nivel_educacion = @NivelEducacion,
    eps = @EPS,
    arl = @ARL,
    afp = @AFP,
    salario_base = @SalarioBase,
    creado_por_id = @CreadoPorId,
    fecha_solicitud = @FechaSolicitud,
    aprobado_por_id = @AprobadoPorId,
    fecha_aprobacion = @FechaAprobacion,
    motivo_rechazo = @MotivoRechazo,
    fecha_modificacion = @FechaModificacion
WHERE id = @Id 
  AND (fecha_modificacion = @VersionAnterior OR (fecha_modificacion IS NULL AND @VersionAnterior IS NULL))";

        using var connection = _context.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            entity.Codigo,
            entity.Cedula,
            entity.Nombres,
            entity.Apellidos,
            entity.FechaNacimiento,
            entity.Genero,
            entity.EstadoCivil,
            entity.Direccion,
            entity.Telefono,
            entity.TelefonoEmergencia,
            entity.ContactoEmergencia,
            entity.Email,
            entity.FotoPath,
            entity.FechaIngreso,
            entity.FechaRetiro,
            entity.Estado,
            entity.TipoContrato,
            entity.CargoId,
            entity.DepartamentoId,
            entity.SupervisorId,
            entity.Observaciones,
            entity.NumeroCuenta,
            entity.Banco,
            entity.NivelEducacion,
            entity.EPS,
            entity.ARL,
            entity.AFP,
            entity.SalarioBase,
            entity.CreadoPorId,
            entity.FechaSolicitud,
            entity.AprobadoPorId,
            entity.FechaAprobacion,
            entity.MotivoRechazo,
            entity.FechaModificacion,
            entity.Id,
            VersionAnterior = versionAnterior
        });
        
        if (rowsAffected == 0)
        {
            // Verificar si el registro existe
            var existe = await connection.QuerySingleOrDefaultAsync<DateTime?>(
                "SELECT fecha_modificacion FROM empleados WHERE id = @Id", 
                new { entity.Id });
            
            if (existe.HasValue || existe == null)
            {
                throw new ConcurrencyConflictException(
                    "Empleado", 
                    entity.Id, 
                    versionAnterior, 
                    existe);
            }
            
            _logger.LogWarning("Empleado {Id} no encontrado para actualizar", entity.Id);
        }
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "UPDATE empleados SET activo = 0, fecha_modificacion = CURRENT_TIMESTAMP WHERE id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<Empleado?> GetByIdAsync(int id)
    {
        const string sql = "SELECT * FROM empleados WHERE id = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Empleado>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Empleado>> GetAllAsync()
    {
        const string sql = "SELECT * FROM empleados";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Empleado>(sql);
    }

    public async Task<IEnumerable<Empleado>> GetAllActiveAsync()
    {
        const string sql = "SELECT * FROM empleados WHERE activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Empleado>(sql);
    }

    public async Task<Empleado?> GetByCodigoAsync(string codigo)
    {
        const string sql = "SELECT * FROM empleados WHERE codigo = @Codigo";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Empleado>(sql, new { Codigo = codigo });
    }

    public async Task<Empleado?> GetByCedulaAsync(string cedula)
    {
        const string sql = "SELECT * FROM empleados WHERE cedula = @Cedula";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Empleado>(sql, new { Cedula = cedula });
    }

    public async Task<Empleado?> GetByIdWithRelationsAsync(int id)
    {
        const string sql = @"SELECT e.*, d.*, c.*
FROM empleados e
LEFT JOIN departamentos d ON e.departamento_id = d.id
LEFT JOIN cargos c ON e.cargo_id = c.id
WHERE e.id = @Id";

        using var connection = _context.CreateConnection();
        Empleado? empleado = null;
        await connection.QueryAsync<Empleado, Departamento, Cargo, Empleado>(sql, (e, d, c) =>
        {
            empleado ??= e;
            empleado.Departamento = d;
            empleado.Cargo = c;
            return empleado;
        }, new { Id = id }, splitOn: "id,id");

        return empleado;
    }

    public async Task<IEnumerable<Empleado>> GetAllWithRelationsAsync()
    {
        const string sql = @"SELECT e.*, d.*, c.*
FROM empleados e
LEFT JOIN departamentos d ON e.departamento_id = d.id
LEFT JOIN cargos c ON e.cargo_id = c.id";

        using var connection = _context.CreateConnection();
        var lookup = new Dictionary<int, Empleado>();
        await connection.QueryAsync<Empleado, Departamento, Cargo, Empleado>(sql, (e, d, c) =>
        {
            if (!lookup.TryGetValue(e.Id, out var existing))
            {
                existing = e;
                lookup[e.Id] = existing;
            }
            existing.Departamento = d;
            existing.Cargo = c;
            return existing;
        }, splitOn: "id,id");
        return lookup.Values;
    }

    public async Task<IEnumerable<Empleado>> GetAllActiveWithRelationsAsync()
    {
        const string sql = @"SELECT e.*, d.*, c.*
FROM empleados e
LEFT JOIN departamentos d ON e.departamento_id = d.id
LEFT JOIN cargos c ON e.cargo_id = c.id
WHERE e.activo = 1";

        using var connection = _context.CreateConnection();
        var lookup = new Dictionary<int, Empleado>();
        await connection.QueryAsync<Empleado, Departamento, Cargo, Empleado>(sql, (e, d, c) =>
        {
            if (!lookup.TryGetValue(e.Id, out var existing))
            {
                existing = e;
                lookup[e.Id] = existing;
            }
            existing.Departamento = d;
            existing.Cargo = c;
            return existing;
        }, splitOn: "id,id");
        return lookup.Values;
    }

    public async Task<IEnumerable<Empleado>> GetByDepartamentoAsync(int departamentoId)
    {
        const string sql = "SELECT * FROM empleados WHERE departamento_id = @DepartamentoId AND activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Empleado>(sql, new { DepartamentoId = departamentoId });
    }

    public async Task<IEnumerable<Empleado>> GetByCargoAsync(int cargoId)
    {
        const string sql = "SELECT * FROM empleados WHERE cargo_id = @CargoId AND activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Empleado>(sql, new { CargoId = cargoId });
    }

    public async Task<IEnumerable<Empleado>> GetByEstadoAsync(EstadoEmpleado estado)
    {
        const string sql = "SELECT * FROM empleados WHERE estado = @Estado AND activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Empleado>(sql, new { Estado = estado });
    }

    public async Task<IEnumerable<Empleado>> SearchAsync(string searchTerm)
    {
        var term = $"%{searchTerm}%";
        const string sql = @"SELECT * FROM empleados
WHERE (lower(nombres) LIKE lower(@Term)
    OR lower(apellidos) LIKE lower(@Term)
    OR lower(cedula) LIKE lower(@Term)
    OR lower(codigo) LIKE lower(@Term))
  AND activo = 1";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Empleado>(sql, new { Term = term });
    }

    public async Task<bool> ExistsCodigoAsync(string codigo, int? excludeId = null)
    {
        const string sql = "SELECT COUNT(1) FROM empleados WHERE codigo = @Codigo AND (@ExcludeId IS NULL OR id <> @ExcludeId)";
        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Codigo = codigo, ExcludeId = excludeId });
        return count > 0;
    }

    public async Task<bool> ExistsCedulaAsync(string cedula, int? excludeId = null)
    {
        const string sql = "SELECT COUNT(1) FROM empleados WHERE cedula = @Cedula AND (@ExcludeId IS NULL OR id <> @ExcludeId)";
        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Cedula = cedula, ExcludeId = excludeId });
        return count > 0;
    }

    public async Task<string> GetNextCodigoAsync()
    {
        const string sql = "SELECT codigo FROM empleados ORDER BY id DESC LIMIT 1";
        using var connection = _context.CreateConnection();
        var last = await connection.QuerySingleOrDefaultAsync<string>(sql);
        if (string.IsNullOrWhiteSpace(last))
        {
            return "EMP-0001";
        }

        var numeric = int.TryParse(last.Split('-').LastOrDefault(), out var number) ? number + 1 : 1;
        return $"EMP-{numeric:0000}";
    }

    public async Task<int> CountActiveAsync()
    {
        const string sql = "SELECT COUNT(1) FROM empleados WHERE activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task<bool> ExistsEmailAsync(string email, int? excludeId = null)
    {
        const string sql = "SELECT COUNT(1) FROM empleados WHERE lower(email) = lower(@Email) AND (@ExcludeId IS NULL OR id <> @ExcludeId)";
        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Email = email, ExcludeId = excludeId });
        return count > 0;
    }

    public void InvalidateCache()
    {
        // no caching layer yet
    }

    public Task<int> SaveChangesAsync() => Task.FromResult(0);
}
