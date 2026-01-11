using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Infrastructure.Data;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Repositories;

/// <summary>
/// Repositorio para gestión de prestaciones sociales según legislación colombiana
/// </summary>
public class PrestacionRepository : IPrestacionRepository
{
    private readonly DapperContext _context;
    private readonly ILogger<PrestacionRepository> _logger;

    public PrestacionRepository(DapperContext context, ILogger<PrestacionRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Prestacion> AddAsync(Prestacion entity)
    {
        entity.FechaCreacion = DateTime.Now;
        const string sql = @"
            INSERT INTO prestaciones (
                empleado_id, periodo, tipo, fecha_inicio, fecha_fin,
                salario_base, valor_calculado, valor_pagado, fecha_pago,
                estado, metodo_pago, comprobante_referencia, observaciones,
                valor_base, porcentaje_aplicado, dias_proporcionales,
                activo, fecha_creacion
            ) VALUES (
                @EmpleadoId, @Periodo, @Tipo, @FechaInicio, @FechaFin,
                @SalarioBase, @ValorCalculado, @ValorPagado, @FechaPago,
                @Estado, @MetodoPago, @ComprobanteReferencia, @Observaciones,
                @ValorBase, @PorcentajeAplicado, @DiasProporcionales,
                1, @FechaCreacion
            );
            SELECT last_insert_rowid();";

        using var connection = _context.CreateConnection();
        entity.Id = await connection.ExecuteScalarAsync<int>(sql, entity);
        _logger.LogInformation("Prestación creada: ID={Id}, Tipo={Tipo}, Empleado={EmpleadoId}", 
            entity.Id, entity.Tipo, entity.EmpleadoId);
        return entity;
    }

    public async Task UpdateAsync(Prestacion entity)
    {
        entity.FechaModificacion = DateTime.Now;
        const string sql = @"
            UPDATE prestaciones SET
                empleado_id = @EmpleadoId,
                periodo = @Periodo,
                tipo = @Tipo,
                fecha_inicio = @FechaInicio,
                fecha_fin = @FechaFin,
                salario_base = @SalarioBase,
                valor_calculado = @ValorCalculado,
                valor_pagado = @ValorPagado,
                fecha_pago = @FechaPago,
                estado = @Estado,
                metodo_pago = @MetodoPago,
                comprobante_referencia = @ComprobanteReferencia,
                observaciones = @Observaciones,
                valor_base = @ValorBase,
                porcentaje_aplicado = @PorcentajeAplicado,
                dias_proporcionales = @DiasProporcionales,
                fecha_modificacion = @FechaModificacion
            WHERE id = @Id";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, entity);
        _logger.LogInformation("Prestación actualizada: ID={Id}", entity.Id);
    }

    public async Task DeleteAsync(int id)
    {
        // Hard delete - elimina permanentemente el registro
        const string sql = "DELETE FROM prestaciones WHERE id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id });
        _logger.LogInformation("Prestación eliminada: ID={Id}", id);
    }

    public async Task<Prestacion?> GetByIdAsync(int id)
    {
        const string sql = "SELECT * FROM prestaciones WHERE id = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Prestacion>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Prestacion>> GetAllAsync()
    {
        const string sql = "SELECT * FROM prestaciones ORDER BY fecha_creacion DESC";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Prestacion>(sql);
    }

    public async Task<IEnumerable<Prestacion>> GetAllActiveAsync()
    {
        const string sql = "SELECT * FROM prestaciones WHERE activo = 1 ORDER BY fecha_creacion DESC";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Prestacion>(sql);
    }

    public async Task<IEnumerable<Prestacion>> GetByEmpleadoIdAsync(int empleadoId)
    {
        const string sql = @"SELECT * FROM prestaciones 
            WHERE empleado_id = @EmpleadoId AND activo = 1 
            ORDER BY periodo DESC, tipo";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Prestacion>(sql, new { EmpleadoId = empleadoId });
    }

    public async Task<IEnumerable<Prestacion>> GetByPeriodoAsync(int periodo)
    {
        const string sql = @"SELECT * FROM prestaciones 
            WHERE periodo = @Periodo AND activo = 1 
            ORDER BY empleado_id, tipo";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Prestacion>(sql, new { Periodo = periodo });
    }

    public async Task<IEnumerable<Prestacion>> GetByTipoAsync(TipoPrestacion tipo)
    {
        const string sql = @"SELECT * FROM prestaciones 
            WHERE tipo = @Tipo AND activo = 1 
            ORDER BY periodo DESC, empleado_id";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Prestacion>(sql, new { Tipo = (int)tipo });
    }

    public async Task<Prestacion?> GetByEmpleadoAndPeriodoAsync(int empleadoId, int periodo, TipoPrestacion tipo)
    {
        const string sql = @"SELECT * FROM prestaciones 
            WHERE empleado_id = @EmpleadoId AND periodo = @Periodo AND tipo = @Tipo AND activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Prestacion>(sql, new 
        { 
            EmpleadoId = empleadoId, 
            Periodo = periodo, 
            Tipo = (int)tipo 
        });
    }

    public async Task<decimal> GetTotalPagadoByEmpleadoAsync(int empleadoId, int periodo)
    {
        const string sql = @"SELECT COALESCE(SUM(valor_pagado), 0) 
            FROM prestaciones 
            WHERE empleado_id = @EmpleadoId AND periodo = @Periodo AND activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.ExecuteScalarAsync<decimal>(sql, new { EmpleadoId = empleadoId, Periodo = periodo });
    }

    public async Task<IEnumerable<Prestacion>> GetPendientesPagoAsync()
    {
        const string sql = @"SELECT * FROM prestaciones 
            WHERE estado IN (1, 2) AND activo = 1 
            ORDER BY periodo, empleado_id, tipo";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Prestacion>(sql);
    }

    public async Task<IEnumerable<Prestacion>> GetByEmpleadoAndEstadoAsync(int empleadoId, EstadoPrestacion estado)
    {
        const string sql = @"SELECT * FROM prestaciones 
            WHERE empleado_id = @EmpleadoId AND estado = @Estado AND activo = 1 
            ORDER BY periodo DESC, tipo";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Prestacion>(sql, new { EmpleadoId = empleadoId, Estado = (int)estado });
    }

    public Task<int> SaveChangesAsync() => Task.FromResult(0);
}
