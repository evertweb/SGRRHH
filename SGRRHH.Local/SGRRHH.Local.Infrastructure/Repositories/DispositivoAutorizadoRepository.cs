using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Repositories;

public class DispositivoAutorizadoRepository : IDispositivoAutorizadoRepository
{
    private readonly Data.DapperContext _context;
    private readonly ILogger<DispositivoAutorizadoRepository> _logger;

    public DispositivoAutorizadoRepository(Data.DapperContext context, ILogger<DispositivoAutorizadoRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DispositivoAutorizado> AddAsync(DispositivoAutorizado entity)
    {
        entity.FechaCreacion = DateTime.Now;
        const string sql = @"
            INSERT INTO dispositivos_autorizados 
                (usuario_id, device_token, nombre_dispositivo, huella_navegador, ip_autorizacion, 
                 fecha_creacion, fecha_expiracion, activo)
            VALUES 
                (@UsuarioId, @DeviceToken, @NombreDispositivo, @HuellaNavegador, @IpAutorizacion, 
                 @FechaCreacion, @FechaExpiracion, @Activo);
            SELECT last_insert_rowid();";

        using var connection = _context.CreateConnection();
        entity.Id = await connection.ExecuteScalarAsync<int>(sql, entity);
        _logger.LogInformation("Dispositivo autorizado creado: {Id} para usuario {UsuarioId}", entity.Id, entity.UsuarioId);
        return entity;
    }

    public async Task UpdateAsync(DispositivoAutorizado entity)
    {
        entity.FechaModificacion = DateTime.Now;
        const string sql = @"
            UPDATE dispositivos_autorizados
            SET nombre_dispositivo = @NombreDispositivo,
                huella_navegador = @HuellaNavegador,
                fecha_modificacion = @FechaModificacion,
                fecha_expiracion = @FechaExpiracion,
                activo = @Activo
            WHERE id = @Id";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "DELETE FROM dispositivos_autorizados WHERE id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id });
        _logger.LogInformation("Dispositivo autorizado eliminado: {Id}", id);
    }

    public async Task<DispositivoAutorizado?> GetByIdAsync(int id)
    {
        const string sql = "SELECT * FROM dispositivos_autorizados WHERE id = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<DispositivoAutorizado>(sql, new { Id = id });
    }

    public async Task<DispositivoAutorizado?> GetByTokenAsync(string deviceToken)
    {
        const string sql = @"
            SELECT * FROM dispositivos_autorizados 
            WHERE device_token = @DeviceToken 
              AND activo = 1
              AND (fecha_expiracion IS NULL OR fecha_expiracion > @Ahora)";
        
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<DispositivoAutorizado>(
            sql, new { DeviceToken = deviceToken, Ahora = DateTime.Now });
    }

    public async Task<IEnumerable<DispositivoAutorizado>> GetByUsuarioIdAsync(int usuarioId)
    {
        const string sql = @"
            SELECT * FROM dispositivos_autorizados 
            WHERE usuario_id = @UsuarioId 
            ORDER BY fecha_creacion DESC";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<DispositivoAutorizado>(sql, new { UsuarioId = usuarioId });
    }

    public async Task<IEnumerable<DispositivoAutorizado>> GetAllAsync()
    {
        const string sql = "SELECT * FROM dispositivos_autorizados ORDER BY fecha_creacion DESC";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<DispositivoAutorizado>(sql);
    }

    public async Task RevocarAsync(int id)
    {
        const string sql = @"
            UPDATE dispositivos_autorizados 
            SET activo = 0, fecha_modificacion = @Ahora 
            WHERE id = @Id";
        
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id, Ahora = DateTime.Now });
        _logger.LogInformation("Dispositivo revocado: {Id}", id);
    }

    public async Task RevocarTodosDeUsuarioAsync(int usuarioId)
    {
        const string sql = @"
            UPDATE dispositivos_autorizados 
            SET activo = 0, fecha_modificacion = @Ahora 
            WHERE usuario_id = @UsuarioId";
        
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { UsuarioId = usuarioId, Ahora = DateTime.Now });
        _logger.LogInformation("Todos los dispositivos revocados para usuario: {UsuarioId}", usuarioId);
    }

    public async Task ActualizarUltimoUsoAsync(int id)
    {
        const string sql = @"
            UPDATE dispositivos_autorizados 
            SET fecha_ultimo_uso = @Ahora 
            WHERE id = @Id";
        
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id, Ahora = DateTime.Now });
    }

    public async Task<bool> TokenActivoExisteAsync(string deviceToken)
    {
        const string sql = @"
            SELECT COUNT(1) FROM dispositivos_autorizados 
            WHERE device_token = @DeviceToken 
              AND activo = 1
              AND (fecha_expiracion IS NULL OR fecha_expiracion > @Ahora)";
        
        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { DeviceToken = deviceToken, Ahora = DateTime.Now });
        return count > 0;
    }

    public async Task<int> ContarDispositivosActivosAsync(int usuarioId)
    {
        const string sql = @"
            SELECT COUNT(1) FROM dispositivos_autorizados 
            WHERE usuario_id = @UsuarioId AND activo = 1";
        
        using var connection = _context.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, new { UsuarioId = usuarioId });
    }
}
