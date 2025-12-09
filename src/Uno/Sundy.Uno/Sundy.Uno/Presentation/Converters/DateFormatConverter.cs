using System;
using Microsoft.UI.Xaml.Data;

namespace Sundy.Uno.Presentation.Converters;

public sealed class DateFormatConverter : IValueConverter
{
    public string? Format { get; set; }

    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        var format = parameter as string ?? Format;
        if (value is DateTime dt && !string.IsNullOrEmpty(format))
        {
            return dt.ToString(format);
        }
        return value?.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException();
}
