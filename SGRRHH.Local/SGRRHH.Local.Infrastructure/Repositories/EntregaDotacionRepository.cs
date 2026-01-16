using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Repositories;

public class EntregaDotacionRepository : IEntregaDotacionRepository
{
    private readonly Data.DapperContext _context;
    private readonly ILogger<EntregaDotacionRepository> _logger;

    public EntregaDotacionRepository(Data.DapperContext context, ILogger<EntregaDotacionRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<EntregaDotacion> AddAsync(EntregaDotacion entity)
    {
        entity.FechaCreacion = DateTime.Now;
        entity.Activo = true;
        
        const string sql = @"INSERT INTO entregas_dotacion (
            empleado_id, fecha_entrega, periodo, tipo_entrega, numero_entrega_anual,
            estado, fecha_entrega_real, documento_acta_id, observaciones,
            entregado_por_usuario_id, entregado_por_nombre,
            activo, fecha_creacion)
        VALUES (
            @EmpleadoId, @FechaEntrega, @Periodo, @TipoEntrega, @NumeroEntregaAnual,
            @Estado, @FechaEntregaReal, @DocumentoActaId, @Observaciones,
            @EntregadoPorUsuarioId, @EntregadoPorNombre,
            @Activo, @FechaCreacion);
        SELECT last_insert_rowid();";

        using var connection = _context.CreateConnection();
        entity.Id = await connection.ExecuteScalarAsync<int>(sql, new
        {
            entity.EmpleadoId,
            FechaEntrega = entity.FechaEntrega.ToString("yyyy-MM-dd"),
            entity.Periodo,
            TipoEntrega = (int)entity.TipoEntrega,
            entity.NumeroEntregaAnual,
            Estado = (int)entity.Estado,
            FechaEntregaReal = entity.FechaEntregaReal?.ToString("yyyy-MM-dd"),
            entity.DocumentoActaId,
            entity.Observaciones,
            entity.EntregadoPorUsuarioId,
            entity.EntregadoPorNombre,
            entity.Activo,
            FechaCreacion = entity.FechaCreacion.ToString("yyyy-MM-dd HH:mm:ss")
        });
        
        return entity;
    }

    public async Task UpdateAsync(EntregaDotacion entity)
    {
        entity.FechaModificacion = DateTime.Now;
        
        const string sql = @"UPDATE entregas_dotacion
        SET empleado_id = @EmpleadoId,
            fecha_entrega = @FechaEntrega,
            periodo = @Periodo,
            tipo_entrega = @TipoEntrega,
            numero_entrega_anual = @NumeroEntregaAnual,
            estado = @Estado,
            fecha_entrega_real = @FechaEntregaReal,
            documento_acta_id = @DocumentoActaId,
            observaciones = @Observaciones,
            entregado_por_usuario_id = @EntregadoPorUsuarioId,
            entregado_por_nombre = @EntregadoPorNombre,
            activo = @Activo,
            fecha_modificacion = @FechaModificacion
        WHERE id = @Id";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            entity.Id,
            entity.EmpleadoId,
            FechaEntrega = entity.FechaEntrega.ToString("yyyy-MM-dd"),
            entity.Periodo,
            TipoEntrega = (int)entity.TipoEntrega,
            entity.NumeroEntregaAnual,
            Estado = (int)entity.Estado,
            FechaEntregaReal = entity.FechaEntregaReal?.ToString("yyyy-MM-dd"),
            entity.DocumentoActaId,
            entity.Observaciones,
            entity.EntregadoPorUsuarioId,
            entity.EntregadoPorNombre,
            entity.Activo,
            FechaModificacion = entity.FechaModificacion?.ToString("yyyy-MM-dd HH:mm:ss")
        });
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "UPDATE entregas_dotacion SET activo = 0, fecha_modificacion = @Fecha WHERE id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id, Fecha = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") });
    }

    public async Task<EntregaDotacion?> GetByIdAsync(int id)
    {
        const string sql = @"SELECT * FROM entregas_dotacion WHERE id = @Id AND activo = 1";
        using var connection = _context.CreateConnection();
        var result = await connection.QueryFirstOrDefaultAsync<EntregaDotacionDb>(sql, new { Id = id });
        return result?.ToEntity();
    }

    public async Task<IEnumerable<EntregaDotacion>> GetAllAsync()
    {
        using var conn = _context.CreateConnection();
        var dbEntregas = await conn.QueryAsync<EntregaDotacionDb>(
            "SELECT * FROM entregas_dotacion WHERE activo = 1 ORDER BY fecha_entrega DESC");
        return dbEntregas.Select(db => db.ToEntity());
    }

    public async Task<IEnumerable<EntregaDotacion>> GetAllActiveAsync()
    {
        return await GetAllAsync();
    }

    public Task<int> SaveChangesAsync()
    {
        return Task.FromResult(0);
    }

    public async Task<IEnumerable<EntregaDotacion>> GetByEmpleadoIdAsync(int empleadoId)
    {
        const string sql = @"SELECT * FROM entregas_dotacion 
            WHERE empleado_id = @EmpleadoId AND activo = 1 
            ORDER BY fecha_entrega DESC";
        
        using var connection = _context.CreateConnection();
        var results = await connection.QueryAsync<EntregaDotacionDb>(sql, new { EmpleadoId = empleadoId });
        return results.Select(r => r.ToEntity());
    }

    public async Task<IEnumerable<EntregaDotacion>> GetByEmpleadoIdWithDetallesAsync(int empleadoId)
    {
        const string sql = @"
            SELECT e.*, d.*
            FROM entregas_dotacion e
            LEFT JOIN detalle_entrega_dotacion d ON e.id = d.entrega_id AND d.activo = 1
            WHERE e.empleado_id = @EmpleadoId AND e.activo = 1
            ORDER BY e.fecha_entrega DESC";
        
        using var connection = _context.CreateConnection();
        
        var entregasDict = new Dictionary<int, EntregaDotacion>();
        
        await connection.QueryAsync<EntregaDotacionDb, DetalleEntregaDotacionDb, EntregaDotacion>(
            sql,
            (entregaDb, detalleDb) =>
            {
                if (!entregasDict.TryGetValue(entregaDb.id, out var entrega))
                {
                    entrega = entregaDb.ToEntity();
                    entregasDict.Add(entregaDb.id, entrega);
                }

                if (detalleDb != null && detalleDb.id > 0)
                {
                    entrega.Detalles.Add(detalleDb.ToEntity());
                }

                return entrega;
            },
            new { EmpleadoId = empleadoId },
            splitOn: "id"
        );

        return entregasDict.Values;
    }

    public async Task<EntregaDotacion?> GetByIdWithDetallesAsync(int id)
    {
        const string sql = @"
            SELECT e.*, d.*
            FROM entregas_dotacion e
            LEFT JOIN detalle_entrega_dotacion d ON e.id = d.entrega_id AND d.activo = 1
            WHERE e.id = @Id AND e.activo = 1";
        
        using var connection = _context.CreateConnection();
        
        EntregaDotacion? entrega = null;
        
        await connection.QueryAsync<EntregaDotacionDb, DetalleEntregaDotacionDb, EntregaDotacion>(
            sql,
            (entregaDb, detalleDb) =>
            {
                if (entrega == null)
                {
                    entrega = entregaDb.ToEntity();
                }

                if (detalleDb != null && detalleDb.id > 0)
                {
                    entrega.Detalles.Add(detalleDb.ToEntity());
                }

                return entrega;
            },
            new { Id = id },
            splitOn: "id"
        );

        return entrega;
    }

    public async Task<IEnumerable<EntregaDotacion>> GetProximasEntregasAsync(int diasAnticipacion = 30)
    {
        const string sql = @"SELECT * FROM entregas_dotacion 
            WHERE activo = 1 
              AND estado = 1
              AND fecha_entrega BETWEEN date('now') AND date('now', '+' || @Dias || ' days')
            ORDER BY fecha_entrega ASC";
        
        using var connection = _context.CreateConnection();
        var results = await connection.QueryAsync<EntregaDotacionDb>(sql, new { Dias = diasAnticipacion });
        return results.Select(r => r.ToEntity());
    }

    public async Task<IEnumerable<EntregaDotacion>> GetEntregasPendientesAsync()
    {
        const string sql = @"SELECT * FROM entregas_dotacion 
            WHERE activo = 1 
              AND estado = 1
            ORDER BY fecha_entrega ASC";
        
        using var connection = _context.CreateConnection();
        var results = await connection.QueryAsync<EntregaDotacionDb>(sql);
        return results.Select(r => r.ToEntity());
    }

    public async Task<bool> EmpleadoTieneEntregaProgramadaAsync(int empleadoId, string periodo)
    {
        const string sql = @"SELECT COUNT(*) FROM entregas_dotacion 
            WHERE empleado_id = @EmpleadoId 
              AND periodo = @Periodo 
              AND activo = 1";
        
        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { EmpleadoId = empleadoId, Periodo = periodo });
        return count > 0;
    }

    // Clase auxiliar para mapeo desde SQLite
    private class EntregaDotacionDb
    {
        public int id { get; set; }
        public int empleado_id { get; set; }
        public string fecha_entrega { get; set; } = string.Empty;
        public string periodo { get; set; } = string.Empty;
        public int tipo_entrega { get; set; }
        public int? numero_entrega_anual { get; set; }
        public int estado { get; set; }
        public string? fecha_entrega_real { get; set; }
        public int? documento_acta_id { get; set; }
        public string? observaciones { get; set; }
        public int? entregado_por_usuario_id { get; set; }
        public string? entregado_por_nombre { get; set; }
        public int activo { get; set; }
        public string fecha_creacion { get; set; } = string.Empty;
        public string? fecha_modificacion { get; set; }

        public EntregaDotacion ToEntity() => new()
        {
            Id = id,
            EmpleadoId = empleado_id,
            FechaEntrega = DateTime.Parse(fecha_entrega),
            Periodo = periodo,
            TipoEntrega = (TipoEntregaDotacion)tipo_entrega,
            NumeroEntregaAnual = numero_entrega_anual,
            Estado = (EstadoEntregaDotacion)estado,
            FechaEntregaReal = string.IsNullOrEmpty(fecha_entrega_real) ? null : DateTime.Parse(fecha_entrega_real),
            DocumentoActaId = documento_acta_id,
            Observaciones = observaciones,
            EntregadoPorUsuarioId = entregado_por_usuario_id,
            EntregadoPorNombre = entregado_por_nombre,
            Activo = activo == 1,
            FechaCreacion = DateTime.Parse(fecha_creacion),
            FechaModificacion = string.IsNullOrEmpty(fecha_modificacion) ? null : DateTime.Parse(fecha_modificacion)
        };
    }

    private class DetalleEntregaDotacionDb
    {
        public int id { get; set; }
        public int entrega_id { get; set; }
        public int categoria_elemento { get; set; }
        public string nombre_elemento { get; set; } = string.Empty;
        public int cantidad { get; set; }
        public string? talla { get; set; }
        public int es_dotacion_legal { get; set; }
        public int es_epp { get; set; }
        public string? marca { get; set; }
        public string? referencia { get; set; }
        public decimal? valor_unitario { get; set; }
        public string? observaciones { get; set; }
        public int activo { get; set; }
        public string fecha_creacion { get; set; } = string.Empty;

        public DetalleEntregaDotacion ToEntity() => new()
        {
            Id = id,
            EntregaId = entrega_id,
            CategoriaElemento = (CategoriaElementoDotacion)categoria_elemento,
            NombreElemento = nombre_elemento,
            Cantidad = cantidad,
            Talla = talla,
            EsDotacionLegal = es_dotacion_legal == 1,
            EsEPP = es_epp == 1,
            Marca = marca,
            Referencia = referencia,
            ValorUnitario = valor_unitario,
            Observaciones = observaciones,
            Activo = activo == 1,
            FechaCreacion = DateTime.Parse(fecha_creacion)
        };
    }
}
