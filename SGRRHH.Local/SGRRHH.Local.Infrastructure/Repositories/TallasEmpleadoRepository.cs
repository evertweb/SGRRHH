using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Repositories;

public class TallasEmpleadoRepository : ITallasEmpleadoRepository
{
    private readonly Data.DapperContext _context;
    private readonly ILogger<TallasEmpleadoRepository> _logger;

    public TallasEmpleadoRepository(Data.DapperContext context, ILogger<TallasEmpleadoRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TallasEmpleado> AddAsync(TallasEmpleado entity)
    {
        entity.FechaCreacion = DateTime.Now;
        entity.Activo = true;
        
        const string sql = @"INSERT INTO tallas_empleado (
            empleado_id, talla_camisa, talla_pantalon, talla_overall, talla_chaqueta,
            talla_calzado_numero, ancho_calzado, tipo_calzado_preferido,
            talla_guantes, talla_casco, talla_gafas, observaciones,
            activo, fecha_creacion)
        VALUES (
            @EmpleadoId, @TallaCamisa, @TallaPantalon, @TallaOverall, @TallaChaqueta,
            @TallaCalzadoNumero, @AnchoCalzado, @TipoCalzadoPreferido,
            @TallaGuantes, @TallaCasco, @TallaGafas, @Observaciones,
            @Activo, @FechaCreacion);
        SELECT last_insert_rowid();";

        using var connection = _context.CreateConnection();
        entity.Id = await connection.ExecuteScalarAsync<int>(sql, new
        {
            entity.EmpleadoId,
            entity.TallaCamisa,
            entity.TallaPantalon,
            entity.TallaOverall,
            entity.TallaChaqueta,
            entity.TallaCalzadoNumero,
            entity.AnchoCalzado,
            entity.TipoCalzadoPreferido,
            entity.TallaGuantes,
            entity.TallaCasco,
            entity.TallaGafas,
            entity.Observaciones,
            entity.Activo,
            entity.FechaCreacion
        });
        
        return entity;
    }

    public async Task UpdateAsync(TallasEmpleado entity)
    {
        entity.FechaModificacion = DateTime.Now;
        
        const string sql = @"UPDATE tallas_empleado
        SET empleado_id = @EmpleadoId,
            talla_camisa = @TallaCamisa,
            talla_pantalon = @TallaPantalon,
            talla_overall = @TallaOverall,
            talla_chaqueta = @TallaChaqueta,
            talla_calzado_numero = @TallaCalzadoNumero,
            ancho_calzado = @AnchoCalzado,
            tipo_calzado_preferido = @TipoCalzadoPreferido,
            talla_guantes = @TallaGuantes,
            talla_casco = @TallaCasco,
            talla_gafas = @TallaGafas,
            observaciones = @Observaciones,
            activo = @Activo,
            fecha_modificacion = @FechaModificacion
        WHERE id = @Id";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            entity.Id,
            entity.EmpleadoId,
            entity.TallaCamisa,
            entity.TallaPantalon,
            entity.TallaOverall,
            entity.TallaChaqueta,
            entity.TallaCalzadoNumero,
            entity.AnchoCalzado,
            entity.TipoCalzadoPreferido,
            entity.TallaGuantes,
            entity.TallaCasco,
            entity.TallaGafas,
            entity.Observaciones,
            entity.Activo,
            entity.FechaModificacion
        });
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "UPDATE tallas_empleado SET activo = 0, fecha_modificacion = @Fecha WHERE id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id, Fecha = DateTime.Now });
    }

    public async Task<TallasEmpleado?> GetByIdAsync(int id)
    {
        const string sql = @"SELECT * FROM tallas_empleado WHERE id = @Id AND activo = 1";
        using var connection = _context.CreateConnection();
        var result = await connection.QueryFirstOrDefaultAsync<TallasEmpleadoDb>(sql, new { Id = id });
        return result?.ToEntity();
    }

    public async Task<IEnumerable<TallasEmpleado>> GetAllAsync()
    {
        using var conn = _context.CreateConnection();
        var dbTallas = await conn.QueryAsync<TallasEmpleadoDb>(
            "SELECT * FROM tallas_empleado WHERE activo = 1 ORDER BY fecha_creacion DESC");
        return dbTallas.Select(db => db.ToEntity());
    }

    public async Task<IEnumerable<TallasEmpleado>> GetAllActiveAsync()
    {
        return await GetAllAsync();
    }

    public Task<int> SaveChangesAsync()
    {
        return Task.FromResult(0);
    }

    public async Task<TallasEmpleado?> GetByEmpleadoIdAsync(int empleadoId)
    {
        const string sql = @"SELECT * FROM tallas_empleado 
            WHERE empleado_id = @EmpleadoId AND activo = 1 
            LIMIT 1";
        
        using var connection = _context.CreateConnection();
        var result = await connection.QueryFirstOrDefaultAsync<TallasEmpleadoDb>(sql, new { EmpleadoId = empleadoId });
        return result?.ToEntity();
    }

    public async Task<bool> EmpleadoTieneTallasRegistradasAsync(int empleadoId)
    {
        const string sql = @"SELECT COUNT(*) FROM tallas_empleado 
            WHERE empleado_id = @EmpleadoId AND activo = 1";
        
        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { EmpleadoId = empleadoId });
        return count > 0;
    }

    // Clase auxiliar para mapeo desde SQLite
    private class TallasEmpleadoDb
    {
        public int id { get; set; }
        public int empleado_id { get; set; }
        public string? talla_camisa { get; set; }
        public string? talla_pantalon { get; set; }
        public string? talla_overall { get; set; }
        public string? talla_chaqueta { get; set; }
        public int? talla_calzado_numero { get; set; }
        public string? ancho_calzado { get; set; }
        public string? tipo_calzado_preferido { get; set; }
        public string? talla_guantes { get; set; }
        public string? talla_casco { get; set; }
        public string? talla_gafas { get; set; }
        public string? observaciones { get; set; }
        public int activo { get; set; }
        public DateTime fecha_creacion { get; set; }
        public DateTime? fecha_modificacion { get; set; }

        public TallasEmpleado ToEntity() => new()
        {
            Id = id,
            EmpleadoId = empleado_id,
            TallaCamisa = talla_camisa,
            TallaPantalon = talla_pantalon,
            TallaOverall = talla_overall,
            TallaChaqueta = talla_chaqueta,
            TallaCalzadoNumero = talla_calzado_numero,
            AnchoCalzado = ancho_calzado,
            TipoCalzadoPreferido = tipo_calzado_preferido,
            TallaGuantes = talla_guantes,
            TallaCasco = talla_casco,
            TallaGafas = talla_gafas,
            Observaciones = observaciones,
            Activo = activo == 1,
            FechaCreacion = fecha_creacion,
            FechaModificacion = fecha_modificacion
        };
    }
}
