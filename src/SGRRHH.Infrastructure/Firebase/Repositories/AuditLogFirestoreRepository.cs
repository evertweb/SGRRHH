using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Infrastructure.Firebase.Repositories;

/// <summary>
/// Implementación del repositorio de Logs de Auditoría para Firestore.
/// Colección: "audit-logs"
/// 
/// Campos desnormalizados:
/// - usuarioNombre (nombre del usuario al momento de la acción)
/// 
/// Nota: Esta colección no usa soft delete, los registros se eliminan permanentemente
/// cuando sea necesario (cleanup de logs antiguos).
/// </summary>
public class AuditLogFirestoreRepository : FirestoreRepository<AuditLog>, IAuditLogRepository
{
    private const string COLLECTION_NAME = "audit-logs";
    
    public AuditLogFirestoreRepository(FirebaseInitializer firebase, ILogger<AuditLogFirestoreRepository>? logger = null)
        : base(firebase, COLLECTION_NAME, logger)
    {
    }
    
    #region Entity <-> Document Mapping
    
    protected override Dictionary<string, object?> EntityToDocument(AuditLog entity)
    {
        // No llamar a base porque AuditLog tiene estructura diferente
        return new Dictionary<string, object?>
        {
            ["id"] = entity.Id,
            ["activo"] = entity.Activo,
            ["fechaCreacion"] = Timestamp.FromDateTime(entity.FechaCreacion.ToUniversalTime()),
            ["fechaModificacion"] = entity.FechaModificacion.HasValue 
                ? Timestamp.FromDateTime(entity.FechaModificacion.Value.ToUniversalTime()) 
                : null,
            
            // Fecha y hora de la acción
            ["fechaHora"] = Timestamp.FromDateTime(entity.FechaHora.ToUniversalTime()),
            
            // Usuario - con datos desnormalizados
            ["usuarioId"] = entity.UsuarioId,
            ["usuarioNombre"] = entity.UsuarioNombre,
            ["usuarioFirebaseUid"] = entity.Usuario?.FirebaseUid,
            
            // Acción
            ["accion"] = entity.Accion,
            
            // Entidad afectada
            ["entidad"] = entity.Entidad,
            ["entidadId"] = entity.EntidadId,
            
            // Descripción
            ["descripcion"] = entity.Descripcion,
            
            // Metadatos
            ["direccionIp"] = entity.DireccionIp,
            ["datosAdicionales"] = entity.DatosAdicionales
        };
    }
    
    protected override AuditLog DocumentToEntity(DocumentSnapshot document)
    {
        var entity = new AuditLog();
        
        // Campos base
        if (document.TryGetValue<int>("id", out var id))
            entity.Id = id;
        
        if (document.TryGetValue<bool>("activo", out var activo))
            entity.Activo = activo;
        
        if (document.TryGetValue<Timestamp>("fechaCreacion", out var fc))
            entity.FechaCreacion = fc.ToDateTime().ToLocalTime();
        
        if (document.TryGetValue<Timestamp?>("fechaModificacion", out var fm) && fm.HasValue)
            entity.FechaModificacion = fm.Value.ToDateTime().ToLocalTime();
        
        // Fecha y hora de la acción
        if (document.TryGetValue<Timestamp>("fechaHora", out var fechaHora))
            entity.FechaHora = fechaHora.ToDateTime().ToLocalTime();
        
        // Usuario
        if (document.TryGetValue<int?>("usuarioId", out var usuarioId))
            entity.UsuarioId = usuarioId;
        
        if (document.TryGetValue<string>("usuarioNombre", out var usuarioNombre))
            entity.UsuarioNombre = usuarioNombre ?? string.Empty;
        
        // Crear objeto Usuario para compatibilidad
        if (entity.UsuarioId.HasValue)
        {
            entity.Usuario = new Usuario
            {
                Id = entity.UsuarioId.Value,
                NombreCompleto = entity.UsuarioNombre
            };
            
            if (document.TryGetValue<string>("usuarioFirebaseUid", out var uid))
                entity.Usuario.FirebaseUid = uid;
        }
        
        // Acción
        if (document.TryGetValue<string>("accion", out var accion))
            entity.Accion = accion ?? string.Empty;
        
        // Entidad afectada
        if (document.TryGetValue<string>("entidad", out var entidad))
            entity.Entidad = entidad ?? string.Empty;
        
        if (document.TryGetValue<int?>("entidadId", out var entidadId))
            entity.EntidadId = entidadId;
        
        // Descripción
        if (document.TryGetValue<string>("descripcion", out var descripcion))
            entity.Descripcion = descripcion ?? string.Empty;
        
        // Metadatos
        if (document.TryGetValue<string>("direccionIp", out var ip))
            entity.DireccionIp = ip;
        
        if (document.TryGetValue<string>("datosAdicionales", out var datos))
            entity.DatosAdicionales = datos;
        
        // Guardar Document ID
        entity.SetFirestoreDocumentId(document.Id);
        
        return entity;
    }
    
    #endregion
    
    #region IAuditLogRepository Implementation
    
    /// <summary>
    /// Obtiene registros de auditoría filtrados
    /// </summary>
    public async Task<List<AuditLog>> GetFilteredAsync(
        DateTime? fechaDesde, 
        DateTime? fechaHasta, 
        string? entidad, 
        int? usuarioId, 
        int maxRegistros)
    {
        try
        {
            Query query = Collection;
            
            // Filtrar por usuario (puede combinar con orderBy en fechaHora)
            if (usuarioId.HasValue)
            {
                query = query.WhereEqualTo("usuarioId", usuarioId.Value);
            }
            
            // Filtrar por entidad
            if (!string.IsNullOrWhiteSpace(entidad))
            {
                query = query.WhereEqualTo("entidad", entidad);
            }
            
            // Obtener resultados
            var snapshot = await query.GetSnapshotAsync();
            var logs = snapshot.Documents.Select(DocumentToEntity).ToList();
            
            // Filtrar por fechas en memoria (Firestore tiene limitaciones con múltiples range)
            if (fechaDesde.HasValue)
                logs = logs.Where(l => l.FechaHora >= fechaDesde.Value).ToList();
            
            if (fechaHasta.HasValue)
                logs = logs.Where(l => l.FechaHora <= fechaHasta.Value).ToList();
            
            // Ordenar y limitar
            return logs
                .OrderByDescending(l => l.FechaHora)
                .Take(maxRegistros)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener logs de auditoría filtrados");
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene registros de auditoría de una entidad específica
    /// </summary>
    public async Task<List<AuditLog>> GetByEntidadAsync(string entidad, int entidadId)
    {
        try
        {
            var query = Collection
                .WhereEqualTo("entidad", entidad)
                .WhereEqualTo("entidadId", entidadId);
            
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents
                .Select(DocumentToEntity)
                .OrderByDescending(l => l.FechaHora)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener logs de auditoría para {Entidad} {EntidadId}", entidad, entidadId);
            throw;
        }
    }
    
    /// <summary>
    /// Elimina registros anteriores a una fecha (hard delete para limpieza)
    /// </summary>
    public async Task<int> DeleteOlderThanAsync(DateTime fecha)
    {
        try
        {
            var fechaUtc = fecha.ToUniversalTime();
            
            var query = Collection.WhereLessThan("fechaHora", Timestamp.FromDateTime(fechaUtc));
            var snapshot = await query.GetSnapshotAsync();
            
            if (!snapshot.Documents.Any())
                return 0;
            
            // Eliminar en batches de 500 (límite de Firestore)
            int deleted = 0;
            var documents = snapshot.Documents.ToList();
            
            for (int i = 0; i < documents.Count; i += 500)
            {
                var batch = CreateBatch();
                var batchDocs = documents.Skip(i).Take(500);
                
                foreach (var doc in batchDocs)
                {
                    batch.Delete(doc.Reference);
                    deleted++;
                }
                
                await batch.CommitAsync();
            }
            
            _logger?.LogInformation("Eliminados {Count} logs de auditoría anteriores a {Fecha}", deleted, fecha);
            return deleted;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al eliminar logs de auditoría antiguos");
            throw;
        }
    }
    
    #endregion
    
    #region Additional Query Methods
    
    /// <summary>
    /// Obtiene los últimos logs de auditoría
    /// </summary>
    public async Task<List<AuditLog>> GetLatestAsync(int cantidad = 100)
    {
        try
        {
            // Nota: Firestore requiere un índice para orderByDescending en campos timestamp
            var snapshot = await Collection.GetSnapshotAsync();
            
            return snapshot.Documents
                .Select(DocumentToEntity)
                .OrderByDescending(l => l.FechaHora)
                .Take(cantidad)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener últimos logs de auditoría");
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene logs por tipo de acción
    /// </summary>
    public async Task<List<AuditLog>> GetByAccionAsync(string accion, int maxRegistros = 100)
    {
        try
        {
            var query = Collection.WhereEqualTo("accion", accion);
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents
                .Select(DocumentToEntity)
                .OrderByDescending(l => l.FechaHora)
                .Take(maxRegistros)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener logs por acción {Accion}", accion);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene logs de un usuario por su Firebase UID
    /// </summary>
    public async Task<List<AuditLog>> GetByUsuarioFirebaseUidAsync(string firebaseUid, int maxRegistros = 100)
    {
        try
        {
            var query = Collection.WhereEqualTo("usuarioFirebaseUid", firebaseUid);
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents
                .Select(DocumentToEntity)
                .OrderByDescending(l => l.FechaHora)
                .Take(maxRegistros)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener logs por Firebase UID {Uid}", firebaseUid);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene estadísticas de logs por día
    /// </summary>
    public async Task<Dictionary<DateTime, int>> GetStatsByDateRangeAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        try
        {
            var query = Collection
                .WhereGreaterThanOrEqualTo("fechaHora", Timestamp.FromDateTime(fechaInicio.ToUniversalTime()))
                .WhereLessThanOrEqualTo("fechaHora", Timestamp.FromDateTime(fechaFin.ToUniversalTime()));
            
            var snapshot = await query.GetSnapshotAsync();
            
            var stats = new Dictionary<DateTime, int>();
            
            foreach (var doc in snapshot.Documents)
            {
                if (doc.TryGetValue<Timestamp>("fechaHora", out var ts))
                {
                    var date = ts.ToDateTime().ToLocalTime().Date;
                    if (stats.ContainsKey(date))
                        stats[date]++;
                    else
                        stats[date] = 1;
                }
            }
            
            return stats;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener estadísticas de logs");
            throw;
        }
    }
    
    /// <summary>
    /// Cuenta el total de logs en un rango de fechas
    /// </summary>
    public async Task<int> CountByDateRangeAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        try
        {
            var query = Collection
                .WhereGreaterThanOrEqualTo("fechaHora", Timestamp.FromDateTime(fechaInicio.ToUniversalTime()))
                .WhereLessThanOrEqualTo("fechaHora", Timestamp.FromDateTime(fechaFin.ToUniversalTime()));
            
            var snapshot = await query.GetSnapshotAsync();
            return snapshot.Count;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al contar logs en rango de fechas");
            throw;
        }
    }
    
    #endregion
    
    #region Override Add to auto-set FechaHora
    
    /// <summary>
    /// Agrega un nuevo log de auditoría
    /// </summary>
    public override async Task<AuditLog> AddAsync(AuditLog entity)
    {
        // Asegurar que FechaHora esté establecida
        if (entity.FechaHora == default)
            entity.FechaHora = DateTime.Now;
        
        return await base.AddAsync(entity);
    }
    
    #endregion
}
