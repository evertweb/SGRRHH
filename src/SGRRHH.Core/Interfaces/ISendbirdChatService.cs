using SGRRHH.Core.Entities;

namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Servicio de chat usando Sendbird como proveedor
/// </summary>
public interface ISendbirdChatService
{
    /// <summary>
    /// Usuario actual del chat
    /// </summary>
    Usuario? CurrentUser { get; }

    /// <summary>
    /// Conecta al usuario a Sendbird
    /// </summary>
    Task<bool> ConnectAsync(Usuario user);

    /// <summary>
    /// Desconecta al usuario de Sendbird
    /// </summary>
    Task DisconnectAsync();

    /// <summary>
    /// Envía un mensaje a un canal (conversación 1-a-1)
    /// </summary>
    Task<SendbirdMessage?> SendMessageAsync(string channelUrl, string message);

    /// <summary>
    /// Crea o recupera un canal directo con otro usuario
    /// </summary>
    Task<SendbirdChannel?> CreateOrGetDirectChannelAsync(string otherUserId, string otherUserName);

    /// <summary>
    /// Obtiene la lista de canales (conversaciones) del usuario actual
    /// </summary>
    Task<IEnumerable<SendbirdChannel>> GetMyChannelsAsync();

    /// <summary>
    /// Obtiene los mensajes de un canal
    /// </summary>
    Task<IEnumerable<SendbirdMessage>> GetMessagesAsync(string channelUrl, int limit = 50);

    /// <summary>
    /// Marca un canal como leído
    /// </summary>
    Task MarkChannelAsReadAsync(string channelUrl);

    /// <summary>
    /// Obtiene el total de mensajes no leídos
    /// </summary>
    Task<int> GetTotalUnreadCountAsync();

    /// <summary>
    /// Obtiene la lista de usuarios de Sendbird con su estado online
    /// </summary>
    Task<IEnumerable<SendbirdUser>> GetUsersAsync(bool onlineOnly = false);

    /// <summary>
    /// Crea o actualiza un usuario en Sendbird (sin conectarlo)
    /// Útil para sincronizar usuarios de Firebase a Sendbird
    /// </summary>
    Task<bool> EnsureUserExistsAsync(string userId, string nickname);

    /// <summary>
    /// Envía un archivo a un canal
    /// </summary>
    Task<SendbirdMessage?> SendFileAsync(string channelUrl, string filePath);

    /// <summary>
    /// Evento que se dispara cuando llega un nuevo mensaje
    /// </summary>
    event EventHandler<SendbirdMessage>? MessageReceived;

    /// <summary>
    /// Evento que se dispara cuando cambia el contador de no leídos
    /// </summary>
    event EventHandler<int>? UnreadCountChanged;
}

/// <summary>
/// Representa un canal (conversación) de Sendbird
/// </summary>
public class SendbirdChannel
{
    public string Url { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? CoverUrl { get; set; }
    public int UnreadMessageCount { get; set; }
    public SendbirdMessage? LastMessage { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public List<SendbirdMember> Members { get; set; } = new();
}

/// <summary>
/// Representa un miembro de un canal
/// </summary>
public class SendbirdMember
{
    public string UserId { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;
    public string? ProfileUrl { get; set; }
    public bool IsOnline { get; set; }
}

/// <summary>
/// Representa un mensaje de Sendbird
/// </summary>
public class SendbirdMessage
{
    public long MessageId { get; set; }
    public string ChannelUrl { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsMyMessage { get; set; }

    // Propiedades para archivos adjuntos
    public bool IsFileMessage { get; set; }
    public string? FileUrl { get; set; }
    public string? FileName { get; set; }
    public string? FileType { get; set; }
    public long FileSize { get; set; }
}

/// <summary>
/// Representa un usuario de Sendbird con estado de presencia
/// </summary>
public class SendbirdUser
{
    public string UserId { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;
    public string? ProfileUrl { get; set; }
    public bool IsOnline { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
