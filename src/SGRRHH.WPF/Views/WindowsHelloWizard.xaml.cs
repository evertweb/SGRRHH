using System.Windows;
using System.Windows.Input;
using SGRRHH.WPF.ViewModels;

namespace SGRRHH.WPF.Views;

/// <summary>
/// Wizard para configuración guiada de Windows Hello
/// </summary>
public partial class WindowsHelloWizard : Window
{
    public WindowsHelloWizard()
    {
        InitializeComponent();
        
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is WindowsHelloWizardViewModel oldVm)
        {
            oldVm.RequestClose -= OnRequestClose;
        }

        if (e.NewValue is WindowsHelloWizardViewModel newVm)
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
    /// Muestra el wizard de forma asíncrona
    /// </summary>
    public Task<bool?> ShowDialogAsync()
    {
        var tcs = new TaskCompletionSource<bool?>();

        Closed += (s, e) => tcs.SetResult(DialogResult);

        Show();

        return tcs.Task;
    }
}
