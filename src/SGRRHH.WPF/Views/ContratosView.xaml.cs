using System.Windows.Controls;
using SGRRHH.WPF.ViewModels;

namespace SGRRHH.WPF.Views;

/// <summary>
/// Lógica de interacción para ContratosView.xaml
/// </summary>
public partial class ContratosView : UserControl
{
    public ContratosView()
    {
        InitializeComponent();
    }
    
    public ContratosView(ContratosViewModel viewModel) : this()
    {
        DataContext = viewModel;
        Loaded += async (s, e) => await viewModel.LoadDataAsync();
    }
}
