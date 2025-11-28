using SGRRHH.Core.Entities;

namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Servicio de chat/mensajería entre usuarios
/// </summary>
public interface IChatService
{
    /// <summary>
    /// Usuario actual del chat
    /// </summary>
    Usuario? CurrentUser { get; }
    
    /// <summary>
    /// Envía un mensaje a otro usuario
    /// </summary>
    Task<ChatMessage> SendMessageAsync(int receiverId, string receiverName, string content);
    
    /// <summary>
    /// Obtiene los mensajes de una conversación
    /// </summary>
    Task<IEnumerable<ChatMessage>> GetConversationAsync(int otherUserId, int limit = 50);
    
    /// <summary>
    /// Obtiene el resumen de conversaciones recientes
    /// </summary>
    Task<IEnumerable<ChatConversationSummary>> GetRecentConversationsAsync();
    
    /// <summary>
    /// Marca una conversación como leída
    /// </summary>
    Task MarkConversationAsReadAsync(int otherUserId);
    
    /// <summary>
    /// Obtiene el total de mensajes no leídos
    /// </summary>
    Task<int> GetUnreadCountAsync();
    
    /// <summary>
    /// Configura el usuario actual para el servicio
    /// </summary>
    void SetCurrentUser(Usuario user);
    
    /// <summary>
    /// Evento que se dispara cuando llega un nuevo mensaje
    /// </summary>
    event EventHandler<ChatMessage>? NewMessageReceived;
    
    /// <summary>
    /// Evento que se dispara cuando cambia el conteo de no leídos
    /// </summary>
    event EventHandler<int>? UnreadCountChanged;
    
    /// <summary>
    /// Inicia la escucha de mensajes en tiempo real
    /// </summary>
    IDisposable StartListening();
    
    /// <summary>
    /// Escucha mensajes de una conversación específica en tiempo real
    /// </summary>
    IDisposable ListenToConversation(int otherUserId, DateTime afterDate, Action<ChatMessage> onNewMessage);
}
