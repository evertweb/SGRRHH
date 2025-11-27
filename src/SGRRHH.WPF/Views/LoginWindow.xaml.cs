using System.Windows;
using System.Windows.Input;
using SGRRHH.WPF.ViewModels;

namespace SGRRHH.WPF.Views;

/// <summary>
/// Lógica de interacción para LoginWindow.xaml
/// </summary>
public partial class LoginWindow : Window
{
    private readonly LoginViewModel _viewModel;
    
    public LoginWindow(LoginViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        
        // Suscribirse al evento de login exitoso
        _viewModel.LoginSuccessful += OnLoginSuccessful;
        
        // Poner foco en el campo de usuario
        Loaded += (s, e) => 
        {
            var textBox = FindName("Username") as System.Windows.Controls.TextBox;
            textBox?.Focus();
        };
    }
    
    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 1)
        {
            DragMove();
        }
    }
    
    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel viewModel)
        {
            viewModel.Password = PasswordBox.Password;
        }
    }
    
    private void OnLoginSuccessful(object? sender, EventArgs e)
    {
        DialogResult = true;
        Close();
    }
    
    protected override void OnClosed(EventArgs e)
    {
        _viewModel.LoginSuccessful -= OnLoginSuccessful;
        base.OnClosed(e);
    }
}
