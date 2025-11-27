using System.Windows.Controls;
using SGRRHH.WPF.ViewModels;

namespace SGRRHH.WPF.Views;

/// <summary>
/// Lógica de interacción para CargosListView.xaml
/// </summary>
public partial class CargosListView : UserControl
{
    public CargosListView()
    {
        InitializeComponent();
    }
    
    public CargosListView(CargosListViewModel viewModel) : this()
    {
        DataContext = viewModel;
        Loaded += async (s, e) => await viewModel.LoadAsync();
    }
}
