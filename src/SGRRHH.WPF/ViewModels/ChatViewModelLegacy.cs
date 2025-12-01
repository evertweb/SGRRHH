using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// ViewModel LEGACY para la vista de Chat y usuarios activos (Firebase).
/// Combina la funcionalidad de ver usuarios online y enviar/recibir mensajes.
/// </summary>
public partial class ChatViewModelLegacy : ObservableObject, IDisposable
{
    private readonly IPresenceService _presenceService;
    private readonly IChatService _chatService;
    private readonly IUsuarioService _usuarioService;
    
    private IDisposable? _onlineUsersListener;
    private IDisposable? _conversationListener;
    private IDisposable? _globalMessageListener;
    private bool _disposed;
    
    // OPTIMIZACIÓN: Debounce para evitar escrituras excesivas de MarkAsRead
    private DateTime _lastMarkAsReadTime = DateTime.MinValue;
    private const int MARK_READ_DEBOUNCE_MS = 3000; // 3 segundos entre llamadas
    
    #region Properties
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private string _loadingMessage = "Cargando...";
    
    /// <summary>
    /// Lista de usuarios actualmente online
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<UserPresenceViewModel> _onlineUsers = new();
    
    /// <summary>
    /// Lista de todos los usuarios disponibles para chat
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<UserPresenceViewModel> _allUsers = new();
    
    /// <summary>
    /// Usuario seleccionado para chat
    /// </summary>
    [ObservableProperty]
    private UserPresenceViewModel? _selectedUser;
    
    /// <summary>
    /// Mensajes de la conversación actual
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ChatMessageViewModel> _messages = new();
    
    /// <summary>
    /// Texto del mensaje a enviar
    /// </summary>
    [ObservableProperty]
    private string _messageText = string.Empty;
    
    /// <summary>
    /// Total de mensajes no leídos
    /// </summary>
    [ObservableProperty]
    private int _totalUnreadCount;
    
    /// <summary>
    /// Indica si hay una conversación activa
    /// </summary>
    [ObservableProperty]
    private bool _hasActiveConversation;
    
    /// <summary>
    /// Nombre del usuario con quien se está chateando
    /// </summary>
    [ObservableProperty]
    private string _chatPartnerName = string.Empty;
    
    /// <summary>
    /// Indica si el chat partner está online
    /// </summary>
    [ObservableProperty]
    private bool _isChatPartnerOnline;
    
    /// <summary>
    /// Filtro de búsqueda de usuarios
    /// </summary>
    [ObservableProperty]
    private string _searchFilter = string.Empty;
    
    /// <summary>
    /// Indica si solo se muestran usuarios online
    /// </summary>
    [ObservableProperty]
    private bool _showOnlyOnline = true;
    
    #endregion
    
    /// <summary>
    /// Evento para solicitar scroll al final de los mensajes
    /// </summary>
    public event EventHandler? ScrollToBottomRequested;
    
    /// <summary>
    /// Evento para notificar nuevo mensaje recibido
    /// </summary>
    public event EventHandler<ChatMessage>? NewMessageNotification;
    
    public ChatViewModelLegacy(
        IPresenceService presenceService,
        IChatService chatService,
        IUsuarioService usuarioService)
    {
        _presenceService = presenceService;
        _chatService = chatService;
        _usuarioService = usuarioService;
    }
    
    /// <summary>
    /// Inicializa el ViewModel con el usuario actual
    /// </summary>
    public async Task InitializeAsync()
    {
        if (App.CurrentUser == null)
            return;
        
        IsLoading = true;
        LoadingMessage = "Iniciando servicios de chat...";
        
        try
        {
            // Configurar servicios con usuario actual
            _chatService.SetCurrentUser(App.CurrentUser);
            
            // Iniciar servicio de presencia
            await _presenceService.StartAsync(App.CurrentUser);
            
            // Suscribirse a eventos
            _chatService.NewMessageReceived += OnNewMessageReceived;
            _chatService.UnreadCountChanged += OnUnreadCountChanged;
            
            // Cargar datos iniciales
            await LoadUsersAsync();
            await LoadUnreadCountAsync();
            
            // Iniciar escucha de mensajes global
            _globalMessageListener = _chatService.StartListening();
            
            // Iniciar escucha de usuarios online
            _onlineUsersListener = _presenceService.ListenToOnlineUsers(OnOnlineUsersChanged);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al inicializar ChatViewModel: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    /// <summary>
    /// Carga la lista de usuarios disponibles
    /// </summary>
    private async Task LoadUsersAsync()
    {
        try
        {
            var currentUserId = App.CurrentUser?.Id ?? 0;
            var currentFirebaseUid = App.CurrentUser?.FirebaseUid ?? string.Empty;
            
            System.Diagnostics.Debug.WriteLine($"LoadUsersAsync - CurrentUser: Id={currentUserId}, FirebaseUid={currentFirebaseUid}");
            
            // Cargar todos los usuarios activos
            var usuarios = await _usuarioService.GetAllActiveAsync();
            System.Diagnostics.Debug.WriteLine($"LoadUsersAsync - Usuarios cargados: {usuarios.Count()}");
            
            // Cargar estado de presencia
            var onlineUsers = await _presenceService.GetOnlineUsersAsync();
            System.Diagnostics.Debug.WriteLine($"LoadUsersAsync - Usuarios online: {onlineUsers.Count()}");
            foreach (var ou in onlineUsers)
            {
                System.Diagnostics.Debug.WriteLine($"  - Online: {ou.Username} (Id={ou.UserId}, FirebaseUid={ou.FirebaseUid})");
            }
            
            // Crear sets para búsqueda rápida - solo incluir IDs válidos (> 0)
            var onlineUserIds = onlineUsers
                .Where(u => u.UserId > 0)
                .Select(u => u.UserId)
                .ToHashSet();
            var onlineFirebaseUids = onlineUsers
                .Where(u => !string.IsNullOrEmpty(u.FirebaseUid))
                .Select(u => u.FirebaseUid!)
                .ToHashSet();
            
            // Obtener conversaciones recientes para saber mensajes no leídos
            var conversations = await _chatService.GetRecentConversationsAsync();
            var unreadByUser = conversations.ToDictionary(c => c.OtherUserId, c => c.UnreadCount);
            
            AllUsers.Clear();
            OnlineUsers.Clear();
            
            // Filtrar el usuario actual por FirebaseUid (más confiable que Id)
            foreach (var usuario in usuarios)
            {
                // Excluir usuario actual - priorizar FirebaseUid ya que Id puede ser 0 en Firebase
                bool isCurrentUser = false;
                
                // Comparar por FirebaseUid si está disponible (más confiable)
                if (!string.IsNullOrEmpty(currentFirebaseUid) && !string.IsNullOrEmpty(usuario.FirebaseUid))
                {
                    isCurrentUser = usuario.FirebaseUid == currentFirebaseUid;
                }
                // Fallback a Id solo si ambos son > 0
                else if (currentUserId > 0 && usuario.Id > 0)
                {
                    isCurrentUser = usuario.Id == currentUserId;
                }
                
                if (isCurrentUser)
                {
                    System.Diagnostics.Debug.WriteLine($"  - Excluyendo usuario actual: {usuario.Username}");
                    continue;
                }
                
                // Verificar si está online - priorizar FirebaseUid sobre Id
                bool isOnline = false;
                UserPresence? presence = null;
                
                // Buscar por FirebaseUid primero (más confiable)
                if (!string.IsNullOrEmpty(usuario.FirebaseUid))
                {
                    isOnline = onlineFirebaseUids.Contains(usuario.FirebaseUid);
                    presence = onlineUsers.FirstOrDefault(p => 
                        !string.IsNullOrEmpty(p.FirebaseUid) && p.FirebaseUid == usuario.FirebaseUid);
                }
                
                // Fallback a Id si no se encontró por FirebaseUid y el Id es válido
                if (!isOnline && usuario.Id > 0)
                {
                    isOnline = onlineUserIds.Contains(usuario.Id);
                    presence ??= onlineUsers.FirstOrDefault(p => p.UserId == usuario.Id);
                }
                
                var unreadCount = unreadByUser.TryGetValue(usuario.Id, out var count) ? count : 0;
                
                var vm = new UserPresenceViewModel
                {
                    UserId = usuario.Id,
                    FirebaseUid = usuario.FirebaseUid,
                    Username = usuario.Username,
                    NombreCompleto = usuario.NombreCompleto,
                    IsOnline = isOnline,
                    Status = presence?.Status ?? "Desconectado",
                    StatusMessage = presence?.StatusMessage,
                    UnreadCount = unreadCount
                };
                
                AllUsers.Add(vm);
                System.Diagnostics.Debug.WriteLine($"  - Agregado usuario: {usuario.Username} (Online: {isOnline})");
                
                if (isOnline)
                {
                    OnlineUsers.Add(vm);
                }
            }
            
            System.Diagnostics.Debug.WriteLine($"LoadUsersAsync - Total en AllUsers: {AllUsers.Count}, OnlineUsers: {OnlineUsers.Count}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al cargar usuarios: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
        }
    }
    
    private async Task LoadUnreadCountAsync()
    {
        TotalUnreadCount = await _chatService.GetUnreadCountAsync();
    }
    
    partial void OnSelectedUserChanged(UserPresenceViewModel? value)
    {
        if (value != null)
        {
            _ = LoadConversationAsync(value);
        }
        else
        {
            HasActiveConversation = false;
            ChatPartnerName = string.Empty;
            Messages.Clear();
            _conversationListener?.Dispose();
            _conversationListener = null;
        }
    }
    
    partial void OnSearchFilterChanged(string value)
    {
        FilterUsers();
    }
    
    partial void OnShowOnlyOnlineChanged(bool value)
    {
        FilterUsers();
    }
    
    private void FilterUsers()
    {
        // La UI debería filtrar basándose en SearchFilter y ShowOnlyOnline
        // Esta implementación básica recarga la lista filtrada
    }
    
    /// <summary>
    /// Carga la conversación con un usuario
    /// </summary>
    private async Task LoadConversationAsync(UserPresenceViewModel user)
    {
        IsLoading = true;
        LoadingMessage = "Cargando conversación...";
        
        try
        {
            HasActiveConversation = true;
            ChatPartnerName = user.NombreCompleto;
            IsChatPartnerOnline = user.IsOnline;
            
            // Detener listener anterior
            _conversationListener?.Dispose();
            
            // Cargar mensajes
            var messages = await _chatService.GetConversationAsync(user.UserId);
            
            Messages.Clear();
            foreach (var msg in messages)
            {
                Messages.Add(CreateMessageViewModel(msg));
            }
            
            // Marcar como leídos
            await _chatService.MarkConversationAsReadAsync(user.UserId);
            user.UnreadCount = 0;
            
            // Iniciar escucha de nuevos mensajes en esta conversación
            var lastMessageDate = messages.Any() 
                ? messages.Max(m => m.SentAt) 
                : DateTime.UtcNow.AddSeconds(-5); // Buffer de seguridad
                
            _conversationListener = _chatService.ListenToConversation(user.UserId, lastMessageDate, OnConversationNewMessage);
            
            // Scroll al final
            ScrollToBottomRequested?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al cargar conversación: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    private ChatMessageViewModel CreateMessageViewModel(ChatMessage msg)
    {
        var currentUserId = App.CurrentUser?.Id ?? 0;
        return new ChatMessageViewModel
        {
            Id = msg.Id,
            Content = msg.Content,
            SentAt = msg.SentAt,
            SenderName = msg.SenderName,
            IsMine = IsMessageMine(msg),
            IsRead = msg.IsRead
        };
    }
    
    [RelayCommand]
    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(MessageText) || SelectedUser == null)
            return;
        
        var messageContent = MessageText.Trim();
        MessageText = string.Empty;
        
        try
        {
            var sentMessage = await _chatService.SendMessageAsync(
                SelectedUser.UserId,
                SelectedUser.NombreCompleto,
                messageContent);
            
            // Agregar mensaje a la UI
            Messages.Add(CreateMessageViewModel(sentMessage));
            
            // Scroll al final
            ScrollToBottomRequested?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            MessageText = messageContent; // Restaurar el mensaje si falla
            System.Diagnostics.Debug.WriteLine($"Error al enviar mensaje: {ex.Message}");
            MessageBox.Show("Error al enviar el mensaje. Intente nuevamente.", 
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    [RelayCommand]
    private void SelectUser(UserPresenceViewModel user)
    {
        SelectedUser = user;
    }
    
    [RelayCommand]
    private async Task RefreshUsersAsync()
    {
        await LoadUsersAsync();
    }
    
    [RelayCommand]
    private async Task UpdateStatusAsync(string status)
    {
        await _presenceService.UpdateStatusAsync(status);
    }
    
    private void OnNewMessageReceived(object? sender, ChatMessage message)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            // Actualizar contador de no leídos del usuario
            var user = AllUsers.FirstOrDefault(u => u.UserId == message.SenderId);
            if (user != null)
            {
                // Si no estamos en esa conversación, incrementar contador
                if (SelectedUser?.UserId != message.SenderId)
                {
                    user.UnreadCount++;
                }
            }
            
            // Notificar para mostrar toast/notificación
            NewMessageNotification?.Invoke(this, message);
        });
    }
    
    private void OnConversationNewMessage(ChatMessage message)
    {
        Application.Current.Dispatcher.Invoke(async () =>
        {
            // Si es un mensaje nuevo en la conversación actual (no enviado por mí)
            if (!IsMessageMine(message))
            {
                // Verificar que no esté ya en la lista
                if (!Messages.Any(m => m.Id == message.Id))
                {
                    Messages.Add(CreateMessageViewModel(message));
                    ScrollToBottomRequested?.Invoke(this, EventArgs.Empty);
                    
                    // OPTIMIZACIÓN: Marcar como leído con debounce para evitar escrituras excesivas
                    if (SelectedUser != null)
                    {
                        var timeSinceLastMark = (DateTime.Now - _lastMarkAsReadTime).TotalMilliseconds;
                        if (timeSinceLastMark > MARK_READ_DEBOUNCE_MS)
                        {
                            _lastMarkAsReadTime = DateTime.Now;
                            await _chatService.MarkConversationAsReadAsync(SelectedUser.UserId);
                        }
                    }
                }
            }
        });
    }
    
    private void OnUnreadCountChanged(object? sender, int count)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            TotalUnreadCount = count;
        });
    }
    
    private void OnOnlineUsersChanged(IEnumerable<UserPresence> onlineUsers)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            // Usar FirebaseUid como identificador principal (más confiable para múltiples sesiones)
            var onlineFirebaseUids = onlineUsers
                .Where(u => !string.IsNullOrEmpty(u.FirebaseUid))
                .Select(u => u.FirebaseUid!)
                .ToHashSet();
            // Solo incluir UserIds válidos (> 0) para evitar falsos positivos
            var onlineUserIds = onlineUsers
                .Where(u => u.UserId > 0)
                .Select(u => u.UserId)
                .ToHashSet();
            
            System.Diagnostics.Debug.WriteLine($"OnOnlineUsersChanged - Usuarios online: {onlineFirebaseUids.Count}");
            foreach (var uid in onlineFirebaseUids)
            {
                System.Diagnostics.Debug.WriteLine($"  - FirebaseUid online: {uid}");
            }
            
            OnlineUsers.Clear();
            
            foreach (var user in AllUsers)
            {
                // Verificar por FirebaseUid primero (más confiable), luego por UserId solo si es > 0
                bool isNowOnline = false;
                UserPresence? presence = null;
                
                if (!string.IsNullOrEmpty(user.FirebaseUid))
                {
                    isNowOnline = onlineFirebaseUids.Contains(user.FirebaseUid);
                    presence = onlineUsers.FirstOrDefault(p => 
                        !string.IsNullOrEmpty(p.FirebaseUid) && p.FirebaseUid == user.FirebaseUid);
                }
                
                // Fallback a UserId solo si no se encontró por FirebaseUid y el Id es válido
                if (!isNowOnline && user.UserId > 0)
                {
                    isNowOnline = onlineUserIds.Contains(user.UserId);
                    presence ??= onlineUsers.FirstOrDefault(p => p.UserId == user.UserId);
                }
                
                user.IsOnline = isNowOnline;
                    
                if (presence != null)
                {
                    user.Status = presence.Status;
                    user.StatusMessage = presence.StatusMessage;
                }
                else
                {
                    user.Status = "Desconectado";
                }
                
                if (isNowOnline)
                {
                    OnlineUsers.Add(user);
                    System.Diagnostics.Debug.WriteLine($"  - Usuario {user.Username} está online");
                }
            }
            
            // Actualizar estado del chat partner
            if (SelectedUser != null)
            {
                bool partnerOnline = false;
                if (!string.IsNullOrEmpty(SelectedUser.FirebaseUid))
                {
                    partnerOnline = onlineFirebaseUids.Contains(SelectedUser.FirebaseUid);
                }
                if (!partnerOnline && SelectedUser.UserId > 0)
                {
                    partnerOnline = onlineUserIds.Contains(SelectedUser.UserId);
                }
                IsChatPartnerOnline = partnerOnline;
            }
            
            System.Diagnostics.Debug.WriteLine($"OnOnlineUsersChanged - Total AllUsers: {AllUsers.Count}, OnlineUsers: {OnlineUsers.Count}");
        });
    }
    
    /// <summary>
    /// Limpia recursos al cerrar
    /// </summary>
    public async Task CleanupAsync()
    {
        try
        {
            _chatService.NewMessageReceived -= OnNewMessageReceived;
            _chatService.UnreadCountChanged -= OnUnreadCountChanged;
            
            _onlineUsersListener?.Dispose();
            _conversationListener?.Dispose();
            _globalMessageListener?.Dispose();
            
            await _presenceService.StopAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en cleanup: {ex.Message}");
        }
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
            _onlineUsersListener?.Dispose();
            _conversationListener?.Dispose();
            _globalMessageListener?.Dispose();
        }
        
        _disposed = true;
    }
    private bool IsMessageMine(ChatMessage msg)
    {
        var currentUser = App.CurrentUser;
        if (currentUser == null) return false;

        // Priorizar FirebaseUid si está disponible en ambos
        if (!string.IsNullOrEmpty(currentUser.FirebaseUid) && !string.IsNullOrEmpty(msg.SenderFirebaseUid))
        {
            return currentUser.FirebaseUid == msg.SenderFirebaseUid;
        }

        // Fallback a ID numérico (solo si son válidos > 0)
        if (currentUser.Id > 0 && msg.SenderId > 0)
        {
            return currentUser.Id == msg.SenderId;
        }

        // Si no hay forma confiable de comparar, asumir falso para evitar mostrar como propio
        return false;
    }
}

/// <summary>
/// ViewModel para representar un usuario en la lista de presencia
/// </summary>
public partial class UserPresenceViewModel : ObservableObject
{
    [ObservableProperty]
    private int _userId;
    
    [ObservableProperty]
    private string? _firebaseUid;
    
    [ObservableProperty]
    private string _username = string.Empty;
    
    [ObservableProperty]
    private string _nombreCompleto = string.Empty;
    
    [ObservableProperty]
    private bool _isOnline;
    
    [ObservableProperty]
    private string _status = "Desconectado";
    
    [ObservableProperty]
    private string? _statusMessage;
    
    [ObservableProperty]
    private int _unreadCount;
    
    /// <summary>
    /// Indica si hay mensajes sin leer
    /// </summary>
    public bool HasUnread => UnreadCount > 0;
    
    partial void OnUnreadCountChanged(int value)
    {
        OnPropertyChanged(nameof(HasUnread));
    }
}

/// <summary>
/// ViewModel para representar un mensaje en el chat
/// </summary>
public partial class ChatMessageViewModel : ObservableObject
{
    [ObservableProperty]
    private int _id;
    
    [ObservableProperty]
    private string _content = string.Empty;
    
    [ObservableProperty]
    private DateTime _sentAt;
    
    [ObservableProperty]
    private string _senderName = string.Empty;
    
    [ObservableProperty]
    private bool _isMine;
    
    [ObservableProperty]
    private bool _isRead;
    
    /// <summary>
    /// Hora formateada para mostrar
    /// </summary>
    public string FormattedTime => SentAt.ToString("HH:mm");
    
    /// <summary>
    /// Fecha formateada para mostrar (si no es hoy)
    /// </summary>
    public string FormattedDate
    {
        get
        {
            if (SentAt.Date == DateTime.Today)
                return "Hoy";
            if (SentAt.Date == DateTime.Today.AddDays(-1))
                return "Ayer";
            return SentAt.ToString("dd/MM/yyyy");
        }
    }
}
