using System.Windows.Controls;
using SGRRHH.WPF.ViewModels;

namespace SGRRHH.WPF.Views;

/// <summary>
/// Lógica de interacción para VacacionesView.xaml
/// </summary>
public partial class VacacionesView : UserControl
{
    public VacacionesView()
    {
        InitializeComponent();
    }
    
    public VacacionesView(VacacionesViewModel viewModel) : this()
    {
        DataContext = viewModel;
        Loaded += async (s, e) => await viewModel.LoadDataAsync();
    }
}
