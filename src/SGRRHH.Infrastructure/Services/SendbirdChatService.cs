using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;
using SGRRHH.Core.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace SGRRHH.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de chat usando Sendbird REST API
/// </summary>
public class SendbirdChatService : ISendbirdChatService, IDisposable
{
    private readonly ILogger<SendbirdChatService>? _logger;
    private readonly SendbirdSettings _settings;
    private readonly HttpClient _httpClient;
    private bool _disposed;
    private string _currentUserId = string.Empty;
    private string _sessionToken = string.Empty;

    public Usuario? CurrentUser { get; private set; }

    public event EventHandler<SendbirdMessage>? MessageReceived;
    public event EventHandler<int>? UnreadCountChanged;

    public SendbirdChatService(
        IOptions<SendbirdSettings> settings,
        ILogger<SendbirdChatService>? logger = null)
    {
        _settings = settings.Value;
        _logger = logger;

        // Validar configuración
        if (string.IsNullOrWhiteSpace(_settings.ApplicationId))
        {
            _logger?.LogWarning("Sendbird ApplicationId no está configurado");
        }

        // Configurar HTTP client
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri("https://api-" + _settings.ApplicationId + ".sendbird.com/");
        _httpClient.DefaultRequestHeaders.Add("Api-Token", _settings.ApiToken ?? "");
    }

    /// <summary>
    /// Conecta al usuario a Sendbird
    /// </summary>
    public async Task<bool> ConnectAsync(Usuario user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        try
        {
            CurrentUser = user;
            _currentUserId = !string.IsNullOrEmpty(user.FirebaseUid) ? user.FirebaseUid : user.Username;

            // Crear o actualizar usuario en Sendbird
            var userData = new
            {
                user_id = _currentUserId,
                nickname = user.NombreCompleto,
                profile_url = ""
            };

            var content = new StringContent(
                JsonSerializer.Serialize(userData),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("v3/users", content);

            if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                _logger?.LogInformation("Usuario {Username} conectado a Sendbird", user.Username);

                // Obtener contador de no leídos
                await UpdateUnreadCountAsync();

                return true;
            }

            _logger?.LogError("Error al crear usuario en Sendbird: {StatusCode}", response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al conectar usuario a Sendbird");
            return false;
        }
    }

    /// <summary>
    /// Desconecta al usuario de Sendbird
    /// </summary>
    public Task DisconnectAsync()
    {
        CurrentUser = null;
        _currentUserId = string.Empty;
        _sessionToken = string.Empty;

        _logger?.LogInformation("Usuario desconectado de Sendbird");

        return Task.CompletedTask;
    }

    /// <summary>
    /// Crea o recupera un canal directo con otro usuario
    /// </summary>
    public async Task<SendbirdChannel?> CreateOrGetDirectChannelAsync(string otherUserId, string otherUserName)
    {
        try
        {
            var channelData = new
            {
                user_ids = new[] { _currentUserId, otherUserId },
                is_distinct = true,
                name = $"Chat con {otherUserName}",
                channel_url = "",
                cover_url = ""
            };

            var content = new StringContent(
                JsonSerializer.Serialize(channelData),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("v3/group_channels", content);
            var result = await response.Content.ReadFromJsonAsync<JsonDocument>();

            if (result != null)
            {
                return ParseChannel(result.RootElement);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al crear canal con usuario {UserId}", otherUserId);
            return null;
        }
    }

    /// <summary>
    /// Obtiene la lista de canales del usuario actual
    /// </summary>
    public async Task<IEnumerable<SendbirdChannel>> GetMyChannelsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"v3/users/{_currentUserId}/my_group_channels?limit=100");

            if (!response.IsSuccessStatusCode)
                return Enumerable.Empty<SendbirdChannel>();

            var result = await response.Content.ReadFromJsonAsync<JsonDocument>();

            if (result != null && result.RootElement.TryGetProperty("channels", out var channelsArray))
            {
                var channels = new List<SendbirdChannel>();

                foreach (var channelElement in channelsArray.EnumerateArray())
                {
                    var channel = ParseChannel(channelElement);
                    if (channel != null)
                        channels.Add(channel);
                }

                return channels;
            }

            return Enumerable.Empty<SendbirdChannel>();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener canales del usuario");
            return Enumerable.Empty<SendbirdChannel>();
        }
    }

    /// <summary>
    /// Obtiene los mensajes de un canal
    /// </summary>
    public async Task<IEnumerable<SendbirdMessage>> GetMessagesAsync(string channelUrl, int limit = 50)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"v3/group_channels/{Uri.EscapeDataString(channelUrl)}/messages?message_limit={limit}");

            if (!response.IsSuccessStatusCode)
                return Enumerable.Empty<SendbirdMessage>();

            var result = await response.Content.ReadFromJsonAsync<JsonDocument>();

            if (result != null && result.RootElement.TryGetProperty("messages", out var messagesArray))
            {
                var messages = new List<SendbirdMessage>();

                foreach (var messageElement in messagesArray.EnumerateArray())
                {
                    var message = ParseMessage(channelUrl, messageElement);
                    if (message != null)
                        messages.Add(message);
                }

                return messages;
            }

            return Enumerable.Empty<SendbirdMessage>();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener mensajes del canal {ChannelUrl}", channelUrl);
            return Enumerable.Empty<SendbirdMessage>();
        }
    }

    /// <summary>
    /// Envía un mensaje a un canal
    /// </summary>
    public async Task<SendbirdMessage?> SendMessageAsync(string channelUrl, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("El mensaje no puede estar vacío", nameof(message));

        try
        {
            var messageData = new
            {
                message_type = "MESG",
                user_id = _currentUserId,
                message = message.Trim()
            };

            var content = new StringContent(
                JsonSerializer.Serialize(messageData),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(
                $"v3/group_channels/{Uri.EscapeDataString(channelUrl)}/messages",
                content);

            if (!response.IsSuccessStatusCode)
            {
                _logger?.LogError("Error al enviar mensaje: {StatusCode}", response.StatusCode);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<JsonDocument>();

            if (result != null)
            {
                return ParseMessage(channelUrl, result.RootElement);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al enviar mensaje al canal {ChannelUrl}", channelUrl);
            return null;
        }
    }

    /// <summary>
    /// Marca un canal como leído
    /// </summary>
    public async Task MarkChannelAsReadAsync(string channelUrl)
    {
        try
        {
            var response = await _httpClient.PutAsync(
                $"v3/group_channels/{Uri.EscapeDataString(channelUrl)}/messages/mark_as_read",
                null);

            if (response.IsSuccessStatusCode)
            {
                await UpdateUnreadCountAsync();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al marcar canal como leído");
        }
    }

    /// <summary>
    /// Envía un archivo a un canal
    /// </summary>
    public async Task<SendbirdMessage?> SendFileAsync(string channelUrl, string filePath)
    {
        // TODO: Implementar envío de archivos con multipart/form-data
        // Por ahora retorna null (no implementado)
        _logger?.LogWarning("SendFileAsync no está implementado aún");
        await Task.CompletedTask;
        return null;
    }

    /// <summary>
    /// Obtiene el total de mensajes no leídos
    /// </summary>
    public async Task<int> GetTotalUnreadCountAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"v3/users/{_currentUserId}/unread_message_count");

            if (!response.IsSuccessStatusCode)
                return 0;

            var result = await response.Content.ReadFromJsonAsync<JsonDocument>();

            if (result != null && result.RootElement.TryGetProperty("unread_count", out var countElement))
            {
                return countElement.GetInt32();
            }

            return 0;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener contador de mensajes no leídos");
            return 0;
        }
    }

    /// <summary>
    /// Actualiza el contador de no leídos y notifica
    /// </summary>
    private async Task UpdateUnreadCountAsync()
    {
        try
        {
            var count = await GetTotalUnreadCountAsync();
            UnreadCountChanged?.Invoke(this, count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al actualizar contador de no leídos");
        }
    }

    #region Parsers

    private SendbirdChannel? ParseChannel(JsonElement element)
    {
        try
        {
            var channel = new SendbirdChannel
            {
                Url = element.TryGetProperty("channel_url", out var url) ? url.GetString() ?? "" : "",
                Name = element.TryGetProperty("name", out var name) ? name.GetString() ?? "" : "",
                CoverUrl = element.TryGetProperty("cover_url", out var cover) ? cover.GetString() : null,
                UnreadMessageCount = element.TryGetProperty("unread_message_count", out var unread) ? unread.GetInt32() : 0
            };

            // Parsear último mensaje
            if (element.TryGetProperty("last_message", out var lastMsg))
            {
                channel.LastMessage = ParseMessage(channel.Url, lastMsg);
                channel.LastMessageAt = channel.LastMessage?.CreatedAt;
            }

            // Parsear miembros
            if (element.TryGetProperty("members", out var members))
            {
                foreach (var memberElement in members.EnumerateArray())
                {
                    var member = new SendbirdMember
                    {
                        UserId = memberElement.TryGetProperty("user_id", out var userId) ? userId.GetString() ?? "" : "",
                        Nickname = memberElement.TryGetProperty("nickname", out var nick) ? nick.GetString() ?? "" : "",
                        ProfileUrl = memberElement.TryGetProperty("profile_url", out var profileUrl) ? profileUrl.GetString() : null,
                        IsOnline = memberElement.TryGetProperty("is_online", out var isOnline) && isOnline.GetBoolean()
                    };

                    channel.Members.Add(member);
                }
            }

            return channel;
        }
        catch
        {
            return null;
        }
    }

    private SendbirdMessage? ParseMessage(string channelUrl, JsonElement element)
    {
        try
        {
            var message = new SendbirdMessage
            {
                MessageId = element.TryGetProperty("message_id", out var msgId) ? msgId.GetInt64() : 0,
                ChannelUrl = channelUrl,
                Message = element.TryGetProperty("message", out var msg) ? msg.GetString() ?? "" : "",
                SenderId = element.TryGetProperty("user_id", out var senderId)
                    ? senderId.GetString() ?? ""
                    : (element.TryGetProperty("user", out var userObj) && userObj.TryGetProperty("user_id", out var uid)
                        ? uid.GetString() ?? ""
                        : ""),
                SenderName = element.TryGetProperty("nickname", out var nickname)
                    ? nickname.GetString() ?? ""
                    : (element.TryGetProperty("user", out var userObj2) && userObj2.TryGetProperty("nickname", out var nick)
                        ? nick.GetString() ?? ""
                        : ""),
                CreatedAt = element.TryGetProperty("created_at", out var createdAt)
                    ? DateTimeOffset.FromUnixTimeMilliseconds(createdAt.GetInt64()).DateTime
                    : DateTime.UtcNow,
                IsMyMessage = false
            };

            message.IsMyMessage = message.SenderId == _currentUserId;

            return message;
        }
        catch
        {
            return null;
        }
    }

    #endregion

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
            _httpClient?.Dispose();
        }

        _disposed = true;
    }
}
