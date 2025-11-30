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
        var baseUrl = $"https://api-{_settings.ApplicationId}.sendbird.com/";
        _httpClient.BaseAddress = new Uri(baseUrl);
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
            // Validar configuración antes de intentar conectar
            if (string.IsNullOrWhiteSpace(_settings.ApplicationId))
            {
                _logger?.LogError("Sendbird ApplicationId no está configurado");
                return false;
            }

            if (string.IsNullOrWhiteSpace(_settings.ApiToken))
            {
                _logger?.LogError("Sendbird ApiToken no está configurado");
                return false;
            }

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
            var responseContent = await response.Content.ReadAsStringAsync();

            // Verificar si fue exitoso o si el usuario ya existe
            var isSuccess = response.IsSuccessStatusCode;
            var isConflict = response.StatusCode == System.Net.HttpStatusCode.Conflict;
            var isUserExists = false;

            // Verificar si es error 400202 (usuario ya existe - unique constraint violation)
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                try
                {
                    var errorResponse = JsonSerializer.Deserialize<JsonDocument>(responseContent);
                    if (errorResponse != null && 
                        errorResponse.RootElement.TryGetProperty("code", out var codeElement) &&
                        codeElement.GetInt32() == 400202)
                    {
                        isUserExists = true;
                        _logger?.LogDebug("Usuario {Username} ya existe en Sendbird", user.Username);
                    }
                }
                catch { }
            }

            if (isSuccess || isConflict || isUserExists)
            {
                _logger?.LogInformation("Usuario {Username} conectado a Sendbird", user.Username);

                // Obtener contador de no leídos
                await UpdateUnreadCountAsync();

                return true;
            }

            _logger?.LogError("Error al crear usuario en Sendbird: {StatusCode} - {Response}", 
                response.StatusCode, responseContent);
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
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            throw new ArgumentException("El archivo no existe", nameof(filePath));

        try
        {
            // 1. Leer archivo
            var fileInfo = new FileInfo(filePath);
            var fileName = fileInfo.Name;
            var fileBytes = await File.ReadAllBytesAsync(filePath);

            // Validar tamaño (25 MB límite de Sendbird)
            if (fileBytes.Length > 25 * 1024 * 1024)
            {
                _logger?.LogError("Archivo demasiado grande: {Size} bytes", fileBytes.Length);
                return null;
            }

            // 2. Preparar multipart/form-data
            using var content = new MultipartFormDataContent();

            // Archivo
            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                GetMimeType(fileName));
            content.Add(fileContent, "file", fileName);

            // Metadata
            content.Add(new StringContent(_currentUserId), "user_id");
            content.Add(new StringContent("FILE"), "message_type");

            // 3. Enviar a Sendbird
            var response = await _httpClient.PostAsync(
                $"v3/group_channels/{Uri.EscapeDataString(channelUrl)}/messages",
                content);

            if (!response.IsSuccessStatusCode)
            {
                _logger?.LogError("Error al enviar archivo: {StatusCode}", response.StatusCode);
                return null;
            }

            // 4. Parsear respuesta
            var result = await response.Content.ReadFromJsonAsync<JsonDocument>();

            if (result != null)
            {
                return ParseMessage(channelUrl, result.RootElement);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al enviar archivo al canal {ChannelUrl}", channelUrl);
            return null;
        }
    }

    /// <summary>
    /// Obtiene el MIME type según la extensión del archivo
    /// </summary>
    private string GetMimeType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".txt" => "text/plain",
            ".csv" => "text/csv",
            ".zip" => "application/zip",
            ".rar" => "application/x-rar-compressed",
            _ => "application/octet-stream"
        };
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
    /// Obtiene la lista de usuarios de Sendbird con su estado online
    /// </summary>
    public async Task<IEnumerable<SendbirdUser>> GetUsersAsync(bool onlineOnly = false)
    {
        try
        {
            // Obtener todos los usuarios activos de Sendbird
            var url = "v3/users?limit=100&active_mode=activated";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger?.LogError("Error al obtener usuarios de Sendbird: {StatusCode}", response.StatusCode);
                return Enumerable.Empty<SendbirdUser>();
            }

            var result = await response.Content.ReadFromJsonAsync<JsonDocument>();

            if (result != null && result.RootElement.TryGetProperty("users", out var usersArray))
            {
                var users = new List<SendbirdUser>();

                foreach (var userElement in usersArray.EnumerateArray())
                {
                    var user = ParseUser(userElement);
                    if (user != null)
                    {
                        // Excluir el usuario actual
                        if (user.UserId != _currentUserId)
                        {
                            // Filtrar solo online si se solicita
                            if (!onlineOnly || user.IsOnline)
                            {
                                users.Add(user);
                            }
                        }
                    }
                }

                // Ordenar: primero online, luego por nombre
                return users.OrderByDescending(u => u.IsOnline)
                            .ThenBy(u => u.Nickname);
            }

            return Enumerable.Empty<SendbirdUser>();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener usuarios de Sendbird");
            return Enumerable.Empty<SendbirdUser>();
        }
    }

    /// <summary>
    /// Parsea un usuario de Sendbird desde JSON
    /// </summary>
    private SendbirdUser? ParseUser(JsonElement element)
    {
        try
        {
            return new SendbirdUser
            {
                UserId = element.TryGetProperty("user_id", out var userId) ? userId.GetString() ?? "" : "",
                Nickname = element.TryGetProperty("nickname", out var nickname) ? nickname.GetString() ?? "" : "",
                ProfileUrl = element.TryGetProperty("profile_url", out var profileUrl) ? profileUrl.GetString() : null,
                IsOnline = element.TryGetProperty("is_online", out var isOnline) && isOnline.GetBoolean(),
                IsActive = element.TryGetProperty("is_active", out var isActive) && isActive.GetBoolean(),
                LastSeenAt = element.TryGetProperty("last_seen_at", out var lastSeen) && lastSeen.GetInt64() > 0
                    ? DateTimeOffset.FromUnixTimeMilliseconds(lastSeen.GetInt64()).DateTime
                    : null,
                CreatedAt = element.TryGetProperty("created_at", out var createdAt)
                    ? DateTimeOffset.FromUnixTimeSeconds(createdAt.GetInt64()).DateTime
                    : DateTime.UtcNow
            };
        }
        catch
        {
            return null;
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

            // Detectar si es mensaje de archivo
            if (element.TryGetProperty("type", out var typeElement) && typeElement.GetString() == "file")
            {
                message.IsFileMessage = true;

                // Extraer información del archivo
                if (element.TryGetProperty("file", out var fileObj))
                {
                    message.FileUrl = fileObj.TryGetProperty("url", out var url) ? url.GetString() : null;
                    message.FileName = fileObj.TryGetProperty("name", out var name) ? name.GetString() : null;
                    message.FileType = fileObj.TryGetProperty("type", out var type) ? type.GetString() : null;
                    message.FileSize = fileObj.TryGetProperty("size", out var size) ? size.GetInt64() : 0;
                }

                // Si no hay texto en el mensaje, usar el nombre del archivo
                if (string.IsNullOrEmpty(message.Message) && !string.IsNullOrEmpty(message.FileName))
                {
                    message.Message = $"📎 {message.FileName}";
                }
            }

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
