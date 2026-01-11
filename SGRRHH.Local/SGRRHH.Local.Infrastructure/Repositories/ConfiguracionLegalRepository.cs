using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Infrastructure.Data;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Repositories;

/// <summary>
/// Repositorio para gestión de configuración legal de Colombia (SMLMV, porcentajes, etc.)
/// </summary>
public class ConfiguracionLegalRepository : IConfiguracionLegalRepository
{
    private readonly DapperContext _context;
    private readonly ILogger<ConfiguracionLegalRepository> _logger;

    public ConfiguracionLegalRepository(DapperContext context, ILogger<ConfiguracionLegalRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ConfiguracionLegal> AddAsync(ConfiguracionLegal entity)
    {
        entity.FechaCreacion = DateTime.Now;
        const string sql = @"
            INSERT INTO configuracion_legal (
                año, salario_minimo_mensual, auxilio_transporte,
                porcentaje_salud_empleado, porcentaje_salud_empleador,
                porcentaje_pension_empleado, porcentaje_pension_empleador,
                porcentaje_caja_compensacion, porcentaje_icbf, porcentaje_sena,
                arl_clase1_min, arl_clase5_max, porcentaje_intereses_cesantias,
                dias_vacaciones_año, horas_maximas_semanales, horas_ordinarias_diarias,
                recargo_hora_extra_diurna, recargo_hora_extra_nocturna,
                recargo_hora_nocturna, recargo_hora_dominical_festivo,
                edad_minima_laboral, observaciones, es_vigente, activo, fecha_creacion
            ) VALUES (
                @Año, @SalarioMinimoMensual, @AuxilioTransporte,
                @PorcentajeSaludEmpleado, @PorcentajeSaludEmpleador,
                @PorcentajePensionEmpleado, @PorcentajePensionEmpleador,
                @PorcentajeCajaCompensacion, @PorcentajeICBF, @PorcentajeSENA,
                @ARLClase1Min, @ARLClase5Max, @PorcentajeInteresesCesantias,
                @DiasVacacionesAño, @HorasMaximasSemanales, @HorasOrdinariasDiarias,
                @RecargoHoraExtraDiurna, @RecargoHoraExtraNocturna,
                @RecargoHoraNocturna, @RecargoHoraDominicalFestivo,
                @EdadMinimaLaboral, @Observaciones, @EsVigente, 1, @FechaCreacion
            );
            SELECT last_insert_rowid();";

        using var connection = _context.CreateConnection();
        entity.Id = await connection.ExecuteScalarAsync<int>(sql, entity);
        _logger.LogInformation("Configuración legal creada: Año={Año}, SMLMV={SMLMV}", 
            entity.Año, entity.SalarioMinimoMensual);
        return entity;
    }

    public async Task UpdateAsync(ConfiguracionLegal entity)
    {
        entity.FechaModificacion = DateTime.Now;
        const string sql = @"
            UPDATE configuracion_legal SET
                año = @Año,
                salario_minimo_mensual = @SalarioMinimoMensual,
                auxilio_transporte = @AuxilioTransporte,
                porcentaje_salud_empleado = @PorcentajeSaludEmpleado,
                porcentaje_salud_empleador = @PorcentajeSaludEmpleador,
                porcentaje_pension_empleado = @PorcentajePensionEmpleado,
                porcentaje_pension_empleador = @PorcentajePensionEmpleador,
                porcentaje_caja_compensacion = @PorcentajeCajaCompensacion,
                porcentaje_icbf = @PorcentajeICBF,
                porcentaje_sena = @PorcentajeSENA,
                arl_clase1_min = @ARLClase1Min,
                arl_clase5_max = @ARLClase5Max,
                porcentaje_intereses_cesantias = @PorcentajeInteresesCesantias,
                dias_vacaciones_año = @DiasVacacionesAño,
                horas_maximas_semanales = @HorasMaximasSemanales,
                horas_ordinarias_diarias = @HorasOrdinariasDiarias,
                recargo_hora_extra_diurna = @RecargoHoraExtraDiurna,
                recargo_hora_extra_nocturna = @RecargoHoraExtraNocturna,
                recargo_hora_nocturna = @RecargoHoraNocturna,
                recargo_hora_dominical_festivo = @RecargoHoraDominicalFestivo,
                edad_minima_laboral = @EdadMinimaLaboral,
                observaciones = @Observaciones,
                es_vigente = @EsVigente,
                fecha_modificacion = @FechaModificacion
            WHERE id = @Id";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, entity);
        _logger.LogInformation("Configuración legal actualizada: ID={Id}, Año={Año}", entity.Id, entity.Año);
    }

    public async Task DeleteAsync(int id)
    {
        // Hard delete - elimina permanentemente el registro
        const string sql = "DELETE FROM configuracion_legal WHERE id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<ConfiguracionLegal?> GetByIdAsync(int id)
    {
        const string sql = "SELECT * FROM configuracion_legal WHERE id = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<ConfiguracionLegal>(sql, new { Id = id });
    }

    public async Task<IEnumerable<ConfiguracionLegal>> GetAllAsync()
    {
        const string sql = "SELECT * FROM configuracion_legal ORDER BY año DESC";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<ConfiguracionLegal>(sql);
    }

    public async Task<IEnumerable<ConfiguracionLegal>> GetAllActiveAsync()
    {
        const string sql = "SELECT * FROM configuracion_legal WHERE activo = 1 ORDER BY año DESC";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<ConfiguracionLegal>(sql);
    }

    public async Task<ConfiguracionLegal?> GetVigenteAsync()
    {
        const string sql = "SELECT * FROM configuracion_legal WHERE es_vigente = 1 AND activo = 1 LIMIT 1";
        using var connection = _context.CreateConnection();
        var config = await connection.QuerySingleOrDefaultAsync<ConfiguracionLegal>(sql);
        
        // Si no hay configuración vigente, usar la del año actual o la más reciente
        if (config == null)
        {
            const string sqlFallback = @"SELECT * FROM configuracion_legal 
                WHERE activo = 1 ORDER BY año DESC LIMIT 1";
            config = await connection.QuerySingleOrDefaultAsync<ConfiguracionLegal>(sqlFallback);
        }
        
        return config;
    }

    public async Task<ConfiguracionLegal?> GetByAñoAsync(int año)
    {
        const string sql = "SELECT * FROM configuracion_legal WHERE año = @Año AND activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<ConfiguracionLegal>(sql, new { Año = año });
    }

    public async Task SetVigenteAsync(int id)
    {
        using var connection = _context.CreateConnection();
        using var transaction = connection.BeginTransaction();
        
        try
        {
            // Desactivar todas las configuraciones vigentes
            await connection.ExecuteAsync(
                "UPDATE configuracion_legal SET es_vigente = 0, fecha_modificacion = CURRENT_TIMESTAMP WHERE es_vigente = 1",
                transaction: transaction);
            
            // Activar la configuración seleccionada
            await connection.ExecuteAsync(
                "UPDATE configuracion_legal SET es_vigente = 1, fecha_modificacion = CURRENT_TIMESTAMP WHERE id = @Id",
                new { Id = id },
                transaction: transaction);
            
            transaction.Commit();
            _logger.LogInformation("Configuración legal ID={Id} establecida como vigente", id);
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.LogError(ex, "Error al establecer configuración legal como vigente: ID={Id}", id);
            throw;
        }
    }

    public async Task<bool> ExisteAñoAsync(int año)
    {
        const string sql = "SELECT COUNT(1) FROM configuracion_legal WHERE año = @Año AND activo = 1";
        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Año = año });
        return count > 0;
    }

    public async Task<decimal> GetSalarioMinimoVigenteAsync()
    {
        var config = await GetVigenteAsync();
        return config?.SalarioMinimoMensual ?? 0;
    }

    public async Task<decimal> GetAuxilioTransporteVigenteAsync()
    {
        var config = await GetVigenteAsync();
        return config?.AuxilioTransporte ?? 0;
    }

    public Task<int> SaveChangesAsync() => Task.FromResult(0);
}
