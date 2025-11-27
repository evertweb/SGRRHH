using System.Windows.Controls;
using SGRRHH.WPF.ViewModels;

namespace SGRRHH.WPF.Views;

/// <summary>
/// Lógica de interacción para DepartamentosListView.xaml
/// </summary>
public partial class DepartamentosListView : UserControl
{
    public DepartamentosListView()
    {
        InitializeComponent();
    }
    
    public DepartamentosListView(DepartamentosListViewModel viewModel) : this()
    {
        DataContext = viewModel;
        Loaded += async (s, e) => await viewModel.LoadAsync();
    }
}
