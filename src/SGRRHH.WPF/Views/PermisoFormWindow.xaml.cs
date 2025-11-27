using System.Windows;
using SGRRHH.WPF.ViewModels;

namespace SGRRHH.WPF.Views;

/// <summary>
/// Lógica de interacción para PermisoFormWindow.xaml
/// </summary>
public partial class PermisoFormWindow : Window
{
    public PermisoFormWindow(PermisoFormViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        
        // Suscribirse a eventos
        viewModel.SaveCompleted += (s, success) =>
        {
            if (success)
            {
                DialogResult = true;
                Close();
            }
        };
        
        viewModel.CloseRequested += (s, e) =>
        {
            DialogResult = false;
            Close();
        };
    }
}
