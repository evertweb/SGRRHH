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

            if (_sendbirdService != null)
            {
                // Modo Sendbird: obtener usuarios de Sendbird con estado online
                var sendbirdUsers = await _sendbirdService.GetUsersAsync(onlineOnly: false);
                
                foreach (var sbUser in sendbirdUsers)
                {
                    _allUsers.Add(new UserListItem
                    {
                        UserId = sbUser.UserId,
                        NombreCompleto = sbUser.Nickname,
                        Username = sbUser.UserId,
                        IsOnline = sbUser.IsOnline,
                        LastSeenAt = sbUser.LastSeenAt,
                        SendbirdUser = sbUser
                    });
                }
            }
            else
            {
                // Modo legacy: obtener usuarios de la base de datos
                var allUsers = await _usuarioService.GetAllActiveAsync();
                
                foreach (var user in allUsers.Where(u => u.Id != App.CurrentUser?.Id))
                {
                    _allUsers.Add(new UserListItem
                    {
                        UserId = user.FirebaseUid ?? user.Username,
                        NombreCompleto = user.NombreCompleto,
                        Username = user.Username,
                        IsOnline = false,
                        Usuario = user
                    });
                }
            }

            // Ordenar: online primero, luego por nombre
            _allUsers = _allUsers.OrderByDescending(u => u.IsOnline)
                                 .ThenBy(u => u.NombreCompleto)
                                 .ToList();

            _filteredUsers = new List<UserListItem>(_allUsers);
            UsersListBox.ItemsSource = _filteredUsers;
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
