using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Sundy.Controls.Scheduler;
public class YPositionToTranslateConverter : IValueConverter
{
    public static readonly YPositionToTranslateConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double y)
        {
            return new TranslateTransform(0, y);
        }
        return new TranslateTransform(0, 0);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
public class DayBackgroundConverter : IMultiValueConverter
{
    public static readonly DayBackgroundConverter Instance = new();

    private static readonly IBrush SelectedBrush = new SolidColorBrush(Color.Parse("#3B82F6"));
    private static readonly IBrush TodayBrush = new SolidColorBrush(Color.Parse("#404040"));
    private static readonly IBrush DefaultBrush = Brushes.Transparent;

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2) return DefaultBrush;
        
        var isSelected = values[0] is true;
        var isToday = values[1] is true;

        if (isSelected) return SelectedBrush;
        if (isToday) return TodayBrush;
        return DefaultBrush;
    }
}

public class DayForegroundConverter : IMultiValueConverter
{
    public static readonly DayForegroundConverter Instance = new();

    private static readonly IBrush SelectedForeground = Brushes.White;
    private static readonly IBrush TodayForeground = Brushes.White;
    private static readonly IBrush DefaultForeground = new SolidColorBrush(Color.Parse("#B0B0B0"));

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2) return DefaultForeground;
        
        var isSelected = values[0] is true;
        var isToday = values[1] is true;

        if (isSelected) return SelectedForeground;
        if (isToday) return TodayForeground;
        return DefaultForeground;
    }
}

public class TimeToYConverter : IMultiValueConverter
{
    public static readonly TimeToYConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2) return 0.0;
        if (values[0] is not TimeOnly time) return 0.0;
        if (values[1] is not double pixelsPerMinute) return 0.0;

        return (time.Hour * 60 + time.Minute) * pixelsPerMinute;
    }
}

public class DurationToHeightConverter : IMultiValueConverter
{
    public static readonly DurationToHeightConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 3) return 60.0;
        if (values[0] is not TimeOnly startTime) return 60.0;
        if (values[1] is not TimeOnly endTime) return 60.0;
        if (values[2] is not double pixelsPerMinute) return 60.0;

        var durationMinutes = (endTime - startTime).TotalMinutes;
        return durationMinutes * pixelsPerMinute;
    }
}

public class HourToYConverter : IValueConverter
{
    public static readonly HourToYConverter Instance = new();

    public double PixelsPerHour { get; set; } = 60.0;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int hour) return 0.0;
        return hour * PixelsPerHour;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class BoolToDoubleConverter : IValueConverter
{
    public static readonly BoolToDoubleConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool boolValue) return 0.5;
        return boolValue ? 1.0 : 0.5;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class HourToCanvasTopConverter : IMultiValueConverter
{
    public static readonly HourToCanvasTopConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2) return 0.0;
        if (values[0] is not int hour) return 0.0;
        if (values[1] is not double pixelsPerHour) return 0.0;

        return hour * pixelsPerHour;
    }
}

public class TimeToCanvasTopConverter : IMultiValueConverter
{
    public static readonly TimeToCanvasTopConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2) return 0.0;
        if (values[0] is not TimeOnly time) return 0.0;
        if (values[1] is not double pixelsPerMinute) return 0.0;

        return (time.Hour * 60 + time.Minute) * pixelsPerMinute;
    }
}

public class DateToBackgroundConverter : IValueConverter
{
    public static readonly DateToBackgroundConverter Instance = new();

    private static readonly IBrush SelectedBrush = new SolidColorBrush(Color.Parse("#3B82F6"));
    private static readonly IBrush TodayBrush = new SolidColorBrush(Color.Parse("#404040"));
    private static readonly IBrush DefaultBrush = Brushes.Transparent;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DateOnly date) return DefaultBrush;
        
        var today = DateOnly.FromDateTime(DateTime.Today);
        if (date == today) return TodayBrush;
        
        // Check if this date matches the selected date (passed as parameter or other mechanism)
        // For now, we'll return default - the actual selection logic should be in the view
        return DefaultBrush;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class SubtractConverter : IValueConverter
{
    public double Offset { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double d)
        {
            return Math.Max(0, d - Offset);
        }
        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
