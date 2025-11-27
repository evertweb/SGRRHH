using System.Windows.Controls;
using SGRRHH.WPF.ViewModels;

namespace SGRRHH.WPF.Views;

/// <summary>
/// Lógica de interacción para CatalogosView.xaml
/// </summary>
public partial class CatalogosView : UserControl
{
    public CatalogosView()
    {
        InitializeComponent();
    }
    
    public CatalogosView(CatalogosViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
