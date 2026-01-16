using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Repositories;

public class DetalleEntregaDotacionRepository : IDetalleEntregaDotacionRepository
{
    private readonly Data.DapperContext _context;
    private readonly ILogger<DetalleEntregaDotacionRepository> _logger;

    public DetalleEntregaDotacionRepository(Data.DapperContext context, ILogger<DetalleEntregaDotacionRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DetalleEntregaDotacion> AddAsync(DetalleEntregaDotacion entity)
    {
        entity.FechaCreacion = DateTime.Now;
        entity.Activo = true;
        
        const string sql = @"INSERT INTO detalle_entrega_dotacion (
            entrega_id, categoria_elemento, nombre_elemento, cantidad, talla,
            es_dotacion_legal, es_epp, marca, referencia, valor_unitario, observaciones,
            activo, fecha_creacion)
        VALUES (
            @EntregaId, @CategoriaElemento, @NombreElemento, @Cantidad, @Talla,
            @EsDotacionLegal, @EsEPP, @Marca, @Referencia, @ValorUnitario, @Observaciones,
            @Activo, @FechaCreacion);
        SELECT last_insert_rowid();";

        using var connection = _context.CreateConnection();
        entity.Id = await connection.ExecuteScalarAsync<int>(sql, new
        {
            entity.EntregaId,
            CategoriaElemento = (int)entity.CategoriaElemento,
            entity.NombreElemento,
            entity.Cantidad,
            entity.Talla,
            EsDotacionLegal = entity.EsDotacionLegal ? 1 : 0,
            EsEPP = entity.EsEPP ? 1 : 0,
            entity.Marca,
            entity.Referencia,
            entity.ValorUnitario,
            entity.Observaciones,
            entity.Activo,
            FechaCreacion = entity.FechaCreacion.ToString("yyyy-MM-dd HH:mm:ss")
        });
        
        return entity;
    }

    public async Task UpdateAsync(DetalleEntregaDotacion entity)
    {
        const string sql = @"UPDATE detalle_entrega_dotacion
        SET entrega_id = @EntregaId,
            categoria_elemento = @CategoriaElemento,
            nombre_elemento = @NombreElemento,
            cantidad = @Cantidad,
            talla = @Talla,
            es_dotacion_legal = @EsDotacionLegal,
            es_epp = @EsEPP,
            marca = @Marca,
            referencia = @Referencia,
            valor_unitario = @ValorUnitario,
            observaciones = @Observaciones,
            activo = @Activo
        WHERE id = @Id";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            entity.Id,
            entity.EntregaId,
            CategoriaElemento = (int)entity.CategoriaElemento,
            entity.NombreElemento,
            entity.Cantidad,
            entity.Talla,
            EsDotacionLegal = entity.EsDotacionLegal ? 1 : 0,
            EsEPP = entity.EsEPP ? 1 : 0,
            entity.Marca,
            entity.Referencia,
            entity.ValorUnitario,
            entity.Observaciones,
            entity.Activo
        });
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "UPDATE detalle_entrega_dotacion SET activo = 0 WHERE id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<DetalleEntregaDotacion?> GetByIdAsync(int id)
    {
        const string sql = @"SELECT * FROM detalle_entrega_dotacion WHERE id = @Id AND activo = 1";
        using var connection = _context.CreateConnection();
        var result = await connection.QueryFirstOrDefaultAsync<DetalleEntregaDotacionDb>(sql, new { Id = id });
        return result?.ToEntity();
    }

    public async Task<IEnumerable<DetalleEntregaDotacion>> GetAllAsync()
    {
        using var conn = _context.CreateConnection();
        var dbDetalles = await conn.QueryAsync<DetalleEntregaDotacionDb>(
            "SELECT * FROM detalle_entrega_dotacion WHERE activo = 1 ORDER BY fecha_creacion DESC");
        return dbDetalles.Select(db => db.ToEntity());
    }

    public async Task<IEnumerable<DetalleEntregaDotacion>> GetAllActiveAsync()
    {
        return await GetAllAsync();
    }

    public Task<int> SaveChangesAsync()
    {
        return Task.FromResult(0);
    }

    public async Task<IEnumerable<DetalleEntregaDotacion>> GetByEntregaIdAsync(int entregaId)
    {
        const string sql = @"SELECT * FROM detalle_entrega_dotacion 
            WHERE entrega_id = @EntregaId AND activo = 1 
            ORDER BY fecha_creacion ASC";
        
        using var connection = _context.CreateConnection();
        var results = await connection.QueryAsync<DetalleEntregaDotacionDb>(sql, new { EntregaId = entregaId });
        return results.Select(r => r.ToEntity());
    }

    public async Task DeleteByEntregaIdAsync(int entregaId)
    {
        const string sql = "UPDATE detalle_entrega_dotacion SET activo = 0 WHERE entrega_id = @EntregaId";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { EntregaId = entregaId });
    }

    // Clase auxiliar para mapeo desde SQLite
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
