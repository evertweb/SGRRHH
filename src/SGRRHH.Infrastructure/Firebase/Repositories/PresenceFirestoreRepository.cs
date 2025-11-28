using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Infrastructure.Firebase.Repositories;

/// <summary>
/// Implementación del repositorio de Presencia de usuarios para Firestore.
/// Colección: "user_presence"
/// 
/// Gestiona el estado online/offline de los usuarios en tiempo real.
/// </summary>
public class PresenceFirestoreRepository : FirestoreRepository<UserPresence>, IPresenceRepository
{
    private const string COLLECTION_NAME = "user_presence";
    
    public PresenceFirestoreRepository(FirebaseInitializer firebase, ILogger<PresenceFirestoreRepository>? logger = null)
        : base(firebase, COLLECTION_NAME, logger)
    {
    }
    
    #region Entity <-> Document Mapping
    
    protected override Dictionary<string, object?> EntityToDocument(UserPresence entity)
    {
        var doc = base.EntityToDocument(entity);
        
        doc["userId"] = entity.UserId;
        doc["firebaseUid"] = entity.FirebaseUid;
        doc["username"] = entity.Username;
        doc["nombreCompleto"] = entity.NombreCompleto;
        doc["isOnline"] = entity.IsOnline;
        doc["lastSeen"] = Timestamp.FromDateTime(entity.LastSeen.ToUniversalTime());
        doc["lastHeartbeat"] = Timestamp.FromDateTime(entity.LastHeartbeat.ToUniversalTime());
        doc["deviceName"] = entity.DeviceName;
        doc["status"] = entity.Status;
        doc["statusMessage"] = entity.StatusMessage;
        
        return doc;
    }
    
    protected override UserPresence DocumentToEntity(DocumentSnapshot document)
    {
        var entity = base.DocumentToEntity(document);
        
        if (document.TryGetValue<int>("userId", out var userId))
            entity.UserId = userId;
        
        if (document.TryGetValue<string>("firebaseUid", out var firebaseUid))
            entity.FirebaseUid = firebaseUid;
        
        if (document.TryGetValue<string>("username", out var username))
            entity.Username = username ?? string.Empty;
        
        if (document.TryGetValue<string>("nombreCompleto", out var nombreCompleto))
            entity.NombreCompleto = nombreCompleto ?? string.Empty;
        
        if (document.TryGetValue<bool>("isOnline", out var isOnline))
            entity.IsOnline = isOnline;
        
        if (document.TryGetValue<Timestamp>("lastSeen", out var lastSeen))
            entity.LastSeen = lastSeen.ToDateTime().ToLocalTime();
        
        if (document.TryGetValue<Timestamp>("lastHeartbeat", out var lastHeartbeat))
            entity.LastHeartbeat = lastHeartbeat.ToDateTime().ToLocalTime();
        
        if (document.TryGetValue<string>("deviceName", out var deviceName))
            entity.DeviceName = deviceName;
        
        if (document.TryGetValue<string>("status", out var status))
            entity.Status = status ?? "Disponible";
        
        if (document.TryGetValue<string>("statusMessage", out var statusMessage))
            entity.StatusMessage = statusMessage;
        
        return entity;
    }
    
    #endregion
    
    #region IPresenceRepository Implementation
    
    /// <summary>
    /// Obtiene la presencia de un usuario por su UserId
    /// </summary>
    public async Task<UserPresence?> GetByUserIdAsync(int userId)
    {
        try
        {
            var query = Collection.WhereEqualTo("userId", userId).Limit(1);
            var snapshot = await query.GetSnapshotAsync();
            
            var doc = snapshot.Documents.FirstOrDefault();
            return doc == null ? null : DocumentToEntity(doc);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener presencia del usuario: {UserId}", userId);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene la presencia de un usuario por su FirebaseUid
    /// </summary>
    public async Task<UserPresence?> GetByFirebaseUidAsync(string firebaseUid)
    {
        try
        {
            // El FirebaseUid es el Document ID
            var docRef = Collection.Document(firebaseUid);
            var snapshot = await docRef.GetSnapshotAsync();
            
            if (!snapshot.Exists)
                return null;
            
            return DocumentToEntity(snapshot);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener presencia por FirebaseUid: {FirebaseUid}", firebaseUid);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene todos los usuarios que están actualmente online
    /// </summary>
    public async Task<IEnumerable<UserPresence>> GetOnlineUsersAsync()
    {
        try
        {
            var query = Collection.WhereEqualTo("isOnline", true);
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents
                .Select(DocumentToEntity)
                .OrderBy(p => p.NombreCompleto)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener usuarios online");
            throw;
        }
    }
    
    /// <summary>
    /// Actualiza el estado online de un usuario
    /// </summary>
    public async Task UpdateOnlineStatusAsync(int userId, bool isOnline)
    {
        try
        {
            var existing = await GetByUserIdAsync(userId);
            if (existing == null)
            {
                _logger?.LogWarning("No se encontró presencia para el usuario {UserId}", userId);
                return;
            }
            
            var documentId = existing.GetFirestoreDocumentId();
            if (string.IsNullOrEmpty(documentId))
            {
                _logger?.LogWarning("No se encontró DocumentId para presencia del usuario {UserId}", userId);
                return;
            }
            
            var updates = new Dictionary<string, object>
            {
                ["isOnline"] = isOnline,
                ["lastSeen"] = Timestamp.FromDateTime(DateTime.UtcNow),
                ["fechaModificacion"] = Timestamp.FromDateTime(DateTime.UtcNow)
            };
            
            if (isOnline)
            {
                updates["lastHeartbeat"] = Timestamp.FromDateTime(DateTime.UtcNow);
            }
            
            var docRef = Collection.Document(documentId);
            await docRef.UpdateAsync(updates);
            
            _logger?.LogInformation("Actualizado estado online para usuario {UserId}: {IsOnline}", userId, isOnline);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al actualizar estado online del usuario: {UserId}", userId);
            throw;
        }
    }
    
    /// <summary>
    /// Actualiza el heartbeat del usuario (para detectar desconexiones)
    /// Usa FirebaseUid como identificador principal
    /// </summary>
    public async Task UpdateHeartbeatAsync(int userId, string? firebaseUid = null)
    {
        try
        {
            string? documentId = null;
            
            // Si tenemos FirebaseUid, usarlo directamente (más eficiente)
            if (!string.IsNullOrEmpty(firebaseUid))
            {
                documentId = firebaseUid;
            }
            else
            {
                // Fallback: buscar por UserId
                var existing = await GetByUserIdAsync(userId);
                if (existing == null)
                {
                    _logger?.LogWarning("No se encontró presencia para heartbeat del usuario {UserId}", userId);
                    return;
                }
                
                documentId = existing.GetFirestoreDocumentId();
            }
            
            if (string.IsNullOrEmpty(documentId))
                return;
            
            var docRef = Collection.Document(documentId);
            await docRef.UpdateAsync(new Dictionary<string, object>
            {
                ["lastHeartbeat"] = Timestamp.FromDateTime(DateTime.UtcNow),
                ["lastSeen"] = Timestamp.FromDateTime(DateTime.UtcNow)
            });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al actualizar heartbeat del usuario: {UserId}", userId);
            // No lanzar excepción - el heartbeat no es crítico
        }
    }
    
    /// <summary>
    /// Marca como offline a usuarios con heartbeat antiguo
    /// </summary>
    public async Task MarkInactiveUsersOfflineAsync(TimeSpan timeout)
    {
        try
        {
            var cutoffTime = DateTime.UtcNow.Subtract(timeout);
            var cutoffTimestamp = Timestamp.FromDateTime(cutoffTime);
            
            // Buscar usuarios online con heartbeat antiguo
            var query = Collection
                .WhereEqualTo("isOnline", true)
                .WhereLessThan("lastHeartbeat", cutoffTimestamp);
            
            var snapshot = await query.GetSnapshotAsync();
            
            var batch = Firestore.StartBatch();
            var count = 0;
            
            foreach (var doc in snapshot.Documents)
            {
                batch.Update(doc.Reference, new Dictionary<string, object>
                {
                    ["isOnline"] = false,
                    ["fechaModificacion"] = Timestamp.FromDateTime(DateTime.UtcNow)
                });
                count++;
            }
            
            if (count > 0)
            {
                await batch.CommitAsync();
                _logger?.LogInformation("Marcados {Count} usuarios como offline por inactividad", count);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al marcar usuarios inactivos como offline");
        }
    }
    
    /// <summary>
    /// Crea o actualiza la presencia de un usuario usando FirebaseUid como Document ID
    /// </summary>
    public async Task<UserPresence> UpsertAsync(UserPresence presence)
    {
        try
        {
            // Si tiene FirebaseUid, usarlo como Document ID para consistencia
            if (!string.IsNullOrEmpty(presence.FirebaseUid))
            {
                var data = EntityToDocument(presence);
                var docRef = Collection.Document(presence.FirebaseUid);
                await docRef.SetAsync(data, SetOptions.MergeAll);
                presence.SetFirestoreDocumentId(presence.FirebaseUid);
                
                _logger?.LogInformation("Presencia upserted para usuario {Username} con FirebaseUid: {FirebaseUid}", 
                    presence.Username, presence.FirebaseUid);
                
                return presence;
            }
            
            // Fallback: buscar por UserId
            var existing = await GetByUserIdAsync(presence.UserId);
            
            if (existing != null)
            {
                // Actualizar
                presence.Id = existing.Id;
                var documentId = existing.GetFirestoreDocumentId();
                if (!string.IsNullOrEmpty(documentId))
                {
                    presence.SetFirestoreDocumentId(documentId);
                    await UpdateByDocumentIdAsync(documentId, presence);
                    return presence;
                }
            }
            
            // Crear nuevo
            return await AddAsync(presence);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al upsert presencia del usuario: {UserId}", presence.UserId);
            throw;
        }
    }
    
    /// <summary>
    /// Escucha cambios en los usuarios online en tiempo real
    /// </summary>
    public IDisposable ListenToOnlineUsers(Action<IEnumerable<UserPresence>> onChanged)
    {
        var query = Collection.WhereEqualTo("isOnline", true);
        
        var listener = query.Listen(snapshot =>
        {
            var users = snapshot.Documents
                .Select(DocumentToEntity)
                .OrderBy(p => p.NombreCompleto)
                .ToList();
            
            onChanged(users);
        });
        
        return new ListenerDisposable(listener);
    }
    
    /// <summary>
    /// Helper class para disponer el listener
    /// </summary>
    private class ListenerDisposable : IDisposable
    {
        private readonly FirestoreChangeListener _listener;
        
        public ListenerDisposable(FirestoreChangeListener listener)
        {
            _listener = listener;
        }
        
        public void Dispose()
        {
            _ = _listener.StopAsync();
        }
    }
    
    #endregion
}
