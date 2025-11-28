using System;
using System.Threading.Tasks;
using System.Windows;

namespace SGRRHH.WPF.Helpers;

/// <summary>
/// Helper para ejecutar tareas async de forma segura en ViewModels
/// Evita problemas con fire-and-forget y maneja excepciones correctamente
/// </summary>
public static class AsyncHelper
{
    /// <summary>
    /// Ejecuta una tarea async de forma segura, capturando excepciones
    /// </summary>
    /// <param name="task">Tarea a ejecutar</param>
    /// <param name="onError">Acción a ejecutar en caso de error (opcional)</param>
    /// <param name="showErrorMessage">Si debe mostrar mensaje de error al usuario</param>
    public static async void SafeFireAndForget(
        this Task task, 
        Action<Exception>? onError = null,
        bool showErrorMessage = true)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex);
            
            if (showErrorMessage)
            {
                // Ejecutar en el hilo de UI
                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    MessageBox.Show(
                        $"Error: {ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                });
            }
            
            // Log the error
            LogError(ex);
        }
    }
    
    /// <summary>
    /// Ejecuta una tarea async de forma segura con callback de éxito
    /// </summary>
    public static async void SafeFireAndForget<T>(
        this Task<T> task,
        Action<T>? onSuccess = null,
        Action<Exception>? onError = null,
        bool showErrorMessage = true)
    {
        try
        {
            var result = await task.ConfigureAwait(false);
            
            if (onSuccess != null)
            {
                Application.Current?.Dispatcher?.Invoke(() => onSuccess(result));
            }
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex);
            
            if (showErrorMessage)
            {
                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    MessageBox.Show(
                        $"Error: {ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                });
            }
            
            LogError(ex);
        }
    }
    
    /// <summary>
    /// Ejecuta una tarea async en el hilo de UI
    /// </summary>
    public static async Task RunOnUIThreadAsync(Action action)
    {
        if (Application.Current?.Dispatcher == null)
        {
            action();
            return;
        }
        
        if (Application.Current.Dispatcher.CheckAccess())
        {
            action();
        }
        else
        {
            await Application.Current.Dispatcher.InvokeAsync(action);
        }
    }
    
    /// <summary>
    /// Registra el error en el log
    /// </summary>
    private static void LogError(Exception ex)
    {
        try
        {
            var logPath = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, 
                "data", "logs", 
                $"error_{DateTime.Now:yyyy-MM-dd}.log");
            
            var logDir = System.IO.Path.GetDirectoryName(logPath);
            if (!string.IsNullOrEmpty(logDir) && !System.IO.Directory.Exists(logDir))
            {
                System.IO.Directory.CreateDirectory(logDir);
            }
            
            var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] AsyncHelper Error: {ex.Message}\n{ex.StackTrace}\n\n";
            System.IO.File.AppendAllText(logPath, logMessage);
        }
        catch
        {
            // Silenciar errores de logging
        }
    }
}
