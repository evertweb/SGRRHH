using System.Windows;
using SGRRHH.WPF.ViewModels;

namespace SGRRHH.WPF.Views;

/// <summary>
/// Interaction logic for EmpleadoFormWindow.xaml
/// </summary>
public partial class EmpleadoFormWindow : Window
{
    private readonly EmpleadoFormViewModel _viewModel;
    
    public EmpleadoFormWindow(EmpleadoFormViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        
        // Suscribirse a eventos
        _viewModel.CloseRequested += OnCloseRequested;
        _viewModel.SaveCompleted += OnSaveCompleted;
    }
    
    private void OnCloseRequested(object? sender, EventArgs e)
    {
        Close();
    }
    
    private void OnSaveCompleted(object? sender, EventArgs e)
    {
        DialogResult = true;
    }
    
    protected override void OnClosed(EventArgs e)
    {
        _viewModel.CloseRequested -= OnCloseRequested;
        _viewModel.SaveCompleted -= OnSaveCompleted;
        base.OnClosed(e);
    }
}
