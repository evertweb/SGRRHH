using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using SGRRHH.WPF.ViewModels;

namespace SGRRHH.WPF.Views;

public partial class DocumentsView : UserControl
{
    private readonly DocumentsViewModel _viewModel;
    private bool _dataLoaded;

    public DocumentsView(DocumentsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        _viewModel = viewModel;
        Loaded += DocumentsView_Loaded;
        Unloaded += DocumentsView_Unloaded;
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    private async void DocumentsView_Loaded(object sender, RoutedEventArgs e)
    {
        if (!_dataLoaded)
        {
            _dataLoaded = true;
            await _viewModel.LoadDataAsync();
        }

        await EnsureWebViewAsync();
        UpdatePreview();
    }

    private void DocumentsView_Unloaded(object? sender, RoutedEventArgs e)
    {
        _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DocumentsViewModel.PreviewFilePath))
        {
            Dispatcher.Invoke(UpdatePreview);
        }
    }

    private async Task EnsureWebViewAsync()
    {
        if (PreviewWebView.CoreWebView2 == null)
        {
            await PreviewWebView.EnsureCoreWebView2Async();
        }
    }

    private async void UpdatePreview()
    {
        if (string.IsNullOrWhiteSpace(_viewModel.PreviewFilePath) || !File.Exists(_viewModel.PreviewFilePath))
        {
            if (PreviewWebView.CoreWebView2 != null)
            {
                PreviewWebView.CoreWebView2.NavigateToString("<html><body style='font-family:Segoe UI;color:#9E9E9E;text-align:center;padding-top:40px;'>Genera un documento para verlo aquí.</body></html>");
            }
            return;
        }

        await EnsureWebViewAsync();
        var uri = new System.Uri(_viewModel.PreviewFilePath);
        PreviewWebView.CoreWebView2.Navigate(uri.AbsoluteUri);
    }
}
