using System.Windows;
using System.Windows.Input;
using SGRRHH.WPF.ViewModels;

namespace SGRRHH.WPF.Views;

/// <summary>
/// Diálogo para ofrecer configuración de Windows Hello después del login
/// </summary>
public partial class WindowsHelloSetupDialog : Window
{
    public WindowsHelloSetupDialog()
    {
        InitializeComponent();
        
        // Suscribirse al evento de cierre desde el ViewModel
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is WindowsHelloSetupDialogViewModel oldVm)
        {
            oldVm.RequestClose -= OnRequestClose;
        }

        if (e.NewValue is WindowsHelloSetupDialogViewModel newVm)
        {
            newVm.RequestClose += OnRequestClose;
        }
    }

    private void OnRequestClose(object? sender, bool dialogResult)
    {
        DialogResult = dialogResult;
        Close();
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    /// <summary>
    /// Muestra el diálogo de forma asíncrona
    /// </summary>
    public Task<bool?> ShowDialogAsync()
    {
        var tcs = new TaskCompletionSource<bool?>();

        Closed += (s, e) => tcs.SetResult(DialogResult);

        Show();

        return tcs.Task;
    }
}
