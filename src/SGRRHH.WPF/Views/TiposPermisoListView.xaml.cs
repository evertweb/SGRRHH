using System.Windows.Controls;
using SGRRHH.WPF.ViewModels;

namespace SGRRHH.WPF.Views;

/// <summary>
/// Lógica de interacción para TiposPermisoListView.xaml
/// </summary>
public partial class TiposPermisoListView : UserControl
{
    public TiposPermisoListView(TiposPermisoListViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
