using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Domain.Interfaces;
using SGRRHH.Local.Infrastructure.Data;

namespace SGRRHH.Local.Infrastructure.Repositories;

/// <summary>
/// Repositorio para gestión de PDFs de hojas de vida con Dapper
/// </summary>
public class HojaVidaPdfRepositorio : IHojaVidaPdfRepositorio
{
    private readonly DapperContext _context;
    private readonly ILogger<HojaVidaPdfRepositorio> _logger;

    public HojaVidaPdfRepositorio(DapperContext context, ILogger<HojaVidaPdfRepositorio> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<HojaVidaPdf?> ObtenerPorIdAsync(int id)
    {
        const string sql = "SELECT * FROM hoja_vida_pdf WHERE id = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<HojaVidaPdf>(sql, new { Id = id });
    }

    public async Task<List<HojaVidaPdf>> ObtenerPorAspiranteAsync(int aspiranteId)
    {
        const string sql = @"
            SELECT * FROM hoja_vida_pdf 
            WHERE aspirante_id = @AspiranteId AND activo = 1 
            ORDER BY version DESC";

        using var connection = _context.CreateConnection();
        var resultado = await connection.QueryAsync<HojaVidaPdf>(sql, new { AspiranteId = aspiranteId });
        return resultado.ToList();
    }

    public async Task<List<HojaVidaPdf>> ObtenerPorEmpleadoAsync(int empleadoId)
    {
        const string sql = @"
            SELECT * FROM hoja_vida_pdf 
            WHERE empleado_id = @EmpleadoId AND activo = 1 
            ORDER BY version DESC";

        using var connection = _context.CreateConnection();
        var resultado = await connection.QueryAsync<HojaVidaPdf>(sql, new { EmpleadoId = empleadoId });
        return resultado.ToList();
    }

    public async Task<HojaVidaPdf?> ObtenerUltimaVersionAspiranteAsync(int aspiranteId)
    {
        const string sql = @"
            SELECT * FROM hoja_vida_pdf 
            WHERE aspirante_id = @AspiranteId AND activo = 1 
            ORDER BY version DESC 
            LIMIT 1";

        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<HojaVidaPdf>(sql, new { AspiranteId = aspiranteId });
    }

    public async Task<HojaVidaPdf?> ObtenerUltimaVersionEmpleadoAsync(int empleadoId)
    {
        const string sql = @"
            SELECT * FROM hoja_vida_pdf 
            WHERE empleado_id = @EmpleadoId AND activo = 1 
            ORDER BY version DESC 
            LIMIT 1";

        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<HojaVidaPdf>(sql, new { EmpleadoId = empleadoId });
    }

    public async Task<int> CrearAsync(HojaVidaPdf hojaVidaPdf)
    {
        hojaVidaPdf.FechaCreacion = DateTime.Now;
        hojaVidaPdf.FechaSubida = DateTime.Now;
        hojaVidaPdf.Activo = true;
        hojaVidaPdf.EsActivo = true;

        // Obtener siguiente versión
        hojaVidaPdf.Version = await ObtenerSiguienteVersionAsync(hojaVidaPdf.AspiranteId, hojaVidaPdf.EmpleadoId);

        const string sql = @"
            INSERT INTO hoja_vida_pdf (
                aspirante_id, empleado_id, documento_empleado_id, version,
                hash_contenido, origen, fecha_generacion, fecha_subida,
                datos_extraidos, tiene_firma, es_valido, errores_validacion,
                es_activo, activo, fecha_creacion
            ) VALUES (
                @AspiranteId, @EmpleadoId, @DocumentoEmpleadoId, @Version,
                @HashContenido, @Origen, @FechaGeneracion, @FechaSubida,
                @DatosExtraidos, @TieneFirma, @EsValido, @ErroresValidacion,
                @EsActivo, @Activo, @FechaCreacion
            );
            SELECT last_insert_rowid();";

        using var connection = _context.CreateConnection();
        var id = await connection.ExecuteScalarAsync<int>(sql, new
        {
            hojaVidaPdf.AspiranteId,
            hojaVidaPdf.EmpleadoId,
            hojaVidaPdf.DocumentoEmpleadoId,
            hojaVidaPdf.Version,
            hojaVidaPdf.HashContenido,
            Origen = hojaVidaPdf.Origen.ToString(),
            hojaVidaPdf.FechaGeneracion,
            hojaVidaPdf.FechaSubida,
            hojaVidaPdf.DatosExtraidos,
            hojaVidaPdf.TieneFirma,
            hojaVidaPdf.EsValido,
            hojaVidaPdf.ErroresValidacion,
            hojaVidaPdf.EsActivo,
            hojaVidaPdf.Activo,
            hojaVidaPdf.FechaCreacion
        });

        _logger.LogInformation("HojaVidaPdf {Id} creada (v{Version})", id, hojaVidaPdf.Version);
        return id;
    }

    public async Task<bool> ActualizarAsync(HojaVidaPdf hojaVidaPdf)
    {
        hojaVidaPdf.FechaModificacion = DateTime.Now;

        const string sql = @"
            UPDATE hoja_vida_pdf SET
                datos_extraidos = @DatosExtraidos,
                tiene_firma = @TieneFirma,
                es_valido = @EsValido,
                errores_validacion = @ErroresValidacion,
                es_activo = @EsActivo,
                fecha_modificacion = @FechaModificacion
            WHERE id = @Id";

        using var connection = _context.CreateConnection();
        var filas = await connection.ExecuteAsync(sql, new
        {
            hojaVidaPdf.DatosExtraidos,
            hojaVidaPdf.TieneFirma,
            hojaVidaPdf.EsValido,
            hojaVidaPdf.ErroresValidacion,
            hojaVidaPdf.EsActivo,
            hojaVidaPdf.FechaModificacion,
            hojaVidaPdf.Id
        });

        return filas > 0;
    }

    public async Task<bool> MarcarInvalidoAsync(int id, string errores)
    {
        const string sql = @"
            UPDATE hoja_vida_pdf SET
                es_valido = 0,
                errores_validacion = @Errores,
                fecha_modificacion = @FechaModificacion
            WHERE id = @Id";

        using var connection = _context.CreateConnection();
        var filas = await connection.ExecuteAsync(sql, new
        {
            Errores = errores,
            FechaModificacion = DateTime.Now,
            Id = id
        });

        if (filas > 0)
        {
            _logger.LogWarning("HojaVidaPdf {Id} marcada como inválida: {Errores}", id, errores);
        }

        return filas > 0;
    }

    public async Task<bool> ExisteHashAsync(string hashContenido)
    {
        const string sql = "SELECT COUNT(1) FROM hoja_vida_pdf WHERE hash_contenido = @Hash AND activo = 1";
        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Hash = hashContenido });
        return count > 0;
    }

    public async Task<int> ObtenerSiguienteVersionAsync(int? aspiranteId, int? empleadoId)
    {
        string sql;
        object parametros;

        if (aspiranteId.HasValue)
        {
            sql = "SELECT COALESCE(MAX(version), 0) + 1 FROM hoja_vida_pdf WHERE aspirante_id = @Id";
            parametros = new { Id = aspiranteId.Value };
        }
        else if (empleadoId.HasValue)
        {
            sql = "SELECT COALESCE(MAX(version), 0) + 1 FROM hoja_vida_pdf WHERE empleado_id = @Id";
            parametros = new { Id = empleadoId.Value };
        }
        else
        {
            return 1;
        }

        using var connection = _context.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, parametros);
    }

    public async Task<bool> EliminarAsync(int id)
    {
        const string sql = @"
            UPDATE hoja_vida_pdf SET 
                activo = 0,
                es_activo = 0,
                fecha_modificacion = @FechaModificacion
            WHERE id = @Id";

        using var connection = _context.CreateConnection();
        var filas = await connection.ExecuteAsync(sql, new
        {
            FechaModificacion = DateTime.Now,
            Id = id
        });

        if (filas > 0)
        {
            _logger.LogInformation("HojaVidaPdf {Id} eliminada (soft delete)", id);
        }

        return filas > 0;
    }

    public async Task<List<HojaVidaPdf>> BuscarFtsAsync(string termino)
    {
        // Búsqueda en la tabla virtual FTS5
        const string sql = @"
            SELECT h.* FROM hoja_vida_pdf h
            INNER JOIN hojas_vida_fts fts ON h.id = fts.rowid
            WHERE hojas_vida_fts MATCH @Termino
            AND h.activo = 1
            ORDER BY rank";

        try
        {
            using var connection = _context.CreateConnection();
            var resultado = await connection.QueryAsync<HojaVidaPdf>(sql, new { Termino = termino });
            return resultado.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en búsqueda FTS con término: {Termino}", termino);
            return new List<HojaVidaPdf>();
        }
    }

    public async Task<bool> ActualizarIndiceFtsAsync(int aspiranteId)
    {
        try
        {
            // Obtener datos del aspirante para indexar
            const string sqlAspirante = @"
                SELECT a.id, a.nombres, a.apellidos, a.cedula,
                       a.titulo_obtenido as formacion
                FROM aspirantes a
                WHERE a.id = @AspiranteId";

            using var connection = _context.CreateConnection();
            var aspirante = await connection.QuerySingleOrDefaultAsync<dynamic>(sqlAspirante, new { AspiranteId = aspiranteId });

            if (aspirante == null) return false;

            // Obtener experiencia concatenada
            var experiencias = await connection.QueryAsync<string>(
                "SELECT empresa || ' - ' || cargo FROM experiencia_aspirante WHERE aspirante_id = @Id AND activo = 1",
                new { Id = aspiranteId });
            var experienciaTexto = string.Join("; ", experiencias);

            // Eliminar índice existente
            await connection.ExecuteAsync(
                "DELETE FROM hojas_vida_fts WHERE aspirante_id = @AspiranteId",
                new { AspiranteId = aspiranteId.ToString() });

            // Insertar nuevo índice
            const string insertFts = @"
                INSERT INTO hojas_vida_fts (
                    aspirante_id, empleado_id, nombres, apellidos, 
                    cedula, formacion, experiencia, habilidades
                ) VALUES (
                    @AspiranteId, '', @Nombres, @Apellidos,
                    @Cedula, @Formacion, @Experiencia, ''
                )";

            await connection.ExecuteAsync(insertFts, new
            {
                AspiranteId = aspiranteId.ToString(),
                Nombres = (string)aspirante.nombres,
                Apellidos = (string)aspirante.apellidos,
                Cedula = (string)aspirante.cedula,
                Formacion = (string?)aspirante.formacion ?? "",
                Experiencia = experienciaTexto
            });

            _logger.LogDebug("Índice FTS actualizado para aspirante {Id}", aspiranteId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error actualizando índice FTS para aspirante {Id}", aspiranteId);
            return false;
        }
    }

    public async Task<List<HojaVidaPdf>> ObtenerPorOrigenAsync(OrigenHojaVida origen)
    {
        const string sql = @"
            SELECT * FROM hoja_vida_pdf 
            WHERE origen = @Origen AND activo = 1 
            ORDER BY fecha_subida DESC";

        using var connection = _context.CreateConnection();
        var resultado = await connection.QueryAsync<HojaVidaPdf>(sql, new { Origen = origen.ToString() });
        return resultado.ToList();
    }
}
