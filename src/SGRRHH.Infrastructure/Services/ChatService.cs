using Microsoft.Extensions.Logging;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;
using SGRRHH.Infrastructure.Firebase.Repositories;

namespace SGRRHH.Infrastructure.Services;

/// <summary>
/// Servicio de chat/mensajería entre usuarios.
/// Permite enviar y recibir mensajes SMS-like entre usuarios del sistema.
/// </summary>
public class ChatService : IChatService, IDisposable
{
    private readonly ChatMessageFirestoreRepository _chatRepository;
    private readonly ILogger<ChatService>? _logger;
    
    private IDisposable? _messageListener;
    private bool _disposed;
    
    public Usuario? CurrentUser { get; private set; }
    
    public event EventHandler<ChatMessage>? NewMessageReceived;
    public event EventHandler<int>? UnreadCountChanged;
    
    public ChatService(ChatMessageFirestoreRepository chatRepository, ILogger<ChatService>? logger = null)
    {
        _chatRepository = chatRepository;
        _logger = logger;
    }
    
    /// <summary>
    /// Configura el usuario actual para el servicio
    /// </summary>
    public void SetCurrentUser(Usuario user)
    {
        CurrentUser = user;
        _logger?.LogInformation("ChatService configurado para usuario {Username}", user.Username);
    }
    
    /// <summary>
    /// Envía un mensaje a otro usuario
    /// </summary>
    public async Task<ChatMessage> SendMessageAsync(int receiverId, string receiverName, string content)
    {
        if (CurrentUser == null)
            throw new InvalidOperationException("No hay usuario configurado en el servicio de chat");
        
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("El contenido del mensaje no puede estar vacío", nameof(content));
        
        try
        {
            var message = new ChatMessage
            {
                SenderId = CurrentUser.Id,
                SenderName = CurrentUser.NombreCompleto,
                ReceiverId = receiverId,
                ReceiverName = receiverName,
                Content = content.Trim(),
                SentAt = DateTime.Now,
                IsRead = false,
                ConversationId = ChatMessage.GenerateConversationId(CurrentUser.Id, receiverId)
            };
            
            var savedMessage = await _chatRepository.AddAsync(message);
            
            _logger?.LogInformation("Mensaje enviado de {Sender} a {Receiver}", 
                CurrentUser.Username, receiverName);
            
            return savedMessage;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al enviar mensaje a {ReceiverId}", receiverId);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene los mensajes de una conversación
    /// </summary>
    public async Task<IEnumerable<ChatMessage>> GetConversationAsync(int otherUserId, int limit = 50)
    {
        if (CurrentUser == null)
            return Enumerable.Empty<ChatMessage>();
        
        try
        {
            return await _chatRepository.GetConversationAsync(CurrentUser.Id, otherUserId, limit);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener conversación con {OtherUserId}", otherUserId);
            return Enumerable.Empty<ChatMessage>();
        }
    }
    
    /// <summary>
    /// Obtiene el resumen de conversaciones recientes
    /// </summary>
    public async Task<IEnumerable<ChatConversationSummary>> GetRecentConversationsAsync()
    {
        if (CurrentUser == null)
            return Enumerable.Empty<ChatConversationSummary>();
        
        try
        {
            return await _chatRepository.GetRecentConversationsAsync(CurrentUser.Id);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener conversaciones recientes");
            return Enumerable.Empty<ChatConversationSummary>();
        }
    }
    
    /// <summary>
    /// Marca una conversación como leída
    /// </summary>
    public async Task MarkConversationAsReadAsync(int otherUserId)
    {
        if (CurrentUser == null)
            return;
        
        try
        {
            await _chatRepository.MarkConversationAsReadAsync(CurrentUser.Id, otherUserId);
            
            // Notificar cambio en contador de no leídos
            var unreadCount = await GetUnreadCountAsync();
            UnreadCountChanged?.Invoke(this, unreadCount);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al marcar conversación como leída");
        }
    }
    
    /// <summary>
    /// Obtiene el total de mensajes no leídos
    /// </summary>
    public async Task<int> GetUnreadCountAsync()
    {
        if (CurrentUser == null)
            return 0;
        
        try
        {
            return await _chatRepository.CountUnreadMessagesAsync(CurrentUser.Id);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al contar mensajes no leídos");
            return 0;
        }
    }
    
    /// <summary>
    /// Inicia la escucha de mensajes en tiempo real
    /// </summary>
    public IDisposable StartListening()
    {
        if (CurrentUser == null)
            throw new InvalidOperationException("No hay usuario configurado en el servicio de chat");
        
        _messageListener?.Dispose();
        
        _messageListener = _chatRepository.ListenToUserMessages(CurrentUser.Id, async message =>
        {
            // Solo notificar si es un mensaje nuevo (no enviado por el usuario actual)
            if (message.SenderId != CurrentUser.Id)
            {
                NewMessageReceived?.Invoke(this, message);
                
                // Actualizar contador de no leídos
                var unreadCount = await GetUnreadCountAsync();
                UnreadCountChanged?.Invoke(this, unreadCount);
            }
        });
        
        _logger?.LogInformation("Iniciada escucha de mensajes para usuario {Username}", CurrentUser.Username);
        
        return _messageListener;
    }
    
    /// <summary>
    /// Escucha mensajes de una conversación específica en tiempo real
    /// </summary>
    public IDisposable ListenToConversation(int otherUserId, DateTime afterDate, Action<ChatMessage> onNewMessage)
    {
        if (CurrentUser == null)
            throw new InvalidOperationException("No hay usuario configurado en el servicio de chat");
        
        return _chatRepository.ListenToConversation(CurrentUser.Id, otherUserId, afterDate, onNewMessage);
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        
        if (disposing)
        {
            _messageListener?.Dispose();
        }
        
        _disposed = true;
    }
}
