using System.Windows.Controls;
using System.Windows.Input;
using SGRRHH.WPF.ViewModels;

namespace SGRRHH.WPF.Views;

/// <summary>
/// Lógica de interacción para ActividadesListView.xaml
/// </summary>
public partial class ActividadesListView : UserControl
{
    private readonly ActividadesListViewModel _viewModel;
    
    public ActividadesListView(ActividadesListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
    }
    
    private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (_viewModel.SelectedActividad != null)
        {
            _viewModel.EditActividadCommand.Execute(null);
        }
    }
}
