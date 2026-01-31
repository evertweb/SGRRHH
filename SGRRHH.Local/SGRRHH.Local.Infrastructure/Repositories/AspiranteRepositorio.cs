using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Domain.Interfaces;
using SGRRHH.Local.Infrastructure.Data;

namespace SGRRHH.Local.Infrastructure.Repositories;

/// <summary>
/// Repositorio para gestión de aspirantes con Dapper
/// </summary>
public class AspiranteRepositorio : IAspiranteRepositorio
{
    private readonly DapperContext _context;
    private readonly ILogger<AspiranteRepositorio> _logger;

    public AspiranteRepositorio(DapperContext context, ILogger<AspiranteRepositorio> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Aspirante?> ObtenerPorIdAsync(int id)
    {
        const string sql = "SELECT * FROM aspirantes WHERE id = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Aspirante>(sql, new { Id = id });
    }

    public async Task<Aspirante?> ObtenerPorCedulaAsync(string cedula)
    {
        const string sql = "SELECT * FROM aspirantes WHERE cedula = @Cedula";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Aspirante>(sql, new { Cedula = cedula });
    }

    public async Task<List<Aspirante>> ObtenerTodosAsync(bool incluirInactivos = false)
    {
        var sql = incluirInactivos
            ? "SELECT * FROM aspirantes ORDER BY fecha_registro DESC"
            : "SELECT * FROM aspirantes WHERE activo = 1 ORDER BY fecha_registro DESC";

        using var connection = _context.CreateConnection();
        var resultado = await connection.QueryAsync<Aspirante>(sql);
        return resultado.ToList();
    }

    public async Task<List<Aspirante>> ObtenerPorVacanteAsync(int vacanteId)
    {
        const string sql = @"
            SELECT * FROM aspirantes 
            WHERE vacante_id = @VacanteId AND activo = 1 
            ORDER BY fecha_registro DESC";

        using var connection = _context.CreateConnection();
        var resultado = await connection.QueryAsync<Aspirante>(sql, new { VacanteId = vacanteId });
        return resultado.ToList();
    }

    public async Task<List<Aspirante>> ObtenerPorEstadoAsync(EstadoAspirante estado)
    {
        const string sql = @"
            SELECT * FROM aspirantes 
            WHERE estado = @Estado AND activo = 1 
            ORDER BY fecha_registro DESC";

        using var connection = _context.CreateConnection();
        var resultado = await connection.QueryAsync<Aspirante>(sql, new { Estado = estado.ToString() });
        return resultado.ToList();
    }

    public async Task<List<Aspirante>> BuscarAsync(string termino)
    {
        var busqueda = $"%{termino}%";
        const string sql = @"
            SELECT * FROM aspirantes 
            WHERE (lower(nombres) LIKE lower(@Termino)
                OR lower(apellidos) LIKE lower(@Termino)
                OR cedula LIKE @Termino)
                AND activo = 1
            ORDER BY fecha_registro DESC";

        using var connection = _context.CreateConnection();
        var resultado = await connection.QueryAsync<Aspirante>(sql, new { Termino = busqueda });
        return resultado.ToList();
    }

    public async Task<int> CrearAsync(Aspirante aspirante)
    {
        aspirante.FechaCreacion = DateTime.Now;
        aspirante.FechaRegistro = DateTime.Now;
        aspirante.Activo = true;
        aspirante.EsActivo = true;

        const string sql = @"
            INSERT INTO aspirantes (
                vacante_id, cedula, nombres, apellidos, fecha_nacimiento,
                genero, estado_civil, direccion, ciudad, departamento,
                telefono, email, nivel_educacion, titulo_obtenido, institucion_educativa,
                tallas_casco, tallas_botas, estado, fecha_registro, notas,
                puntaje_evaluacion, es_activo, activo, fecha_creacion
            ) VALUES (
                @VacanteId, @Cedula, @Nombres, @Apellidos, @FechaNacimiento,
                @Genero, @EstadoCivil, @Direccion, @Ciudad, @Departamento,
                @Telefono, @Email, @NivelEducacion, @TituloObtenido, @InstitucionEducativa,
                @TallasCasco, @TallasBotas, @Estado, @FechaRegistro, @Notas,
                @PuntajeEvaluacion, @EsActivo, @Activo, @FechaCreacion
            );
            SELECT last_insert_rowid();";

        using var connection = _context.CreateConnection();
        var id = await connection.ExecuteScalarAsync<int>(sql, new
        {
            aspirante.VacanteId,
            aspirante.Cedula,
            aspirante.Nombres,
            aspirante.Apellidos,
            aspirante.FechaNacimiento,
            aspirante.Genero,
            aspirante.EstadoCivil,
            aspirante.Direccion,
            aspirante.Ciudad,
            aspirante.Departamento,
            aspirante.Telefono,
            aspirante.Email,
            aspirante.NivelEducacion,
            aspirante.TituloObtenido,
            aspirante.InstitucionEducativa,
            aspirante.TallasCasco,
            aspirante.TallasBotas,
            Estado = aspirante.Estado.ToString(),
            aspirante.FechaRegistro,
            aspirante.Notas,
            aspirante.PuntajeEvaluacion,
            aspirante.EsActivo,
            aspirante.Activo,
            aspirante.FechaCreacion
        });

        _logger.LogInformation("Aspirante {Id} creado: {Nombres} {Apellidos}", id, aspirante.Nombres, aspirante.Apellidos);
        return id;
    }

    public async Task<bool> ActualizarAsync(Aspirante aspirante)
    {
        aspirante.FechaModificacion = DateTime.Now;

        const string sql = @"
            UPDATE aspirantes SET
                vacante_id = @VacanteId,
                cedula = @Cedula,
                nombres = @Nombres,
                apellidos = @Apellidos,
                fecha_nacimiento = @FechaNacimiento,
                genero = @Genero,
                estado_civil = @EstadoCivil,
                direccion = @Direccion,
                ciudad = @Ciudad,
                departamento = @Departamento,
                telefono = @Telefono,
                email = @Email,
                nivel_educacion = @NivelEducacion,
                titulo_obtenido = @TituloObtenido,
                institucion_educativa = @InstitucionEducativa,
                tallas_casco = @TallasCasco,
                tallas_botas = @TallasBotas,
                estado = @Estado,
                notas = @Notas,
                puntaje_evaluacion = @PuntajeEvaluacion,
                es_activo = @EsActivo,
                fecha_modificacion = @FechaModificacion
            WHERE id = @Id";

        using var connection = _context.CreateConnection();
        var filas = await connection.ExecuteAsync(sql, new
        {
            aspirante.VacanteId,
            aspirante.Cedula,
            aspirante.Nombres,
            aspirante.Apellidos,
            aspirante.FechaNacimiento,
            aspirante.Genero,
            aspirante.EstadoCivil,
            aspirante.Direccion,
            aspirante.Ciudad,
            aspirante.Departamento,
            aspirante.Telefono,
            aspirante.Email,
            aspirante.NivelEducacion,
            aspirante.TituloObtenido,
            aspirante.InstitucionEducativa,
            aspirante.TallasCasco,
            aspirante.TallasBotas,
            Estado = aspirante.Estado.ToString(),
            aspirante.Notas,
            aspirante.PuntajeEvaluacion,
            aspirante.EsActivo,
            aspirante.FechaModificacion,
            aspirante.Id
        });

        if (filas == 0)
        {
            _logger.LogWarning("Aspirante {Id} no encontrado para actualizar", aspirante.Id);
            return false;
        }

        return true;
    }

    public async Task<bool> CambiarEstadoAsync(int id, EstadoAspirante nuevoEstado, string? notas = null)
    {
        var sql = notas != null
            ? "UPDATE aspirantes SET estado = @Estado, notas = @Notas, fecha_modificacion = @FechaModificacion WHERE id = @Id"
            : "UPDATE aspirantes SET estado = @Estado, fecha_modificacion = @FechaModificacion WHERE id = @Id";

        using var connection = _context.CreateConnection();
        var filas = await connection.ExecuteAsync(sql, new
        {
            Estado = nuevoEstado.ToString(),
            Notas = notas,
            FechaModificacion = DateTime.Now,
            Id = id
        });

        if (filas == 0)
        {
            _logger.LogWarning("No se pudo cambiar estado de aspirante {Id}", id);
            return false;
        }

        _logger.LogInformation("Aspirante {Id} cambió a estado {Estado}", id, nuevoEstado);
        return true;
    }

    public async Task<bool> ActualizarPuntajeAsync(int id, int puntaje)
    {
        const string sql = @"
            UPDATE aspirantes SET 
                puntaje_evaluacion = @Puntaje,
                fecha_modificacion = @FechaModificacion
            WHERE id = @Id";

        using var connection = _context.CreateConnection();
        var filas = await connection.ExecuteAsync(sql, new
        {
            Puntaje = puntaje,
            FechaModificacion = DateTime.Now,
            Id = id
        });

        return filas > 0;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        const string sql = @"
            UPDATE aspirantes SET 
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

        if (filas == 0)
        {
            _logger.LogWarning("No se pudo eliminar aspirante {Id}", id);
            return false;
        }

        _logger.LogInformation("Aspirante {Id} eliminado (soft delete)", id);
        return true;
    }

    public async Task<bool> ExisteCedulaAsync(string cedula, int? excluirId = null)
    {
        const string sql = @"
            SELECT COUNT(1) FROM aspirantes 
            WHERE cedula = @Cedula 
            AND (@ExcluirId IS NULL OR id <> @ExcluirId)";

        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Cedula = cedula, ExcluirId = excluirId });
        return count > 0;
    }

    public async Task<List<FormacionAspirante>> ObtenerFormacionAsync(int aspiranteId)
    {
        const string sql = @"
            SELECT * FROM formacion_aspirante 
            WHERE aspirante_id = @AspiranteId AND activo = 1 
            ORDER BY fecha_inicio DESC";

        using var connection = _context.CreateConnection();
        var resultado = await connection.QueryAsync<FormacionAspirante>(sql, new { AspiranteId = aspiranteId });
        return resultado.ToList();
    }

    public async Task<List<ExperienciaAspirante>> ObtenerExperienciaAsync(int aspiranteId)
    {
        const string sql = @"
            SELECT * FROM experiencia_aspirante 
            WHERE aspirante_id = @AspiranteId AND activo = 1 
            ORDER BY fecha_inicio DESC";

        using var connection = _context.CreateConnection();
        var resultado = await connection.QueryAsync<ExperienciaAspirante>(sql, new { AspiranteId = aspiranteId });
        return resultado.ToList();
    }

    public async Task<List<ReferenciaAspirante>> ObtenerReferenciasAsync(int aspiranteId)
    {
        const string sql = @"
            SELECT * FROM referencias_aspirante 
            WHERE aspirante_id = @AspiranteId AND activo = 1 
            ORDER BY tipo, nombre_completo";

        using var connection = _context.CreateConnection();
        var resultado = await connection.QueryAsync<ReferenciaAspirante>(sql, new { AspiranteId = aspiranteId });
        return resultado.ToList();
    }

    public async Task<bool> GuardarFormacionAsync(int aspiranteId, List<FormacionAspirante> formaciones)
    {
        using var connection = (SqliteConnection)_context.CreateConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            // Desactivar formaciones anteriores
            await connection.ExecuteAsync(
                "UPDATE formacion_aspirante SET activo = 0 WHERE aspirante_id = @AspiranteId",
                new { AspiranteId = aspiranteId },
                transaction);

            // Insertar nuevas formaciones
            const string insertSql = @"
                INSERT INTO formacion_aspirante (
                    aspirante_id, nivel, titulo, institucion,
                    fecha_inicio, fecha_fin, en_curso, es_activo, activo, fecha_creacion
                ) VALUES (
                    @AspiranteId, @Nivel, @Titulo, @Institucion,
                    @FechaInicio, @FechaFin, @EnCurso, 1, 1, @FechaCreacion
                )";

            foreach (var formacion in formaciones)
            {
                formacion.AspiranteId = aspiranteId;
                formacion.FechaCreacion = DateTime.Now;
                await connection.ExecuteAsync(insertSql, formacion, transaction);
            }

            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.LogError(ex, "Error guardando formación del aspirante {Id}", aspiranteId);
            return false;
        }
    }

    public async Task<bool> GuardarExperienciaAsync(int aspiranteId, List<ExperienciaAspirante> experiencias)
    {
        using var connection = (SqliteConnection)_context.CreateConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            // Desactivar experiencias anteriores
            await connection.ExecuteAsync(
                "UPDATE experiencia_aspirante SET activo = 0 WHERE aspirante_id = @AspiranteId",
                new { AspiranteId = aspiranteId },
                transaction);

            // Insertar nuevas experiencias
            const string insertSql = @"
                INSERT INTO experiencia_aspirante (
                    aspirante_id, empresa, cargo, fecha_inicio, fecha_fin,
                    trabajo_actual, funciones, motivo_retiro, es_activo, activo, fecha_creacion
                ) VALUES (
                    @AspiranteId, @Empresa, @Cargo, @FechaInicio, @FechaFin,
                    @TrabajoActual, @Funciones, @MotivoRetiro, 1, 1, @FechaCreacion
                )";

            foreach (var experiencia in experiencias)
            {
                experiencia.AspiranteId = aspiranteId;
                experiencia.FechaCreacion = DateTime.Now;
                await connection.ExecuteAsync(insertSql, experiencia, transaction);
            }

            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.LogError(ex, "Error guardando experiencia del aspirante {Id}", aspiranteId);
            return false;
        }
    }

    public async Task<bool> GuardarReferenciasAsync(int aspiranteId, List<ReferenciaAspirante> referencias)
    {
        using var connection = (SqliteConnection)_context.CreateConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            // Desactivar referencias anteriores
            await connection.ExecuteAsync(
                "UPDATE referencias_aspirante SET activo = 0 WHERE aspirante_id = @AspiranteId",
                new { AspiranteId = aspiranteId },
                transaction);

            // Insertar nuevas referencias
            const string insertSql = @"
                INSERT INTO referencias_aspirante (
                    aspirante_id, tipo, nombre_completo, telefono,
                    relacion, empresa, cargo, es_activo, activo, fecha_creacion
                ) VALUES (
                    @AspiranteId, @Tipo, @NombreCompleto, @Telefono,
                    @Relacion, @Empresa, @Cargo, 1, 1, @FechaCreacion
                )";

            foreach (var referencia in referencias)
            {
                referencia.AspiranteId = aspiranteId;
                referencia.FechaCreacion = DateTime.Now;
                await connection.ExecuteAsync(insertSql, referencia, transaction);
            }

            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.LogError(ex, "Error guardando referencias del aspirante {Id}", aspiranteId);
            return false;
        }
    }
}
