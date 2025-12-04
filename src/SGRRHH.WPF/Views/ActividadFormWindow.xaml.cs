using System.Windows;
using SGRRHH.WPF.ViewModels;

namespace SGRRHH.WPF.Views;

/// <summary>
/// Ventana para crear/editar actividades
/// </summary>
public partial class ActividadFormWindow : Window
{
    public ActividadFormWindow(ActividadFormViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        
        // Suscribirse a eventos del ViewModel
        viewModel.CloseRequested += (s, result) =>
        {
            DialogResult = result;
            Close();
        };
    }
}
