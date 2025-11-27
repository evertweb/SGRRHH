using SGRRHH.WPF.ViewModels;
using System.Windows.Controls;

namespace SGRRHH.WPF.Views;

/// <summary>
/// Lógica de interacción para ConfiguracionView.xaml
/// </summary>
public partial class ConfiguracionView : UserControl
{
    public ConfiguracionView(ConfiguracionViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
