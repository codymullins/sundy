using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Sundy.Core;

namespace Sundy.ViewModels;

public partial class EventViewModel : ObservableObject
{
    private const double HourHeight = 60.0;
    private const double DayWidth = 100.0; // Approximate, actual is calculated by UniformGrid

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

    // Canvas positioning for week/day views
    public double CanvasLeft { get; private set; }
    public double CanvasTop { get; private set; }
    public double CanvasWidth { get; private set; }
    public double CanvasHeight { get; private set; }

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
        CanvasHeight = Math.Max(((endMinutes - startMinutes) / 60.0) * HourHeight, 20); // Minimum height

        if (viewMode == CalendarViewMode.Week)
        {
            // Calculate horizontal position based on day of week
            var dayIndex = (int)(evt.StartTime.Date - viewStart.Date).TotalDays;
            if (dayIndex >= 0 && dayIndex < 7)
            {
                // Width will be handled by the parent container
                CanvasLeft = dayIndex * DayWidth;
                CanvasWidth = DayWidth - 8; // Leave some margin
            }
        }
        else
        {
            // Day view - full width
            CanvasLeft = 0;
            CanvasWidth = double.NaN; // Let it stretch
        }
    }
}