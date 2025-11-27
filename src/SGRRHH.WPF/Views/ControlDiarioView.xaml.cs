using System.Windows.Controls;
using SGRRHH.WPF.ViewModels;

namespace SGRRHH.WPF.Views;

/// <summary>
/// Lógica de interacción para ControlDiarioView.xaml
/// </summary>
public partial class ControlDiarioView : UserControl
{
    public ControlDiarioView(ControlDiarioViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
