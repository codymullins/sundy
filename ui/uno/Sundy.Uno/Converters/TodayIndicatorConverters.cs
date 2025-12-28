using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace Sundy.Uno.Converters;

/// <summary>
/// Converts boolean to background brush for today indicator.
/// True (is today) -> AccentPrimary, False -> Transparent
/// </summary>
public class BoolToBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isToday && isToday)
        {
            return Application.Current.Resources["AccentPrimary"] as SolidColorBrush 
                   ?? new SolidColorBrush(Colors.Purple);
        }
        return new SolidColorBrush(Colors.Transparent);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts boolean to foreground brush for today indicator.
/// True (is today) -> White, False -> ForegroundPrimary
/// </summary>
public class BoolToForegroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isToday && isToday)
        {
            return new SolidColorBrush(Colors.White);
        }
        return Application.Current.Resources["ForegroundPrimary"] as SolidColorBrush 
               ?? new SolidColorBrush(Colors.Black);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts boolean to font weight for today indicator.
/// True (is today) -> SemiBold, False -> Normal
/// </summary>
public class BoolToFontWeightConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isToday && isToday)
        {
            return new Windows.UI.Text.FontWeight { Weight = 600 }; // SemiBold
        }
        return new Windows.UI.Text.FontWeight { Weight = 400 }; // Normal
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
