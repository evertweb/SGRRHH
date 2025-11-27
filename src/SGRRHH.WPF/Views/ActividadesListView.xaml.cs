using System.Windows.Controls;
using SGRRHH.WPF.ViewModels;

namespace SGRRHH.WPF.Views;

/// <summary>
/// Lógica de interacción para ActividadesListView.xaml
/// </summary>
public partial class ActividadesListView : UserControl
{
    public ActividadesListView(ActividadesListViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
