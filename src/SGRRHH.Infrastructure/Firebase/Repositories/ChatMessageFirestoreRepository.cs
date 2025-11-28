using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Infrastructure.Firebase.Repositories;

/// <summary>
/// Implementación del repositorio de Mensajes de Chat para Firestore.
/// Colección: "chat_messages"
/// 
/// Gestiona los mensajes de chat entre usuarios.
/// </summary>
public class ChatMessageFirestoreRepository : FirestoreRepository<ChatMessage>, IChatMessageRepository
{
    private const string COLLECTION_NAME = "chat_messages";
    
    public ChatMessageFirestoreRepository(FirebaseInitializer firebase, ILogger<ChatMessageFirestoreRepository>? logger = null)
        : base(firebase, COLLECTION_NAME, logger)
    {
    }
    
    #region Entity <-> Document Mapping
    
    protected override Dictionary<string, object?> EntityToDocument(ChatMessage entity)
    {
        var doc = base.EntityToDocument(entity);
        
        doc["senderId"] = entity.SenderId;
        doc["senderName"] = entity.SenderName;
        doc["receiverId"] = entity.ReceiverId;
        doc["receiverName"] = entity.ReceiverName;
        doc["content"] = entity.Content;
        doc["sentAt"] = Timestamp.FromDateTime(entity.SentAt.ToUniversalTime());
        doc["isRead"] = entity.IsRead;
        doc["readAt"] = entity.ReadAt.HasValue 
            ? Timestamp.FromDateTime(entity.ReadAt.Value.ToUniversalTime()) 
            : null;
        doc["conversationId"] = entity.ConversationId;
        
        return doc;
    }
    
    protected override ChatMessage DocumentToEntity(DocumentSnapshot document)
    {
        var entity = base.DocumentToEntity(document);
        
        if (document.TryGetValue<int>("senderId", out var senderId))
            entity.SenderId = senderId;
        
        if (document.TryGetValue<string>("senderName", out var senderName))
            entity.SenderName = senderName ?? string.Empty;
        
        if (document.TryGetValue<int>("receiverId", out var receiverId))
            entity.ReceiverId = receiverId;
        
        if (document.TryGetValue<string>("receiverName", out var receiverName))
            entity.ReceiverName = receiverName ?? string.Empty;
        
        if (document.TryGetValue<string>("content", out var content))
            entity.Content = content ?? string.Empty;
        
        if (document.TryGetValue<Timestamp>("sentAt", out var sentAt))
            entity.SentAt = sentAt.ToDateTime().ToLocalTime();
        
        if (document.TryGetValue<bool>("isRead", out var isRead))
            entity.IsRead = isRead;
        
        if (document.TryGetValue<Timestamp?>("readAt", out var readAt) && readAt.HasValue)
            entity.ReadAt = readAt.Value.ToDateTime().ToLocalTime();
        
        if (document.TryGetValue<string>("conversationId", out var conversationId))
            entity.ConversationId = conversationId ?? string.Empty;
        
        return entity;
    }
    
    #endregion
    
    #region IChatMessageRepository Implementation
    
    /// <summary>
    /// Obtiene todos los mensajes de una conversación entre dos usuarios
    /// </summary>
    public async Task<IEnumerable<ChatMessage>> GetConversationAsync(int userId1, int userId2, int limit = 50)
    {
        try
        {
            var conversationId = ChatMessage.GenerateConversationId(userId1, userId2);
            
            var query = Collection
                .WhereEqualTo("conversationId", conversationId)
                .OrderByDescending("sentAt")
                .Limit(limit);
            
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents
                .Select(DocumentToEntity)
                .Reverse() // Para mostrar en orden cronológico
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener conversación entre {UserId1} y {UserId2}", userId1, userId2);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene los mensajes no leídos para un usuario
    /// </summary>
    public async Task<IEnumerable<ChatMessage>> GetUnreadMessagesAsync(int userId)
    {
        try
        {
            var query = Collection
                .WhereEqualTo("receiverId", userId)
                .WhereEqualTo("isRead", false)
                .OrderByDescending("sentAt");
            
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents
                .Select(DocumentToEntity)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener mensajes no leídos del usuario: {UserId}", userId);
            throw;
        }
    }
    
    /// <summary>
    /// Cuenta los mensajes no leídos para un usuario
    /// </summary>
    public async Task<int> CountUnreadMessagesAsync(int userId)
    {
        try
        {
            var query = Collection
                .WhereEqualTo("receiverId", userId)
                .WhereEqualTo("isRead", false);
            
            var snapshot = await query.GetSnapshotAsync();
            return snapshot.Count;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al contar mensajes no leídos del usuario: {UserId}", userId);
            throw;
        }
    }
    
    /// <summary>
    /// Cuenta los mensajes no leídos de una conversación específica
    /// </summary>
    public async Task<int> CountUnreadInConversationAsync(int currentUserId, int otherUserId)
    {
        try
        {
            var conversationId = ChatMessage.GenerateConversationId(currentUserId, otherUserId);
            
            var query = Collection
                .WhereEqualTo("conversationId", conversationId)
                .WhereEqualTo("receiverId", currentUserId)
                .WhereEqualTo("isRead", false);
            
            var snapshot = await query.GetSnapshotAsync();
            return snapshot.Count;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al contar mensajes no leídos en conversación {CurrentUserId}-{OtherUserId}", 
                currentUserId, otherUserId);
            throw;
        }
    }
    
    /// <summary>
    /// Marca todos los mensajes de una conversación como leídos
    /// </summary>
    public async Task MarkConversationAsReadAsync(int currentUserId, int otherUserId)
    {
        try
        {
            var conversationId = ChatMessage.GenerateConversationId(currentUserId, otherUserId);
            
            // Obtener mensajes no leídos donde el usuario actual es el receptor
            var query = Collection
                .WhereEqualTo("conversationId", conversationId)
                .WhereEqualTo("receiverId", currentUserId)
                .WhereEqualTo("isRead", false);
            
            var snapshot = await query.GetSnapshotAsync();
            
            if (!snapshot.Documents.Any())
                return;
            
            var batch = Firestore.StartBatch();
            var now = Timestamp.FromDateTime(DateTime.UtcNow);
            
            foreach (var doc in snapshot.Documents)
            {
                batch.Update(doc.Reference, new Dictionary<string, object>
                {
                    ["isRead"] = true,
                    ["readAt"] = now
                });
            }
            
            await batch.CommitAsync();
            
            _logger?.LogInformation("Marcados {Count} mensajes como leídos en conversación {ConversationId}", 
                snapshot.Count, conversationId);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al marcar conversación como leída");
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene las conversaciones recientes de un usuario con el último mensaje
    /// </summary>
    public async Task<IEnumerable<ChatConversationSummary>> GetRecentConversationsAsync(int userId)
    {
        try
        {
            // Obtener mensajes donde el usuario es emisor o receptor
            // Necesitamos dos consultas por las limitaciones de Firestore
            var sentQuery = Collection.WhereEqualTo("senderId", userId);
            var receivedQuery = Collection.WhereEqualTo("receiverId", userId);
            
            var sentSnapshot = await sentQuery.GetSnapshotAsync();
            var receivedSnapshot = await receivedQuery.GetSnapshotAsync();
            
            // Combinar y procesar mensajes
            var allMessages = sentSnapshot.Documents
                .Concat(receivedSnapshot.Documents)
                .Select(DocumentToEntity)
                .ToList();
            
            // Agrupar por conversación y obtener el último mensaje
            var conversations = allMessages
                .GroupBy(m => m.ConversationId)
                .Select(g =>
                {
                    var lastMessage = g.OrderByDescending(m => m.SentAt).First();
                    var otherUserId = lastMessage.SenderId == userId ? lastMessage.ReceiverId : lastMessage.SenderId;
                    var otherUserName = lastMessage.SenderId == userId ? lastMessage.ReceiverName : lastMessage.SenderName;
                    
                    return new ChatConversationSummary
                    {
                        OtherUserId = otherUserId,
                        OtherUserName = otherUserName,
                        LastMessage = lastMessage.Content.Length > 50 
                            ? lastMessage.Content.Substring(0, 47) + "..." 
                            : lastMessage.Content,
                        LastMessageDate = lastMessage.SentAt,
                        UnreadCount = g.Count(m => m.ReceiverId == userId && !m.IsRead)
                    };
                })
                .OrderByDescending(c => c.LastMessageDate)
                .ToList();
            
            return conversations;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener conversaciones recientes del usuario: {UserId}", userId);
            throw;
        }
    }
    
    /// <summary>
    /// Escucha mensajes nuevos en tiempo real para una conversación
    /// </summary>
    public IDisposable ListenToConversation(int userId1, int userId2, DateTime afterDate, Action<ChatMessage> onNewMessage)
    {
        var conversationId = ChatMessage.GenerateConversationId(userId1, userId2);
        
        // Escuchar mensajes posteriores a la fecha indicada
        var query = Collection
            .WhereEqualTo("conversationId", conversationId)
            .WhereGreaterThan("sentAt", Timestamp.FromDateTime(afterDate.ToUniversalTime()))
            .OrderBy("sentAt");
        
        var listener = query.Listen(snapshot =>
        {
            foreach (var change in snapshot.Changes)
            {
                if (change.ChangeType == DocumentChange.Type.Added)
                {
                    var message = DocumentToEntity(change.Document);
                    onNewMessage(message);
                }
            }
        });
        
        return new ListenerDisposable(listener);
    }
    
    /// <summary>
    /// Escucha mensajes nuevos en tiempo real para todos los mensajes de un usuario
    /// </summary>
    public IDisposable ListenToUserMessages(int userId, Action<ChatMessage> onNewMessage)
    {
        // Solo escuchar mensajes nuevos (llegados después de ahora)
        // Esto evita procesar todo el historial de mensajes
        var query = Collection
            .WhereEqualTo("receiverId", userId)
            .WhereGreaterThan("sentAt", Timestamp.FromDateTime(DateTime.UtcNow))
            .OrderBy("sentAt"); // Necesario para el WhereGreaterThan
        
        var listener = query.Listen(snapshot =>
        {
            foreach (var change in snapshot.Changes)
            {
                if (change.ChangeType == DocumentChange.Type.Added)
                {
                    var message = DocumentToEntity(change.Document);
                    onNewMessage(message);
                }
            }
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
