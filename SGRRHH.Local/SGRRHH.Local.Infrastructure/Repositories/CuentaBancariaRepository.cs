using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Repositories;

public class CuentaBancariaRepository : ICuentaBancariaRepository
{
    private readonly Data.DapperContext _context;
    private readonly ILogger<CuentaBancariaRepository> _logger;

    public CuentaBancariaRepository(Data.DapperContext context, ILogger<CuentaBancariaRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CuentaBancaria> AddAsync(CuentaBancaria entity)
    {
        entity.FechaCreacion = DateTime.Now;
        entity.Activo = true;
        
        const string sql = @"INSERT INTO cuentas_bancarias (
            empleado_id, banco, tipo_cuenta, numero_cuenta, nombre_titular, documento_titular,
            es_cuenta_nomina, esta_activa, fecha_apertura, documento_certificacion_id, observaciones,
            activo, fecha_creacion)
        VALUES (
            @EmpleadoId, @Banco, @TipoCuenta, @NumeroCuenta, @NombreTitular, @DocumentoTitular,
            @EsCuentaNomina, @EstaActiva, @FechaApertura, @DocumentoCertificacionId, @Observaciones,
            @Activo, @FechaCreacion);
        SELECT last_insert_rowid();";

        using var connection = _context.CreateConnection();
        entity.Id = await connection.ExecuteScalarAsync<int>(sql, new
        {
            entity.EmpleadoId,
            entity.Banco,
            TipoCuenta = (int)entity.TipoCuenta,
            entity.NumeroCuenta,
            entity.NombreTitular,
            entity.DocumentoTitular,
            EsCuentaNomina = entity.EsCuentaNomina ? 1 : 0,
            EstaActiva = entity.EstaActiva ? 1 : 0,
            entity.FechaApertura,
            entity.DocumentoCertificacionId,
            entity.Observaciones,
            entity.Activo,
            entity.FechaCreacion
        });
        
        return entity;
    }

    public async Task UpdateAsync(CuentaBancaria entity)
    {
        entity.FechaModificacion = DateTime.Now;
        
        const string sql = @"UPDATE cuentas_bancarias
        SET empleado_id = @EmpleadoId,
            banco = @Banco,
            tipo_cuenta = @TipoCuenta,
            numero_cuenta = @NumeroCuenta,
            nombre_titular = @NombreTitular,
            documento_titular = @DocumentoTitular,
            es_cuenta_nomina = @EsCuentaNomina,
            esta_activa = @EstaActiva,
            fecha_apertura = @FechaApertura,
            documento_certificacion_id = @DocumentoCertificacionId,
            observaciones = @Observaciones,
            activo = @Activo,
            fecha_modificacion = @FechaModificacion
        WHERE id = @Id";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            entity.Id,
            entity.EmpleadoId,
            entity.Banco,
            TipoCuenta = (int)entity.TipoCuenta,
            entity.NumeroCuenta,
            entity.NombreTitular,
            entity.DocumentoTitular,
            EsCuentaNomina = entity.EsCuentaNomina ? 1 : 0,
            EstaActiva = entity.EstaActiva ? 1 : 0,
            entity.FechaApertura,
            entity.DocumentoCertificacionId,
            entity.Observaciones,
            entity.Activo,
            entity.FechaModificacion
        });
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "UPDATE cuentas_bancarias SET activo = 0, fecha_modificacion = @Fecha WHERE id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id, Fecha = DateTime.Now });
    }

    public async Task<CuentaBancaria?> GetByIdAsync(int id)
    {
        const string sql = @"SELECT * FROM cuentas_bancarias WHERE id = @Id AND activo = 1";
        using var connection = _context.CreateConnection();
        var result = await connection.QueryFirstOrDefaultAsync<CuentaBancariaDb>(sql, new { Id = id });
        return result?.ToEntity();
    }

    public async Task<IEnumerable<CuentaBancaria>> GetAllAsync()
    {
        using var conn = _context.CreateConnection();
        var dbCuentas = await conn.QueryAsync<CuentaBancariaDb>(
            "SELECT * FROM cuentas_bancarias WHERE activo = 1 ORDER BY fecha_creacion DESC");
        return dbCuentas.Select(db => db.ToEntity());
    }
    
    public async Task<IEnumerable<CuentaBancaria>> GetAllActiveAsync()
    {
        using var conn = _context.CreateConnection();
        var dbCuentas = await conn.QueryAsync<CuentaBancariaDb>(
            "SELECT * FROM cuentas_bancarias WHERE activo = 1 AND esta_activa = 1 ORDER BY fecha_creacion DESC");
        return dbCuentas.Select(db => db.ToEntity());
    }
    
    public Task<int> SaveChangesAsync()
    {
        // Este repositorio usa Dapper (no tiene contexto de cambios)
        // Los cambios se persisten inmediatamente en cada operación
        return Task.FromResult(0);
    }
    
    // Método específico para obtener cuentas de un empleado
    public async Task<IEnumerable<CuentaBancaria>> GetByEmpleadoIdAsync(int empleadoId)
    {
        const string sql = @"SELECT * FROM cuentas_bancarias 
            WHERE empleado_id = @EmpleadoId AND activo = 1 
            ORDER BY es_cuenta_nomina DESC, fecha_creacion DESC";
        
        using var connection = _context.CreateConnection();
        var results = await connection.QueryAsync<CuentaBancariaDb>(sql, new { EmpleadoId = empleadoId });
        return results.Select(r => r.ToEntity());
    }

    public async Task<CuentaBancaria?> GetCuentaNominaActivaAsync(int empleadoId)
    {
        const string sql = @"SELECT * FROM cuentas_bancarias 
            WHERE empleado_id = @EmpleadoId 
              AND es_cuenta_nomina = 1 
              AND esta_activa = 1 
              AND activo = 1
            ORDER BY fecha_creacion DESC
            LIMIT 1";
        
        using var connection = _context.CreateConnection();
        var result = await connection.QueryFirstOrDefaultAsync<CuentaBancariaDb>(sql, new { EmpleadoId = empleadoId });
        return result?.ToEntity();
    }

    public async Task<bool> SetCuentaNominaPrincipalAsync(int cuentaBancariaId)
    {
        using var connection = _context.CreateConnection();
        using var transaction = connection.BeginTransaction();
        
        try
        {
            // Obtener el empleado_id de la cuenta
            const string sqlGetEmpleado = "SELECT empleado_id FROM cuentas_bancarias WHERE id = @Id";
            var empleadoId = await connection.ExecuteScalarAsync<int>(sqlGetEmpleado, new { Id = cuentaBancariaId }, transaction);
            
            // Desmarcar todas las cuentas de nómina del empleado
            const string sqlUnmark = @"UPDATE cuentas_bancarias 
                SET es_cuenta_nomina = 0, fecha_modificacion = @Fecha 
                WHERE empleado_id = @EmpleadoId AND activo = 1";
            await connection.ExecuteAsync(sqlUnmark, new { EmpleadoId = empleadoId, Fecha = DateTime.Now }, transaction);
            
            // Marcar la cuenta seleccionada como nómina
            const string sqlMark = @"UPDATE cuentas_bancarias 
                SET es_cuenta_nomina = 1, fecha_modificacion = @Fecha 
                WHERE id = @Id AND activo = 1";
            await connection.ExecuteAsync(sqlMark, new { Id = cuentaBancariaId, Fecha = DateTime.Now }, transaction);
            
            transaction.Commit();
            return true;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.LogError(ex, "Error al establecer cuenta de nómina principal {CuentaId}", cuentaBancariaId);
            return false;
        }
    }

    public async Task<CuentaBancaria?> GetByIdWithDocumentoAsync(int id)
    {
        const string sql = @"
            SELECT 
                cb.*,
                d.id AS doc_id,
                d.nombre AS doc_nombre,
                d.tipo_documento AS doc_tipo,
                d.archivo_path AS doc_path
            FROM cuentas_bancarias cb
            LEFT JOIN documentos_empleado d ON cb.documento_certificacion_id = d.id AND d.activo = 1
            WHERE cb.id = @Id AND cb.activo = 1";
        
        using var connection = _context.CreateConnection();
        var result = await connection.QueryAsync<CuentaBancariaDb, DocumentoEmpleadoSimple, CuentaBancaria>(
            sql,
            (cuentaDb, docSimple) =>
            {
                var cuenta = cuentaDb.ToEntity();
                if (docSimple?.doc_id > 0)
                {
                    cuenta.DocumentoCertificacion = new DocumentoEmpleado
                    {
                        Id = docSimple.doc_id,
                        Nombre = docSimple.doc_nombre ?? "",
                        TipoDocumento = (TipoDocumentoEmpleado)docSimple.doc_tipo,
                        ArchivoPath = docSimple.doc_path ?? ""
                    };
                }
                return cuenta;
            },
            new { Id = id },
            splitOn: "doc_id"
        );
        
        return result.FirstOrDefault();
    }

    // Clase auxiliar para mapeo desde SQLite
    private class CuentaBancariaDb
    {
        public int id { get; set; }
        public int empleado_id { get; set; }
        public string banco { get; set; } = string.Empty;
        public int tipo_cuenta { get; set; }
        public string numero_cuenta { get; set; } = string.Empty;
        public string? nombre_titular { get; set; }
        public string? documento_titular { get; set; }
        public int es_cuenta_nomina { get; set; }
        public int esta_activa { get; set; }
        public DateTime? fecha_apertura { get; set; }
        public int? documento_certificacion_id { get; set; }
        public string? observaciones { get; set; }
        public int activo { get; set; }
        public DateTime fecha_creacion { get; set; }
        public DateTime? fecha_modificacion { get; set; }

        public CuentaBancaria ToEntity() => new()
        {
            Id = id,
            EmpleadoId = empleado_id,
            Banco = banco,
            TipoCuenta = (TipoCuentaBancaria)tipo_cuenta,
            NumeroCuenta = numero_cuenta,
            NombreTitular = nombre_titular,
            DocumentoTitular = documento_titular,
            EsCuentaNomina = es_cuenta_nomina == 1,
            EstaActiva = esta_activa == 1,
            FechaApertura = fecha_apertura,
            DocumentoCertificacionId = documento_certificacion_id,
            Observaciones = observaciones,
            Activo = activo == 1,
            FechaCreacion = fecha_creacion,
            FechaModificacion = fecha_modificacion
        };
    }

    private class DocumentoEmpleadoSimple
    {
        public int doc_id { get; set; }
        public string? doc_nombre { get; set; }
        public int doc_tipo { get; set; }
        public string? doc_path { get; set; }
    }
}
