using System.Windows.Controls;
using SGRRHH.WPF.ViewModels;

namespace SGRRHH.WPF.Views;

/// <summary>
/// Lógica de interacción para ProyectosListView.xaml
/// </summary>
public partial class ProyectosListView : UserControl
{
    public ProyectosListView(ProyectosListViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
