using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Sundy.Uno.Presentation.Converters;

public sealed class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var invert = parameter is string s && s.Equals("Invert", StringComparison.OrdinalIgnoreCase);
        if (value is bool b)
        {
            var visible = invert ? !b : b;
            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException();
}
