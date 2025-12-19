using Microsoft.UI.Xaml.Data;

namespace Sundy.Uno.Converters;

/// <summary>
/// Converter to check equality between value and parameter.
/// Migrated from Avalonia EqualityConverter - preserves same logic.
/// Doc reference: https://platform.uno/docs/articles/external/uno.extensions/doc/Learn/Markup/Converters.html
/// </summary>
public class EqualityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value == null && parameter == null)
            return true;
        
        if (value == null || parameter == null)
            return false;
            
        return value.ToString() == parameter.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        throw new NotImplementedException();
    }
}
