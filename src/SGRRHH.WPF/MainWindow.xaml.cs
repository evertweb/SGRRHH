using System.Windows;
using SGRRHH.WPF.ViewModels;

namespace SGRRHH.WPF;

/// <summary>
/// Ventana principal de la aplicación
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        
        // Suscribirse al evento de logout
        _viewModel.LogoutRequested += OnLogoutRequested;
        
        // Navegar al Dashboard después de que la ventana esté completamente cargada
        Loaded += OnWindowLoaded;
    }
    
    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        // Navegar al Dashboard
        if (_viewModel.SelectedMenuItem != null)
        {
            _viewModel.NavigateToCommand.Execute(_viewModel.SelectedMenuItem);
        }
    }
    
    private void OnLogoutRequested(object? sender, EventArgs e)
    {
        // Notificar a App.xaml.cs que se requiere logout
        DialogResult = false;
        Close();
    }
    
    protected override void OnClosed(EventArgs e)
    {
        _viewModel.LogoutRequested -= OnLogoutRequested;
        Loaded -= OnWindowLoaded;
        
        // Limpiar recursos del ViewModel
        _viewModel.Cleanup();
        
        base.OnClosed(e);
    }
}