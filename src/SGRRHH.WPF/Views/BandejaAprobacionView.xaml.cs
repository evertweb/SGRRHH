using System.Windows.Controls;
using SGRRHH.WPF.ViewModels;

namespace SGRRHH.WPF.Views;

/// <summary>
/// Lógica de interacción para BandejaAprobacionView.xaml
/// </summary>
public partial class BandejaAprobacionView : UserControl
{
    public BandejaAprobacionView(BandejaAprobacionViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
