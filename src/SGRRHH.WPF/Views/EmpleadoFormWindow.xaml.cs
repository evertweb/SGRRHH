using System.Windows;
using Microsoft.Extensions.DependencyInjection;
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
        _viewModel.OpenDocumentosRequested += OnOpenDocumentosRequested;
    }
    
    private void OnCloseRequested(object? sender, EventArgs e)
    {
        Close();
    }
    
    private void OnSaveCompleted(object? sender, EventArgs e)
    {
        DialogResult = true;
    }
    
    private void OnOpenDocumentosRequested(object? sender, int empleadoId)
    {
        // Abrir ventana de documentos del empleado
        var documentosViewModel = App.Services!.GetRequiredService<DocumentosEmpleadoViewModel>();
        documentosViewModel.Initialize(empleadoId);
        
        var documentosWindow = new DocumentosEmpleadoWindow(documentosViewModel);
        documentosWindow.Owner = this.Owner; // Usar el owner de esta ventana
        
        // Cargar los documentos (será vacío para empleado nuevo)
        _ = documentosViewModel.LoadDataAsync();
        
        documentosWindow.ShowDialog();
    }
    
    protected override void OnClosed(EventArgs e)
    {
        _viewModel.CloseRequested -= OnCloseRequested;
        _viewModel.SaveCompleted -= OnSaveCompleted;
        _viewModel.OpenDocumentosRequested -= OnOpenDocumentosRequested;
        base.OnClosed(e);
    }
}
