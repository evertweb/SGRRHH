using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Domain.Interfaces;
using SGRRHH.Local.Infrastructure.Data;

namespace SGRRHH.Local.Infrastructure.Repositories;

/// <summary>
/// Repositorio para gestión de vacantes con Dapper
/// </summary>
public class VacanteRepositorio : IVacanteRepositorio
{
    private readonly DapperContext _context;
    private readonly ILogger<VacanteRepositorio> _logger;

    public VacanteRepositorio(DapperContext context, ILogger<VacanteRepositorio> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Vacante?> ObtenerPorIdAsync(int id)
    {
        const string sql = @"
            SELECT v.*, c.nombre as cargo_nombre, d.nombre as departamento_nombre
            FROM vacantes v
            LEFT JOIN cargos c ON v.cargo_id = c.id
            LEFT JOIN departamentos d ON v.departamento_id = d.id
            WHERE v.id = @Id";

        using var connection = _context.CreateConnection();
        var vacante = await connection.QuerySingleOrDefaultAsync<Vacante>(sql, new { Id = id });
        return vacante;
    }

    public async Task<List<Vacante>> ObtenerTodosAsync(bool incluirInactivos = false)
    {
        var sql = incluirInactivos
            ? "SELECT * FROM vacantes ORDER BY fecha_publicacion DESC"
            : "SELECT * FROM vacantes WHERE activo = 1 ORDER BY fecha_publicacion DESC";

        using var connection = _context.CreateConnection();
        var resultado = await connection.QueryAsync<Vacante>(sql);
        return resultado.ToList();
    }

    public async Task<List<Vacante>> ObtenerPorEstadoAsync(EstadoVacante estado)
    {
        const string sql = @"
            SELECT * FROM vacantes 
            WHERE estado = @Estado AND activo = 1 
            ORDER BY fecha_publicacion DESC";

        using var connection = _context.CreateConnection();
        var resultado = await connection.QueryAsync<Vacante>(sql, new { Estado = estado.ToString() });
        return resultado.ToList();
    }

    public async Task<List<Vacante>> ObtenerAbiertasAsync()
    {
        const string sql = @"
            SELECT v.*, c.nombre as cargo_nombre, d.nombre as departamento_nombre
            FROM vacantes v
            LEFT JOIN cargos c ON v.cargo_id = c.id
            LEFT JOIN departamentos d ON v.departamento_id = d.id
            WHERE v.estado IN ('Abierta', 'EnProceso') AND v.activo = 1 
            ORDER BY v.fecha_publicacion DESC";

        using var connection = _context.CreateConnection();
        var resultado = await connection.QueryAsync<Vacante>(sql);
        return resultado.ToList();
    }

    public async Task<int> CrearAsync(Vacante vacante)
    {
        vacante.FechaCreacion = DateTime.Now;
        vacante.Activo = true;

        const string sql = @"
            INSERT INTO vacantes (
                cargo_id, departamento_id, titulo, descripcion, requisitos,
                salario_minimo, salario_maximo, fecha_publicacion, fecha_cierre,
                estado, cantidad_posiciones, es_activo, activo, fecha_creacion
            ) VALUES (
                @CargoId, @DepartamentoId, @Titulo, @Descripcion, @Requisitos,
                @SalarioMinimo, @SalarioMaximo, @FechaPublicacion, @FechaCierre,
                @Estado, @CantidadPosiciones, @EsActivo, @Activo, @FechaCreacion
            );
            SELECT last_insert_rowid();";

        using var connection = _context.CreateConnection();
        var id = await connection.ExecuteScalarAsync<int>(sql, new
        {
            vacante.CargoId,
            vacante.DepartamentoId,
            vacante.Titulo,
            vacante.Descripcion,
            vacante.Requisitos,
            vacante.SalarioMinimo,
            vacante.SalarioMaximo,
            vacante.FechaPublicacion,
            vacante.FechaCierre,
            Estado = vacante.Estado.ToString(),
            vacante.CantidadPosiciones,
            vacante.EsActivo,
            vacante.Activo,
            vacante.FechaCreacion
        });

        _logger.LogInformation("Vacante {Id} creada: {Titulo}", id, vacante.Titulo);
        return id;
    }

    public async Task<bool> ActualizarAsync(Vacante vacante)
    {
        vacante.FechaModificacion = DateTime.Now;

        const string sql = @"
            UPDATE vacantes SET
                cargo_id = @CargoId,
                departamento_id = @DepartamentoId,
                titulo = @Titulo,
                descripcion = @Descripcion,
                requisitos = @Requisitos,
                salario_minimo = @SalarioMinimo,
                salario_maximo = @SalarioMaximo,
                fecha_publicacion = @FechaPublicacion,
                fecha_cierre = @FechaCierre,
                estado = @Estado,
                cantidad_posiciones = @CantidadPosiciones,
                es_activo = @EsActivo,
                fecha_modificacion = @FechaModificacion
            WHERE id = @Id";

        using var connection = _context.CreateConnection();
        var filas = await connection.ExecuteAsync(sql, new
        {
            vacante.CargoId,
            vacante.DepartamentoId,
            vacante.Titulo,
            vacante.Descripcion,
            vacante.Requisitos,
            vacante.SalarioMinimo,
            vacante.SalarioMaximo,
            vacante.FechaPublicacion,
            vacante.FechaCierre,
            Estado = vacante.Estado.ToString(),
            vacante.CantidadPosiciones,
            vacante.EsActivo,
            vacante.FechaModificacion,
            vacante.Id
        });

        if (filas == 0)
        {
            _logger.LogWarning("Vacante {Id} no encontrada para actualizar", vacante.Id);
            return false;
        }

        _logger.LogInformation("Vacante {Id} actualizada", vacante.Id);
        return true;
    }

    public async Task<bool> CambiarEstadoAsync(int id, EstadoVacante nuevoEstado)
    {
        const string sql = @"
            UPDATE vacantes SET 
                estado = @Estado,
                fecha_modificacion = @FechaModificacion
            WHERE id = @Id";

        using var connection = _context.CreateConnection();
        var filas = await connection.ExecuteAsync(sql, new
        {
            Estado = nuevoEstado.ToString(),
            FechaModificacion = DateTime.Now,
            Id = id
        });

        if (filas == 0)
        {
            _logger.LogWarning("No se pudo cambiar estado de vacante {Id}", id);
            return false;
        }

        _logger.LogInformation("Vacante {Id} cambió a estado {Estado}", id, nuevoEstado);
        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        const string sql = @"
            UPDATE vacantes SET 
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
            _logger.LogWarning("No se pudo eliminar vacante {Id}", id);
            return false;
        }

        _logger.LogInformation("Vacante {Id} eliminada (soft delete)", id);
        return true;
    }

    public async Task<int> ContarAspirantesAsync(int vacanteId)
    {
        const string sql = "SELECT COUNT(1) FROM aspirantes WHERE vacante_id = @VacanteId AND activo = 1";

        using var connection = _context.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, new { VacanteId = vacanteId });
    }
}
