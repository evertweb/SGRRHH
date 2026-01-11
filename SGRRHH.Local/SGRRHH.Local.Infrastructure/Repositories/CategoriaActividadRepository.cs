using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Repositories;

/// <summary>
/// Repositorio para gestionar las categorías de actividades silviculturales
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
            INSERT INTO CategoriasActividades (Codigo, Nombre, Descripcion, Icono, ColorHex, Orden, Activo, FechaCreacion)
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
            UPDATE CategoriasActividades
            SET Codigo = @Codigo,
                Nombre = @Nombre,
                Descripcion = @Descripcion,
                Icono = @Icono,
                ColorHex = @ColorHex,
                Orden = @Orden,
                Activo = @Activo,
                FechaModificacion = @FechaModificacion
            WHERE Id = @Id";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(int id)
    {
        // Soft delete - desactivar la categoría
        const string sql = @"
            UPDATE CategoriasActividades 
            SET Activo = 0, FechaModificacion = @FechaModificacion 
            WHERE Id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id, FechaModificacion = DateTime.Now });
    }

    public async Task<CategoriaActividad?> GetByIdAsync(int id)
    {
        const string sql = "SELECT * FROM CategoriasActividades WHERE Id = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<CategoriaActividad>(sql, new { Id = id });
    }

    public async Task<IEnumerable<CategoriaActividad>> GetAllAsync()
    {
        const string sql = "SELECT * FROM CategoriasActividades ORDER BY Orden, Nombre";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<CategoriaActividad>(sql);
    }

    public async Task<IEnumerable<CategoriaActividad>> GetAllActiveAsync()
    {
        const string sql = "SELECT * FROM CategoriasActividades WHERE Activo = 1 ORDER BY Orden, Nombre";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<CategoriaActividad>(sql);
    }

    public async Task<CategoriaActividad?> GetByCodigoAsync(string codigo)
    {
        const string sql = "SELECT * FROM CategoriasActividades WHERE Codigo = @Codigo";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<CategoriaActividad>(sql, new { Codigo = codigo });
    }

    public async Task<bool> ExistsCodigoAsync(string codigo, int? excludeId = null)
    {
        const string sql = @"
            SELECT COUNT(1) FROM CategoriasActividades 
            WHERE Codigo = @Codigo AND (@ExcludeId IS NULL OR Id <> @ExcludeId)";
        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Codigo = codigo, ExcludeId = excludeId });
        return count > 0;
    }

    public async Task<IEnumerable<CategoriaActividad>> GetAllOrderedAsync()
    {
        const string sql = "SELECT * FROM CategoriasActividades WHERE Activo = 1 ORDER BY Orden ASC";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<CategoriaActividad>(sql);
    }

    public Task<int> SaveChangesAsync() => Task.FromResult(0);
}
