using SGRRHH.WPF.ViewModels;
using System.Windows.Controls;

namespace SGRRHH.WPF.Views;

public partial class ChatView : UserControl
{
    private readonly ChatViewModel _viewModel;
    
    public ChatView(ChatViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        _viewModel = viewModel;
        
        // Suscribirse al evento de scroll al final
        _viewModel.ScrollToBottomRequested += OnScrollToBottomRequested;
        
        // Inicializar cuando se cargue el control
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }
    
    private async void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        await _viewModel.InitializeAsync();
    }
    
    private async void OnUnloaded(object sender, System.Windows.RoutedEventArgs e)
    {
        _viewModel.ScrollToBottomRequested -= OnScrollToBottomRequested;
        await _viewModel.CleanupAsync();
    }
    
    private void OnScrollToBottomRequested(object? sender, System.EventArgs e)
    {
        // Scroll al final de los mensajes
        Dispatcher.BeginInvoke(new System.Action(() =>
        {
            MessagesScrollViewer.ScrollToEnd();
        }), System.Windows.Threading.DispatcherPriority.Background);
    }
}
