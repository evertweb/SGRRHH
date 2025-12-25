using System.Windows.Controls;
using SGRRHH.WPF.ViewModels;

namespace SGRRHH.WPF.Views;

/// <summary>
/// Bandeja de aprobación de vacaciones (para Admin e Ingeniera)
/// </summary>
public partial class BandejaVacacionesView : UserControl
{
    public BandejaVacacionesView()
    {
        InitializeComponent();
    }
    
    /// <summary>
    /// Se llama cuando la vista se carga para inicializar datos
    /// </summary>
    public async Task InitializeAsync()
    {
        if (DataContext is BandejaVacacionesViewModel viewModel)
        {
            await viewModel.LoadDataAsync();
        }
    }
}
