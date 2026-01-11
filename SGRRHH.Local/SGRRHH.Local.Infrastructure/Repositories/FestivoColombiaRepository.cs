using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Infrastructure.Data;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Repositories;

/// <summary>
/// Repositorio para gestión de festivos colombianos según Ley 51 de 1983 (Ley Emiliani)
/// </summary>
public class FestivoColombiaRepository : IFestivoColombiaRepository
{
    private readonly DapperContext _context;
    private readonly ILogger<FestivoColombiaRepository> _logger;

    public FestivoColombiaRepository(DapperContext context, ILogger<FestivoColombiaRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<FestivoColombia> AddAsync(FestivoColombia entity)
    {
        entity.FechaCreacion = DateTime.Now;
        const string sql = @"
            INSERT INTO festivos_colombia (
                fecha, nombre, descripcion, es_ley_emiliani, fecha_original,
                tipo, es_fecha_fija, año, activo, fecha_creacion
            ) VALUES (
                @Fecha, @Nombre, @Descripcion, @EsLeyEmiliani, @FechaOriginal,
                @Tipo, @EsFechaFija, @Año, 1, @FechaCreacion
            );
            SELECT last_insert_rowid();";

        using var connection = _context.CreateConnection();
        entity.Id = await connection.ExecuteScalarAsync<int>(sql, entity);
        _logger.LogInformation("Festivo creado: {Nombre} ({Fecha})", entity.Nombre, entity.Fecha.ToString("yyyy-MM-dd"));
        return entity;
    }

    public async Task UpdateAsync(FestivoColombia entity)
    {
        entity.FechaModificacion = DateTime.Now;
        const string sql = @"
            UPDATE festivos_colombia SET
                fecha = @Fecha,
                nombre = @Nombre,
                descripcion = @Descripcion,
                es_ley_emiliani = @EsLeyEmiliani,
                fecha_original = @FechaOriginal,
                tipo = @Tipo,
                es_fecha_fija = @EsFechaFija,
                año = @Año,
                fecha_modificacion = @FechaModificacion
            WHERE id = @Id";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(int id)
    {
        // Hard delete - elimina permanentemente el registro
        const string sql = "DELETE FROM festivos_colombia WHERE id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<FestivoColombia?> GetByIdAsync(int id)
    {
        const string sql = "SELECT * FROM festivos_colombia WHERE id = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<FestivoColombia>(sql, new { Id = id });
    }

    public async Task<IEnumerable<FestivoColombia>> GetAllAsync()
    {
        const string sql = "SELECT * FROM festivos_colombia ORDER BY fecha";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<FestivoColombia>(sql);
    }

    public async Task<IEnumerable<FestivoColombia>> GetAllActiveAsync()
    {
        const string sql = "SELECT * FROM festivos_colombia WHERE activo = 1 ORDER BY fecha";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<FestivoColombia>(sql);
    }

    public async Task<IEnumerable<FestivoColombia>> GetByAñoAsync(int año)
    {
        const string sql = "SELECT * FROM festivos_colombia WHERE año = @Año AND activo = 1 ORDER BY fecha";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<FestivoColombia>(sql, new { Año = año });
    }

    public async Task<FestivoColombia?> GetByFechaAsync(DateTime fecha)
    {
        // Comparar solo la parte de fecha (sin hora)
        const string sql = "SELECT * FROM festivos_colombia WHERE date(fecha) = date(@Fecha) AND activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<FestivoColombia>(sql, new { Fecha = fecha.ToString("yyyy-MM-dd") });
    }

    public async Task<bool> EsFestivoAsync(DateTime fecha)
    {
        const string sql = "SELECT COUNT(1) FROM festivos_colombia WHERE date(fecha) = date(@Fecha) AND activo = 1";
        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Fecha = fecha.ToString("yyyy-MM-dd") });
        return count > 0;
    }

    public async Task<IEnumerable<FestivoColombia>> GetFestivosRangoAsync(DateTime inicio, DateTime fin)
    {
        const string sql = @"SELECT * FROM festivos_colombia 
            WHERE date(fecha) >= date(@Inicio) AND date(fecha) <= date(@Fin) AND activo = 1 
            ORDER BY fecha";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<FestivoColombia>(sql, new 
        { 
            Inicio = inicio.ToString("yyyy-MM-dd"), 
            Fin = fin.ToString("yyyy-MM-dd") 
        });
    }

    public async Task<int> ContarFestivosEnPeriodoAsync(DateTime inicio, DateTime fin)
    {
        const string sql = @"SELECT COUNT(1) FROM festivos_colombia 
            WHERE date(fecha) >= date(@Inicio) AND date(fecha) <= date(@Fin) AND activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, new 
        { 
            Inicio = inicio.ToString("yyyy-MM-dd"), 
            Fin = fin.ToString("yyyy-MM-dd") 
        });
    }

    public async Task<bool> ExisteFestivosAñoAsync(int año)
    {
        const string sql = "SELECT COUNT(1) FROM festivos_colombia WHERE año = @Año AND activo = 1";
        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Año = año });
        return count > 0;
    }

    public Task<int> SaveChangesAsync() => Task.FromResult(0);
}
