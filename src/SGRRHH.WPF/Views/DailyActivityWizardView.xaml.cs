using SGRRHH.WPF.ViewModels;
using System.Windows.Controls;

namespace SGRRHH.WPF.Views;

public partial class DailyActivityWizardView : UserControl
{
    public DailyActivityWizardView()
    {
        InitializeComponent();
        Loaded += DailyActivityWizardView_Loaded;
    }

    public DailyActivityWizardView(DailyActivityWizardViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }

    private async void DailyActivityWizardView_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is DailyActivityWizardViewModel viewModel)
        {
            await viewModel.LoadDataCommand.ExecuteAsync(null);
        }
    }
}
