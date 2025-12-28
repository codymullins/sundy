using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;

namespace Sundy.Controls;

public partial class TimeSelectionPreview : UserControl
{
    public static readonly StyledProperty<DateOnly> SelectedDateProperty =
        AvaloniaProperty.Register<TimeSelectionPreview, DateOnly>(nameof(SelectedDate));

    public static readonly StyledProperty<string?> TimeRangeTextProperty =
        AvaloniaProperty.Register<TimeSelectionPreview, string?>(nameof(TimeRangeText));

    public static readonly StyledProperty<string?> DurationTextProperty =
        AvaloniaProperty.Register<TimeSelectionPreview, string?>(nameof(DurationText));

    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<TimeSelectionPreview, ICommand?>(nameof(Command));

    public DateOnly SelectedDate
    {
        get => GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }

    public string? TimeRangeText
    {
        get => GetValue(TimeRangeTextProperty);
        set => SetValue(TimeRangeTextProperty, value);
    }

    public string? DurationText
    {
        get => GetValue(DurationTextProperty);
        set => SetValue(DurationTextProperty, value);
    }

    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public TimeSelectionPreview()
    {
        InitializeComponent();
    }
}
