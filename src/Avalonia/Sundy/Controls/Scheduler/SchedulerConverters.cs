using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
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

/// <summary>
/// Converts day index and container width to Canvas.Left position for week view events
/// </summary>
public class DayIndexToLeftConverter : IMultiValueConverter
{
    public static readonly DayIndexToLeftConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2) return 0.0;
        if (values[0] is not int dayIndex) return 0.0;
        if (values[1] is not double containerWidth || containerWidth <= 0) return 0.0;

        var dayWidth = containerWidth / 7.0;
        return (dayIndex * dayWidth) + 4; // 4px margin
    }
}

/// <summary>
/// Converts container width to event width for week view (1/7 of container minus margins)
/// </summary>
public class ContainerWidthToEventWidthConverter : IValueConverter
{
    public static readonly ContainerWidthToEventWidthConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double containerWidth || containerWidth <= 0) return 100.0;

        var dayWidth = containerWidth / 7.0;
        return Math.Max(20, dayWidth - 8); // 8px total margin (4px on each side)
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

/// <summary>
/// Converts sidebar visibility to navigation margin - when sidebar is visible, less left margin is needed
/// </summary>
public class SidebarMarginConverter : IValueConverter
{
    public static readonly SidebarMarginConverter Instance = new();
    private static readonly bool IsMacOS = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    private static readonly bool IsIOS = RuntimeInformation.IsOSPlatform(OSPlatform.Create("IOS"));
    private static readonly bool IsAndroid = RuntimeInformation.IsOSPlatform(OSPlatform.Create("ANDROID"));
    private static readonly bool IsMobile = IsIOS || IsAndroid;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isSidebarVisible = value is true;

        // Mobile: minimal margins to maximize screen real estate
        if (IsMobile)
        {
            return new Avalonia.Thickness(0, 10, 0, 6);
        }

        // macOS needs extra left margin to clear traffic lights when sidebar is hidden
        if (IsMacOS && !isSidebarVisible)
        {
            return new Avalonia.Thickness(80, 10, 16, 6);
        }

        return new Avalonia.Thickness(10, 10, 16, 6);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

/// <summary>
/// Converts sidebar visibility to container width for slide animation
/// </summary>
public class SidebarWidthConverter : IValueConverter
{
    public static readonly SidebarWidthConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isSidebarVisible = value is true;
        // 266 = 260 (sidebar width) + 6 (left margin)
        return isSidebarVisible ? 266.0 : 0.0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

/// <summary>
/// Converts mobile layout flag to calendar content margin - removes left/right margin on mobile
/// </summary>
public class MobileMarginConverter : IValueConverter
{
    public static readonly MobileMarginConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isMobileLayout = value is true;
        // Mobile: no left/right margin to maximize screen real estate
        // Desktop: keep 6px left/right margins
        return isMobileLayout
            ? new Avalonia.Thickness(0, 0, 0, 6)
            : new Avalonia.Thickness(6, 0, 6, 6);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

/// <summary>
/// Converts mobile layout flag to corner radius - removes rounded corners on mobile
/// </summary>
public class MobileCornerRadiusConverter : IValueConverter
{
    public static readonly MobileCornerRadiusConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isMobileLayout = value is true;
        return isMobileLayout ? new Avalonia.CornerRadius(0) : new Avalonia.CornerRadius(12);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

/// <summary>
/// Converts mobile layout flag to border thickness - removes border on mobile
/// </summary>
public class MobileBorderThicknessConverter : IValueConverter
{
    public static readonly MobileBorderThicknessConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isMobileLayout = value is true;
        return isMobileLayout ? new Avalonia.Thickness(0) : new Avalonia.Thickness(1);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

/// <summary>
/// Converts current time (in minutes from midnight) to a relative Y position for month view cells.
/// Maps 0-1440 minutes to approximately 0-100% of cell height (assuming ~80px usable height).
/// </summary>
public class MonthTimeIndicatorConverter : IValueConverter
{
    public static readonly MonthTimeIndicatorConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double minutes) return 0.0;

        // CurrentTimeTop is minutes from midnight (0-1440)
        // Map to a percentage of the day, then scale to approximate cell content height
        var percentOfDay = minutes / 1440.0;
        // Assume usable content area is roughly 80px in a month cell
        return percentOfDay * 80.0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

/// <summary>
/// Converts current time (in minutes from midnight) to a top margin for month view time indicator.
/// Maps 0-1440 minutes to a percentage-based margin of the cell height.
/// </summary>
public class MonthTimeMarginConverter : IValueConverter
{
    public static readonly MonthTimeMarginConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double minutes) return new Avalonia.Thickness(0);

        // CurrentTimeTop is minutes from midnight (0-1440)
        // Map to a percentage of the day, then scale to approximate cell content height
        var percentOfDay = minutes / 1440.0;
        // Assume usable content area is roughly 100px in a month cell
        var topMargin = percentOfDay * 100.0;
        return new Avalonia.Thickness(0, topMargin, 0, 0);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

