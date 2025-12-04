namespace SGRRHH.WPF.Models;

/// <summary>
/// Clase genérica para items de ComboBox basados en enums.
/// Permite mostrar un nombre legible y mantener el valor del enum.
/// </summary>
/// <typeparam name="T">Tipo del enum</typeparam>
public class EnumComboItem<T> where T : struct, Enum
{
    /// <summary>
    /// Nombre a mostrar en el ComboBox
    /// </summary>
    public string Nombre { get; set; } = string.Empty;
    
    /// <summary>
    /// Valor del enum (null para opción "Todos")
    /// </summary>
    public T? Valor { get; set; }
}
