using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace SGRRHH.WPF.Converters;

/// <summary>
/// Verifica si un elemento está contenido en una colección
/// </summary>
public class CollectionContainsConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2) return false;

        var item = values[0];
        var collection = values[1] as IList;

        if (item != null && collection != null)
        {
            return collection.Contains(item);
        }

        return false;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
