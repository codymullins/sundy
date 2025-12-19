using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sundy.Core;

namespace Sundy.Uno.ViewModels;

/// <summary>
/// ViewModel for displaying and interacting with calendar events.
/// Migrated from Avalonia - uses same CommunityToolkit.Mvvm pattern.
/// </summary>
public partial class EventViewModel : ObservableObject
{
    private const double HourHeight = 60.0;

    public EventViewModel(CalendarEvent evt, Calendar? calendar)
    {
        Event = evt;
        Calendar = calendar;
        Title = evt.Title ?? "";
        Location = evt.Location ?? string.Empty;
        HasLocation = !string.IsNullOrEmpty(evt.Location);
        CalendarColor = calendar?.Color ?? "#4A90E2";

        var startTime = evt.StartTime.ToString("h:mm tt");
        var endTime = evt.EndTime.ToString("h:mm tt");
        TimeRange = $"{startTime} - {endTime}";
    }

    public EventViewModel(CalendarEvent evt, Calendar? calendar, DateTime viewStart, CalendarViewMode viewMode)
        : this(evt, calendar)
    {
        CalculatePosition(evt, viewStart, viewMode);
    }

    public CalendarEvent Event { get; }
    public Calendar? Calendar { get; }

    public string Title { get; }
    public string Location { get; }
    public bool HasLocation { get; }
    public string CalendarColor { get; }
    public string TimeRange { get; }

    [ObservableProperty]
    private bool _isSelected;

    // Canvas positioning for week/day views
    [ObservableProperty]
    private double _canvasLeft;
    
    [ObservableProperty]
    private double _canvasTop;
    
    [ObservableProperty]
    private double _canvasWidth;
    
    [ObservableProperty]
    private double _canvasHeight;
    
    // For week view positioning as a percentage (0-6 for day index)
    public int DayIndex { get; private set; }
    public double WidthPercentage { get; private set; } = 100.0 / 7.0; // Default to 1/7 of grid

    /// <summary>
    /// Updates the Canvas position for dynamic layout in week/day views
    /// </summary>
    public void UpdateCanvasPosition(double left, double width)
    {
        CanvasLeft = left;
        CanvasWidth = width;
    }

    private void CalculatePosition(CalendarEvent evt, DateTime viewStart, CalendarViewMode viewMode)
    {
        // Calculate vertical position based on time
        var startMinutes = evt.StartTime.TimeOfDay.TotalMinutes;
        var endMinutes = evt.EndTime.TimeOfDay.TotalMinutes;

        // Handle events that span multiple days - clamp to current view
        if (evt.StartTime.Date < viewStart.Date)
        {
            startMinutes = 0;
        }
        if (evt.EndTime.Date > viewStart.Date && viewMode == CalendarViewMode.Day)
        {
            endMinutes = 24 * 60;
        }

        CanvasTop = (startMinutes / 60.0) * HourHeight;
        CanvasHeight = Math.Max(((endMinutes - startMinutes) / 60.0) * HourHeight, 30); // Minimum height of 30px

        if (viewMode == CalendarViewMode.Week)
        {
            // Calculate horizontal position based on day of week
            DayIndex = (int)(evt.StartTime.Date - viewStart.Date).TotalDays;
            if (DayIndex >= 0 && DayIndex < 7)
            {
                // Use percentage-based width calculation for responsiveness
                // Events should be positioned based on day column index
                WidthPercentage = (100.0 / 7.0) - 1; // ~13.3% per day minus margin
                CanvasWidth = 120; // Will be overridden by binding
            }
        }
        else
        {
            // Day view - full width with margins
            DayIndex = 0;
            CanvasLeft = 4;
            CanvasWidth = double.NaN; // Let container handle width
        }
    }

    [RelayCommand]
    private void Select()
    {
        Selected?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void RequestEdit()
    {
        EditRequested?.Invoke(this, Event);
    }

    public event EventHandler? Selected;
    public event EventHandler<CalendarEvent>? EditRequested;
}
