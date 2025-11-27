using SGRRHH.WPF.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SGRRHH.WPF.Views;

public partial class ReportsView : UserControl
{
    public ReportsView(ReportsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void OnPrintClick(object sender, RoutedEventArgs e)
    {
        PrintDialog printDialog = new PrintDialog();
        if (printDialog.ShowDialog() == true)
        {
            printDialog.PrintVisual(ResultsGrid, "Reporte SGRRHH");
        }
    }
}
