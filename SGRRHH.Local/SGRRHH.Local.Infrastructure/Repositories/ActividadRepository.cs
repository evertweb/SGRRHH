using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Repositories;

public class ActividadRepository : IActividadRepository
{
    private readonly Data.DapperContext _context;
    private readonly ILogger<ActividadRepository> _logger;

    public ActividadRepository(Data.DapperContext context, ILogger<ActividadRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Actividad> AddAsync(Actividad entity)
    {
        entity.FechaCreacion = DateTime.Now;
        const string sql = @"
            INSERT INTO actividades (codigo, nombre, descripcion, categoria, CategoriaId, CategoriaTexto, 
                UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RendimientoMinimo, CostoUnitario,
                requiere_proyecto, RequiereCantidad, TiposProyectoAplicables, EspeciesAplicables, 
                orden, EsDestacada, activo, fecha_creacion)
            VALUES (@Codigo, @Nombre, @Descripcion, @CategoriaTexto, @CategoriaId, @CategoriaTexto,
                @UnidadMedida, @UnidadAbreviatura, @RendimientoEsperado, @RendimientoMinimo, @CostoUnitario,
                @RequiereProyecto, @RequiereCantidad, @TiposProyectoAplicables, @EspeciesAplicables,
                @Orden, @EsDestacada, 1, @FechaCreacion);
            SELECT last_insert_rowid();";

        using var connection = _context.CreateConnection();
        entity.Id = await connection.ExecuteScalarAsync<int>(sql, entity);
        return entity;
    }

    public async Task UpdateAsync(Actividad entity)
    {
        entity.FechaModificacion = DateTime.Now;
        const string sql = @"
            UPDATE actividades
            SET codigo = @Codigo,
                nombre = @Nombre,
                descripcion = @Descripcion,
                categoria = @CategoriaTexto,
                CategoriaId = @CategoriaId,
                CategoriaTexto = @CategoriaTexto,
                UnidadMedida = @UnidadMedida,
                UnidadAbreviatura = @UnidadAbreviatura,
                RendimientoEsperado = @RendimientoEsperado,
                RendimientoMinimo = @RendimientoMinimo,
                CostoUnitario = @CostoUnitario,
                requiere_proyecto = @RequiereProyecto,
                RequiereCantidad = @RequiereCantidad,
                TiposProyectoAplicables = @TiposProyectoAplicables,
                EspeciesAplicables = @EspeciesAplicables,
                orden = @Orden,
                EsDestacada = @EsDestacada,
                fecha_modificacion = @FechaModificacion
            WHERE id = @Id";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(int id)
    {
        // Hard delete - elimina permanentemente el registro
        const string sql = "DELETE FROM actividades WHERE id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<Actividad?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT a.*, c.Id, c.Codigo, c.Nombre, c.ColorHex, c.Orden
            FROM actividades a
            LEFT JOIN CategoriasActividades c ON a.CategoriaId = c.Id
            WHERE a.id = @Id";
        using var connection = _context.CreateConnection();
        
        var result = await connection.QueryAsync<Actividad, CategoriaActividad, Actividad>(
            sql,
            (actividad, categoria) =>
            {
                actividad.Categoria = categoria;
                return actividad;
            },
            new { Id = id },
            splitOn: "Id"
        );
        
        return result.FirstOrDefault();
    }

    public async Task<IEnumerable<Actividad>> GetAllAsync()
    {
        const string sql = "SELECT * FROM actividades ORDER BY orden, nombre";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Actividad>(sql);
    }

    public async Task<IEnumerable<Actividad>> GetAllActiveAsync()
    {
        const string sql = "SELECT * FROM actividades WHERE activo = 1 ORDER BY orden, nombre";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Actividad>(sql);
    }

    public async Task<IEnumerable<Actividad>> GetAllWithCategoriaAsync()
    {
        const string sql = @"
            SELECT a.*, c.Id, c.Codigo, c.Nombre, c.ColorHex, c.Orden as CatOrden
            FROM actividades a
            LEFT JOIN CategoriasActividades c ON a.CategoriaId = c.Id
            ORDER BY COALESCE(c.Orden, 999), a.orden, a.nombre";
        
        using var connection = _context.CreateConnection();
        
        var result = await connection.QueryAsync<Actividad, CategoriaActividad, Actividad>(
            sql,
            (actividad, categoria) =>
            {
                if (categoria?.Id > 0)
                {
                    actividad.Categoria = categoria;
                }
                return actividad;
            },
            splitOn: "Id"
        );
        
        return result;
    }

    public async Task<IEnumerable<Actividad>> GetAllActiveWithCategoriaAsync()
    {
        const string sql = @"
            SELECT a.*, c.Id, c.Codigo, c.Nombre, c.ColorHex, c.Orden as CatOrden
            FROM actividades a
            LEFT JOIN CategoriasActividades c ON a.CategoriaId = c.Id
            WHERE a.activo = 1
            ORDER BY COALESCE(c.Orden, 999), a.orden, a.nombre";
        
        using var connection = _context.CreateConnection();
        
        var result = await connection.QueryAsync<Actividad, CategoriaActividad, Actividad>(
            sql,
            (actividad, categoria) =>
            {
                if (categoria?.Id > 0)
                {
                    actividad.Categoria = categoria;
                }
                return actividad;
            },
            splitOn: "Id"
        );
        
        return result;
    }

    public async Task<IEnumerable<Actividad>> GetByCategoriaAsync(string categoria)
    {
        const string sql = @"
            SELECT * FROM actividades 
            WHERE (categoria = @Categoria OR CategoriaTexto = @Categoria) AND activo = 1
            ORDER BY orden, nombre";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Actividad>(sql, new { Categoria = categoria });
    }

    public async Task<IEnumerable<Actividad>> GetByCategoriaIdAsync(int categoriaId)
    {
        const string sql = @"
            SELECT a.*, c.Id, c.Codigo, c.Nombre, c.ColorHex
            FROM actividades a
            LEFT JOIN CategoriasActividades c ON a.CategoriaId = c.Id
            WHERE a.CategoriaId = @CategoriaId AND a.activo = 1
            ORDER BY a.orden, a.nombre";
        
        using var connection = _context.CreateConnection();
        
        var result = await connection.QueryAsync<Actividad, CategoriaActividad, Actividad>(
            sql,
            (actividad, categoria) =>
            {
                actividad.Categoria = categoria;
                return actividad;
            },
            new { CategoriaId = categoriaId },
            splitOn: "Id"
        );
        
        return result;
    }

    public async Task<IEnumerable<string>> GetCategoriasAsync()
    {
        const string sql = @"
            SELECT DISTINCT COALESCE(CategoriaTexto, categoria) as Categoria 
            FROM actividades 
            WHERE COALESCE(CategoriaTexto, categoria) IS NOT NULL 
              AND COALESCE(CategoriaTexto, categoria) <> ''
            ORDER BY Categoria";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<string>(sql);
    }

    public async Task<IEnumerable<Actividad>> GetDestacadasAsync()
    {
        const string sql = @"
            SELECT a.*, c.Id, c.Codigo, c.Nombre, c.ColorHex
            FROM actividades a
            LEFT JOIN CategoriasActividades c ON a.CategoriaId = c.Id
            WHERE a.EsDestacada = 1 AND a.activo = 1
            ORDER BY a.orden, a.nombre";
        
        using var connection = _context.CreateConnection();
        
        var result = await connection.QueryAsync<Actividad, CategoriaActividad, Actividad>(
            sql,
            (actividad, categoria) =>
            {
                if (categoria?.Id > 0)
                {
                    actividad.Categoria = categoria;
                }
                return actividad;
            },
            splitOn: "Id"
        );
        
        return result;
    }

    public async Task<IEnumerable<Actividad>> SearchAsync(string searchTerm)
    {
        var term = $"%{searchTerm}%";
        const string sql = @"
            SELECT a.*, c.Id, c.Codigo, c.Nombre, c.ColorHex
            FROM actividades a
            LEFT JOIN CategoriasActividades c ON a.CategoriaId = c.Id
            WHERE lower(a.nombre) LIKE lower(@Term) 
               OR lower(a.codigo) LIKE lower(@Term) 
               OR lower(a.descripcion) LIKE lower(@Term)
               OR lower(a.CategoriaTexto) LIKE lower(@Term)
            ORDER BY a.orden, a.nombre";

        using var connection = _context.CreateConnection();
        
        var result = await connection.QueryAsync<Actividad, CategoriaActividad, Actividad>(
            sql,
            (actividad, categoria) =>
            {
                if (categoria?.Id > 0)
                {
                    actividad.Categoria = categoria;
                }
                return actividad;
            },
            new { Term = term },
            splitOn: "Id"
        );
        
        return result;
    }

    public async Task<bool> ExistsCodigoAsync(string codigo, int? excludeId = null)
    {
        const string sql = "SELECT COUNT(1) FROM actividades WHERE codigo = @Codigo AND (@ExcludeId IS NULL OR id <> @ExcludeId)";
        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Codigo = codigo, ExcludeId = excludeId });
        return count > 0;
    }

    public async Task<string> GetNextCodigoAsync()
    {
        const string sql = "SELECT codigo FROM actividades ORDER BY id DESC LIMIT 1";
        using var connection = _context.CreateConnection();
        var last = await connection.QuerySingleOrDefaultAsync<string>(sql);
        if (string.IsNullOrWhiteSpace(last))
        {
            return "ACT-001";
        }

        var numeric = int.TryParse(last.Split('-').LastOrDefault(), out var number) ? number + 1 : 1;
        return $"ACT-{numeric:000}";
    }

    public Task<int> SaveChangesAsync() => Task.FromResult(0);
}
