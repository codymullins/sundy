using System;
using Microsoft.UI.Xaml.Data;

namespace Sundy.Uno.Presentation.Converters;

public sealed class IndexEqualsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int index && parameter is string param && int.TryParse(param, out var target))
        {
            return index == target;
        }

        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isChecked && isChecked && parameter is string param && int.TryParse(param, out var target))
        {
            return target;
        }

        return Microsoft.UI.Xaml.DependencyProperty.UnsetValue;
    }
}
