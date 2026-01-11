using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.DTOs;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Repositories;

public class PermisoRepository : IPermisoRepository
{
    private readonly Data.DapperContext _context;
    private readonly ILogger<PermisoRepository> _logger;

    public PermisoRepository(Data.DapperContext context, ILogger<PermisoRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Permiso> AddAsync(Permiso entity)
    {
        entity.FechaCreacion = DateTime.Now;
        const string sql = @"INSERT INTO permisos (
    numero_acta, empleado_id, tipo_permiso_id, motivo, fecha_solicitud, fecha_inicio, fecha_fin, total_dias,
    estado, observaciones, documento_soporte_path, dias_pendientes_compensacion, fecha_compensacion, solicitado_por_id,
    aprobado_por_id, fecha_aprobacion, motivo_rechazo, activo, fecha_creacion,
    tipo_resolucion, requiere_documento_posterior, fecha_limite_documento, fecha_entrega_documento,
    horas_compensar, horas_compensadas, fecha_limite_compensacion, monto_descuento, periodo_descuento, completado)
VALUES (
    @NumeroActa, @EmpleadoId, @TipoPermisoId, @Motivo, @FechaSolicitud, @FechaInicio, @FechaFin, @TotalDias,
    @Estado, @Observaciones, @DocumentoSoportePath, @DiasPendientesCompensacion, @FechaCompensacion, @SolicitadoPorId,
    @AprobadoPorId, @FechaAprobacion, @MotivoRechazo, @Activo, @FechaCreacion,
    @TipoResolucion, @RequiereDocumentoPosterior, @FechaLimiteDocumento, @FechaEntregaDocumento,
    @HorasCompensar, @HorasCompensadas, @FechaLimiteCompensacion, @MontoDescuento, @PeriodoDescuento, @Completado);
SELECT last_insert_rowid();";

        using var connection = _context.CreateConnection();
        entity.Id = await connection.ExecuteScalarAsync<int>(sql, entity);
        return entity;
    }

    public async Task UpdateAsync(Permiso entity)
    {
        entity.FechaModificacion = DateTime.Now;
        const string sql = @"UPDATE permisos
SET numero_acta = @NumeroActa,
    empleado_id = @EmpleadoId,
    tipo_permiso_id = @TipoPermisoId,
    motivo = @Motivo,
    fecha_solicitud = @FechaSolicitud,
    fecha_inicio = @FechaInicio,
    fecha_fin = @FechaFin,
    total_dias = @TotalDias,
    estado = @Estado,
    observaciones = @Observaciones,
    documento_soporte_path = @DocumentoSoportePath,
    dias_pendientes_compensacion = @DiasPendientesCompensacion,
    fecha_compensacion = @FechaCompensacion,
    solicitado_por_id = @SolicitadoPorId,
    aprobado_por_id = @AprobadoPorId,
    fecha_aprobacion = @FechaAprobacion,
    motivo_rechazo = @MotivoRechazo,
    fecha_modificacion = @FechaModificacion,
    tipo_resolucion = @TipoResolucion,
    requiere_documento_posterior = @RequiereDocumentoPosterior,
    fecha_limite_documento = @FechaLimiteDocumento,
    fecha_entrega_documento = @FechaEntregaDocumento,
    horas_compensar = @HorasCompensar,
    horas_compensadas = @HorasCompensadas,
    fecha_limite_compensacion = @FechaLimiteCompensacion,
    monto_descuento = @MontoDescuento,
    periodo_descuento = @PeriodoDescuento,
    completado = @Completado
WHERE id = @Id";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(int id)
    {
        // Hard delete - elimina permanentemente el registro
        const string sql = "DELETE FROM permisos WHERE id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<Permiso?> GetByIdAsync(int id)
    {
        const string sql = "SELECT * FROM permisos WHERE id = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Permiso>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Permiso>> GetAllAsync()
    {
        const string sql = "SELECT * FROM permisos";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Permiso>(sql);
    }

    public async Task<IEnumerable<Permiso>> GetAllActiveAsync()
    {
        const string sql = "SELECT * FROM permisos WHERE activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Permiso>(sql);
    }

    public async Task<IEnumerable<Permiso>> GetPendientesAsync()
    {
        const string sql = "SELECT * FROM permisos WHERE estado = @Estado AND activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Permiso>(sql, new { Estado = EstadoPermiso.Pendiente });
    }

    public async Task<IEnumerable<Permiso>> GetByEmpleadoIdAsync(int empleadoId)
    {
        const string sql = "SELECT * FROM permisos WHERE empleado_id = @EmpleadoId AND activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Permiso>(sql, new { EmpleadoId = empleadoId });
    }

    public async Task<IEnumerable<Permiso>> GetByRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        const string sql = "SELECT * FROM permisos WHERE fecha_inicio >= @FechaInicio AND fecha_fin <= @FechaFin";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Permiso>(sql, new { FechaInicio = fechaInicio, FechaFin = fechaFin });
    }

    public async Task<IEnumerable<Permiso>> GetByEstadoAsync(EstadoPermiso estado)
    {
        const string sql = "SELECT * FROM permisos WHERE estado = @Estado";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Permiso>(sql, new { Estado = estado });
    }

    public async Task<string> GetProximoNumeroActaAsync()
    {
        var year = DateTime.Now.Year;
        const string sql = @"SELECT numero_acta FROM permisos WHERE substr(numero_acta, 6, 4) = @YearText ORDER BY numero_acta DESC LIMIT 1";
        using var connection = _context.CreateConnection();
        var last = await connection.QuerySingleOrDefaultAsync<string>(sql, new { YearText = year.ToString() });
        var nextSeq = 1;
        if (!string.IsNullOrWhiteSpace(last))
        {
            var parts = last.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out var current))
            {
                nextSeq = current + 1;
            }
        }
        return $"PERM-{year}-{nextSeq:0000}";
    }

    public async Task<bool> ExisteSolapamientoAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin, int? excludePermisoId = null)
    {
        const string sql = @"SELECT COUNT(1) FROM permisos
WHERE empleado_id = @EmpleadoId
  AND activo = 1
  AND fecha_inicio <= @FechaFin
  AND fecha_fin >= @FechaInicio
  AND (@ExcludeId IS NULL OR id <> @ExcludeId)";

        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { EmpleadoId = empleadoId, FechaInicio = fechaInicio, FechaFin = fechaFin, ExcludeId = excludePermisoId });
        return count > 0;
    }

    public async Task<IEnumerable<Permiso>> GetAllWithFiltersAsync(int? empleadoId = null, EstadoPermiso? estado = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null)
    {
        var sql = "SELECT * FROM permisos WHERE 1=1";
        if (empleadoId.HasValue) sql += " AND empleado_id = @EmpleadoId";
        if (estado.HasValue) sql += " AND estado = @Estado";
        if (fechaDesde.HasValue) sql += " AND fecha_inicio >= @FechaDesde";
        if (fechaHasta.HasValue) sql += " AND fecha_fin <= @FechaHasta";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Permiso>(sql, new
        {
            EmpleadoId = empleadoId,
            Estado = estado,
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta
        });
    }

    // ===== NUEVOS MÉTODOS PARA SEGUIMIENTO =====

    public async Task<IEnumerable<Permiso>> GetPendientesDocumentoAsync()
    {
        const string sql = @"
            SELECT * FROM permisos 
            WHERE requiere_documento_posterior = 1 
              AND fecha_entrega_documento IS NULL
              AND estado IN (5, 2)  -- AprobadoPendienteDocumento o Aprobado
              AND activo = 1
            ORDER BY fecha_limite_documento ASC";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Permiso>(sql);
    }

    public async Task<IEnumerable<Permiso>> GetEnCompensacionAsync()
    {
        const string sql = @"
            SELECT * FROM permisos 
            WHERE tipo_resolucion = 3  -- Compensado
              AND horas_compensadas < COALESCE(horas_compensar, 0)
              AND completado = 0
              AND activo = 1
            ORDER BY fecha_limite_compensacion ASC";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Permiso>(sql);
    }

    public async Task<IEnumerable<Permiso>> GetParaDescuentoNominaAsync(string periodo)
    {
        const string sql = @"
            SELECT * FROM permisos 
            WHERE tipo_resolucion = 2  -- Descontado
              AND (periodo_descuento = @Periodo OR periodo_descuento IS NULL)
              AND monto_descuento > 0
              AND activo = 1
            ORDER BY fecha_inicio ASC";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Permiso>(sql, new { Periodo = periodo });
    }

    public async Task<IEnumerable<Permiso>> GetConDocumentoVencidoAsync()
    {
        var hoy = DateTime.Now.Date;
        const string sql = @"
            SELECT * FROM permisos 
            WHERE requiere_documento_posterior = 1 
              AND fecha_entrega_documento IS NULL
              AND fecha_limite_documento < @Hoy
              AND completado = 0
              AND activo = 1
            ORDER BY fecha_limite_documento ASC";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Permiso>(sql, new { Hoy = hoy });
    }

    public async Task<IEnumerable<Permiso>> GetConCompensacionVencidaAsync()
    {
        var hoy = DateTime.Now.Date;
        const string sql = @"
            SELECT * FROM permisos 
            WHERE tipo_resolucion = 3  -- Compensado
              AND horas_compensadas < COALESCE(horas_compensar, 0)
              AND fecha_limite_compensacion < @Hoy
              AND completado = 0
              AND activo = 1
            ORDER BY fecha_limite_compensacion ASC";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Permiso>(sql, new { Hoy = hoy });
    }

    public async Task ActualizarResolucionAsync(int permisoId, TipoResolucionPermiso resolucion)
    {
        const string sql = @"
            UPDATE permisos 
            SET tipo_resolucion = @Resolucion, 
                fecha_modificacion = @FechaMod
            WHERE id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = permisoId, Resolucion = resolucion, FechaMod = DateTime.Now });
        _logger.LogInformation("Resolución actualizada: Permiso {PermisoId} -> {Resolucion}", permisoId, resolucion);
    }

    public async Task RegistrarEntregaDocumentoAsync(int permisoId, string rutaDocumento, int usuarioId)
    {
        const string sql = @"
            UPDATE permisos 
            SET documento_soporte_path = @RutaDocumento,
                fecha_entrega_documento = @FechaEntrega,
                requiere_documento_posterior = 0,
                estado = CASE WHEN tipo_resolucion = 1 THEN 7 ELSE estado END,  -- Completado si es Remunerado
                completado = CASE WHEN tipo_resolucion = 1 THEN 1 ELSE completado END,
                fecha_modificacion = @FechaMod
            WHERE id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new 
        { 
            Id = permisoId, 
            RutaDocumento = rutaDocumento, 
            FechaEntrega = DateTime.Now,
            FechaMod = DateTime.Now 
        });
        _logger.LogInformation("Documento entregado: Permiso {PermisoId}, Usuario {UsuarioId}", permisoId, usuarioId);
    }

    public async Task<EstadisticasSeguimiento> GetEstadisticasSeguimientoAsync()
    {
        var hoy = DateTime.Now.Date;
        var stats = new EstadisticasSeguimiento();
        
        using var connection = _context.CreateConnection();
        
        // Total pendientes de aprobación
        stats.TotalPermisosPendientes = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM permisos WHERE estado = 1 AND activo = 1");
        
        // Total pendientes de documento
        stats.TotalPendientesDocumento = await connection.ExecuteScalarAsync<int>(
            @"SELECT COUNT(*) FROM permisos 
              WHERE requiere_documento_posterior = 1 
                AND fecha_entrega_documento IS NULL 
                AND fecha_limite_documento >= @Hoy
                AND activo = 1", new { Hoy = hoy });
        
        // Total en compensación
        stats.TotalEnCompensacion = await connection.ExecuteScalarAsync<int>(
            @"SELECT COUNT(*) FROM permisos 
              WHERE tipo_resolucion = 3 
                AND horas_compensadas < COALESCE(horas_compensar, 0)
                AND (fecha_limite_compensacion IS NULL OR fecha_limite_compensacion >= @Hoy)
                AND completado = 0
                AND activo = 1", new { Hoy = hoy });
        
        // Total para descuento
        stats.TotalParaDescuento = await connection.ExecuteScalarAsync<int>(
            @"SELECT COUNT(*) FROM permisos 
              WHERE tipo_resolucion = 2 
                AND monto_descuento > 0
                AND activo = 1");
        
        // Total documentos vencidos
        stats.TotalDocumentosVencidos = await connection.ExecuteScalarAsync<int>(
            @"SELECT COUNT(*) FROM permisos 
              WHERE requiere_documento_posterior = 1 
                AND fecha_entrega_documento IS NULL
                AND fecha_limite_documento < @Hoy
                AND completado = 0
                AND activo = 1", new { Hoy = hoy });
        
        // Total compensaciones vencidas
        stats.TotalCompensacionesVencidas = await connection.ExecuteScalarAsync<int>(
            @"SELECT COUNT(*) FROM permisos 
              WHERE tipo_resolucion = 3 
                AND horas_compensadas < COALESCE(horas_compensar, 0)
                AND fecha_limite_compensacion < @Hoy
                AND completado = 0
                AND activo = 1", new { Hoy = hoy });
        
        // Monto total descuentos
        stats.MontoTotalDescuentos = await connection.ExecuteScalarAsync<decimal>(
            @"SELECT COALESCE(SUM(monto_descuento), 0) FROM permisos 
              WHERE tipo_resolucion = 2 AND monto_descuento > 0 AND activo = 1");
        
        return stats;
    }

    public async Task<IEnumerable<PermisoConSeguimiento>> GetPermisosParaSeguimientoAsync(
        EstadoPermiso? estado = null, 
        TipoResolucionPermiso? tipoResolucion = null,
        bool? soloVencidos = null,
        bool? soloPendientesDocumento = null,
        bool? soloEnCompensacion = null)
    {
        var hoy = DateTime.Now.Date;
        var sql = @"
            SELECT 
                p.id as Id,
                p.numero_acta as NumeroActa,
                e.nombre || ' ' || e.apellidos as EmpleadoNombre,
                p.empleado_id as EmpleadoId,
                tp.nombre as TipoPermiso,
                p.tipo_permiso_id as TipoPermisoId,
                p.fecha_inicio as FechaInicio,
                p.fecha_fin as FechaFin,
                p.total_dias as TotalDias,
                p.estado as Estado,
                p.tipo_resolucion as TipoResolucion,
                CASE 
                    WHEN p.requiere_documento_posterior = 1 THEN p.fecha_limite_documento
                    WHEN p.tipo_resolucion = 3 THEN p.fecha_limite_compensacion
                    ELSE NULL
                END as FechaLimite,
                p.horas_compensar as HorasCompensar,
                p.horas_compensadas as HorasCompensadas,
                p.monto_descuento as MontoDescuento,
                p.periodo_descuento as PeriodoDescuento
            FROM permisos p
            LEFT JOIN empleados e ON p.empleado_id = e.id
            LEFT JOIN tipos_permiso tp ON p.tipo_permiso_id = tp.id
            WHERE p.activo = 1";
        
        if (estado.HasValue)
            sql += " AND p.estado = @Estado";
        if (tipoResolucion.HasValue)
            sql += " AND p.tipo_resolucion = @TipoResolucion";
        if (soloPendientesDocumento == true)
            sql += " AND p.requiere_documento_posterior = 1 AND p.fecha_entrega_documento IS NULL";
        if (soloEnCompensacion == true)
            sql += " AND p.tipo_resolucion = 3 AND p.horas_compensadas < COALESCE(p.horas_compensar, 0) AND p.completado = 0";
        if (soloVencidos == true)
            sql += @" AND ((p.requiere_documento_posterior = 1 AND p.fecha_entrega_documento IS NULL AND p.fecha_limite_documento < @Hoy)
                      OR (p.tipo_resolucion = 3 AND p.horas_compensadas < COALESCE(p.horas_compensar, 0) AND p.fecha_limite_compensacion < @Hoy))";
        
        sql += " ORDER BY p.fecha_inicio DESC";
        
        using var connection = _context.CreateConnection();
        var permisos = await connection.QueryAsync<PermisoConSeguimiento>(sql, new 
        { 
            Estado = estado, 
            TipoResolucion = tipoResolucion,
            Hoy = hoy 
        });
        
        // Calcular días restantes y estados de vencimiento
        foreach (var p in permisos)
        {
            if (p.FechaLimite.HasValue)
            {
                p.DiasRestantes = (int)(p.FechaLimite.Value.Date - hoy).TotalDays;
                p.EstaVencido = p.DiasRestantes < 0;
                p.VencePronto = p.DiasRestantes >= 0 && p.DiasRestantes <= 3;
                
                if (p.EstaVencido)
                    p.MotivoUrgencia = "VENCIDO";
                else if (p.VencePronto)
                    p.MotivoUrgencia = $"Vence en {p.DiasRestantes} días";
            }
        }
        
        return permisos;
    }

    public async Task MarcarCompletadoAsync(int permisoId)
    {
        const string sql = @"
            UPDATE permisos 
            SET completado = 1,
                estado = 7,  -- Completado
                fecha_modificacion = @FechaMod
            WHERE id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = permisoId, FechaMod = DateTime.Now });
        _logger.LogInformation("Permiso marcado como completado: {PermisoId}", permisoId);
    }

    public async Task ActualizarHorasCompensadasAsync(int permisoId, int horasCompensadas)
    {
        const string sql = @"
            UPDATE permisos 
            SET horas_compensadas = @Horas,
                fecha_modificacion = @FechaMod
            WHERE id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = permisoId, Horas = horasCompensadas, FechaMod = DateTime.Now });
    }

    public Task<int> SaveChangesAsync() => Task.FromResult(0);
}
