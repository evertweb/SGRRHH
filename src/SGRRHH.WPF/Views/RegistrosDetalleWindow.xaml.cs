using System.Windows;
using SGRRHH.WPF.ViewModels;

namespace SGRRHH.WPF.Views;

/// <summary>
/// Lógica de interacción para RegistrosDetalleWindow.xaml
/// </summary>
public partial class RegistrosDetalleWindow : Window
{
    public RegistrosDetalleWindow()
    {
        InitializeComponent();
    }

    public RegistrosDetalleWindow(RegistrosDetalleViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
