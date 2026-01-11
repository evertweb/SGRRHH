using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Infrastructure.Data;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Repositories;

/// <summary>
/// Repositorio para gestión de nóminas mensuales
/// </summary>
public class NominaRepository : INominaRepository
{
    private readonly DapperContext _context;
    private readonly ILogger<NominaRepository> _logger;

    public NominaRepository(DapperContext context, ILogger<NominaRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Nomina> AddAsync(Nomina entity)
    {
        entity.FechaCreacion = DateTime.Now;
        const string sql = @"
            INSERT INTO nominas (
                empleado_id, periodo, fecha_pago, salario_base, auxilio_transporte,
                horas_extras_diurnas, horas_extras_nocturnas, horas_nocturnas, horas_dominicales_festivos,
                comisiones, bonificaciones, otros_devengos,
                deduccion_salud, deduccion_pension, retencion_fuente, prestamos, embargos, fondo_empleados, otras_deducciones,
                aporte_salud_empleador, aporte_pension_empleador, aporte_arl, aporte_caja_compensacion, aporte_icbf, aporte_sena,
                estado, aprobado_por_id, fecha_aprobacion, comprobante_numero, observaciones,
                activo, fecha_creacion
            ) VALUES (
                @EmpleadoId, @Periodo, @FechaPago, @SalarioBase, @AuxilioTransporte,
                @HorasExtrasDiurnas, @HorasExtrasNocturnas, @HorasNocturnas, @HorasDominicalesFestivos,
                @Comisiones, @Bonificaciones, @OtrosDevengos,
                @DeduccionSalud, @DeduccionPension, @RetencionFuente, @Prestamos, @Embargos, @FondoEmpleados, @OtrasDeducciones,
                @AporteSaludEmpleador, @AportePensionEmpleador, @AporteARL, @AporteCajaCompensacion, @AporteICBF, @AporteSENA,
                @Estado, @AprobadoPorId, @FechaAprobacion, @ComprobanteNumero, @Observaciones,
                1, @FechaCreacion
            );
            SELECT last_insert_rowid();";

        using var connection = _context.CreateConnection();
        entity.Id = await connection.ExecuteScalarAsync<int>(sql, entity);
        _logger.LogInformation("Nómina creada: ID={Id}, Empleado={EmpleadoId}, Periodo={Periodo}", 
            entity.Id, entity.EmpleadoId, entity.Periodo.ToString("yyyy-MM"));
        return entity;
    }

    public async Task UpdateAsync(Nomina entity)
    {
        entity.FechaModificacion = DateTime.Now;
        const string sql = @"
            UPDATE nominas SET
                empleado_id = @EmpleadoId,
                periodo = @Periodo,
                fecha_pago = @FechaPago,
                salario_base = @SalarioBase,
                auxilio_transporte = @AuxilioTransporte,
                horas_extras_diurnas = @HorasExtrasDiurnas,
                horas_extras_nocturnas = @HorasExtrasNocturnas,
                horas_nocturnas = @HorasNocturnas,
                horas_dominicales_festivos = @HorasDominicalesFestivos,
                comisiones = @Comisiones,
                bonificaciones = @Bonificaciones,
                otros_devengos = @OtrosDevengos,
                deduccion_salud = @DeduccionSalud,
                deduccion_pension = @DeduccionPension,
                retencion_fuente = @RetencionFuente,
                prestamos = @Prestamos,
                embargos = @Embargos,
                fondo_empleados = @FondoEmpleados,
                otras_deducciones = @OtrasDeducciones,
                aporte_salud_empleador = @AporteSaludEmpleador,
                aporte_pension_empleador = @AportePensionEmpleador,
                aporte_arl = @AporteARL,
                aporte_caja_compensacion = @AporteCajaCompensacion,
                aporte_icbf = @AporteICBF,
                aporte_sena = @AporteSENA,
                estado = @Estado,
                aprobado_por_id = @AprobadoPorId,
                fecha_aprobacion = @FechaAprobacion,
                comprobante_numero = @ComprobanteNumero,
                observaciones = @Observaciones,
                fecha_modificacion = @FechaModificacion
            WHERE id = @Id";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, entity);
        _logger.LogInformation("Nómina actualizada: ID={Id}", entity.Id);
    }

    public async Task DeleteAsync(int id)
    {
        // Hard delete - elimina permanentemente el registro
        const string sql = "DELETE FROM nominas WHERE id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id });
        _logger.LogInformation("Nómina eliminada: ID={Id}", id);
    }

    public async Task<Nomina?> GetByIdAsync(int id)
    {
        const string sql = "SELECT * FROM nominas WHERE id = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Nomina>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Nomina>> GetAllAsync()
    {
        const string sql = "SELECT * FROM nominas ORDER BY periodo DESC, empleado_id";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Nomina>(sql);
    }

    public async Task<IEnumerable<Nomina>> GetAllActiveAsync()
    {
        const string sql = "SELECT * FROM nominas WHERE activo = 1 ORDER BY periodo DESC, empleado_id";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Nomina>(sql);
    }

    public async Task<IEnumerable<Nomina>> GetByEmpleadoIdAsync(int empleadoId)
    {
        const string sql = @"SELECT * FROM nominas 
            WHERE empleado_id = @EmpleadoId AND activo = 1 
            ORDER BY periodo DESC";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Nomina>(sql, new { EmpleadoId = empleadoId });
    }

    public async Task<IEnumerable<Nomina>> GetByPeriodoAsync(DateTime periodo)
    {
        // Comparar solo año y mes
        var periodoInicio = new DateTime(periodo.Year, periodo.Month, 1);
        var periodoFin = periodoInicio.AddMonths(1).AddDays(-1);
        
        const string sql = @"SELECT * FROM nominas 
            WHERE date(periodo) >= date(@PeriodoInicio) AND date(periodo) <= date(@PeriodoFin) AND activo = 1 
            ORDER BY empleado_id";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Nomina>(sql, new 
        { 
            PeriodoInicio = periodoInicio.ToString("yyyy-MM-dd"), 
            PeriodoFin = periodoFin.ToString("yyyy-MM-dd") 
        });
    }

    public async Task<Nomina?> GetByEmpleadoAndPeriodoAsync(int empleadoId, DateTime periodo)
    {
        var periodoInicio = new DateTime(periodo.Year, periodo.Month, 1);
        var periodoFin = periodoInicio.AddMonths(1).AddDays(-1);
        
        const string sql = @"SELECT * FROM nominas 
            WHERE empleado_id = @EmpleadoId 
            AND date(periodo) >= date(@PeriodoInicio) AND date(periodo) <= date(@PeriodoFin) 
            AND activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Nomina>(sql, new 
        { 
            EmpleadoId = empleadoId, 
            PeriodoInicio = periodoInicio.ToString("yyyy-MM-dd"), 
            PeriodoFin = periodoFin.ToString("yyyy-MM-dd") 
        });
    }

    public async Task<IEnumerable<Nomina>> GetByEstadoAsync(EstadoNomina estado)
    {
        const string sql = @"SELECT * FROM nominas 
            WHERE estado = @Estado AND activo = 1 
            ORDER BY periodo DESC, empleado_id";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Nomina>(sql, new { Estado = (int)estado });
    }

    public async Task<decimal> GetTotalNominaPeriodoAsync(DateTime periodo)
    {
        var periodoInicio = new DateTime(periodo.Year, periodo.Month, 1);
        var periodoFin = periodoInicio.AddMonths(1).AddDays(-1);
        
        const string sql = @"SELECT COALESCE(SUM(
            salario_base + auxilio_transporte + horas_extras_diurnas + horas_extras_nocturnas + 
            horas_nocturnas + horas_dominicales_festivos + comisiones + bonificaciones + otros_devengos -
            deduccion_salud - deduccion_pension - retencion_fuente - prestamos - embargos - fondo_empleados - otras_deducciones
        ), 0) FROM nominas 
            WHERE date(periodo) >= date(@PeriodoInicio) AND date(periodo) <= date(@PeriodoFin) AND activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.ExecuteScalarAsync<decimal>(sql, new 
        { 
            PeriodoInicio = periodoInicio.ToString("yyyy-MM-dd"), 
            PeriodoFin = periodoFin.ToString("yyyy-MM-dd") 
        });
    }

    public async Task<IEnumerable<Nomina>> GetPendientesAprobacionAsync()
    {
        const string sql = @"SELECT * FROM nominas 
            WHERE estado IN (1, 2) AND activo = 1 
            ORDER BY periodo, empleado_id";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Nomina>(sql);
    }

    public async Task<IEnumerable<Nomina>> GetByPeriodoWithEmpleadoAsync(DateTime periodo)
    {
        var periodoInicio = new DateTime(periodo.Year, periodo.Month, 1);
        var periodoFin = periodoInicio.AddMonths(1).AddDays(-1);
        
        const string sql = @"SELECT n.*, e.* FROM nominas n
            INNER JOIN empleados e ON n.empleado_id = e.id
            WHERE date(n.periodo) >= date(@PeriodoInicio) AND date(n.periodo) <= date(@PeriodoFin) AND n.activo = 1 
            ORDER BY e.apellidos, e.nombres";

        using var connection = _context.CreateConnection();
        var result = await connection.QueryAsync<Nomina, Empleado, Nomina>(sql, 
            (nomina, empleado) =>
            {
                nomina.Empleado = empleado;
                return nomina;
            }, 
            new 
            { 
                PeriodoInicio = periodoInicio.ToString("yyyy-MM-dd"), 
                PeriodoFin = periodoFin.ToString("yyyy-MM-dd") 
            },
            splitOn: "id");
        
        return result;
    }

    public async Task<bool> ExisteNominaAsync(int empleadoId, DateTime periodo)
    {
        var periodoInicio = new DateTime(periodo.Year, periodo.Month, 1);
        var periodoFin = periodoInicio.AddMonths(1).AddDays(-1);
        
        const string sql = @"SELECT COUNT(1) FROM nominas 
            WHERE empleado_id = @EmpleadoId 
            AND date(periodo) >= date(@PeriodoInicio) AND date(periodo) <= date(@PeriodoFin) 
            AND activo = 1";
        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new 
        { 
            EmpleadoId = empleadoId, 
            PeriodoInicio = periodoInicio.ToString("yyyy-MM-dd"), 
            PeriodoFin = periodoFin.ToString("yyyy-MM-dd") 
        });
        return count > 0;
    }

    public async Task<(decimal TotalDevengado, decimal TotalDeducciones, decimal TotalNeto, decimal TotalAportesPatronales)> GetResumenPeriodoAsync(DateTime periodo)
    {
        var periodoInicio = new DateTime(periodo.Year, periodo.Month, 1);
        var periodoFin = periodoInicio.AddMonths(1).AddDays(-1);
        
        const string sql = @"SELECT 
            COALESCE(SUM(salario_base + auxilio_transporte + horas_extras_diurnas + horas_extras_nocturnas + 
                        horas_nocturnas + horas_dominicales_festivos + comisiones + bonificaciones + otros_devengos), 0) as TotalDevengado,
            COALESCE(SUM(deduccion_salud + deduccion_pension + retencion_fuente + prestamos + embargos + fondo_empleados + otras_deducciones), 0) as TotalDeducciones,
            COALESCE(SUM(aporte_salud_empleador + aporte_pension_empleador + aporte_arl + aporte_caja_compensacion + aporte_icbf + aporte_sena), 0) as TotalAportesPatronales
        FROM nominas 
        WHERE date(periodo) >= date(@PeriodoInicio) AND date(periodo) <= date(@PeriodoFin) AND activo = 1";
        
        using var connection = _context.CreateConnection();
        var result = await connection.QuerySingleAsync(sql, new 
        { 
            PeriodoInicio = periodoInicio.ToString("yyyy-MM-dd"), 
            PeriodoFin = periodoFin.ToString("yyyy-MM-dd") 
        });
        
        decimal totalDevengado = (decimal)result.TotalDevengado;
        decimal totalDeducciones = (decimal)result.TotalDeducciones;
        decimal totalAportesPatronales = (decimal)result.TotalAportesPatronales;
        decimal totalNeto = totalDevengado - totalDeducciones;
        
        return (totalDevengado, totalDeducciones, totalNeto, totalAportesPatronales);
    }

    public Task<int> SaveChangesAsync() => Task.FromResult(0);
}
