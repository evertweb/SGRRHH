using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Repositories;

public class ProyectoRepository : IProyectoRepository
{
    private readonly Data.DapperContext _context;
    private readonly ILogger<ProyectoRepository> _logger;

    public ProyectoRepository(Data.DapperContext context, ILogger<ProyectoRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Proyecto> AddAsync(Proyecto entity)
    {
        entity.FechaCreacion = DateTime.Now;
        const string sql = @"
            INSERT INTO proyectos (
                codigo, nombre, descripcion, cliente, ubicacion, presupuesto, progreso, 
                fecha_inicio, fecha_fin, estado, responsable_id, activo, fecha_creacion,
                tipo_proyecto, predio, lote, departamento, municipio, vereda,
                latitud, longitud, altitud_msnm, especie_id, area_hectareas, fecha_siembra,
                densidad_inicial, densidad_actual, turno_cosecha_anios, tipo_tenencia, certificacion,
                total_horas_trabajadas, costo_mano_obra_acumulado, total_jornales, fecha_ultima_actualizacion_metricas
            )
            VALUES (
                @Codigo, @Nombre, @Descripcion, @Cliente, @Ubicacion, @Presupuesto, @Progreso,
                @FechaInicio, @FechaFin, @Estado, @ResponsableId, 1, @FechaCreacion,
                @TipoProyecto, @Predio, @Lote, @Departamento, @Municipio, @Vereda,
                @Latitud, @Longitud, @AltitudMsnm, @EspecieId, @AreaHectareas, @FechaSiembra,
                @DensidadInicial, @DensidadActual, @TurnoCosechaAnios, @TipoTenencia, @Certificacion,
                @TotalHorasTrabajadas, @CostoManoObraAcumulado, @TotalJornales, @FechaUltimaActualizacionMetricas
            );
            SELECT last_insert_rowid();";

        using var connection = _context.CreateConnection();
        entity.Id = await connection.ExecuteScalarAsync<int>(sql, entity);
        return entity;
    }

    public async Task UpdateAsync(Proyecto entity)
    {
        entity.FechaModificacion = DateTime.Now;
        const string sql = @"
            UPDATE proyectos SET
                codigo = @Codigo,
                nombre = @Nombre,
                descripcion = @Descripcion,
                cliente = @Cliente,
                ubicacion = @Ubicacion,
                presupuesto = @Presupuesto,
                progreso = @Progreso,
                fecha_inicio = @FechaInicio,
                fecha_fin = @FechaFin,
                estado = @Estado,
                responsable_id = @ResponsableId,
                fecha_modificacion = @FechaModificacion,
                tipo_proyecto = @TipoProyecto,
                predio = @Predio,
                lote = @Lote,
                departamento = @Departamento,
                municipio = @Municipio,
                vereda = @Vereda,
                latitud = @Latitud,
                longitud = @Longitud,
                altitud_msnm = @AltitudMsnm,
                especie_id = @EspecieId,
                area_hectareas = @AreaHectareas,
                fecha_siembra = @FechaSiembra,
                densidad_inicial = @DensidadInicial,
                densidad_actual = @DensidadActual,
                turno_cosecha_anios = @TurnoCosechaAnios,
                tipo_tenencia = @TipoTenencia,
                certificacion = @Certificacion,
                total_horas_trabajadas = @TotalHorasTrabajadas,
                costo_mano_obra_acumulado = @CostoManoObraAcumulado,
                total_jornales = @TotalJornales,
                fecha_ultima_actualizacion_metricas = @FechaUltimaActualizacionMetricas
            WHERE id = @Id";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(int id)
    {
        // Hard delete - elimina permanentemente el registro
        const string sql = "DELETE FROM proyectos WHERE id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<Proyecto?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT p.*, ef.*
            FROM proyectos p
            LEFT JOIN especies_forestales ef ON p.especie_id = ef.id
            WHERE p.id = @Id";
        
        using var connection = _context.CreateConnection();
        var result = await connection.QueryAsync<Proyecto, EspecieForestal, Proyecto>(
            sql,
            (proyecto, especie) =>
            {
                proyecto.Especie = especie;
                return proyecto;
            },
            new { Id = id },
            splitOn: "id");
        
        return result.FirstOrDefault();
    }

    public async Task<IEnumerable<Proyecto>> GetAllAsync()
    {
        const string sql = @"
            SELECT p.*, ef.*
            FROM proyectos p
            LEFT JOIN especies_forestales ef ON p.especie_id = ef.id
            ORDER BY p.nombre";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Proyecto, EspecieForestal, Proyecto>(
            sql,
            (proyecto, especie) =>
            {
                proyecto.Especie = especie;
                return proyecto;
            },
            splitOn: "id");
    }

    public async Task<IEnumerable<Proyecto>> GetAllActiveAsync()
    {
        const string sql = @"
            SELECT p.*, ef.*
            FROM proyectos p
            LEFT JOIN especies_forestales ef ON p.especie_id = ef.id
            WHERE p.activo = 1
            ORDER BY p.nombre";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Proyecto, EspecieForestal, Proyecto>(
            sql,
            (proyecto, especie) =>
            {
                proyecto.Especie = especie;
                return proyecto;
            },
            splitOn: "id");
    }

    public async Task<IEnumerable<Proyecto>> GetByEstadoAsync(EstadoProyecto estado)
    {
        const string sql = @"
            SELECT p.*, ef.*
            FROM proyectos p
            LEFT JOIN especies_forestales ef ON p.especie_id = ef.id
            WHERE p.estado = @Estado
            ORDER BY p.nombre";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Proyecto, EspecieForestal, Proyecto>(
            sql,
            (proyecto, especie) =>
            {
                proyecto.Especie = especie;
                return proyecto;
            },
            new { Estado = estado },
            splitOn: "id");
    }

    public async Task<IEnumerable<Proyecto>> SearchAsync(string searchTerm)
    {
        return await SearchAsync(searchTerm, null);
    }

    public async Task<IEnumerable<Proyecto>> SearchAsync(string? searchTerm, EstadoProyecto? estado)
    {
        var sql = @"
            SELECT p.*, ef.*
            FROM proyectos p
            LEFT JOIN especies_forestales ef ON p.especie_id = ef.id
            WHERE 1=1";
        
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            sql += @" AND (
                lower(p.nombre) LIKE lower(@Term) OR 
                lower(p.codigo) LIKE lower(@Term) OR 
                lower(p.descripcion) LIKE lower(@Term) OR
                lower(p.predio) LIKE lower(@Term) OR
                lower(p.lote) LIKE lower(@Term) OR
                lower(p.departamento) LIKE lower(@Term) OR
                lower(p.municipio) LIKE lower(@Term) OR
                lower(ef.nombre_comun) LIKE lower(@Term)
            )";
        }
        if (estado.HasValue)
        {
            sql += " AND p.estado = @Estado";
        }
        sql += " ORDER BY p.nombre";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Proyecto, EspecieForestal, Proyecto>(
            sql,
            (proyecto, especie) =>
            {
                proyecto.Especie = especie;
                return proyecto;
            },
            new { Term = $"%{searchTerm}%", Estado = estado },
            splitOn: "id");
    }

    public async Task<bool> ExistsCodigoAsync(string codigo, int? excludeId = null)
    {
        const string sql = "SELECT COUNT(1) FROM proyectos WHERE codigo = @Codigo AND (@ExcludeId IS NULL OR id <> @ExcludeId)";
        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Codigo = codigo, ExcludeId = excludeId });
        return count > 0;
    }

    public async Task<string> GetNextCodigoAsync()
    {
        const string sql = "SELECT codigo FROM proyectos ORDER BY id DESC LIMIT 1";
        using var connection = _context.CreateConnection();
        var last = await connection.QuerySingleOrDefaultAsync<string>(sql);
        if (string.IsNullOrWhiteSpace(last))
        {
            return "PROY-001";
        }

        var numeric = int.TryParse(last.Split('-').LastOrDefault(), out var number) ? number + 1 : 1;
        return $"PROY-{numeric:000}";
    }

    public async Task<IEnumerable<Proyecto>> GetProximosAVencerAsync(int diasAnticipacion = 7)
    {
        const string sql = @"SELECT * FROM proyectos
WHERE fecha_fin IS NOT NULL
  AND fecha_fin <= date('now', '+' || @Dias || ' day')
  AND fecha_fin >= date('now')
  AND estado = @Estado";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Proyecto>(sql, new { Dias = diasAnticipacion, Estado = EstadoProyecto.Activo });
    }

    public async Task<IEnumerable<Proyecto>> GetVencidosAsync()
    {
        const string sql = "SELECT * FROM proyectos WHERE fecha_fin < date('now') AND estado = @Estado";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Proyecto>(sql, new { Estado = EstadoProyecto.Activo });
    }

    public async Task<IEnumerable<Proyecto>> GetByResponsableAsync(int empleadoId)
    {
        const string sql = "SELECT * FROM proyectos WHERE responsable_id = @EmpleadoId";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Proyecto>(sql, new { EmpleadoId = empleadoId });
    }

    public async Task<ProyectoEstadisticas> GetEstadisticasAsync()
    {
        const string sql = @"SELECT
    COUNT(1) AS TotalProyectos,
    SUM(CASE WHEN estado = 0 THEN 1 ELSE 0 END) AS Activos,
    SUM(CASE WHEN estado = 1 THEN 1 ELSE 0 END) AS Suspendidos,
    SUM(CASE WHEN estado = 2 THEN 1 ELSE 0 END) AS Finalizados,
    SUM(CASE WHEN estado = 3 THEN 1 ELSE 0 END) AS Cancelados,
    SUM(CASE WHEN estado = 0 AND fecha_fin IS NOT NULL AND fecha_fin <= date('now', '+7 day') AND fecha_fin >= date('now') THEN 1 ELSE 0 END) AS ProximosAVencer,
    SUM(CASE WHEN estado = 0 AND fecha_fin IS NOT NULL AND fecha_fin < date('now') THEN 1 ELSE 0 END) AS Vencidos,
    SUM(COALESCE(presupuesto, 0)) AS PresupuestoTotal
FROM proyectos";

        using var connection = _context.CreateConnection();
        return await connection.QuerySingleAsync<ProyectoEstadisticas>(sql);
    }

    public Task<int> SaveChangesAsync() => Task.FromResult(0);
}
