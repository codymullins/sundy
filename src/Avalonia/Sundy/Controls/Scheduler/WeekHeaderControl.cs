using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace Sundy.Controls.Scheduler;

public class WeekHeaderControl : TemplatedControl
{
    public static readonly StyledProperty<DateOnly> SelectedDateProperty =
        AvaloniaProperty.Register<WeekHeaderControl, DateOnly>(nameof(SelectedDate), 
            DateOnly.FromDateTime(DateTime.Today));

    public static readonly StyledProperty<ICommand?> SelectDateCommandProperty =
        AvaloniaProperty.Register<WeekHeaderControl, ICommand?>(nameof(SelectDateCommand));

    public DateOnly SelectedDate
    {
        get => GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }

    public ICommand? SelectDateCommand
    {
        get => GetValue(SelectDateCommandProperty);
        set => SetValue(SelectDateCommandProperty, value);
    }
}
