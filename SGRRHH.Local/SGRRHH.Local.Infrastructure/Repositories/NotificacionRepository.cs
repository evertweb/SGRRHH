using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.DTOs;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Interfaces;
using SGRRHH.Local.Infrastructure.Data;

namespace SGRRHH.Local.Infrastructure.Repositories;

/// <summary>
/// Repositorio para gestión de notificaciones con persistencia en SQLite
/// </summary>
public class NotificacionRepository : INotificacionRepository
{
    private readonly DapperContext _context;
    private readonly ILogger<NotificacionRepository> _logger;

    public NotificacionRepository(DapperContext context, ILogger<NotificacionRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Notificacion?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT * FROM notificaciones WHERE id = @Id";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Notificacion>(sql, new { Id = id });
    }

    public async Task<List<NotificacionDto>> GetNotificacionesAsync(NotificacionFiltroDto filtro)
    {
        var sql = @"
            SELECT 
                id, titulo, mensaje, icono, tipo, categoria, 
                prioridad, link, leida, fecha_creacion
            FROM notificaciones
            WHERE 1=1";
        
        var parameters = new DynamicParameters();
        
        // Filtrar por usuario (NULL = para todos los aprobadores)
        if (filtro.UsuarioId.HasValue)
        {
            sql += " AND (usuario_destino_id = @UsuarioId OR usuario_destino_id IS NULL)";
            parameters.Add("UsuarioId", filtro.UsuarioId.Value);
        }
        
        // Solo no leídas
        if (filtro.SoloNoLeidas == true)
        {
            sql += " AND leida = 0";
        }
        
        // Filtrar por categoría
        if (!string.IsNullOrEmpty(filtro.Categoria))
        {
            sql += " AND categoria = @Categoria";
            parameters.Add("Categoria", filtro.Categoria);
        }
        
        // Filtrar por tipo
        if (!string.IsNullOrEmpty(filtro.Tipo))
        {
            sql += " AND tipo = @Tipo";
            parameters.Add("Tipo", filtro.Tipo);
        }
        
        // Excluir expiradas
        if (!filtro.IncluirExpiradas)
        {
            sql += " AND (fecha_expiracion IS NULL OR fecha_expiracion > datetime('now', 'localtime'))";
        }
        
        sql += " ORDER BY prioridad DESC, fecha_creacion DESC";
        
        // Límite
        if (filtro.Limite.HasValue && filtro.Limite.Value > 0)
        {
            sql += " LIMIT @Limite";
            parameters.Add("Limite", filtro.Limite.Value);
        }
        
        using var connection = _context.CreateConnection();
        var notificaciones = await connection.QueryAsync<NotificacionDto>(sql, parameters);
        
        // Calcular tiempo relativo
        foreach (var n in notificaciones)
        {
            n.TiempoRelativo = CalcularTiempoRelativo(n.FechaCreacion);
        }
        
        return notificaciones.ToList();
    }

    public async Task<int> GetCountNoLeidasAsync(int? usuarioId)
    {
        var sql = @"
            SELECT COUNT(*) FROM notificaciones 
            WHERE leida = 0 
              AND (fecha_expiracion IS NULL OR fecha_expiracion > datetime('now', 'localtime'))";
        
        if (usuarioId.HasValue)
        {
            sql += " AND (usuario_destino_id = @UsuarioId OR usuario_destino_id IS NULL)";
        }
        
        using var connection = _context.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, new { UsuarioId = usuarioId });
    }

    public async Task<NotificacionResumenDto> GetResumenAsync(int? usuarioId)
    {
        var resumen = new NotificacionResumenDto();
        
        using var connection = _context.CreateConnection();
        
        // Total no leídas
        var sqlBase = @"
            FROM notificaciones 
            WHERE leida = 0 
              AND (fecha_expiracion IS NULL OR fecha_expiracion > datetime('now', 'localtime'))";
        
        if (usuarioId.HasValue)
        {
            sqlBase += " AND (usuario_destino_id = @UsuarioId OR usuario_destino_id IS NULL)";
        }
        
        var param = new { UsuarioId = usuarioId };
        
        resumen.TotalNoLeidas = await connection.ExecuteScalarAsync<int>(
            $"SELECT COUNT(*) {sqlBase}", param);
        
        resumen.Urgentes = await connection.ExecuteScalarAsync<int>(
            $"SELECT COUNT(*) {sqlBase} AND prioridad >= 2", param);
        
        resumen.Permisos = await connection.ExecuteScalarAsync<int>(
            $"SELECT COUNT(*) {sqlBase} AND categoria = 'Permiso'", param);
        
        resumen.Vacaciones = await connection.ExecuteScalarAsync<int>(
            $"SELECT COUNT(*) {sqlBase} AND categoria = 'Vacacion'", param);
        
        resumen.Sistema = await connection.ExecuteScalarAsync<int>(
            $"SELECT COUNT(*) {sqlBase} AND categoria = 'Sistema'", param);
        
        // Últimas notificaciones
        resumen.UltimasNotificaciones = await GetNotificacionesAsync(new NotificacionFiltroDto
        {
            UsuarioId = usuarioId,
            SoloNoLeidas = true,
            Limite = 10
        });
        
        return resumen;
    }

    public async Task<int> CreateAsync(CrearNotificacionDto dto, string? creadoPor = null)
    {
        const string sql = @"
            INSERT INTO notificaciones 
                (usuario_destino_id, titulo, mensaje, icono, tipo, categoria, 
                 prioridad, link, entidad_tipo, entidad_id, fecha_expiracion, creado_por, fecha_creacion)
            VALUES 
                (@UsuarioDestinoId, @Titulo, @Mensaje, @Icono, @Tipo, @Categoria, 
                 @Prioridad, @Link, @EntidadTipo, @EntidadId, @FechaExpiracion, @CreadoPor, datetime('now', 'localtime'));
            SELECT last_insert_rowid();";
        
        var icono = ObtenerIcono(dto.Tipo, dto.Categoria);
        
        using var connection = _context.CreateConnection();
        var id = await connection.ExecuteScalarAsync<int>(sql, new
        {
            dto.UsuarioDestinoId,
            dto.Titulo,
            dto.Mensaje,
            Icono = icono,
            dto.Tipo,
            dto.Categoria,
            dto.Prioridad,
            dto.Link,
            dto.EntidadTipo,
            dto.EntidadId,
            dto.FechaExpiracion,
            CreadoPor = creadoPor
        });
        
        _logger.LogInformation("Notificación creada: {Id} - {Titulo}", id, dto.Titulo);
        return id;
    }

    public async Task<bool> MarcarLeidaAsync(int id)
    {
        const string sql = @"
            UPDATE notificaciones 
            SET leida = 1, fecha_lectura = datetime('now', 'localtime')
            WHERE id = @Id";
        
        using var connection = _context.CreateConnection();
        var affected = await connection.ExecuteAsync(sql, new { Id = id });
        return affected > 0;
    }

    public async Task<int> MarcarTodasLeidasAsync(int? usuarioId)
    {
        var sql = @"
            UPDATE notificaciones 
            SET leida = 1, fecha_lectura = datetime('now', 'localtime')
            WHERE leida = 0";
        
        if (usuarioId.HasValue)
        {
            sql += " AND (usuario_destino_id = @UsuarioId OR usuario_destino_id IS NULL)";
        }
        
        using var connection = _context.CreateConnection();
        return await connection.ExecuteAsync(sql, new { UsuarioId = usuarioId });
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM notificaciones WHERE id = @Id";
        
        using var connection = _context.CreateConnection();
        var affected = await connection.ExecuteAsync(sql, new { Id = id });
        return affected > 0;
    }

    public async Task<int> LimpiarAntiguasAsync(int diasAntiguedad = 30)
    {
        const string sql = @"
            DELETE FROM notificaciones 
            WHERE (fecha_creacion < datetime('now', 'localtime', @Dias))
               OR (fecha_expiracion IS NOT NULL AND fecha_expiracion < datetime('now', 'localtime'))";
        
        using var connection = _context.CreateConnection();
        var deleted = await connection.ExecuteAsync(sql, new { Dias = $"-{diasAntiguedad} days" });
        
        if (deleted > 0)
        {
            _logger.LogInformation("Limpieza de notificaciones: {Count} eliminadas", deleted);
        }
        
        return deleted;
    }

    public async Task<bool> ExisteNotificacionEntidadAsync(string entidadTipo, int entidadId, string tipo)
    {
        const string sql = @"
            SELECT COUNT(*) FROM notificaciones 
            WHERE entidad_tipo = @EntidadTipo 
              AND entidad_id = @EntidadId 
              AND tipo = @Tipo
              AND leida = 0";
        
        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { EntidadTipo = entidadTipo, EntidadId = entidadId, Tipo = tipo });
        return count > 0;
    }

    #region Helpers

    private static string CalcularTiempoRelativo(DateTime fecha)
    {
        var diff = DateTime.Now - fecha;
        
        if (diff.TotalSeconds < 60)
            return "Ahora";
        if (diff.TotalMinutes < 60)
            return $"Hace {(int)diff.TotalMinutes} min";
        if (diff.TotalHours < 24)
            return $"Hace {(int)diff.TotalHours}h";
        if (diff.TotalDays < 7)
            return $"Hace {(int)diff.TotalDays}d";
        
        return fecha.ToString("dd/MM/yyyy");
    }

    private static string ObtenerIcono(string tipo, string categoria)
    {
        // Primero por tipo específico
        var icono = tipo.ToLower() switch
        {
            "success" => "✓",
            "error" => "✕",
            "warning" => "⚠",
            "permisoaprobado" => "✅",
            "permisorechazado" => "❌",
            "permisonuevo" => "📋",
            "vacacionnueva" => "🏖️",
            "vacacionaprobada" => "✅",
            "incapacidadnueva" => "🏥",
            "urgente" => "🚨",
            _ => ""
        };
        
        if (!string.IsNullOrEmpty(icono)) return icono;
        
        // Por categoría
        return categoria.ToLower() switch
        {
            "permiso" => "📋",
            "vacacion" => "🏖️",
            "incapacidad" => "🏥",
            "contrato" => "📄",
            "empleado" => "👤",
            "sistema" => "⚙️",
            _ => "📌"
        };
    }

    #endregion
}
