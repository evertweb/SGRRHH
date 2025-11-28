using SGRRHH.Core.Entities;

namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Repositorio para gestionar mensajes de chat en Firestore
/// </summary>
public interface IChatMessageRepository : IRepository<ChatMessage>
{
    /// <summary>
    /// Obtiene todos los mensajes de una conversación entre dos usuarios
    /// </summary>
    Task<IEnumerable<ChatMessage>> GetConversationAsync(int userId1, int userId2, int limit = 50);
    
    /// <summary>
    /// Obtiene los mensajes no leídos para un usuario
    /// </summary>
    Task<IEnumerable<ChatMessage>> GetUnreadMessagesAsync(int userId);
    
    /// <summary>
    /// Cuenta los mensajes no leídos para un usuario
    /// </summary>
    Task<int> CountUnreadMessagesAsync(int userId);
    
    /// <summary>
    /// Cuenta los mensajes no leídos de una conversación específica
    /// </summary>
    Task<int> CountUnreadInConversationAsync(int currentUserId, int otherUserId);
    
    /// <summary>
    /// Marca todos los mensajes de una conversación como leídos
    /// </summary>
    Task MarkConversationAsReadAsync(int currentUserId, int otherUserId);
    
    /// <summary>
    /// Obtiene las conversaciones recientes de un usuario con el último mensaje
    /// </summary>
    Task<IEnumerable<ChatConversationSummary>> GetRecentConversationsAsync(int userId);
    
    /// <summary>
    /// Escucha mensajes nuevos en tiempo real para una conversación
    /// </summary>
    IDisposable ListenToConversation(int userId1, int userId2, DateTime afterDate, Action<ChatMessage> onNewMessage);
    
    /// <summary>
    /// Escucha mensajes nuevos en tiempo real para todos los mensajes de un usuario
    /// </summary>
    IDisposable ListenToUserMessages(int userId, Action<ChatMessage> onNewMessage);
}

/// <summary>
/// Resumen de una conversación para mostrar en la lista
/// </summary>
public class ChatConversationSummary
{
    /// <summary>
    /// ID del otro usuario en la conversación
    /// </summary>
    public int OtherUserId { get; set; }
    
    /// <summary>
    /// Nombre del otro usuario
    /// </summary>
    public string OtherUserName { get; set; } = string.Empty;
    
    /// <summary>
    /// Último mensaje de la conversación
    /// </summary>
    public string LastMessage { get; set; } = string.Empty;
    
    /// <summary>
    /// Fecha del último mensaje
    /// </summary>
    public DateTime LastMessageDate { get; set; }
    
    /// <summary>
    /// Cantidad de mensajes no leídos
    /// </summary>
    public int UnreadCount { get; set; }
    
    /// <summary>
    /// Indica si el otro usuario está online
    /// </summary>
    public bool IsOnline { get; set; }
}
