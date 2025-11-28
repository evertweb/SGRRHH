using System.Windows;
using Microsoft.Extensions.DependencyInjection;
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
        _viewModel.ViewDocumentsRequested += OnViewDocumentsRequested;
    }
    
    private void OnCloseRequested(object? sender, EventArgs e)
    {
        Close();
    }
    
    private void OnViewDocumentsRequested(object? sender, int empleadoId)
    {
        var viewModel = App.Services!.GetRequiredService<DocumentosEmpleadoViewModel>();
        viewModel.Initialize(empleadoId);
        
        var window = new DocumentosEmpleadoWindow(viewModel);
        window.Owner = this;
        
        // Cargar los documentos del empleado
        _ = viewModel.LoadDataAsync();
        
        window.ShowDialog();
    }
    
    protected override void OnClosed(EventArgs e)
    {
        _viewModel.CloseRequested -= OnCloseRequested;
        _viewModel.ViewDocumentsRequested -= OnViewDocumentsRequested;
        base.OnClosed(e);
    }
}
