using System;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace Sundy.Uno.Presentation.Converters;

public sealed class StringToBrushConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string hex && TryParseColor(hex, out var color))
        {
            return new SolidColorBrush(color);
        }

        return new SolidColorBrush(Colors.DodgerBlue);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException();

    private static bool TryParseColor(string hex, out Color color)
    {
        hex = hex.TrimStart('#');
        color = Colors.Transparent;

        if (hex.Length == 6)
        {
            if (byte.TryParse(hex[..2], System.Globalization.NumberStyles.HexNumber, null, out var r) &&
                byte.TryParse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber, null, out var g) &&
                byte.TryParse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber, null, out var b))
            {
                color = Color.FromArgb(255, r, g, b);
                return true;
            }
        }

        if (hex.Length == 8)
        {
            if (byte.TryParse(hex[..2], System.Globalization.NumberStyles.HexNumber, null, out var a) &&
                byte.TryParse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber, null, out var r) &&
                byte.TryParse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber, null, out var g) &&
                byte.TryParse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber, null, out var b))
            {
                color = Color.FromArgb(a, r, g, b);
                return true;
            }
        }

        return false;
    }
}
