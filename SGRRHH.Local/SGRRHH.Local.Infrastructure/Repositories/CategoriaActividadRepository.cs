using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Repositories;

/// <summary>
/// Repositorio para gestionar las categorías de actividades silviculturales.
/// Usa tabla activity_categories (snake_case estandarizado).
/// </summary>
public class CategoriaActividadRepository : ICategoriaActividadRepository
{
    private readonly Data.DapperContext _context;
    private readonly ILogger<CategoriaActividadRepository> _logger;

    public CategoriaActividadRepository(Data.DapperContext context, ILogger<CategoriaActividadRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CategoriaActividad> AddAsync(CategoriaActividad entity)
    {
        entity.FechaCreacion = DateTime.Now;
        const string sql = @"
            INSERT INTO activity_categories (code, name, description, icon, color_hex, display_order, is_active, created_at)
            VALUES (@Codigo, @Nombre, @Descripcion, @Icono, @ColorHex, @Orden, @Activo, @FechaCreacion);
            SELECT last_insert_rowid();";

        using var connection = _context.CreateConnection();
        entity.Id = await connection.ExecuteScalarAsync<int>(sql, entity);
        return entity;
    }

    public async Task UpdateAsync(CategoriaActividad entity)
    {
        entity.FechaModificacion = DateTime.Now;
        const string sql = @"
            UPDATE activity_categories
            SET code = @Codigo,
                name = @Nombre,
                description = @Descripcion,
                icon = @Icono,
                color_hex = @ColorHex,
                display_order = @Orden,
                is_active = @Activo,
                updated_at = @FechaModificacion
            WHERE id = @Id";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(int id)
    {
        // Soft delete - desactivar la categoría
        const string sql = @"
            UPDATE activity_categories 
            SET is_active = 0, updated_at = @FechaModificacion 
            WHERE id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id, FechaModificacion = DateTime.Now });
    }

    public async Task<CategoriaActividad?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT id, code AS Codigo, name AS Nombre, description AS Descripcion, 
                   icon AS Icono, color_hex AS ColorHex, display_order AS Orden, 
                   is_active AS Activo, created_at AS FechaCreacion, updated_at AS FechaModificacion
            FROM activity_categories WHERE id = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<CategoriaActividad>(sql, new { Id = id });
    }

    public async Task<IEnumerable<CategoriaActividad>> GetAllAsync()
    {
        const string sql = @"
            SELECT id, code AS Codigo, name AS Nombre, description AS Descripcion, 
                   icon AS Icono, color_hex AS ColorHex, display_order AS Orden, 
                   is_active AS Activo, created_at AS FechaCreacion, updated_at AS FechaModificacion
            FROM activity_categories ORDER BY display_order, name";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<CategoriaActividad>(sql);
    }

    public async Task<IEnumerable<CategoriaActividad>> GetAllActiveAsync()
    {
        const string sql = @"
            SELECT id, code AS Codigo, name AS Nombre, description AS Descripcion, 
                   icon AS Icono, color_hex AS ColorHex, display_order AS Orden, 
                   is_active AS Activo, created_at AS FechaCreacion, updated_at AS FechaModificacion
            FROM activity_categories WHERE is_active = 1 ORDER BY display_order, name";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<CategoriaActividad>(sql);
    }

    public async Task<CategoriaActividad?> GetByCodigoAsync(string codigo)
    {
        const string sql = @"
            SELECT id, code AS Codigo, name AS Nombre, description AS Descripcion, 
                   icon AS Icono, color_hex AS ColorHex, display_order AS Orden, 
                   is_active AS Activo, created_at AS FechaCreacion, updated_at AS FechaModificacion
            FROM activity_categories WHERE code = @Codigo";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<CategoriaActividad>(sql, new { Codigo = codigo });
    }

    public async Task<bool> ExistsCodigoAsync(string codigo, int? excludeId = null)
    {
        const string sql = @"
            SELECT COUNT(1) FROM activity_categories 
            WHERE code = @Codigo AND (@ExcludeId IS NULL OR id <> @ExcludeId)";
        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Codigo = codigo, ExcludeId = excludeId });
        return count > 0;
    }

    public async Task<IEnumerable<CategoriaActividad>> GetAllOrderedAsync()
    {
        const string sql = @"
            SELECT id, code AS Codigo, name AS Nombre, description AS Descripcion, 
                   icon AS Icono, color_hex AS ColorHex, display_order AS Orden, 
                   is_active AS Activo, created_at AS FechaCreacion, updated_at AS FechaModificacion
            FROM activity_categories WHERE is_active = 1 ORDER BY display_order ASC";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<CategoriaActividad>(sql);
    }

    public Task<int> SaveChangesAsync() => Task.FromResult(0);
}
