using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Repositories;

public class ContratoRepository : IContratoRepository
{
    private readonly Data.DapperContext _context;
    private readonly ILogger<ContratoRepository> _logger;

    public ContratoRepository(Data.DapperContext context, ILogger<ContratoRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Contrato> AddAsync(Contrato entity)
    {
        entity.FechaCreacion = DateTime.Now;
        const string sql = @"INSERT INTO contratos (empleado_id, tipo_contrato, fecha_inicio, fecha_fin, salario, cargo_id, estado, archivo_adjunto_path, observaciones, activo, fecha_creacion)
VALUES (@EmpleadoId, @TipoContrato, @FechaInicio, @FechaFin, @Salario, @CargoId, @Estado, @ArchivoAdjuntoPath, @Observaciones, 1, @FechaCreacion);
SELECT last_insert_rowid();";

        using var connection = _context.CreateConnection();
        entity.Id = await connection.ExecuteScalarAsync<int>(sql, entity);
        return entity;
    }

    public async Task UpdateAsync(Contrato entity)
    {
        entity.FechaModificacion = DateTime.Now;
        const string sql = @"UPDATE contratos
SET empleado_id = @EmpleadoId,
    tipo_contrato = @TipoContrato,
    fecha_inicio = @FechaInicio,
    fecha_fin = @FechaFin,
    salario = @Salario,
    cargo_id = @CargoId,
    estado = @Estado,
    archivo_adjunto_path = @ArchivoAdjuntoPath,
    observaciones = @Observaciones,
    fecha_modificacion = @FechaModificacion
WHERE id = @Id";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "UPDATE contratos SET activo = 0, fecha_modificacion = CURRENT_TIMESTAMP WHERE id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<Contrato?> GetByIdAsync(int id)
    {
        const string sql = "SELECT * FROM contratos WHERE id = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Contrato>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Contrato>> GetAllAsync()
    {
        const string sql = "SELECT * FROM contratos";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Contrato>(sql);
    }

    public async Task<IEnumerable<Contrato>> GetAllActiveAsync()
    {
        const string sql = "SELECT * FROM contratos WHERE activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Contrato>(sql);
    }

    public async Task<IEnumerable<Contrato>> GetByEmpleadoIdAsync(int empleadoId)
    {
        const string sql = "SELECT * FROM contratos WHERE empleado_id = @EmpleadoId AND activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Contrato>(sql, new { EmpleadoId = empleadoId });
    }

    public async Task<Contrato?> GetContratoActivoByEmpleadoIdAsync(int empleadoId)
    {
        const string sql = @"SELECT * FROM contratos
WHERE empleado_id = @EmpleadoId AND activo = 1 AND (fecha_fin IS NULL OR fecha_fin >= date('now'))
ORDER BY fecha_inicio DESC
LIMIT 1";

        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Contrato>(sql, new { EmpleadoId = empleadoId });
    }

    public async Task<IEnumerable<Contrato>> GetContratosProximosAVencerAsync(int diasAnticipacion)
    {
        const string sql = @"SELECT * FROM contratos
WHERE fecha_fin IS NOT NULL
  AND fecha_fin <= date('now', '+' || @Dias || ' day')
  AND fecha_fin >= date('now')
  AND activo = 1";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Contrato>(sql, new { Dias = diasAnticipacion });
    }

    public async Task<IEnumerable<Contrato>> GetContratosPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        const string sql = @"SELECT * FROM contratos
WHERE fecha_inicio >= @FechaInicio AND (fecha_fin IS NULL OR fecha_fin <= @FechaFin)";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Contrato>(sql, new { FechaInicio = fechaInicio, FechaFin = fechaFin });
    }

    public Task<int> SaveChangesAsync() => Task.FromResult(0);
}
