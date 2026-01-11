using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.DTOs;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Infrastructure.Data;
using SGRRHH.Local.Shared.Interfaces;
using System.Text.Json;

namespace SGRRHH.Local.Infrastructure.Repositories;

/// <summary>
/// Repositorio para gestión de incapacidades
/// </summary>
public class IncapacidadRepository : IIncapacidadRepository
{
    private readonly DapperContext _context;
    private readonly ILogger<IncapacidadRepository> _logger;
    private readonly ISeguimientoIncapacidadRepository _seguimientoRepo;

    public IncapacidadRepository(
        DapperContext context, 
        ILogger<IncapacidadRepository> logger,
        ISeguimientoIncapacidadRepository seguimientoRepo)
    {
        _context = context;
        _logger = logger;
        _seguimientoRepo = seguimientoRepo;
    }

    // ===== CRUD BÁSICO =====

    public async Task<Incapacidad> AddAsync(Incapacidad entity)
    {
        entity.FechaCreacion = DateTime.Now;
        
        const string sql = @"
            INSERT INTO incapacidades (
                numero_incapacidad, empleado_id, permiso_origen_id, incapacidad_anterior_id, es_prorroga,
                fecha_inicio, fecha_fin, total_dias, fecha_expedicion,
                diagnostico_cie10, diagnostico_descripcion, tipo_incapacidad,
                entidad_emisora, entidad_pagadora, dias_empresa, dias_eps_arl,
                porcentaje_pago, valor_dia_base, valor_total_cobrar, estado,
                transcrita, fecha_transcripcion, numero_radicado_eps,
                cobrada, fecha_cobro, valor_cobrado,
                documento_incapacidad_path, documento_transcripcion_path, observaciones,
                registrado_por_id, activo, fecha_creacion
            ) VALUES (
                @NumeroIncapacidad, @EmpleadoId, @PermisoOrigenId, @IncapacidadAnteriorId, @EsProrroga,
                @FechaInicio, @FechaFin, @TotalDias, @FechaExpedicion,
                @DiagnosticoCIE10, @DiagnosticoDescripcion, @TipoIncapacidad,
                @EntidadEmisora, @EntidadPagadora, @DiasEmpresa, @DiasEpsArl,
                @PorcentajePago, @ValorDiaBase, @ValorTotalCobrar, @Estado,
                @Transcrita, @FechaTranscripcion, @NumeroRadicadoEps,
                @Cobrada, @FechaCobro, @ValorCobrado,
                @DocumentoIncapacidadPath, @DocumentoTranscripcionPath, @Observaciones,
                @RegistradoPorId, @Activo, @FechaCreacion
            );
            SELECT last_insert_rowid();";

        using var connection = _context.CreateConnection();
        entity.Id = await connection.ExecuteScalarAsync<int>(sql, entity);
        
        _logger.LogInformation("Incapacidad creada: {Numero} para empleado {EmpleadoId}", 
            entity.NumeroIncapacidad, entity.EmpleadoId);
        
        return entity;
    }

    public async Task UpdateAsync(Incapacidad entity)
    {
        entity.FechaModificacion = DateTime.Now;
        
        const string sql = @"
            UPDATE incapacidades SET
                empleado_id = @EmpleadoId,
                permiso_origen_id = @PermisoOrigenId,
                incapacidad_anterior_id = @IncapacidadAnteriorId,
                es_prorroga = @EsProrroga,
                fecha_inicio = @FechaInicio,
                fecha_fin = @FechaFin,
                total_dias = @TotalDias,
                fecha_expedicion = @FechaExpedicion,
                diagnostico_cie10 = @DiagnosticoCIE10,
                diagnostico_descripcion = @DiagnosticoDescripcion,
                tipo_incapacidad = @TipoIncapacidad,
                entidad_emisora = @EntidadEmisora,
                entidad_pagadora = @EntidadPagadora,
                dias_empresa = @DiasEmpresa,
                dias_eps_arl = @DiasEpsArl,
                porcentaje_pago = @PorcentajePago,
                valor_dia_base = @ValorDiaBase,
                valor_total_cobrar = @ValorTotalCobrar,
                estado = @Estado,
                transcrita = @Transcrita,
                fecha_transcripcion = @FechaTranscripcion,
                numero_radicado_eps = @NumeroRadicadoEps,
                cobrada = @Cobrada,
                fecha_cobro = @FechaCobro,
                valor_cobrado = @ValorCobrado,
                documento_incapacidad_path = @DocumentoIncapacidadPath,
                documento_transcripcion_path = @DocumentoTranscripcionPath,
                observaciones = @Observaciones,
                fecha_modificacion = @FechaModificacion
            WHERE id = @Id";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, entity);
        
        _logger.LogInformation("Incapacidad actualizada: {Id}", entity.Id);
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "UPDATE incapacidades SET activo = 0, fecha_modificacion = @FechaMod WHERE id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id, FechaMod = DateTime.Now });
    }

    public async Task<Incapacidad?> GetByIdAsync(int id)
    {
        const string sql = "SELECT * FROM incapacidades WHERE id = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Incapacidad>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Incapacidad>> GetAllAsync()
    {
        const string sql = "SELECT * FROM incapacidades ORDER BY fecha_inicio DESC";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Incapacidad>(sql);
    }

    public async Task<IEnumerable<Incapacidad>> GetAllActiveAsync()
    {
        const string sql = "SELECT * FROM incapacidades WHERE activo = 1 ORDER BY fecha_inicio DESC";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Incapacidad>(sql);
    }

    // ===== CONSULTAS BÁSICAS =====

    public async Task<Incapacidad?> GetByNumeroAsync(string numeroIncapacidad)
    {
        const string sql = "SELECT * FROM incapacidades WHERE numero_incapacidad = @Numero";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Incapacidad>(sql, new { Numero = numeroIncapacidad });
    }

    public async Task<IEnumerable<Incapacidad>> GetByEmpleadoIdAsync(int empleadoId)
    {
        const string sql = @"SELECT * FROM incapacidades 
            WHERE empleado_id = @EmpleadoId AND activo = 1 
            ORDER BY fecha_inicio DESC";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Incapacidad>(sql, new { EmpleadoId = empleadoId });
    }

    public async Task<IEnumerable<Incapacidad>> GetByEstadoAsync(EstadoIncapacidad estado)
    {
        const string sql = @"SELECT * FROM incapacidades 
            WHERE estado = @Estado AND activo = 1 
            ORDER BY fecha_inicio DESC";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Incapacidad>(sql, new { Estado = estado });
    }

    public async Task<IEnumerable<Incapacidad>> GetByTipoAsync(TipoIncapacidad tipo)
    {
        const string sql = @"SELECT * FROM incapacidades 
            WHERE tipo_incapacidad = @Tipo AND activo = 1 
            ORDER BY fecha_inicio DESC";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Incapacidad>(sql, new { Tipo = tipo });
    }

    public async Task<IEnumerable<Incapacidad>> GetByRangoFechasAsync(DateTime desde, DateTime hasta)
    {
        const string sql = @"SELECT * FROM incapacidades 
            WHERE fecha_inicio >= @Desde AND fecha_fin <= @Hasta AND activo = 1 
            ORDER BY fecha_inicio DESC";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Incapacidad>(sql, new { Desde = desde, Hasta = hasta });
    }

    public async Task<IEnumerable<IncapacidadResumen>> GetAllWithFiltersAsync(
        int? empleadoId = null,
        EstadoIncapacidad? estado = null,
        TipoIncapacidad? tipo = null,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        bool? soloActivas = null,
        bool? soloPendientesTranscripcion = null,
        bool? soloPendientesCobro = null)
    {
        var sql = @"
            SELECT 
                i.id as Id,
                i.numero_incapacidad as NumeroIncapacidad,
                e.nombres || ' ' || e.apellidos as EmpleadoNombre,
                e.cedula as EmpleadoCedula,
                i.empleado_id as EmpleadoId,
                i.fecha_inicio as FechaInicio,
                i.fecha_fin as FechaFin,
                i.total_dias as TotalDias,
                i.tipo_incapacidad as Tipo,
                i.estado as Estado,
                i.transcrita as Transcrita,
                i.cobrada as Cobrada,
                i.valor_total_cobrar as ValorPorCobrar,
                i.diagnostico_descripcion as DiagnosticoDescripcion,
                i.entidad_emisora as EntidadEmisora,
                i.entidad_pagadora as EntidadPagadora,
                i.es_prorroga as EsProrroga
            FROM incapacidades i
            LEFT JOIN empleados e ON i.empleado_id = e.id
            WHERE i.activo = 1";

        if (empleadoId.HasValue) sql += " AND i.empleado_id = @EmpleadoId";
        if (estado.HasValue) sql += " AND i.estado = @Estado";
        if (tipo.HasValue) sql += " AND i.tipo_incapacidad = @Tipo";
        if (fechaDesde.HasValue) sql += " AND i.fecha_inicio >= @FechaDesde";
        if (fechaHasta.HasValue) sql += " AND i.fecha_fin <= @FechaHasta";
        if (soloActivas == true) sql += " AND i.estado = 1"; // Activa
        if (soloPendientesTranscripcion == true) sql += " AND i.transcrita = 0 AND i.estado NOT IN (5)"; // No cancelada
        if (soloPendientesCobro == true) sql += " AND i.transcrita = 1 AND i.cobrada = 0 AND i.estado NOT IN (5)";

        sql += " ORDER BY i.fecha_inicio DESC";

        using var connection = _context.CreateConnection();
        var items = await connection.QueryAsync<IncapacidadResumen>(sql, new
        {
            EmpleadoId = empleadoId,
            Estado = estado,
            Tipo = tipo,
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta
        });

        // Calcular días restantes
        var hoy = DateTime.Today;
        foreach (var item in items)
        {
            item.DiasRestantes = Math.Max(0, (item.FechaFin.Date - hoy).Days);
        }

        return items;
    }

    // ===== CONSULTAS PARA GESTIÓN =====

    public async Task<IEnumerable<IncapacidadResumen>> GetActivasAsync()
    {
        return await GetAllWithFiltersAsync(soloActivas: true);
    }

    public async Task<IEnumerable<IncapacidadResumen>> GetPendientesTranscripcionAsync()
    {
        return await GetAllWithFiltersAsync(soloPendientesTranscripcion: true);
    }

    public async Task<IEnumerable<IncapacidadResumen>> GetPendientesCobroAsync()
    {
        return await GetAllWithFiltersAsync(soloPendientesCobro: true);
    }

    public async Task<IEnumerable<IncapacidadResumen>> GetProximasVencerAsync(int dias = 3)
    {
        var hoy = DateTime.Today;
        var limite = hoy.AddDays(dias);
        
        var todas = await GetAllWithFiltersAsync(soloActivas: true);
        return todas.Where(i => i.FechaFin.Date >= hoy && i.FechaFin.Date <= limite).ToList();
    }

    // ===== PRÓRROGAS =====

    public async Task<IEnumerable<Incapacidad>> GetProrrogasAsync(int incapacidadOrigenId)
    {
        const string sql = @"
            WITH RECURSIVE prorrogas AS (
                SELECT * FROM incapacidades WHERE id = @Id
                UNION ALL
                SELECT i.* FROM incapacidades i
                INNER JOIN prorrogas p ON i.incapacidad_anterior_id = p.id
            )
            SELECT * FROM prorrogas WHERE id != @Id AND activo = 1 ORDER BY fecha_inicio";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Incapacidad>(sql, new { Id = incapacidadOrigenId });
    }

    public async Task<int> GetTotalDiasAcumuladosAsync(int incapacidadOrigenId)
    {
        var incapacidad = await GetByIdAsync(incapacidadOrigenId);
        if (incapacidad == null) return 0;
        
        var totalDias = incapacidad.TotalDias;
        var prorrogas = await GetProrrogasAsync(incapacidadOrigenId);
        totalDias += prorrogas.Sum(p => p.TotalDias);
        
        return totalDias;
    }

    // ===== ESTADÍSTICAS =====

    public async Task<EstadisticasIncapacidades> GetEstadisticasAsync()
    {
        var stats = new EstadisticasIncapacidades();
        var hoy = DateTime.Today;
        var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
        var finMes = inicioMes.AddMonths(1).AddDays(-1);

        using var connection = _context.CreateConnection();

        // Total activas
        stats.TotalActivas = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM incapacidades WHERE estado = 1 AND activo = 1");

        // Pendientes transcribir
        stats.TotalPendientesTranscribir = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM incapacidades WHERE transcrita = 0 AND estado NOT IN (5) AND activo = 1");

        // Pendientes cobro
        stats.TotalPendientesCobro = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM incapacidades WHERE transcrita = 1 AND cobrada = 0 AND estado NOT IN (5) AND activo = 1");

        // Finalizadas este mes
        stats.TotalFinalizadasMes = await connection.ExecuteScalarAsync<int>(
            @"SELECT COUNT(*) FROM incapacidades 
              WHERE estado = 2 AND fecha_fin >= @InicioMes AND fecha_fin <= @FinMes AND activo = 1",
            new { InicioMes = inicioMes, FinMes = finMes });

        // Total por cobrar
        stats.TotalPorCobrar = await connection.ExecuteScalarAsync<decimal>(
            @"SELECT COALESCE(SUM(valor_total_cobrar), 0) FROM incapacidades 
              WHERE transcrita = 1 AND cobrada = 0 AND estado NOT IN (5) AND activo = 1");

        // Total cobrado este mes
        stats.TotalCobradoMes = await connection.ExecuteScalarAsync<decimal>(
            @"SELECT COALESCE(SUM(valor_cobrado), 0) FROM incapacidades 
              WHERE cobrada = 1 AND fecha_cobro >= @InicioMes AND fecha_cobro <= @FinMes AND activo = 1",
            new { InicioMes = inicioMes, FinMes = finMes });

        // Total días incapacidad este mes
        stats.TotalDiasIncapacidadMes = await connection.ExecuteScalarAsync<int>(
            @"SELECT COALESCE(SUM(total_dias), 0) FROM incapacidades 
              WHERE fecha_inicio >= @InicioMes AND fecha_inicio <= @FinMes AND activo = 1",
            new { InicioMes = inicioMes, FinMes = finMes });

        // Incapacidades activas (resumen)
        stats.IncapacidadesActivas = (await GetActivasAsync()).ToList();

        // Próximas a vencer
        stats.ProximasVencer = (await GetProximasVencerAsync(3)).ToList();

        return stats;
    }

    public async Task<decimal> GetTotalPorCobrarAsync()
    {
        const string sql = @"
            SELECT COALESCE(SUM(valor_total_cobrar), 0) FROM incapacidades 
            WHERE transcrita = 1 AND cobrada = 0 AND estado NOT IN (5) AND activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.ExecuteScalarAsync<decimal>(sql);
    }

    public async Task<int> GetTotalDiasIncapacidadMesAsync(int año, int mes)
    {
        var inicioMes = new DateTime(año, mes, 1);
        var finMes = inicioMes.AddMonths(1).AddDays(-1);
        
        const string sql = @"
            SELECT COALESCE(SUM(total_dias), 0) FROM incapacidades 
            WHERE fecha_inicio >= @InicioMes AND fecha_inicio <= @FinMes AND activo = 1";
        
        using var connection = _context.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, new { InicioMes = inicioMes, FinMes = finMes });
    }

    // ===== OPERACIONES ESPECIALES =====

    public async Task<string> GenerarNumeroIncapacidadAsync()
    {
        var año = DateTime.Now.Year;
        const string sql = @"
            SELECT numero_incapacidad FROM incapacidades 
            WHERE substr(numero_incapacidad, 5, 4) = @AñoText 
            ORDER BY numero_incapacidad DESC LIMIT 1";
        
        using var connection = _context.CreateConnection();
        var ultimo = await connection.QuerySingleOrDefaultAsync<string>(sql, new { AñoText = año.ToString() });
        
        var secuencia = 1;
        if (!string.IsNullOrWhiteSpace(ultimo))
        {
            var partes = ultimo.Split('-');
            if (partes.Length == 3 && int.TryParse(partes[2], out var actual))
            {
                secuencia = actual + 1;
            }
        }
        
        return $"INC-{año}-{secuencia:0000}";
    }

    public async Task<Incapacidad> CrearDesdePermisoAsync(int permisoId, CrearIncapacidadDto dto, int usuarioId)
    {
        using var connection = _context.CreateConnection();
        using var transaction = connection.BeginTransaction();
        
        try
        {
            // Obtener permiso
            var permiso = await connection.QuerySingleOrDefaultAsync<Permiso>(
                "SELECT * FROM permisos WHERE id = @Id", new { Id = permisoId }, transaction);
            
            if (permiso == null)
                throw new InvalidOperationException($"Permiso {permisoId} no encontrado");
            
            // Crear incapacidad
            var incapacidad = new Incapacidad
            {
                NumeroIncapacidad = await GenerarNumeroIncapacidadAsync(),
                EmpleadoId = dto.EmpleadoId,
                PermisoOrigenId = permisoId,
                EsProrroga = false,
                FechaInicio = dto.FechaInicio,
                FechaFin = dto.FechaFin,
                TotalDias = (int)(dto.FechaFin.Date - dto.FechaInicio.Date).TotalDays + 1,
                FechaExpedicion = dto.FechaExpedicion,
                DiagnosticoCIE10 = dto.DiagnosticoCIE10,
                DiagnosticoDescripcion = dto.DiagnosticoDescripcion,
                TipoIncapacidad = dto.TipoIncapacidad,
                EntidadEmisora = dto.EntidadEmisora,
                EntidadPagadora = dto.EntidadPagadora,
                DocumentoIncapacidadPath = dto.DocumentoPath,
                Observaciones = dto.Observaciones,
                RegistradoPorId = usuarioId,
                Estado = EstadoIncapacidad.Activa,
                Activo = true,
                FechaCreacion = DateTime.Now
            };
            
            // Calcular distribución de días
            incapacidad.CalcularDistribucionDias();
            
            // Insertar incapacidad
            const string sqlInsert = @"
                INSERT INTO incapacidades (
                    numero_incapacidad, empleado_id, permiso_origen_id, incapacidad_anterior_id, es_prorroga,
                    fecha_inicio, fecha_fin, total_dias, fecha_expedicion,
                    diagnostico_cie10, diagnostico_descripcion, tipo_incapacidad,
                    entidad_emisora, entidad_pagadora, dias_empresa, dias_eps_arl,
                    porcentaje_pago, valor_dia_base, valor_total_cobrar, estado,
                    transcrita, cobrada, documento_incapacidad_path, observaciones,
                    registrado_por_id, activo, fecha_creacion
                ) VALUES (
                    @NumeroIncapacidad, @EmpleadoId, @PermisoOrigenId, @IncapacidadAnteriorId, @EsProrroga,
                    @FechaInicio, @FechaFin, @TotalDias, @FechaExpedicion,
                    @DiagnosticoCIE10, @DiagnosticoDescripcion, @TipoIncapacidad,
                    @EntidadEmisora, @EntidadPagadora, @DiasEmpresa, @DiasEpsArl,
                    @PorcentajePago, @ValorDiaBase, @ValorTotalCobrar, @Estado,
                    @Transcrita, @Cobrada, @DocumentoIncapacidadPath, @Observaciones,
                    @RegistradoPorId, @Activo, @FechaCreacion
                );
                SELECT last_insert_rowid();";
            
            incapacidad.Id = await connection.ExecuteScalarAsync<int>(sqlInsert, incapacidad, transaction);
            
            // Actualizar permiso
            const string sqlUpdatePermiso = @"
                UPDATE permisos SET 
                    incapacidad_id = @IncapacidadId, 
                    convertido_a_incapacidad = 1,
                    fecha_modificacion = @FechaMod
                WHERE id = @PermisoId";
            
            await connection.ExecuteAsync(sqlUpdatePermiso, new 
            { 
                IncapacidadId = incapacidad.Id, 
                PermisoId = permisoId,
                FechaMod = DateTime.Now 
            }, transaction);
            
            transaction.Commit();
            
            // Registrar seguimiento
            await _seguimientoRepo.RegistrarAccionAsync(
                incapacidad.Id,
                TipoAccionSeguimientoIncapacidad.ConversionDesdePermiso,
                $"Incapacidad creada desde permiso {permiso.NumeroActa}",
                usuarioId,
                JsonSerializer.Serialize(new { PermisoId = permisoId, PermisoNumero = permiso.NumeroActa }));
            
            _logger.LogInformation("Incapacidad {Numero} creada desde permiso {PermisoId}", 
                incapacidad.NumeroIncapacidad, permisoId);
            
            return incapacidad;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.LogError(ex, "Error al crear incapacidad desde permiso {PermisoId}", permisoId);
            throw;
        }
    }

    public async Task<Incapacidad> CrearProrrogaAsync(int incapacidadAnteriorId, CrearProrrogaDto dto, int usuarioId)
    {
        var incapacidadAnterior = await GetByIdAsync(incapacidadAnteriorId);
        if (incapacidadAnterior == null)
            throw new InvalidOperationException($"Incapacidad {incapacidadAnteriorId} no encontrada");
        
        var incapacidad = new Incapacidad
        {
            NumeroIncapacidad = await GenerarNumeroIncapacidadAsync(),
            EmpleadoId = incapacidadAnterior.EmpleadoId,
            IncapacidadAnteriorId = incapacidadAnteriorId,
            EsProrroga = true,
            FechaInicio = dto.FechaInicio,
            FechaFin = dto.FechaFin,
            TotalDias = (int)(dto.FechaFin.Date - dto.FechaInicio.Date).TotalDays + 1,
            FechaExpedicion = dto.FechaExpedicion,
            DiagnosticoCIE10 = dto.DiagnosticoCIE10,
            DiagnosticoDescripcion = dto.DiagnosticoDescripcion,
            TipoIncapacidad = incapacidadAnterior.TipoIncapacidad, // Mismo tipo
            EntidadEmisora = dto.EntidadEmisora,
            EntidadPagadora = incapacidadAnterior.EntidadPagadora,
            DocumentoIncapacidadPath = dto.DocumentoPath,
            Observaciones = dto.Observaciones,
            RegistradoPorId = usuarioId,
            Estado = EstadoIncapacidad.Activa,
            Activo = true,
            FechaCreacion = DateTime.Now
        };
        
        // Para prórrogas después de los primeros 2 días, todo va a EPS/ARL
        var diasAcumulados = await GetTotalDiasAcumuladosAsync(incapacidadAnteriorId);
        if (diasAcumulados >= 2)
        {
            incapacidad.DiasEmpresa = 0;
            incapacidad.DiasEpsArl = incapacidad.TotalDias;
        }
        else
        {
            incapacidad.CalcularDistribucionDias();
        }
        
        await AddAsync(incapacidad);
        
        // Registrar seguimiento en ambas incapacidades
        await _seguimientoRepo.RegistrarAccionAsync(
            incapacidad.Id,
            TipoAccionSeguimientoIncapacidad.Registro,
            $"Prórroga de incapacidad {incapacidadAnterior.NumeroIncapacidad}",
            usuarioId);
        
        await _seguimientoRepo.RegistrarAccionAsync(
            incapacidadAnteriorId,
            TipoAccionSeguimientoIncapacidad.Prorroga,
            $"Se registró prórroga: {incapacidad.NumeroIncapacidad}",
            usuarioId);
        
        _logger.LogInformation("Prórroga {Numero} creada para incapacidad {AnteriorId}", 
            incapacidad.NumeroIncapacidad, incapacidadAnteriorId);
        
        return incapacidad;
    }

    public async Task RegistrarTranscripcionAsync(RegistrarTranscripcionDto dto, int usuarioId)
    {
        var incapacidad = await GetByIdAsync(dto.IncapacidadId);
        if (incapacidad == null)
            throw new InvalidOperationException($"Incapacidad {dto.IncapacidadId} no encontrada");
        
        incapacidad.Transcrita = true;
        incapacidad.FechaTranscripcion = dto.FechaTranscripcion;
        incapacidad.NumeroRadicadoEps = dto.NumeroRadicado;
        incapacidad.DocumentoTranscripcionPath = dto.DocumentoPath;
        incapacidad.Estado = EstadoIncapacidad.Transcrita;
        
        await UpdateAsync(incapacidad);
        
        await _seguimientoRepo.RegistrarAccionAsync(
            dto.IncapacidadId,
            TipoAccionSeguimientoIncapacidad.Transcripcion,
            $"Transcrita ante EPS. Radicado: {dto.NumeroRadicado ?? "N/A"}",
            usuarioId,
            dto.Observaciones);
        
        _logger.LogInformation("Transcripción registrada para incapacidad {Id}, radicado {Radicado}", 
            dto.IncapacidadId, dto.NumeroRadicado);
    }

    public async Task RegistrarCobroAsync(RegistrarCobroDto dto, int usuarioId)
    {
        var incapacidad = await GetByIdAsync(dto.IncapacidadId);
        if (incapacidad == null)
            throw new InvalidOperationException($"Incapacidad {dto.IncapacidadId} no encontrada");
        
        incapacidad.Cobrada = true;
        incapacidad.FechaCobro = dto.FechaCobro;
        incapacidad.ValorCobrado = dto.ValorCobrado;
        incapacidad.Estado = EstadoIncapacidad.Cobrada;
        
        await UpdateAsync(incapacidad);
        
        await _seguimientoRepo.RegistrarAccionAsync(
            dto.IncapacidadId,
            TipoAccionSeguimientoIncapacidad.Cobro,
            $"Cobro registrado: ${dto.ValorCobrado:N0}",
            usuarioId,
            dto.Observaciones);
        
        _logger.LogInformation("Cobro registrado para incapacidad {Id}, valor {Valor}", 
            dto.IncapacidadId, dto.ValorCobrado);
    }

    public async Task FinalizarAsync(int incapacidadId, int usuarioId, string? observaciones = null)
    {
        var incapacidad = await GetByIdAsync(incapacidadId);
        if (incapacidad == null)
            throw new InvalidOperationException($"Incapacidad {incapacidadId} no encontrada");
        
        incapacidad.Estado = EstadoIncapacidad.Finalizada;
        await UpdateAsync(incapacidad);
        
        await _seguimientoRepo.RegistrarAccionAsync(
            incapacidadId,
            TipoAccionSeguimientoIncapacidad.Finalizacion,
            observaciones ?? "Incapacidad finalizada",
            usuarioId);
        
        _logger.LogInformation("Incapacidad {Id} finalizada", incapacidadId);
    }

    public async Task CancelarAsync(int incapacidadId, int usuarioId, string motivo)
    {
        var incapacidad = await GetByIdAsync(incapacidadId);
        if (incapacidad == null)
            throw new InvalidOperationException($"Incapacidad {incapacidadId} no encontrada");
        
        incapacidad.Estado = EstadoIncapacidad.Cancelada;
        incapacidad.Observaciones = $"{incapacidad.Observaciones}\n[CANCELADA] {motivo}".Trim();
        await UpdateAsync(incapacidad);
        
        await _seguimientoRepo.RegistrarAccionAsync(
            incapacidadId,
            TipoAccionSeguimientoIncapacidad.Observacion,
            $"CANCELADA: {motivo}",
            usuarioId);
        
        _logger.LogWarning("Incapacidad {Id} cancelada: {Motivo}", incapacidadId, motivo);
    }

    // ===== DETALLE =====

    public async Task<IncapacidadDetalleDto?> GetDetalleAsync(int incapacidadId)
    {
        const string sql = @"
            SELECT 
                i.*,
                e.nombres || ' ' || e.apellidos as EmpleadoNombre,
                e.cedula as EmpleadoCedula,
                c.nombre as EmpleadoCargo,
                d.nombre as EmpleadoDepartamento,
                cont.salario as EmpleadoSalario,
                u.nombre_completo as RegistradoPorNombre,
                p.numero_acta as PermisoOrigenNumero,
                ia.numero_incapacidad as IncapacidadAnteriorNumero
            FROM incapacidades i
            LEFT JOIN empleados e ON i.empleado_id = e.id
            LEFT JOIN contratos cont ON cont.empleado_id = e.id AND cont.activo = 1
            LEFT JOIN cargos c ON cont.cargo_id = c.id
            LEFT JOIN departamentos d ON c.departamento_id = d.id
            LEFT JOIN usuarios u ON i.registrado_por_id = u.id
            LEFT JOIN permisos p ON i.permiso_origen_id = p.id
            LEFT JOIN incapacidades ia ON i.incapacidad_anterior_id = ia.id
            WHERE i.id = @Id";
        
        using var connection = _context.CreateConnection();
        var resultado = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { Id = incapacidadId });
        
        if (resultado == null) return null;
        
        var detalle = new IncapacidadDetalleDto
        {
            Id = resultado.id,
            NumeroIncapacidad = resultado.numero_incapacidad,
            EmpleadoId = resultado.empleado_id,
            EmpleadoNombre = resultado.EmpleadoNombre ?? "",
            EmpleadoCedula = resultado.EmpleadoCedula ?? "",
            EmpleadoCargo = resultado.EmpleadoCargo ?? "",
            EmpleadoDepartamento = resultado.EmpleadoDepartamento ?? "",
            EmpleadoSalario = resultado.EmpleadoSalario,
            PermisoOrigenId = resultado.permiso_origen_id,
            PermisoOrigenNumero = resultado.PermisoOrigenNumero,
            IncapacidadAnteriorId = resultado.incapacidad_anterior_id,
            IncapacidadAnteriorNumero = resultado.IncapacidadAnteriorNumero,
            EsProrroga = resultado.es_prorroga == 1,
            FechaInicio = DateTime.Parse(resultado.fecha_inicio),
            FechaFin = DateTime.Parse(resultado.fecha_fin),
            TotalDias = resultado.total_dias,
            FechaExpedicion = DateTime.Parse(resultado.fecha_expedicion),
            DiagnosticoCIE10 = resultado.diagnostico_cie10,
            DiagnosticoDescripcion = resultado.diagnostico_descripcion ?? "",
            TipoIncapacidad = (TipoIncapacidad)resultado.tipo_incapacidad,
            EntidadEmisora = resultado.entidad_emisora ?? "",
            EntidadPagadora = resultado.entidad_pagadora,
            DiasEmpresa = resultado.dias_empresa,
            DiasEpsArl = resultado.dias_eps_arl,
            PorcentajePago = resultado.porcentaje_pago,
            ValorDiaBase = resultado.valor_dia_base,
            ValorTotalCobrar = resultado.valor_total_cobrar,
            Estado = (EstadoIncapacidad)resultado.estado,
            Transcrita = resultado.transcrita == 1,
            FechaTranscripcion = resultado.fecha_transcripcion != null ? DateTime.Parse(resultado.fecha_transcripcion) : null,
            NumeroRadicadoEps = resultado.numero_radicado_eps,
            Cobrada = resultado.cobrada == 1,
            FechaCobro = resultado.fecha_cobro != null ? DateTime.Parse(resultado.fecha_cobro) : null,
            ValorCobrado = resultado.valor_cobrado,
            DocumentoIncapacidadPath = resultado.documento_incapacidad_path,
            DocumentoTranscripcionPath = resultado.documento_transcripcion_path,
            Observaciones = resultado.observaciones,
            RegistradoPorId = resultado.registrado_por_id,
            RegistradoPorNombre = resultado.RegistradoPorNombre ?? "",
            FechaCreacion = DateTime.Parse(resultado.fecha_creacion)
        };
        
        detalle.DiasRestantes = Math.Max(0, (detalle.FechaFin.Date - DateTime.Today).Days);
        detalle.TipoNombre = detalle.TipoIncapacidad switch
        {
            TipoIncapacidad.EnfermedadGeneral => "Enfermedad General",
            TipoIncapacidad.AccidenteTrabajo => "Accidente de Trabajo",
            TipoIncapacidad.EnfermedadLaboral => "Enfermedad Laboral",
            TipoIncapacidad.LicenciaMaternidad => "Licencia Maternidad",
            TipoIncapacidad.LicenciaPaternidad => "Licencia Paternidad",
            _ => "Desconocido"
        };
        detalle.EstadoNombre = detalle.Estado switch
        {
            EstadoIncapacidad.Activa => "Activa",
            EstadoIncapacidad.Finalizada => "Finalizada",
            EstadoIncapacidad.Transcrita => "Transcrita",
            EstadoIncapacidad.Cobrada => "Cobrada",
            EstadoIncapacidad.Cancelada => "Cancelada",
            _ => "Desconocido"
        };
        
        // Obtener prórrogas
        var prorrogas = await GetProrrogasAsync(incapacidadId);
        detalle.Prorrogas = prorrogas.Select(p => new IncapacidadResumen
        {
            Id = p.Id,
            NumeroIncapacidad = p.NumeroIncapacidad,
            FechaInicio = p.FechaInicio,
            FechaFin = p.FechaFin,
            TotalDias = p.TotalDias,
            Estado = p.Estado,
            EsProrroga = true
        }).ToList();
        
        detalle.TotalDiasAcumulados = detalle.TotalDias + detalle.Prorrogas.Sum(p => p.TotalDias);
        
        // Obtener seguimiento
        detalle.Seguimientos = (await _seguimientoRepo.GetHistorialAsync(incapacidadId)).ToList();
        
        return detalle;
    }

    // ===== REPORTES =====

    public async Task<IEnumerable<Incapacidad>> GetParaReporteCobroAsync(int año, int mes)
    {
        var inicioMes = new DateTime(año, mes, 1);
        var finMes = inicioMes.AddMonths(1).AddDays(-1);
        
        const string sql = @"
            SELECT * FROM incapacidades 
            WHERE dias_eps_arl > 0 
              AND fecha_inicio >= @InicioMes AND fecha_inicio <= @FinMes
              AND estado NOT IN (5)
              AND activo = 1
            ORDER BY fecha_inicio";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Incapacidad>(sql, new { InicioMes = inicioMes, FinMes = finMes });
    }

    public async Task<ReporteCobroEpsDto> GetReporteCobroAsync(int año, int mes)
    {
        var inicioMes = new DateTime(año, mes, 1);
        var finMes = inicioMes.AddMonths(1).AddDays(-1);
        
        const string sql = @"
            SELECT 
                i.numero_incapacidad as NumeroIncapacidad,
                e.cedula as EmpleadoCedula,
                e.nombres || ' ' || e.apellidos as EmpleadoNombre,
                c.nombre as Cargo,
                i.fecha_inicio as FechaInicio,
                i.fecha_fin as FechaFin,
                i.total_dias as TotalDias,
                i.dias_eps_arl as DiasEpsArl,
                i.tipo_incapacidad as TipoIncapacidadInt,
                i.diagnostico_descripcion as DiagnosticoDescripcion,
                COALESCE(i.entidad_pagadora, 'Sin definir') as EntidadPagadora,
                COALESCE(i.valor_dia_base, 0) as ValorDiaBase,
                i.porcentaje_pago as PorcentajePago,
                COALESCE(i.valor_total_cobrar, 0) as ValorCobrar,
                i.transcrita as Transcrita,
                i.numero_radicado_eps as NumeroRadicado
            FROM incapacidades i
            LEFT JOIN empleados e ON i.empleado_id = e.id
            LEFT JOIN contratos cont ON cont.empleado_id = e.id AND cont.activo = 1
            LEFT JOIN cargos c ON cont.cargo_id = c.id
            WHERE i.dias_eps_arl > 0 
              AND i.fecha_inicio >= @InicioMes AND i.fecha_inicio <= @FinMes
              AND i.estado NOT IN (5)
              AND i.activo = 1
            ORDER BY i.fecha_inicio";
        
        using var connection = _context.CreateConnection();
        var items = (await connection.QueryAsync<dynamic>(sql, new { InicioMes = inicioMes, FinMes = finMes }))
            .Select(r => new ItemReporteCobroDto
            {
                NumeroIncapacidad = r.NumeroIncapacidad,
                EmpleadoCedula = r.EmpleadoCedula ?? "",
                EmpleadoNombre = r.EmpleadoNombre ?? "",
                Cargo = r.Cargo ?? "",
                FechaInicio = DateTime.Parse(r.FechaInicio),
                FechaFin = DateTime.Parse(r.FechaFin),
                TotalDias = r.TotalDias,
                DiasEpsArl = r.DiasEpsArl,
                TipoIncapacidad = ((TipoIncapacidad)r.TipoIncapacidadInt) switch
                {
                    TipoIncapacidad.EnfermedadGeneral => "Enfermedad General",
                    TipoIncapacidad.AccidenteTrabajo => "Accidente Trabajo",
                    TipoIncapacidad.EnfermedadLaboral => "Enfermedad Laboral",
                    TipoIncapacidad.LicenciaMaternidad => "Lic. Maternidad",
                    TipoIncapacidad.LicenciaPaternidad => "Lic. Paternidad",
                    _ => "Otro"
                },
                DiagnosticoDescripcion = r.DiagnosticoDescripcion ?? "",
                EntidadPagadora = r.EntidadPagadora,
                ValorDiaBase = r.ValorDiaBase,
                PorcentajePago = r.PorcentajePago,
                ValorCobrar = r.ValorCobrar,
                Transcrita = r.Transcrita == 1,
                NumeroRadicado = r.NumeroRadicado
            }).ToList();
        
        var reporte = new ReporteCobroEpsDto
        {
            Año = año,
            Mes = mes,
            Periodo = $"{año}-{mes:00}",
            TotalPorCobrar = items.Sum(i => i.ValorCobrar),
            TotalIncapacidades = items.Count,
            TotalDias = items.Sum(i => i.DiasEpsArl),
            Items = items,
            TotalesPorEntidad = items.GroupBy(i => i.EntidadPagadora)
                .ToDictionary(g => g.Key, g => g.Sum(i => i.ValorCobrar))
        };
        
        return reporte;
    }

    public Task<int> SaveChangesAsync() => Task.FromResult(0);
}
