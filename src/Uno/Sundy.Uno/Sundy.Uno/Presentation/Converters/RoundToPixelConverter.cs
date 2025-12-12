using System;
using Microsoft.UI.Xaml.Data;

namespace Sundy.Uno.Presentation.Converters;

public sealed class RoundToPixelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is double d)
        {
            return Math.Round(d);
        }

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException();
}
