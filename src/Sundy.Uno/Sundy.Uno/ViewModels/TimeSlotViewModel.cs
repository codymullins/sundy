namespace Sundy.Uno.ViewModels;

/// <summary>
/// ViewModel for time slots (hours) in day/week views.
/// Migrated from Avalonia - same logic preserved.
/// </summary>
public class TimeSlotViewModel
{
    public TimeSlotViewModel(int hour)
    {
        Hour = hour;
        TimeLabel = hour == 0 ? "12 AM" :
            hour < 12 ? $"{hour} AM" :
            hour == 12 ? "12 PM" :
            $"{hour - 12} PM";
    }

    public int Hour { get; }
    public string TimeLabel { get; }
}
