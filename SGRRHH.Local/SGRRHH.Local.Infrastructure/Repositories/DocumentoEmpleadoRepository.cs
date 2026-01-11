using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Repositories;

public class DocumentoEmpleadoRepository : IDocumentoEmpleadoRepository
{
    private readonly Data.DapperContext _context;
    private readonly ILogger<DocumentoEmpleadoRepository> _logger;

    public DocumentoEmpleadoRepository(Data.DapperContext context, ILogger<DocumentoEmpleadoRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DocumentoEmpleado> AddAsync(DocumentoEmpleado entity)
    {
        entity.FechaCreacion = DateTime.Now;
        const string sql = @"INSERT INTO documentos_empleado (
    empleado_id, tipo_documento, nombre, descripcion, archivo_path, nombre_archivo_original, tamano_archivo, tipo_mime,
    fecha_vencimiento, fecha_emision, subido_por_usuario_id, subido_por_nombre, activo, fecha_creacion)
VALUES (@EmpleadoId, @TipoDocumento, @Nombre, @Descripcion, @ArchivoPath, @NombreArchivoOriginal, @TamanoArchivo, @TipoMime,
        @FechaVencimiento, @FechaEmision, @SubidoPorUsuarioId, @SubidoPorNombre, 1, @FechaCreacion);
SELECT last_insert_rowid();";

        using var connection = _context.CreateConnection();
        entity.Id = await connection.ExecuteScalarAsync<int>(sql, entity);
        return entity;
    }

    public async Task UpdateAsync(DocumentoEmpleado entity)
    {
        entity.FechaModificacion = DateTime.Now;
        const string sql = @"UPDATE documentos_empleado
SET empleado_id = @EmpleadoId,
    tipo_documento = @TipoDocumento,
    nombre = @Nombre,
    descripcion = @Descripcion,
    archivo_path = @ArchivoPath,
    nombre_archivo_original = @NombreArchivoOriginal,
    tamano_archivo = @TamanoArchivo,
    tipo_mime = @TipoMime,
    fecha_vencimiento = @FechaVencimiento,
    fecha_emision = @FechaEmision,
    subido_por_usuario_id = @SubidoPorUsuarioId,
    subido_por_nombre = @SubidoPorNombre,
    fecha_modificacion = @FechaModificacion
WHERE id = @Id";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(int id)
    {
        // Hard delete - elimina permanentemente el registro
        const string sql = "DELETE FROM documentos_empleado WHERE id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<DocumentoEmpleado?> GetByIdAsync(int id)
    {
        const string sql = "SELECT * FROM documentos_empleado WHERE id = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<DocumentoEmpleado>(sql, new { Id = id });
    }

    public async Task<IEnumerable<DocumentoEmpleado>> GetAllAsync()
    {
        const string sql = "SELECT * FROM documentos_empleado";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<DocumentoEmpleado>(sql);
    }

    public async Task<IEnumerable<DocumentoEmpleado>> GetAllActiveAsync()
    {
        const string sql = "SELECT * FROM documentos_empleado WHERE activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<DocumentoEmpleado>(sql);
    }

    public async Task<IEnumerable<DocumentoEmpleado>> GetByEmpleadoIdAsync(int empleadoId)
    {
        const string sql = "SELECT * FROM documentos_empleado WHERE empleado_id = @EmpleadoId AND activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<DocumentoEmpleado>(sql, new { EmpleadoId = empleadoId });
    }

    public async Task<IEnumerable<DocumentoEmpleado>> GetByEmpleadoIdAndTipoAsync(int empleadoId, TipoDocumentoEmpleado tipo)
    {
        const string sql = "SELECT * FROM documentos_empleado WHERE empleado_id = @EmpleadoId AND tipo_documento = @Tipo AND activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<DocumentoEmpleado>(sql, new { EmpleadoId = empleadoId, Tipo = tipo });
    }

    public async Task<IEnumerable<DocumentoEmpleado>> GetDocumentosProximosAVencerAsync(int diasAnticipacion)
    {
        const string sql = @"SELECT * FROM documentos_empleado
WHERE fecha_vencimiento IS NOT NULL
  AND fecha_vencimiento <= date('now', '+' || @Dias || ' day')
  AND fecha_vencimiento >= date('now')
  AND activo = 1";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<DocumentoEmpleado>(sql, new { Dias = diasAnticipacion });
    }

    public async Task<bool> EmpleadoTieneDocumentoTipoAsync(int empleadoId, TipoDocumentoEmpleado tipo)
    {
        const string sql = "SELECT COUNT(1) FROM documentos_empleado WHERE empleado_id = @EmpleadoId AND tipo_documento = @Tipo AND activo = 1";
        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { EmpleadoId = empleadoId, Tipo = tipo });
        return count > 0;
    }

    public async Task<int> GetConteoDocumentosByEmpleadoAsync(int empleadoId)
    {
        const string sql = "SELECT COUNT(1) FROM documentos_empleado WHERE empleado_id = @EmpleadoId AND activo = 1";
        using var connection = _context.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, new { EmpleadoId = empleadoId });
    }

    public Task<int> SaveChangesAsync() => Task.FromResult(0);
}
