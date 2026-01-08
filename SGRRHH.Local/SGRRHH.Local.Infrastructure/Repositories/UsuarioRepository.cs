using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly Data.DapperContext _context;
    private readonly ILogger<UsuarioRepository> _logger;

    public UsuarioRepository(Data.DapperContext context, ILogger<UsuarioRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Usuario> AddAsync(Usuario entity)
    {
        entity.FechaCreacion = DateTime.Now;
        const string sql = @"INSERT INTO usuarios (username, password_hash, nombre_completo, email, phone_number, rol, ultimo_acceso, empleado_id, activo, fecha_creacion)
VALUES (@Username, @PasswordHash, @NombreCompleto, @Email, @PhoneNumber, @Rol, @UltimoAcceso, @EmpleadoId, @Activo, @FechaCreacion);
SELECT last_insert_rowid();";

        using var connection = _context.CreateConnection();
        entity.Id = await connection.ExecuteScalarAsync<int>(sql, entity);
        return entity;
    }

    public async Task UpdateAsync(Usuario entity)
    {
        entity.FechaModificacion = DateTime.Now;
        const string sql = @"UPDATE usuarios
SET username = @Username,
    password_hash = @PasswordHash,
    nombre_completo = @NombreCompleto,
    email = @Email,
    phone_number = @PhoneNumber,
    rol = @Rol,
    ultimo_acceso = @UltimoAcceso,
    empleado_id = @EmpleadoId,
    fecha_modificacion = @FechaModificacion
WHERE id = @Id";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "UPDATE usuarios SET activo = 0, fecha_modificacion = CURRENT_TIMESTAMP WHERE id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<Usuario?> GetByIdAsync(int id)
    {
        const string sql = "SELECT * FROM usuarios WHERE id = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Usuario>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Usuario>> GetAllAsync()
    {
        const string sql = "SELECT * FROM usuarios";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Usuario>(sql);
    }

    public async Task<IEnumerable<Usuario>> GetAllActiveAsync()
    {
        const string sql = "SELECT * FROM usuarios WHERE activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Usuario>(sql);
    }

    public async Task<Usuario?> GetByUsernameAsync(string username)
    {
        const string sql = "SELECT * FROM usuarios WHERE username = @Username";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Usuario>(sql, new { Username = username });
    }

    public async Task<bool> ExistsUsernameAsync(string username)
    {
        const string sql = "SELECT COUNT(1) FROM usuarios WHERE lower(username) = lower(@Username)";
        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Username = username });
        return count > 0;
    }

    public async Task UpdateLastAccessAsync(int userId)
    {
        const string sql = "UPDATE usuarios SET ultimo_acceso = @FechaHora WHERE id = @UserId";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { UserId = userId, FechaHora = DateTime.Now });
    }

    public Task<int> SaveChangesAsync() => Task.FromResult(0);
}
