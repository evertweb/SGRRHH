using SGRRHH.WPF.ViewModels;
using System.Windows.Controls;

namespace SGRRHH.WPF.Views;

public partial class DashboardView : UserControl
{
    public DashboardView(DashboardViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
