using SGRRHH.WPF.ViewModels;
using System.Windows;

namespace SGRRHH.WPF.Views;

/// <summary>
/// Lógica de interacción para CambiarPasswordWindow.xaml
/// </summary>
public partial class CambiarPasswordWindow : Window
{
    private readonly CambiarPasswordViewModel _viewModel;
    
    public CambiarPasswordWindow(CambiarPasswordViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        
        // Suscribirse al evento de éxito para cerrar la ventana
        _viewModel.PasswordChanged += (s, e) =>
        {
            DialogResult = true;
            Close();
        };
    }
    
    private async void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
    {
        // Actualizar el ViewModel con los valores de los PasswordBox
        _viewModel.PasswordActual = PasswordActual.Password;
        _viewModel.PasswordNueva = PasswordNueva.Password;
        _viewModel.PasswordConfirmar = PasswordConfirmar.Password;
        
        // Ejecutar el comando
        await _viewModel.CambiarPasswordCommand.ExecuteAsync(null);
    }
    
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
