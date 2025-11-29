namespace SGRRHH.Core.Entities;

/// <summary>
/// Entidad que representa un mensaje de chat entre usuarios.
/// Sistema de mensajería interna de la aplicación.
/// </summary>
public class ChatMessage : EntidadBase
{
    /// <summary>
    /// ID del usuario que envía el mensaje
    /// </summary>
    public int SenderId { get; set; }

    /// <summary>
    /// Firebase UID del usuario que envía (para identificación única confiable)
    /// </summary>
    public string? SenderFirebaseUid { get; set; }
    
    /// <summary>
    /// Nombre del usuario que envía (para mostrar sin consultas adicionales)
    /// </summary>
    public string SenderName { get; set; } = string.Empty;
    
    /// <summary>
    /// ID del usuario que recibe el mensaje
    /// </summary>
    public int ReceiverId { get; set; }
    
    /// <summary>
    /// Nombre del usuario que recibe (para mostrar sin consultas adicionales)
    /// </summary>
    public string ReceiverName { get; set; } = string.Empty;
    
    /// <summary>
    /// Contenido del mensaje (texto)
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// Fecha y hora en que se envió el mensaje
    /// </summary>
    public DateTime SentAt { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Indica si el mensaje ha sido leído por el receptor
    /// </summary>
    public bool IsRead { get; set; }
    
    /// <summary>
    /// Fecha y hora en que se leyó el mensaje (si fue leído)
    /// </summary>
    public DateTime? ReadAt { get; set; }
    
    /// <summary>
    /// ID de conversación (combinación ordenada de SenderId y ReceiverId)
    /// Facilita consultar todos los mensajes de una conversación
    /// </summary>
    public string ConversationId { get; set; } = string.Empty;
    
    /// <summary>
    /// Genera el ID de conversación a partir de dos IDs de usuario.
    /// Siempre ordena los IDs para que sea consistente sin importar quién envía.
    /// </summary>
    public static string GenerateConversationId(int userId1, int userId2)
    {
        var ids = new[] { userId1, userId2 }.OrderBy(x => x);
        return string.Join("_", ids);
    }
}
