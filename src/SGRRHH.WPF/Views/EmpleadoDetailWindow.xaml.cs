using System.Windows;
using SGRRHH.WPF.ViewModels;

namespace SGRRHH.WPF.Views;

/// <summary>
/// Interaction logic for EmpleadoDetailWindow.xaml
/// </summary>
public partial class EmpleadoDetailWindow : Window
{
    private readonly EmpleadoDetailViewModel _viewModel;
    
    public EmpleadoDetailWindow(EmpleadoDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        
        _viewModel.CloseRequested += OnCloseRequested;
    }
    
    private void OnCloseRequested(object? sender, EventArgs e)
    {
        Close();
    }
    
    protected override void OnClosed(EventArgs e)
    {
        _viewModel.CloseRequested -= OnCloseRequested;
        base.OnClosed(e);
    }
}
