using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SGRRHH.WPF.Views;

/// <summary>
/// Diálogo para seleccionar un usuario y crear una nueva conversación
/// </summary>
public partial class NewConversationDialog : Window
{
    private readonly ISendbirdChatService? _sendbirdService;
    private readonly IUsuarioService _usuarioService;
    private List<UserListItem> _allUsers = new();
    private List<UserListItem> _filteredUsers = new();

    public Usuario? SelectedUser { get; private set; }
    public SendbirdUser? SelectedSendbirdUser { get; private set; }

    /// <summary>
    /// Constructor para usar con Sendbird (muestra usuarios online de Sendbird)
    /// </summary>
    public NewConversationDialog(ISendbirdChatService sendbirdService, IUsuarioService usuarioService)
    {
        InitializeComponent();
        _sendbirdService = sendbirdService;
        _usuarioService = usuarioService;
        Loaded += OnLoaded;
    }

    /// <summary>
    /// Constructor legacy para usar solo con servicio de usuarios
    /// </summary>
    public NewConversationDialog(IUsuarioService usuarioService)
    {
        InitializeComponent();
        _usuarioService = usuarioService;
        _sendbirdService = null;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await LoadUsersAsync();
    }

    private async System.Threading.Tasks.Task LoadUsersAsync()
    {
        LoadingOverlay.Visibility = Visibility.Visible;

        try
        {
            _allUsers.Clear();

            // Firebase es la fuente de verdad para usuarios
            var firebaseUsers = await _usuarioService.GetAllActiveAsync();
            
            // Excluir al usuario actual por FirebaseUid (más confiable que Id)
            var currentFirebaseUid = App.CurrentUser?.FirebaseUid;
            var otherUsers = firebaseUsers
                .Where(u => !string.IsNullOrEmpty(u.FirebaseUid) && u.FirebaseUid != currentFirebaseUid)
                .ToList();

            // Obtener estados online de Sendbird
            Dictionary<string, SendbirdUser> sendbirdUserStates = new();
            if (_sendbirdService != null)
            {
                try
                {
                    // Obtener estados de usuarios en Sendbird
                    var sbUsers = await _sendbirdService.GetUsersAsync(onlineOnly: false);
                    foreach (var sbUser in sbUsers)
                    {
                        // El UserId en Sendbird ES el FirebaseUid
                        sendbirdUserStates[sbUser.UserId] = sbUser;
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"Estados obtenidos de Sendbird: {sendbirdUserStates.Count}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al obtener estados de Sendbird: {ex.Message}");
                }
            }

            // Crear lista combinando datos de Firebase con estado de Sendbird
            foreach (var user in otherUsers)
            {
                // IMPORTANTE: El ID de Sendbird es SIEMPRE el FirebaseUid
                var sendbirdUserId = user.FirebaseUid!;
                
                var isOnline = false;
                DateTime? lastSeenAt = null;
                SendbirdUser? sbUser = null;

                // Buscar estado en Sendbird usando FirebaseUid
                if (sendbirdUserStates.TryGetValue(sendbirdUserId, out var foundSbUser))
                {
                    isOnline = foundSbUser.IsOnline;
                    lastSeenAt = foundSbUser.LastSeenAt;
                    sbUser = foundSbUser;
                }

                _allUsers.Add(new UserListItem
                {
                    UserId = sendbirdUserId,  // FirebaseUid
                    NombreCompleto = user.NombreCompleto,
                    Username = user.Username,
                    IsOnline = isOnline,
                    LastSeenAt = lastSeenAt,
                    Usuario = user,
                    SendbirdUser = sbUser
                });
            }

            // Ordenar: online primero, luego por nombre
            _allUsers = _allUsers.OrderByDescending(u => u.IsOnline)
                                 .ThenBy(u => u.NombreCompleto)
                                 .ToList();

            _filteredUsers = new List<UserListItem>(_allUsers);
            UsersListBox.ItemsSource = _filteredUsers;
            
            System.Diagnostics.Debug.WriteLine($"Cargados {_allUsers.Count} usuarios (Firebase + estado Sendbird)");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar usuarios: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
        }
    }

    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var searchText = SearchTextBox.Text.ToLower();

        if (string.IsNullOrWhiteSpace(searchText))
        {
            _filteredUsers = new List<UserListItem>(_allUsers);
        }
        else
        {
            _filteredUsers = _allUsers
                .Where(u =>
                    u.NombreCompleto.ToLower().Contains(searchText) ||
                    u.Username.ToLower().Contains(searchText))
                .ToList();
        }

        UsersListBox.ItemsSource = null;
        UsersListBox.ItemsSource = _filteredUsers;
    }

    private void UserItem_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border border && border.Tag is UserListItem userItem)
        {
            SelectedUser = userItem.Usuario;
            SelectedSendbirdUser = userItem.SendbirdUser;
            
            System.Diagnostics.Debug.WriteLine($"=== Usuario seleccionado ===");
            System.Diagnostics.Debug.WriteLine($"  UserItem.UserId: {userItem.UserId}");
            System.Diagnostics.Debug.WriteLine($"  UserItem.NombreCompleto: {userItem.NombreCompleto}");
            System.Diagnostics.Debug.WriteLine($"  UserItem.IsOnline: {userItem.IsOnline}");
            System.Diagnostics.Debug.WriteLine($"  SelectedUser: {(SelectedUser != null ? SelectedUser.NombreCompleto : "NULL")}");
            System.Diagnostics.Debug.WriteLine($"  SelectedUser.FirebaseUid: {SelectedUser?.FirebaseUid ?? "NULL"}");
            System.Diagnostics.Debug.WriteLine($"  SelectedSendbirdUser: {(SelectedSendbirdUser != null ? SelectedSendbirdUser.UserId : "NULL")}");
            
            DialogResult = true;
            Close();
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}

/// <summary>
/// Clase para representar usuarios en la lista (compatible con ambos modos)
/// </summary>
public class UserListItem
{
    public string UserId { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public bool IsOnline { get; set; }
    public DateTime? LastSeenAt { get; set; }
    
    // Referencias originales
    public Usuario? Usuario { get; set; }
    public SendbirdUser? SendbirdUser { get; set; }

    /// <summary>
    /// Texto del estado para mostrar en la UI
    /// </summary>
    public string StatusText => IsOnline ? "En línea" : GetLastSeenText();

    /// <summary>
    /// Color del indicador de estado
    /// </summary>
    public string StatusColor => IsOnline ? "#43B581" : "#747F8D";

    private string GetLastSeenText()
    {
        if (!LastSeenAt.HasValue)
            return "Desconectado";

        var diff = DateTime.UtcNow - LastSeenAt.Value;
        
        if (diff.TotalMinutes < 5)
            return "Hace un momento";
        if (diff.TotalMinutes < 60)
            return $"Hace {(int)diff.TotalMinutes} min";
        if (diff.TotalHours < 24)
            return $"Hace {(int)diff.TotalHours} h";
        if (diff.TotalDays < 7)
            return $"Hace {(int)diff.TotalDays} días";
        
        return "Desconectado";
    }
}
