using System.Windows.Controls;
using SGRRHH.WPF.ViewModels;

namespace SGRRHH.WPF.Views;

/// <summary>
/// Interaction logic for EmpleadosListView.xaml
/// </summary>
public partial class EmpleadosListView : UserControl
{
    public EmpleadosListView()
    {
        InitializeComponent();
    }
    
    public EmpleadosListView(EmpleadosListViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
