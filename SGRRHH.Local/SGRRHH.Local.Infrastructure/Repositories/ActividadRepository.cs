using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Repositories;

/// <summary>
/// Repositorio para gestionar actividades silviculturales.
/// Usa columnas snake_case estandarizadas con COALESCE para compatibilidad.
/// </summary>
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
            INSERT INTO actividades (codigo, nombre, descripcion, categoria, category_id, category_text, 
                unit_of_measure, unit_abbreviation, expected_yield, minimum_yield, unit_cost,
                requiere_proyecto, requires_quantity, applicable_project_types, applicable_species, 
                orden, is_featured, activo, fecha_creacion)
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
                category_id = @CategoriaId,
                category_text = @CategoriaTexto,
                unit_of_measure = @UnidadMedida,
                unit_abbreviation = @UnidadAbreviatura,
                expected_yield = @RendimientoEsperado,
                minimum_yield = @RendimientoMinimo,
                unit_cost = @CostoUnitario,
                requiere_proyecto = @RequiereProyecto,
                requires_quantity = @RequiereCantidad,
                applicable_project_types = @TiposProyectoAplicables,
                applicable_species = @EspeciesAplicables,
                orden = @Orden,
                is_featured = @EsDestacada,
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
            SELECT a.id, a.codigo, a.nombre, a.descripcion, 
                   COALESCE(a.category_text, a.CategoriaTexto, a.categoria) as CategoriaTexto,
                   COALESCE(a.category_id, a.CategoriaId) as CategoriaId,
                   COALESCE(a.unit_of_measure, a.UnidadMedida, 0) as UnidadMedida,
                   COALESCE(a.unit_abbreviation, a.UnidadAbreviatura) as UnidadAbreviatura,
                   COALESCE(a.expected_yield, a.RendimientoEsperado) as RendimientoEsperado,
                   COALESCE(a.minimum_yield, a.RendimientoMinimo) as RendimientoMinimo,
                   COALESCE(a.unit_cost, a.CostoUnitario) as CostoUnitario,
                   a.requiere_proyecto as RequiereProyecto,
                   COALESCE(a.requires_quantity, a.RequiereCantidad, 0) as RequiereCantidad,
                   COALESCE(a.applicable_project_types, a.TiposProyectoAplicables, a.TiposProyectoAplicables) as TiposProyectoAplicables,
                   COALESCE(a.applicable_species, a.EspeciesAplicables, a.EspeciesAplicables) as EspeciesAplicables,
                   a.orden as Orden,
                   COALESCE(a.is_featured, a.EsDestacada, 0) as EsDestacada,
                   a.activo as Activo,
                   a.fecha_creacion as FechaCreacion,
                   a.fecha_modificacion as FechaModificacion,
                   c.id, c.code AS Codigo, c.name AS Nombre, c.color_hex AS ColorHex, c.display_order AS Orden
            FROM actividades a
            LEFT JOIN activity_categories c ON COALESCE(a.category_id, a.CategoriaId) = c.id
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
            splitOn: "id"
        );
        
        return result.FirstOrDefault();
    }

    public async Task<IEnumerable<Actividad>> GetAllAsync()
    {
        const string sql = @"
            SELECT id, codigo, nombre, descripcion, 
                   COALESCE(category_text, CategoriaTexto, categoria) as CategoriaTexto,
                   COALESCE(category_id, CategoriaId) as CategoriaId,
                   COALESCE(unit_of_measure, UnidadMedida, 0) as UnidadMedida,
                   COALESCE(unit_abbreviation, UnidadAbreviatura) as UnidadAbreviatura,
                   COALESCE(expected_yield, RendimientoEsperado) as RendimientoEsperado,
                   COALESCE(minimum_yield, RendimientoMinimo) as RendimientoMinimo,
                   COALESCE(unit_cost, CostoUnitario) as CostoUnitario,
                   requiere_proyecto as RequiereProyecto,
                   COALESCE(requires_quantity, RequiereCantidad, 0) as RequiereCantidad,
                   COALESCE(applicable_project_types, TiposProyectoAplicables) as TiposProyectoAplicables,
                   COALESCE(applicable_species, EspeciesAplicables) as EspeciesAplicables,
                   orden as Orden,
                   COALESCE(is_featured, 0) as EsDestacada,
                   activo as Activo,
                   fecha_creacion as FechaCreacion,
                   fecha_modificacion as FechaModificacion
            FROM actividades ORDER BY orden, nombre";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Actividad>(sql);
    }

    public async Task<IEnumerable<Actividad>> GetAllActiveAsync()
    {
        const string sql = @"
            SELECT id, codigo, nombre, descripcion, 
                   COALESCE(category_text, CategoriaTexto, categoria) as CategoriaTexto,
                   COALESCE(category_id, CategoriaId) as CategoriaId,
                   COALESCE(unit_of_measure, UnidadMedida, 0) as UnidadMedida,
                   COALESCE(unit_abbreviation, UnidadAbreviatura) as UnidadAbreviatura,
                   COALESCE(expected_yield, RendimientoEsperado) as RendimientoEsperado,
                   COALESCE(minimum_yield, RendimientoMinimo) as RendimientoMinimo,
                   COALESCE(unit_cost, CostoUnitario) as CostoUnitario,
                   requiere_proyecto as RequiereProyecto,
                   COALESCE(requires_quantity, RequiereCantidad, 0) as RequiereCantidad,
                   COALESCE(applicable_project_types, TiposProyectoAplicables) as TiposProyectoAplicables,
                   COALESCE(applicable_species, EspeciesAplicables) as EspeciesAplicables,
                   orden as Orden,
                   COALESCE(is_featured, 0) as EsDestacada,
                   activo as Activo,
                   fecha_creacion as FechaCreacion,
                   fecha_modificacion as FechaModificacion
            FROM actividades WHERE activo = 1 ORDER BY orden, nombre";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Actividad>(sql);
    }

    public async Task<IEnumerable<Actividad>> GetAllWithCategoriaAsync()
    {
        const string sql = @"
            SELECT a.id, a.codigo, a.nombre, a.descripcion, 
                   COALESCE(a.category_text, a.CategoriaTexto, a.categoria) as CategoriaTexto,
                   COALESCE(a.category_id, a.CategoriaId) as CategoriaId,
                   COALESCE(a.unit_of_measure, a.UnidadMedida, 0) as UnidadMedida,
                   COALESCE(a.unit_abbreviation, a.UnidadAbreviatura) as UnidadAbreviatura,
                   COALESCE(a.expected_yield, a.RendimientoEsperado) as RendimientoEsperado,
                   COALESCE(a.minimum_yield, a.RendimientoMinimo) as RendimientoMinimo,
                   COALESCE(a.unit_cost, a.CostoUnitario) as CostoUnitario,
                   a.requiere_proyecto as RequiereProyecto,
                   COALESCE(a.requires_quantity, a.RequiereCantidad, 0) as RequiereCantidad,
                   COALESCE(a.applicable_project_types, a.TiposProyectoAplicables, a.TiposProyectoAplicables) as TiposProyectoAplicables,
                   COALESCE(a.applicable_species, a.EspeciesAplicables, a.EspeciesAplicables) as EspeciesAplicables,
                   a.orden as Orden,
                   COALESCE(a.is_featured, a.EsDestacada, 0) as EsDestacada,
                   a.activo as Activo,
                   a.fecha_creacion as FechaCreacion,
                   a.fecha_modificacion as FechaModificacion,
                   c.id, c.code AS Codigo, c.name AS Nombre, c.color_hex AS ColorHex, c.display_order AS CatOrden
            FROM actividades a
            LEFT JOIN activity_categories c ON COALESCE(a.category_id, a.CategoriaId) = c.id
            ORDER BY COALESCE(c.display_order, 999), a.orden, a.nombre";
        
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
            splitOn: "id"
        );
        
        return result;
    }

    public async Task<IEnumerable<Actividad>> GetAllActiveWithCategoriaAsync()
    {
        const string sql = @"
            SELECT a.id, a.codigo, a.nombre, a.descripcion, 
                   COALESCE(a.category_text, a.CategoriaTexto, a.categoria) as CategoriaTexto,
                   COALESCE(a.category_id, a.CategoriaId) as CategoriaId,
                   COALESCE(a.unit_of_measure, a.UnidadMedida, 0) as UnidadMedida,
                   COALESCE(a.unit_abbreviation, a.UnidadAbreviatura) as UnidadAbreviatura,
                   COALESCE(a.expected_yield, a.RendimientoEsperado) as RendimientoEsperado,
                   COALESCE(a.minimum_yield, a.RendimientoMinimo) as RendimientoMinimo,
                   COALESCE(a.unit_cost, a.CostoUnitario) as CostoUnitario,
                   a.requiere_proyecto as RequiereProyecto,
                   COALESCE(a.requires_quantity, a.RequiereCantidad, 0) as RequiereCantidad,
                   COALESCE(a.applicable_project_types, a.TiposProyectoAplicables, a.TiposProyectoAplicables) as TiposProyectoAplicables,
                   COALESCE(a.applicable_species, a.EspeciesAplicables, a.EspeciesAplicables) as EspeciesAplicables,
                   a.orden as Orden,
                   COALESCE(a.is_featured, a.EsDestacada, 0) as EsDestacada,
                   a.activo as Activo,
                   a.fecha_creacion as FechaCreacion,
                   a.fecha_modificacion as FechaModificacion,
                   c.id, c.code AS Codigo, c.name AS Nombre, c.color_hex AS ColorHex, c.display_order AS CatOrden
            FROM actividades a
            LEFT JOIN activity_categories c ON COALESCE(a.category_id, a.CategoriaId) = c.id
            WHERE a.activo = 1
            ORDER BY COALESCE(c.display_order, 999), a.orden, a.nombre";
        
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
            splitOn: "id"
        );
        
        return result;
    }

    public async Task<IEnumerable<Actividad>> GetByCategoriaAsync(string categoria)
    {
        const string sql = @"
            SELECT id, codigo, nombre, descripcion, 
                   COALESCE(category_text, CategoriaTexto, categoria) as CategoriaTexto,
                   COALESCE(category_id, CategoriaId) as CategoriaId,
                   COALESCE(unit_of_measure, UnidadMedida, 0) as UnidadMedida,
                   COALESCE(unit_abbreviation, UnidadAbreviatura) as UnidadAbreviatura,
                   COALESCE(expected_yield, RendimientoEsperado) as RendimientoEsperado,
                   COALESCE(minimum_yield, RendimientoMinimo) as RendimientoMinimo,
                   COALESCE(unit_cost, CostoUnitario) as CostoUnitario,
                   requiere_proyecto as RequiereProyecto,
                   COALESCE(requires_quantity, RequiereCantidad, 0) as RequiereCantidad,
                   COALESCE(applicable_project_types, TiposProyectoAplicables) as TiposProyectoAplicables,
                   COALESCE(applicable_species, EspeciesAplicables) as EspeciesAplicables,
                   orden as Orden,
                   COALESCE(is_featured, 0) as EsDestacada,
                   activo as Activo,
                   fecha_creacion as FechaCreacion,
                   fecha_modificacion as FechaModificacion
            FROM actividades 
            WHERE (categoria = @Categoria OR COALESCE(category_text, CategoriaTexto) = @Categoria) AND activo = 1
            ORDER BY orden, nombre";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Actividad>(sql, new { Categoria = categoria });
    }

    public async Task<IEnumerable<Actividad>> GetByCategoriaIdAsync(int categoriaId)
    {
        const string sql = @"
            SELECT a.id, a.codigo, a.nombre, a.descripcion, 
                   COALESCE(a.category_text, a.CategoriaTexto, a.categoria) as CategoriaTexto,
                   COALESCE(a.category_id, a.CategoriaId) as CategoriaId,
                   COALESCE(a.unit_of_measure, a.UnidadMedida, 0) as UnidadMedida,
                   COALESCE(a.unit_abbreviation, a.UnidadAbreviatura) as UnidadAbreviatura,
                   COALESCE(a.expected_yield, a.RendimientoEsperado) as RendimientoEsperado,
                   COALESCE(a.minimum_yield, a.RendimientoMinimo) as RendimientoMinimo,
                   COALESCE(a.unit_cost, a.CostoUnitario) as CostoUnitario,
                   a.requiere_proyecto as RequiereProyecto,
                   COALESCE(a.requires_quantity, a.RequiereCantidad, 0) as RequiereCantidad,
                   COALESCE(a.applicable_project_types, a.TiposProyectoAplicables, a.TiposProyectoAplicables) as TiposProyectoAplicables,
                   COALESCE(a.applicable_species, a.EspeciesAplicables, a.EspeciesAplicables) as EspeciesAplicables,
                   a.orden as Orden,
                   COALESCE(a.is_featured, a.EsDestacada, 0) as EsDestacada,
                   a.activo as Activo,
                   a.fecha_creacion as FechaCreacion,
                   a.fecha_modificacion as FechaModificacion,
                   c.id, c.code AS Codigo, c.name AS Nombre, c.color_hex AS ColorHex
            FROM actividades a
            LEFT JOIN activity_categories c ON COALESCE(a.category_id, a.CategoriaId) = c.id
            WHERE COALESCE(a.category_id, a.CategoriaId) = @CategoriaId AND a.activo = 1
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
            splitOn: "id"
        );
        
        return result;
    }

    public async Task<IEnumerable<string>> GetCategoriasAsync()
    {
        const string sql = @"
            SELECT DISTINCT COALESCE(category_text, CategoriaTexto, categoria) as Categoria 
            FROM actividades 
            WHERE COALESCE(category_text, CategoriaTexto, categoria) IS NOT NULL 
              AND COALESCE(category_text, CategoriaTexto, categoria) <> ''
            ORDER BY Categoria";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<string>(sql);
    }

    public async Task<IEnumerable<Actividad>> GetDestacadasAsync()
    {
        const string sql = @"
            SELECT a.id, a.codigo, a.nombre, a.descripcion, 
                   COALESCE(a.category_text, a.CategoriaTexto, a.categoria) as CategoriaTexto,
                   COALESCE(a.category_id, a.CategoriaId) as CategoriaId,
                   COALESCE(a.unit_of_measure, a.UnidadMedida, 0) as UnidadMedida,
                   COALESCE(a.unit_abbreviation, a.UnidadAbreviatura) as UnidadAbreviatura,
                   COALESCE(a.expected_yield, a.RendimientoEsperado) as RendimientoEsperado,
                   COALESCE(a.minimum_yield, a.RendimientoMinimo) as RendimientoMinimo,
                   COALESCE(a.unit_cost, a.CostoUnitario) as CostoUnitario,
                   a.requiere_proyecto as RequiereProyecto,
                   COALESCE(a.requires_quantity, a.RequiereCantidad, 0) as RequiereCantidad,
                   COALESCE(a.applicable_project_types, a.TiposProyectoAplicables, a.TiposProyectoAplicables) as TiposProyectoAplicables,
                   COALESCE(a.applicable_species, a.EspeciesAplicables, a.EspeciesAplicables) as EspeciesAplicables,
                   a.orden as Orden,
                   COALESCE(a.is_featured, a.EsDestacada, 0) as EsDestacada,
                   a.activo as Activo,
                   a.fecha_creacion as FechaCreacion,
                   a.fecha_modificacion as FechaModificacion,
                   c.id, c.code AS Codigo, c.name AS Nombre, c.color_hex AS ColorHex
            FROM actividades a
            LEFT JOIN activity_categories c ON COALESCE(a.category_id, a.CategoriaId) = c.id
            WHERE COALESCE(a.is_featured, a.EsDestacada, 0) = 1 AND a.activo = 1
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
            splitOn: "id"
        );
        
        return result;
    }

    public async Task<IEnumerable<Actividad>> SearchAsync(string searchTerm)
    {
        var term = $"%{searchTerm}%";
        const string sql = @"
            SELECT a.id, a.codigo, a.nombre, a.descripcion, 
                   COALESCE(a.category_text, a.CategoriaTexto, a.categoria) as CategoriaTexto,
                   COALESCE(a.category_id, a.CategoriaId) as CategoriaId,
                   COALESCE(a.unit_of_measure, a.UnidadMedida, 0) as UnidadMedida,
                   COALESCE(a.unit_abbreviation, a.UnidadAbreviatura) as UnidadAbreviatura,
                   COALESCE(a.expected_yield, a.RendimientoEsperado) as RendimientoEsperado,
                   COALESCE(a.minimum_yield, a.RendimientoMinimo) as RendimientoMinimo,
                   COALESCE(a.unit_cost, a.CostoUnitario) as CostoUnitario,
                   a.requiere_proyecto as RequiereProyecto,
                   COALESCE(a.requires_quantity, a.RequiereCantidad, 0) as RequiereCantidad,
                   COALESCE(a.applicable_project_types, a.TiposProyectoAplicables, a.TiposProyectoAplicables) as TiposProyectoAplicables,
                   COALESCE(a.applicable_species, a.EspeciesAplicables, a.EspeciesAplicables) as EspeciesAplicables,
                   a.orden as Orden,
                   COALESCE(a.is_featured, a.EsDestacada, 0) as EsDestacada,
                   a.activo as Activo,
                   a.fecha_creacion as FechaCreacion,
                   a.fecha_modificacion as FechaModificacion,
                   c.id, c.code AS Codigo, c.name AS Nombre, c.color_hex AS ColorHex
            FROM actividades a
            LEFT JOIN activity_categories c ON COALESCE(a.category_id, a.CategoriaId) = c.id
            WHERE lower(a.nombre) LIKE lower(@Term) 
               OR lower(a.codigo) LIKE lower(@Term) 
               OR lower(a.descripcion) LIKE lower(@Term)
               OR lower(COALESCE(a.category_text, a.CategoriaTexto)) LIKE lower(@Term)
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
            splitOn: "id"
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



