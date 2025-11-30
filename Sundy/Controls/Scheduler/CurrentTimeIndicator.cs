using System;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace Sundy.Controls.Scheduler;

public class CurrentTimeIndicator : TemplatedControl
{
    public static readonly StyledProperty<TimeOnly> CurrentTimeProperty =
        AvaloniaProperty.Register<CurrentTimeIndicator, TimeOnly>(nameof(CurrentTime), 
            TimeOnly.FromDateTime(DateTime.Now));

    public static readonly StyledProperty<IBrush?> IndicatorBrushProperty =
        AvaloniaProperty.Register<CurrentTimeIndicator, IBrush?>(nameof(IndicatorBrush));

    public static readonly StyledProperty<string> TimeLabelProperty =
        AvaloniaProperty.Register<CurrentTimeIndicator, string>(nameof(TimeLabel));

    public TimeOnly CurrentTime
    {
        get => GetValue(CurrentTimeProperty);
        set => SetValue(CurrentTimeProperty, value);
    }

    public IBrush? IndicatorBrush
    {
        get => GetValue(IndicatorBrushProperty);
        set => SetValue(IndicatorBrushProperty, value);
    }

    public string TimeLabel
    {
        get => GetValue(TimeLabelProperty);
        set => SetValue(TimeLabelProperty, value);
    }

    static CurrentTimeIndicator()
    {
        CurrentTimeProperty.Changed.AddClassHandler<CurrentTimeIndicator>((x, _) => x.UpdateTimeLabel());
    }

    public CurrentTimeIndicator()
    {
        UpdateTimeLabel();
    }

    private void UpdateTimeLabel()
    {
        TimeLabel = CurrentTime.ToString("h:mm tt").ToUpperInvariant();
    }
}
