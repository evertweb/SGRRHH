using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.DTOs;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Infrastructure.Data;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Repositories;

/// <summary>
/// Repositorio para seguimiento de incapacidades
/// </summary>
public class SeguimientoIncapacidadRepository : ISeguimientoIncapacidadRepository
{
    private readonly DapperContext _context;
    private readonly ILogger<SeguimientoIncapacidadRepository> _logger;

    public SeguimientoIncapacidadRepository(DapperContext context, ILogger<SeguimientoIncapacidadRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ===== CRUD BÁSICO =====

    public async Task<SeguimientoIncapacidad> AddAsync(SeguimientoIncapacidad entity)
    {
        entity.FechaCreacion = DateTime.Now;
        entity.FechaAccion = entity.FechaAccion == default ? DateTime.Now : entity.FechaAccion;

        const string sql = @"
            INSERT INTO seguimiento_incapacidades (
                incapacidad_id, fecha_accion, tipo_accion, descripcion,
                realizado_por_id, datos_adicionales, activo, fecha_creacion
            ) VALUES (
                @IncapacidadId, @FechaAccion, @TipoAccion, @Descripcion,
                @RealizadoPorId, @DatosAdicionales, @Activo, @FechaCreacion
            );
            SELECT last_insert_rowid();";

        using var connection = _context.CreateConnection();
        entity.Id = await connection.ExecuteScalarAsync<int>(sql, entity);
        
        _logger.LogDebug("Seguimiento registrado: Incapacidad {IncapacidadId}, Acción {TipoAccion}", 
            entity.IncapacidadId, entity.TipoAccion);
        
        return entity;
    }

    public async Task UpdateAsync(SeguimientoIncapacidad entity)
    {
        const string sql = @"
            UPDATE seguimiento_incapacidades SET
                fecha_accion = @FechaAccion,
                tipo_accion = @TipoAccion,
                descripcion = @Descripcion,
                datos_adicionales = @DatosAdicionales
            WHERE id = @Id";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "UPDATE seguimiento_incapacidades SET activo = 0 WHERE id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<SeguimientoIncapacidad?> GetByIdAsync(int id)
    {
        const string sql = "SELECT * FROM seguimiento_incapacidades WHERE id = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<SeguimientoIncapacidad>(sql, new { Id = id });
    }

    public async Task<IEnumerable<SeguimientoIncapacidad>> GetAllAsync()
    {
        const string sql = "SELECT * FROM seguimiento_incapacidades ORDER BY fecha_accion DESC";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<SeguimientoIncapacidad>(sql);
    }

    public async Task<IEnumerable<SeguimientoIncapacidad>> GetAllActiveAsync()
    {
        const string sql = "SELECT * FROM seguimiento_incapacidades WHERE activo = 1 ORDER BY fecha_accion DESC";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<SeguimientoIncapacidad>(sql);
    }

    // ===== CONSULTAS ESPECÍFICAS =====

    public async Task<IEnumerable<SeguimientoIncapacidad>> GetByIncapacidadIdAsync(int incapacidadId)
    {
        const string sql = @"
            SELECT * FROM seguimiento_incapacidades 
            WHERE incapacidad_id = @IncapacidadId AND activo = 1 
            ORDER BY fecha_accion DESC";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<SeguimientoIncapacidad>(sql, new { IncapacidadId = incapacidadId });
    }

    public async Task<IEnumerable<SeguimientoIncapacidadDto>> GetHistorialAsync(int incapacidadId)
    {
        const string sql = @"
            SELECT 
                s.id as Id,
                s.fecha_accion as FechaAccion,
                s.tipo_accion as TipoAccion,
                s.descripcion as Descripcion,
                u.nombre_usuario as RealizadoPorNombre,
                s.datos_adicionales as DatosAdicionales
            FROM seguimiento_incapacidades s
            LEFT JOIN usuarios u ON s.realizado_por_id = u.id
            WHERE s.incapacidad_id = @IncapacidadId AND s.activo = 1 
            ORDER BY s.fecha_accion DESC";

        using var connection = _context.CreateConnection();
        var items = await connection.QueryAsync<dynamic>(sql, new { IncapacidadId = incapacidadId });
        
        return items.Select(r => new SeguimientoIncapacidadDto
        {
            Id = r.Id,
            FechaAccion = DateTime.Parse(r.FechaAccion),
            TipoAccion = (TipoAccionSeguimientoIncapacidad)r.TipoAccion,
            TipoAccionNombre = ((TipoAccionSeguimientoIncapacidad)r.TipoAccion) switch
            {
                TipoAccionSeguimientoIncapacidad.Registro => "Registro",
                TipoAccionSeguimientoIncapacidad.Transcripcion => "Transcripción",
                TipoAccionSeguimientoIncapacidad.RadicadoEPS => "Radicado EPS",
                TipoAccionSeguimientoIncapacidad.Cobro => "Cobro",
                TipoAccionSeguimientoIncapacidad.Prorroga => "Prórroga",
                TipoAccionSeguimientoIncapacidad.Finalizacion => "Finalización",
                TipoAccionSeguimientoIncapacidad.Observacion => "Observación",
                TipoAccionSeguimientoIncapacidad.DocumentoAgregado => "Documento Agregado",
                TipoAccionSeguimientoIncapacidad.ConversionDesdePermiso => "Conversión desde Permiso",
                _ => "Desconocido"
            },
            Descripcion = r.Descripcion,
            RealizadoPorNombre = r.RealizadoPorNombre ?? "Sistema",
            DatosAdicionales = r.DatosAdicionales
        }).ToList();
    }

    public async Task RegistrarAccionAsync(
        int incapacidadId,
        TipoAccionSeguimientoIncapacidad tipoAccion,
        string descripcion,
        int usuarioId,
        string? datosAdicionales = null)
    {
        var seguimiento = new SeguimientoIncapacidad
        {
            IncapacidadId = incapacidadId,
            TipoAccion = tipoAccion,
            Descripcion = descripcion,
            RealizadoPorId = usuarioId,
            DatosAdicionales = datosAdicionales,
            FechaAccion = DateTime.Now,
            Activo = true
        };

        await AddAsync(seguimiento);
        
        _logger.LogInformation("Acción registrada: Incapacidad {IncapacidadId}, Tipo {TipoAccion}, Usuario {UsuarioId}", 
            incapacidadId, tipoAccion, usuarioId);
    }

    public async Task<SeguimientoIncapacidad?> GetUltimaAccionAsync(int incapacidadId, TipoAccionSeguimientoIncapacidad tipoAccion)
    {
        const string sql = @"
            SELECT * FROM seguimiento_incapacidades 
            WHERE incapacidad_id = @IncapacidadId AND tipo_accion = @TipoAccion AND activo = 1 
            ORDER BY fecha_accion DESC 
            LIMIT 1";
        
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<SeguimientoIncapacidad>(sql, 
            new { IncapacidadId = incapacidadId, TipoAccion = tipoAccion });
    }

    public Task<int> SaveChangesAsync() => Task.FromResult(0);
}
