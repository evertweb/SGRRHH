namespace SGRRHH.Local.Shared.Helpers;

/// <summary>
/// Helper para validaciones comunes en formularios
/// </summary>
public static class ValidationHelper
{
    /// <summary>
    /// Valida campo requerido y muestra error si está vacío
    /// </summary>
    public static bool ValidarCampoRequerido(string? valor, string nombreCampo, dynamic? messageToast)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            messageToast?.ShowError($"{nombreCampo} es obligatorio");
            return false;
        }
        return true;
    }
    
    /// <summary>
    /// Valida fecha requerida
    /// </summary>
    public static bool ValidarFechaRequerida(DateTime? fecha, string nombreCampo, dynamic? messageToast)
    {
        if (!fecha.HasValue)
        {
            messageToast?.ShowError($"{nombreCampo} es obligatoria");
            return false;
        }
        return true;
    }
}
