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
    private readonly IUsuarioService _usuarioService;
    private List<Usuario> _allUsers = new();
    private List<Usuario> _filteredUsers = new();

    public Usuario? SelectedUser { get; private set; }

    public NewConversationDialog(IUsuarioService usuarioService)
    {
        InitializeComponent();
        _usuarioService = usuarioService;
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
            // Obtener todos los usuarios activos excepto el actual
            var allUsers = await _usuarioService.GetAllActiveAsync();

            _allUsers = allUsers
                .Where(u => u.Id != App.CurrentUser?.Id)
                .OrderBy(u => u.NombreCompleto)
                .ToList();

            _filteredUsers = new List<Usuario>(_allUsers);
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
            _filteredUsers = new List<Usuario>(_allUsers);
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
        if (sender is Border border && border.Tag is Usuario user)
        {
            SelectedUser = user;
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
