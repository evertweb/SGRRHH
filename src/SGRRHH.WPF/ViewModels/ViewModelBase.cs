using CommunityToolkit.Mvvm.ComponentModel;

namespace SGRRHH.WPF.ViewModels;

/// <summary>
/// Clase base para todos los ViewModels de la aplicación.
/// Proporciona funcionalidad común como indicador de carga,
/// mensajes de estado y ejecución segura de operaciones asíncronas.
/// </summary>
public partial class ViewModelBase : ObservableObject
{
    /// <summary>
    /// Indica si hay una operación en progreso
    /// </summary>
    [ObservableProperty]
    private bool _isLoading;

    /// <summary>
    /// Mensaje de estado para mostrar al usuario
    /// </summary>
    [ObservableProperty]
    private string _statusMessage = string.Empty;

    /// <summary>
    /// Ejecuta una operación asíncrona mostrando el indicador de carga
    /// y manejando errores automáticamente.
    /// </summary>
    /// <param name="action">Acción asíncrona a ejecutar</param>
    /// <param name="loadingMessage">Mensaje opcional mientras carga</param>
    /// <param name="errorPrefix">Prefijo para mensajes de error</param>
    /// <returns>True si la operación fue exitosa, False si hubo error</returns>
    protected async Task<bool> ExecuteWithLoadingAsync(
        Func<Task> action, 
        string? loadingMessage = null,
        string? errorPrefix = null)
    {
        if (IsLoading) return false;

        IsLoading = true;
        if (!string.IsNullOrEmpty(loadingMessage))
            StatusMessage = loadingMessage;

        try
        {
            await action();
            return true;
        }
        catch (Exception ex)
        {
            var prefix = errorPrefix ?? "Error";
            StatusMessage = $"{prefix}: {ex.Message}";
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Ejecuta una operación asíncrona que retorna un valor,
    /// mostrando el indicador de carga y manejando errores.
    /// </summary>
    /// <typeparam name="T">Tipo del valor de retorno</typeparam>
    /// <param name="action">Acción asíncrona a ejecutar</param>
    /// <param name="defaultValue">Valor por defecto si hay error</param>
    /// <param name="loadingMessage">Mensaje opcional mientras carga</param>
    /// <param name="errorPrefix">Prefijo para mensajes de error</param>
    /// <returns>El resultado de la operación o el valor por defecto</returns>
    protected async Task<T?> ExecuteWithLoadingAsync<T>(
        Func<Task<T>> action,
        T? defaultValue = default,
        string? loadingMessage = null,
        string? errorPrefix = null)
    {
        if (IsLoading) return defaultValue;

        IsLoading = true;
        if (!string.IsNullOrEmpty(loadingMessage))
            StatusMessage = loadingMessage;

        try
        {
            return await action();
        }
        catch (Exception ex)
        {
            var prefix = errorPrefix ?? "Error";
            StatusMessage = $"{prefix}: {ex.Message}";
            return defaultValue;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Limpia el mensaje de estado
    /// </summary>
    protected void ClearStatus()
    {
        StatusMessage = string.Empty;
    }

    /// <summary>
    /// Establece un mensaje de éxito
    /// </summary>
    protected void SetSuccessMessage(string message)
    {
        StatusMessage = $"✓ {message}";
    }

    /// <summary>
    /// Establece un mensaje de error
    /// </summary>
    protected void SetErrorMessage(string message)
    {
        StatusMessage = $"✗ {message}";
    }
}
