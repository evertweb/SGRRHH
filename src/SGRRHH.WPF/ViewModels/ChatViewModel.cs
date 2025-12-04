using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;
using System.Collections.ObjectModel;
using System.IO;
using System.Timers;
using System.Windows;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// ViewModel para el chat moderno usando Sendbird
/// </summary>
public partial class ChatViewModel : ViewModelBase, IDisposable
{
    private readonly ISendbirdChatService _sendbirdService;
    private readonly IUsuarioService _usuarioService;
    private readonly IDialogService _dialogService;
    private readonly System.Timers.Timer _pollingTimer;
    private readonly System.Timers.Timer _channelsRefreshTimer;
    private bool _disposed;
    private bool _isPolling = false;

    #region Properties

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _loadingMessage = "Conectando...";

    /// <summary>
    /// Lista de canales (conversaciones)
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ChannelViewModel> _channels = new();

    /// <summary>
    /// Canal seleccionado
    /// </summary>
    [ObservableProperty]
    private ChannelViewModel? _selectedChannel;

    /// <summary>
    /// Mensajes del canal actual
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<SendbirdMessage> _messages = new();

    /// <summary>
    /// Texto del mensaje a enviar
    /// </summary>
    [ObservableProperty]
    private string _messageText = string.Empty;

    /// <summary>
    /// Filtro de búsqueda
    /// </summary>
    [ObservableProperty]
    private string _searchFilter = string.Empty;

    /// <summary>
    /// Indica si hay un canal activo
    /// </summary>
    [ObservableProperty]
    private bool _hasActiveChannel;

    /// <summary>
    /// Nombre del canal seleccionado
    /// </summary>
    [ObservableProperty]
    private string _selectedChannelName = string.Empty;

    /// <summary>
    /// Iniciales del otro usuario
    /// </summary>
    [ObservableProperty]
    private string _selectedChannelInitials = string.Empty;

    /// <summary>
    /// Indica si el otro usuario está online
    /// </summary>
    [ObservableProperty]
    private bool _isOtherUserOnline;

    /// <summary>
    /// Estado del otro usuario
    /// </summary>
    [ObservableProperty]
    private string _otherUserStatus = "Desconectado";

    /// <summary>
    /// Total de mensajes no leídos
    /// </summary>
    [ObservableProperty]
    private int _totalUnreadCount;

    #endregion

    /// <summary>
    /// Evento para solicitar scroll al final
    /// </summary>
    public event EventHandler? ScrollToBottomRequested;

    public ChatViewModel(
        ISendbirdChatService sendbirdService,
        IUsuarioService usuarioService,
        IDialogService dialogService)
    {
        _sendbirdService = sendbirdService;
        _usuarioService = usuarioService;
        _dialogService = dialogService;

        // Suscribirse a eventos
        _sendbirdService.MessageReceived += OnMessageReceived;
        _sendbirdService.UnreadCountChanged += OnUnreadCountChanged;

        // Configurar timer de polling para mensajes (cada 3 segundos)
        _pollingTimer = new System.Timers.Timer(3000);
        _pollingTimer.Elapsed += OnPollingTimerElapsed;
        _pollingTimer.AutoReset = true;

        // Configurar timer para refrescar lista de canales (cada 10 segundos)
        _channelsRefreshTimer = new System.Timers.Timer(10000);
        _channelsRefreshTimer.Elapsed += OnChannelsRefreshTimerElapsed;
        _channelsRefreshTimer.AutoReset = true;
    }

    /// <summary>
    /// Inicializa el ViewModel
    /// </summary>
    public async Task InitializeAsync()
    {
        if (App.CurrentUser == null)
            return;

        IsLoading = true;
        LoadingMessage = "Conectando a Sendbird...";

        try
        {
            // Conectar usuario actual a Sendbird
            var connected = await _sendbirdService.ConnectAsync(App.CurrentUser);

            if (!connected)
            {
                _dialogService.ShowError(
                    "No se pudo conectar al servidor de chat. Por favor, verifica tu conexión.",
                    "Error de conexión");
                return;
            }

            // Sincronizar usuarios de Firebase a Sendbird (una sola vez)
            LoadingMessage = "Sincronizando usuarios...";
            try
            {
                var allUsers = await _usuarioService.GetAllActiveAsync();
                await _sendbirdService.SyncAllUsersAsync(allUsers);
            }
            catch (Exception ex)
            {
                // No bloquear la app si falla la sincronización
                System.Diagnostics.Debug.WriteLine($"Sincronización de usuarios (no crítico): {ex.Message}");
            }

            LoadingMessage = "Cargando conversaciones...";

            // Cargar canales existentes
            await LoadChannelsAsync();

            // Actualizar contador de no leídos
            await UpdateUnreadCountAsync();

            // Iniciar timers de polling
            _pollingTimer.Start();
            _channelsRefreshTimer.Start();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al inicializar ChatViewModel: {ex.Message}");
            _dialogService.ShowError("Error al inicializar el chat. Intente nuevamente.", "Error");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Carga la lista de canales
    /// </summary>
    private async Task LoadChannelsAsync()
    {
        try
        {
            var channels = await _sendbirdService.GetMyChannelsAsync();

            Channels.Clear();

            foreach (var channel in channels)
            {
                var vm = new ChannelViewModel
                {
                    Url = channel.Url,
                    Name = GetChannelDisplayName(channel),
                    LastMessagePreview = channel.LastMessage?.Message ?? "Sin mensajes",
                    UnreadCount = channel.UnreadMessageCount,
                    HasUnread = channel.UnreadMessageCount > 0,
                    OtherUserInitials = GetOtherUserInitials(channel),
                    IsOtherUserOnline = GetIsOtherUserOnline(channel)
                };

                Channels.Add(vm);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al cargar canales: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene el nombre a mostrar del canal (nombre del otro usuario)
    /// </summary>
    private string GetChannelDisplayName(SendbirdChannel channel)
    {
        // En un canal directo, mostrar el nombre del otro usuario
        var currentUserId = GetCurrentUserId();
        var otherMember = channel.Members.FirstOrDefault(m => m.UserId != currentUserId);
        return otherMember?.Nickname ?? channel.Name;
    }

    /// <summary>
    /// Obtiene las iniciales del otro usuario
    /// </summary>
    private string GetOtherUserInitials(SendbirdChannel channel)
    {
        var name = GetChannelDisplayName(channel);
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length >= 2)
            return $"{parts[0][0]}{parts[1][0]}".ToUpper();

        if (parts.Length == 1 && parts[0].Length >= 2)
            return parts[0].Substring(0, 2).ToUpper();

        return name.Length >= 2 ? name.Substring(0, 2).ToUpper() : name.ToUpper();
    }

    /// <summary>
    /// Verifica si el otro usuario está online
    /// </summary>
    private bool GetIsOtherUserOnline(SendbirdChannel channel)
    {
        var currentUserId = GetCurrentUserId();
        var otherMember = channel.Members.FirstOrDefault(m => m.UserId != currentUserId);
        return otherMember?.IsOnline ?? false;
    }

    /// <summary>
    /// Obtiene el FirebaseUid del usuario actual (ID usado en Sendbird)
    /// </summary>
    private string GetCurrentUserId()
    {
        return App.CurrentUser?.FirebaseUid ?? string.Empty;
    }

    /// <summary>
    /// Actualiza el contador de no leídos
    /// </summary>
    private async Task UpdateUnreadCountAsync()
    {
        TotalUnreadCount = await _sendbirdService.GetTotalUnreadCountAsync();
    }

    /// <summary>
    /// Evento del timer de polling - verifica nuevos mensajes en el canal actual
    /// </summary>
    private async void OnPollingTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        if (_isPolling || SelectedChannel == null)
            return;

        try
        {
            _isPolling = true;

            // Obtener mensajes actualizados del canal
            var messages = await _sendbirdService.GetMessagesAsync(SelectedChannel.Url);
            var newMessages = messages.OrderBy(m => m.CreatedAt).ToList();

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                // Verificar si hay mensajes nuevos
                foreach (var msg in newMessages)
                {
                    if (!Messages.Any(m => m.MessageId == msg.MessageId))
                    {
                        // Insertar en orden correcto
                        var insertIndex = Messages.Count;
                        for (int i = 0; i < Messages.Count; i++)
                        {
                            if (Messages[i].CreatedAt > msg.CreatedAt)
                            {
                                insertIndex = i;
                                break;
                            }
                        }

                        if (insertIndex == Messages.Count)
                            Messages.Add(msg);
                        else
                            Messages.Insert(insertIndex, msg);

                        // Si es un mensaje nuevo de otra persona, hacer scroll
                        if (!msg.IsMyMessage && insertIndex == Messages.Count - 1)
                        {
                            ScrollToBottomRequested?.Invoke(this, EventArgs.Empty);
                        }
                    }
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en polling: {ex.Message}");
        }
        finally
        {
            _isPolling = false;
        }
    }

    /// <summary>
    /// Evento del timer de refresco - actualiza la lista de canales
    /// </summary>
    private async void OnChannelsRefreshTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        try
        {
            var channels = await _sendbirdService.GetMyChannelsAsync();

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                foreach (var channel in channels)
                {
                    var existingChannel = Channels.FirstOrDefault(c => c.Url == channel.Url);
                    if (existingChannel != null)
                    {
                        // Actualizar datos del canal existente
                        existingChannel.UnreadCount = channel.UnreadMessageCount;
                        existingChannel.HasUnread = channel.UnreadMessageCount > 0;

                        if (channel.LastMessage != null)
                        {
                            existingChannel.LastMessagePreview = channel.LastMessage.Message;
                        }
                    }
                }

                // Actualizar contador total de no leídos
                _ = UpdateUnreadCountAsync();
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al refrescar canales: {ex.Message}");
        }
    }

    partial void OnSelectedChannelChanged(ChannelViewModel? value)
    {
        if (value != null)
        {
            _ = LoadChannelMessagesAsync(value);
        }
        else
        {
            HasActiveChannel = false;
            Messages.Clear();
        }
    }

    /// <summary>
    /// Carga los mensajes de un canal
    /// </summary>
    private async Task LoadChannelMessagesAsync(ChannelViewModel channel)
    {
        IsLoading = true;
        LoadingMessage = "Cargando mensajes...";

        try
        {
            HasActiveChannel = true;
            SelectedChannelName = channel.Name;
            SelectedChannelInitials = channel.OtherUserInitials;
            IsOtherUserOnline = channel.IsOtherUserOnline;
            OtherUserStatus = channel.IsOtherUserOnline ? "En línea" : "Desconectado";

            // Cargar mensajes
            var messages = await _sendbirdService.GetMessagesAsync(channel.Url);

            Messages.Clear();
            foreach (var msg in messages.OrderBy(m => m.CreatedAt))
            {
                Messages.Add(msg);
            }

            // Marcar como leído
            await _sendbirdService.MarkChannelAsReadAsync(channel.Url);
            channel.UnreadCount = 0;
            channel.HasUnread = false;

            // Scroll al final
            ScrollToBottomRequested?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al cargar mensajes del canal: {ex.Message}");
            _dialogService.ShowError("Error al cargar los mensajes. Intente nuevamente.", "Error");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(MessageText) || SelectedChannel == null)
            return;

        var messageContent = MessageText.Trim();
        MessageText = string.Empty;

        try
        {
            var sentMessage = await _sendbirdService.SendMessageAsync(SelectedChannel.Url, messageContent);

            if (sentMessage != null)
            {
                // Agregar mensaje a la UI
                Messages.Add(sentMessage);

                // Scroll al final
                ScrollToBottomRequested?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                _dialogService.ShowError("No se pudo enviar el mensaje. Intente nuevamente.", "Error");
                MessageText = messageContent; // Restaurar el mensaje
            }
        }
        catch (Exception ex)
        {
            MessageText = messageContent; // Restaurar el mensaje si falla
            System.Diagnostics.Debug.WriteLine($"Error al enviar mensaje: {ex.Message}");
            _dialogService.ShowError("Error al enviar el mensaje. Intente nuevamente.", "Error");
        }
    }

    [RelayCommand]
    private void SelectChannel(ChannelViewModel channel)
    {
        SelectedChannel = channel;
    }

    [RelayCommand]
    private async Task NewConversationAsync()
    {
        var logPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "sendbird_debug.log");
        void Log(string msg) {
            var line = $"[{DateTime.Now:HH:mm:ss}] {msg}";
            System.IO.File.AppendAllText(logPath, line + Environment.NewLine);
        }
        
        try
        {
            Log("=== NewConversationAsync INICIO ===");
            
            // Diálogo de selección de usuario
            var dialog = new Views.NewConversationDialog(_sendbirdService, _usuarioService)
            {
                Owner = Application.Current.MainWindow
            };

            var dialogResult = dialog.ShowDialog();
            Log($"DialogResult: {dialogResult}");
            
            if (dialogResult != true)
            {
                Log("Dialog cancelled - saliendo");
                return;
            }

            // Obtener datos del usuario seleccionado
            // IMPORTANTE: El UserId es siempre el FirebaseUid
            string otherUserId;
            string otherUserName;

            Log($"dialog.SelectedUser: {(dialog.SelectedUser != null ? dialog.SelectedUser.NombreCompleto : "NULL")}");
            Log($"dialog.SelectedUser?.FirebaseUid: {dialog.SelectedUser?.FirebaseUid ?? "NULL"}");
            Log($"dialog.SelectedSendbirdUser: {(dialog.SelectedSendbirdUser != null ? dialog.SelectedSendbirdUser.UserId : "NULL")}");

            if (dialog.SelectedUser != null && !string.IsNullOrEmpty(dialog.SelectedUser.FirebaseUid))
            {
                otherUserId = dialog.SelectedUser.FirebaseUid;
                otherUserName = dialog.SelectedUser.NombreCompleto;
                Log($"Usando SelectedUser - otherUserId: {otherUserId}");
            }
            else if (dialog.SelectedSendbirdUser != null)
            {
                // El UserId de SendbirdUser ya es el FirebaseUid
                otherUserId = dialog.SelectedSendbirdUser.UserId;
                otherUserName = dialog.SelectedSendbirdUser.Nickname;
                Log($"Usando SelectedSendbirdUser - otherUserId: {otherUserId}");
            }
            else
            {
                Log("ERROR: No hay usuario válido seleccionado - saliendo!");
                return; // No se seleccionó usuario válido
            }

            IsLoading = true;
            LoadingMessage = "Creando conversación...";

            Log($"Llamando CreateOrGetDirectChannelAsync({otherUserId}, {otherUserName})");

            // Crear o obtener canal directo
            var channel = await _sendbirdService.CreateOrGetDirectChannelAsync(
                otherUserId,
                otherUserName);

            Log($"Resultado canal: {(channel != null ? channel.Url : "NULL")}");

            if (channel != null)
            {
                Log("Canal creado exitosamente, recargando lista de canales...");
                
                // Recargar lista de canales
                await LoadChannelsAsync();
                
                Log($"Canales cargados: {Channels.Count}");
                foreach (var ch in Channels)
                {
                    Log($"  - Canal: {ch.Url} | Nombre: {ch.Name}");
                }

                // Seleccionar el nuevo canal
                var channelVm = Channels.FirstOrDefault(c => c.Url == channel.Url);
                Log($"Buscando canal con URL: {channel.Url}");
                Log($"Canal encontrado: {(channelVm != null ? "SÍ" : "NO")}");
                
                if (channelVm != null)
                {
                    Log("Seleccionando canal...");
                    SelectedChannel = channelVm;
                    Log($"SelectedChannel ahora es: {SelectedChannel?.Name}");
                }
                else
                {
                    Log("ERROR: No se encontró el canal en la lista!");
                }
            }
            else
            {
                // Mostrar más información del error
                var errorMsg = $"No se pudo crear la conversación.\n\n" +
                               $"Usuario destino: {otherUserName}\n" +
                               $"ID: {otherUserId}\n\n" +
                               $"Revisa la ventana Output/Debug para más detalles.";
                _dialogService.ShowError(errorMsg, "Error");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al crear conversación: {ex.Message}");
            _dialogService.ShowError("Error al crear la conversación. Intente nuevamente.", "Error");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task AttachFileAsync()
    {
        if (SelectedChannel == null)
            return;

        try
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Seleccionar archivo",
                Filter = "Todos los archivos (*.*)|*.*|" +
                         "Imágenes (*.jpg;*.png;*.gif)|*.jpg;*.png;*.gif|" +
                         "Documentos (*.pdf;*.docx;*.xlsx)|*.pdf;*.docx;*.xlsx",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                IsLoading = true;
                LoadingMessage = "Enviando archivo...";

                var sentMessage = await _sendbirdService.SendFileAsync(
                    SelectedChannel.Url,
                    openFileDialog.FileName);

                if (sentMessage != null)
                {
                    Messages.Add(sentMessage);
                    ScrollToBottomRequested?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    _dialogService.ShowError("No se pudo enviar el archivo. Verifique el tamaño (máx 25 MB).", "Error");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al adjuntar archivo: {ex.Message}");
            _dialogService.ShowError("Error al enviar el archivo. Intente nuevamente.", "Error");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DownloadFileAsync(SendbirdMessage message)
    {
        if (string.IsNullOrEmpty(message.FileUrl) || string.IsNullOrEmpty(message.FileName))
            return;

        try
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = message.FileName,
                DefaultExt = Path.GetExtension(message.FileName)
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                IsLoading = true;
                LoadingMessage = "Descargando archivo...";

                using var client = new System.Net.Http.HttpClient();
                var bytes = await client.GetByteArrayAsync(message.FileUrl);
                await File.WriteAllBytesAsync(saveFileDialog.FileName, bytes);

                _dialogService.ShowSuccess("Archivo descargado correctamente", "Descarga completa");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al descargar archivo: {ex.Message}");
            _dialogService.ShowError("Error al descargar el archivo. Intente nuevamente.", "Error");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OnMessageReceived(object? sender, SendbirdMessage message)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            // Si es un mensaje del canal actual, agregarlo
            if (SelectedChannel != null && message.ChannelUrl == SelectedChannel.Url)
            {
                // Verificar que no esté ya en la lista
                if (!Messages.Any(m => m.MessageId == message.MessageId))
                {
                    Messages.Add(message);
                    ScrollToBottomRequested?.Invoke(this, EventArgs.Empty);

                    // Marcar como leído si no es mío
                    if (!message.IsMyMessage)
                    {
                        _ = _sendbirdService.MarkChannelAsReadAsync(message.ChannelUrl);
                    }
                }
            }
            else
            {
                // Actualizar contador de no leídos del canal
                var channel = Channels.FirstOrDefault(c => c.Url == message.ChannelUrl);
                if (channel != null)
                {
                    channel.UnreadCount++;
                    channel.HasUnread = true;
                    channel.LastMessagePreview = message.Message;
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

    /// <summary>
    /// Limpia recursos al cerrar
    /// </summary>
    public async Task CleanupAsync()
    {
        try
        {
            // Detener timers
            _pollingTimer?.Stop();
            _channelsRefreshTimer?.Stop();

            _sendbirdService.MessageReceived -= OnMessageReceived;
            _sendbirdService.UnreadCountChanged -= OnUnreadCountChanged;

            await _sendbirdService.DisconnectAsync();
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
            _pollingTimer?.Dispose();
            _channelsRefreshTimer?.Dispose();
            _ = CleanupAsync().ConfigureAwait(false);
        }

        _disposed = true;
    }
}

/// <summary>
/// ViewModel para representar un canal en la lista
/// </summary>
public partial class ChannelViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _url = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _lastMessagePreview = string.Empty;

    [ObservableProperty]
    private int _unreadCount;

    [ObservableProperty]
    private bool _hasUnread;

    [ObservableProperty]
    private string _otherUserInitials = string.Empty;

    [ObservableProperty]
    private bool _isOtherUserOnline;

    [ObservableProperty]
    private bool _isSelected;
}
