namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Servicio para mostrar diálogos al usuario.
/// Centraliza la lógica de MessageBox para facilitar testing y consistencia.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Muestra un mensaje de error
    /// </summary>
    /// <param name="message">Mensaje a mostrar</param>
    /// <param name="title">Título de la ventana (opcional)</param>
    void ShowError(string message, string title = "Error");

    /// <summary>
    /// Muestra un mensaje de éxito/información
    /// </summary>
    /// <param name="message">Mensaje a mostrar</param>
    /// <param name="title">Título de la ventana (opcional)</param>
    void ShowSuccess(string message, string title = "Éxito");

    /// <summary>
    /// Muestra un mensaje informativo
    /// </summary>
    /// <param name="message">Mensaje a mostrar</param>
    /// <param name="title">Título de la ventana (opcional)</param>
    void ShowInfo(string message, string title = "Información");

    /// <summary>
    /// Muestra un mensaje de advertencia
    /// </summary>
    /// <param name="message">Mensaje a mostrar</param>
    /// <param name="title">Título de la ventana (opcional)</param>
    void ShowWarning(string message, string title = "Advertencia");

    /// <summary>
    /// Muestra un diálogo de confirmación Sí/No
    /// </summary>
    /// <param name="message">Pregunta a mostrar</param>
    /// <param name="title">Título de la ventana (opcional)</param>
    /// <returns>True si el usuario confirma, False si cancela</returns>
    bool Confirm(string message, string title = "Confirmar");

    /// <summary>
    /// Muestra un diálogo de confirmación con advertencia (para acciones destructivas)
    /// </summary>
    /// <param name="message">Pregunta a mostrar</param>
    /// <param name="title">Título de la ventana (opcional)</param>
    /// <returns>True si el usuario confirma, False si cancela</returns>
    bool ConfirmWarning(string message, string title = "Advertencia");

    /// <summary>
    /// Muestra un diálogo para ingresar texto
    /// </summary>
    /// <param name="prompt">Texto del prompt</param>
    /// <param name="title">Título de la ventana</param>
    /// <param name="defaultValue">Valor por defecto</param>
    /// <returns>El texto ingresado o null si canceló</returns>
    string? ShowInputDialog(string prompt, string title = "Ingrese valor", string defaultValue = "");
}
