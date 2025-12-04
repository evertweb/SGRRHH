using SGRRHH.Core.Interfaces;
using System.Windows;

namespace SGRRHH.WPF.Services;

/// <summary>
/// Implementación de IDialogService usando MessageBox de WPF.
/// Centraliza todos los diálogos de la aplicación para consistencia visual.
/// </summary>
public class DialogService : IDialogService
{
    private const string APP_NAME = "SGRRHH";

    /// <inheritdoc/>
    public void ShowError(string message, string title = "Error")
    {
        MessageBox.Show(
            message,
            $"{APP_NAME} - {title}",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    /// <inheritdoc/>
    public void ShowSuccess(string message, string title = "Éxito")
    {
        MessageBox.Show(
            message,
            $"{APP_NAME} - {title}",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    /// <inheritdoc/>
    public void ShowInfo(string message, string title = "Información")
    {
        MessageBox.Show(
            message,
            $"{APP_NAME} - {title}",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    /// <inheritdoc/>
    public void ShowWarning(string message, string title = "Advertencia")
    {
        MessageBox.Show(
            message,
            $"{APP_NAME} - {title}",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);
    }

    /// <inheritdoc/>
    public bool Confirm(string message, string title = "Confirmar")
    {
        var result = MessageBox.Show(
            message,
            $"{APP_NAME} - {title}",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        return result == MessageBoxResult.Yes;
    }

    /// <inheritdoc/>
    public bool ConfirmWarning(string message, string title = "Advertencia")
    {
        var result = MessageBox.Show(
            message,
            $"{APP_NAME} - {title}",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        return result == MessageBoxResult.Yes;
    }

    /// <inheritdoc/>
    public string? ShowInputDialog(string prompt, string title = "Ingrese valor", string defaultValue = "")
    {
        // Para un InputDialog más elaborado, se podría crear una ventana personalizada
        // Por ahora usamos un enfoque simple con una ventana de InputBox básica
        return ShowSimpleInputDialog(prompt, title, defaultValue);
    }

    /// <summary>
    /// Muestra un diálogo simple de entrada de texto
    /// </summary>
    private string? ShowSimpleInputDialog(string prompt, string title, string defaultValue)
    {
        // Crear una ventana simple para entrada de texto
        var window = new Window
        {
            Title = $"{APP_NAME} - {title}",
            Width = 400,
            Height = 180,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            ResizeMode = ResizeMode.NoResize,
            WindowStyle = WindowStyle.ToolWindow
        };

        var grid = new System.Windows.Controls.Grid();
        grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = System.Windows.GridLength.Auto });
        grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = System.Windows.GridLength.Auto });
        grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = System.Windows.GridLength.Auto });

        var label = new System.Windows.Controls.TextBlock
        {
            Text = prompt,
            Margin = new Thickness(15, 15, 15, 5),
            TextWrapping = TextWrapping.Wrap
        };
        System.Windows.Controls.Grid.SetRow(label, 0);
        grid.Children.Add(label);

        var textBox = new System.Windows.Controls.TextBox
        {
            Text = defaultValue,
            Margin = new Thickness(15, 5, 15, 15),
            Padding = new Thickness(8, 5, 8, 5)
        };
        System.Windows.Controls.Grid.SetRow(textBox, 1);
        grid.Children.Add(textBox);

        var buttonPanel = new System.Windows.Controls.StackPanel
        {
            Orientation = System.Windows.Controls.Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(15, 0, 15, 15)
        };
        System.Windows.Controls.Grid.SetRow(buttonPanel, 2);

        string? result = null;

        var okButton = new System.Windows.Controls.Button
        {
            Content = "Aceptar",
            Width = 80,
            Margin = new Thickness(0, 0, 10, 0),
            Padding = new Thickness(5),
            IsDefault = true
        };
        okButton.Click += (s, e) => { result = textBox.Text; window.Close(); };
        buttonPanel.Children.Add(okButton);

        var cancelButton = new System.Windows.Controls.Button
        {
            Content = "Cancelar",
            Width = 80,
            Padding = new Thickness(5),
            IsCancel = true
        };
        cancelButton.Click += (s, e) => { result = null; window.Close(); };
        buttonPanel.Children.Add(cancelButton);

        grid.Children.Add(buttonPanel);
        window.Content = grid;

        textBox.Focus();
        textBox.SelectAll();

        window.ShowDialog();
        return result;
    }
}
