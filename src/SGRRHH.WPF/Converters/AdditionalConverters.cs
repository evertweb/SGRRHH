using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace SGRRHH.WPF.Converters;

/// <summary>
/// Convierte un booleano a texto basado en un parámetro "true|false"
/// </summary>
public class BoolToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue && parameter is string paramString)
        {
            var parts = paramString.Split('|');
            if (parts.Length == 2)
            {
                return boolValue ? parts[0] : parts[1];
            }
        }
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Convierte un booleano a un color (verde=true, rojo=false)
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue 
                ? new SolidColorBrush(Color.FromRgb(76, 175, 80))   // Verde #4CAF50
                : new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Rojo #f44336
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Convierte un Enum a su representación de texto más legible
/// </summary>
public class EnumToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return string.Empty;
        
        var enumValue = value.ToString() ?? string.Empty;
        
        // Agregar espacios antes de mayúsculas para nombres PascalCase
        return System.Text.RegularExpressions.Regex.Replace(enumValue, "([a-z])([A-Z])", "$1 $2");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Compara dos valores para determinar si son iguales
/// </summary>
public class EqualityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2) return false;
        
        var value1 = values[0]?.ToString();
        var value2 = values[1]?.ToString();
        
        return string.Equals(value1, value2, StringComparison.OrdinalIgnoreCase);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Convierte el estado de permiso a un color
/// </summary>
public class EstadoPermisoToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var estado = value?.ToString() ?? string.Empty;
        
        return estado switch
        {
            "Aprobado" => new SolidColorBrush(Color.FromRgb(76, 175, 80)),    // Verde
            "Rechazado" => new SolidColorBrush(Color.FromRgb(244, 67, 54)),   // Rojo
            "Pendiente" => new SolidColorBrush(Color.FromRgb(255, 152, 0)),   // Naranja
            _ => new SolidColorBrush(Colors.Gray)
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Convierte un valor decimal a formato de moneda
/// </summary>
public class CurrencyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal decimalValue)
        {
            return decimalValue.ToString("C0", new CultureInfo("es-CO"));
        }
        return "$0";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string stringValue)
        {
            // Remover símbolos de moneda y parsear
            stringValue = stringValue.Replace("$", "").Replace(".", "").Replace(",", "").Trim();
            if (decimal.TryParse(stringValue, out decimal result))
            {
                return result;
            }
        }
        return 0m;
    }
}

/// <summary>
/// Convierte un porcentaje (0-100) al ancho proporcional basado en el ancho del contenedor
/// </summary>
public class PercentToWidthConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2)
            return 0d;
        
        // Primer valor: porcentaje (0-100)
        // Segundo valor: ancho del contenedor
        if (values[0] is double percent && values[1] is double containerWidth)
        {
            // Calcular ancho proporcional
            return Math.Max(5, (percent / 100.0) * containerWidth);
        }
        
        // Intentar con otros tipos numéricos
        if (values[1] is double width)
        {
            double percentage = 0;
            if (values[0] is int intPercent)
                percentage = intPercent;
            else if (values[0] is double doublePercent)
                percentage = doublePercent;
            else if (double.TryParse(values[0]?.ToString(), out double parsedPercent))
                percentage = parsedPercent;
            
            return Math.Max(5, (percentage / 100.0) * width);
        }
        
        return 0d;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Convierte un string de color hex (#RRGGBB) a SolidColorBrush
/// </summary>
public class HexToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string hexColor && !string.IsNullOrEmpty(hexColor))
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(hexColor);
                return new SolidColorBrush(color);
            }
            catch
            {
                return new SolidColorBrush(Colors.Gray);
            }
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
