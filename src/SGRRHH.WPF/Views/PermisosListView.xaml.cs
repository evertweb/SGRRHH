using System.Windows;
using SGRRHH.WPF.ViewModels;

namespace SGRRHH.WPF.Views;

/// <summary>
/// Lógica de interacción para PermisosListView.xaml
/// </summary>
public partial class PermisosListView : System.Windows.Controls.UserControl
{
    public PermisosListView(PermisosListViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
