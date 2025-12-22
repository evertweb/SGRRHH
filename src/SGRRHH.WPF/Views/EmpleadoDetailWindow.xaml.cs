using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SGRRHH.WPF.Services;
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
    
    private void PrintButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var document = PrintService.Instance.CreateEmployeeDocument(
                nombre: _viewModel.NombreCompleto ?? string.Empty,
                cedula: _viewModel.Cedula ?? string.Empty,
                cargo: _viewModel.Cargo ?? string.Empty,
                departamento: _viewModel.Departamento ?? string.Empty,
                fechaIngreso: _viewModel.FechaIngreso ?? string.Empty,
                email: _viewModel.Email ?? string.Empty,
                telefono: _viewModel.Telefono ?? string.Empty,
                observaciones: _viewModel.Observaciones ?? string.Empty
            );
            
            PrintService.Instance.PrintFlowDocument(document, $"Ficha - {_viewModel.NombreCompleto}");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al imprimir: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    protected override void OnClosed(EventArgs e)
    {
        _viewModel.CloseRequested -= OnCloseRequested;
        _viewModel.ViewDocumentsRequested -= OnViewDocumentsRequested;
        base.OnClosed(e);
    }
}
