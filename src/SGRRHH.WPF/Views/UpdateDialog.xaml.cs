using System.Windows;
using System.Windows.Input;
using SGRRHH.WPF.ViewModels;

namespace SGRRHH.WPF.Views;

/// <summary>
/// Diálogo para notificar y gestionar actualizaciones de la aplicación
/// Estilo: Windows Classic 2002
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
    /// Permite arrastrar la ventana desde la barra de título
    /// </summary>
    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }
    
    /// <summary>
    /// Cierra la ventana (solo si CanClose está habilitado)
    /// </summary>
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is UpdateDialogViewModel vm && vm.CanClose)
        {
            // Usar el comando SkipVersion que maneja internamente el DialogResult
            if (vm.SkipVersionCommand.CanExecute(null))
            {
                vm.SkipVersionCommand.Execute(null);
            }
        }
    }
    
    /// <summary>
    /// Resultado del diálogo
    /// </summary>
    public bool? UpdateDialogResult => (DataContext as UpdateDialogViewModel)?.DialogResult;
}
