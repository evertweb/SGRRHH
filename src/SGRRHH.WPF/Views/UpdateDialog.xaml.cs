using System.Windows;
using SGRRHH.WPF.ViewModels;

namespace SGRRHH.WPF.Views;

/// <summary>
/// Diálogo para notificar y gestionar actualizaciones de la aplicación
/// </summary>
public partial class UpdateDialog : Window
{
    public UpdateDialog(UpdateDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        
        // Configurar acción para cerrar
        viewModel.CloseDialog = () =>
        {
            DialogResult = viewModel.DialogResult ?? false;
            Close();
        };
    }
    
    /// <summary>
    /// Resultado del diálogo
    /// </summary>
    public bool? UpdateDialogResult => (DataContext as UpdateDialogViewModel)?.DialogResult;
}
