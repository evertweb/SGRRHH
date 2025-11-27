using System.Windows;
using SGRRHH.WPF.ViewModels;

namespace SGRRHH.WPF.Views;

/// <summary>
/// Lógica de interacción para PermisosListView.xaml
/// </summary>
public partial class PermisosListView : Window
{
    public PermisosListView(PermisosListViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
