using SGRRHH.WPF.ViewModels;
using System.Windows;

namespace SGRRHH.WPF.Views;

/// <summary>
/// Ventana para gestionar los documentos de un empleado
/// </summary>
public partial class DocumentosEmpleadoWindow : Window
{
    private readonly DocumentosEmpleadoViewModel _viewModel;
    
    public DocumentosEmpleadoWindow(DocumentosEmpleadoViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
        
        Loaded += async (s, e) => await _viewModel.LoadDataAsync();
    }
}
