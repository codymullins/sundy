using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sundy.Core;

namespace Sundy.Uno.ViewModels;

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

    [ObservableProperty]
    private double _canvasLeft;
    
    [ObservableProperty]
    private double _canvasTop;
    
    [ObservableProperty]
    private double _canvasWidth;
    
    [ObservableProperty]
    private double _canvasHeight;
    
    public int DayIndex { get; private set; }
    public double WidthPercentage { get; private set; } = 100.0 / 7.0;

    public void UpdateCanvasPosition(double left, double width)
    {
        CanvasLeft = Math.Round(left);
        CanvasWidth = Math.Round(width);
    }

    private void CalculatePosition(CalendarEvent evt, DateTime viewStart, CalendarViewMode viewMode)
    {
        var startMinutes = evt.StartTime.TimeOfDay.TotalMinutes;
        var endMinutes = evt.EndTime.TimeOfDay.TotalMinutes;

        if (evt.StartTime.Date < viewStart.Date)
        {
            startMinutes = 0;
        }
        if (evt.EndTime.Date > viewStart.Date && viewMode == CalendarViewMode.Day)
        {
            endMinutes = 24 * 60;
        }

        CanvasTop = Math.Round((startMinutes / 60.0) * HourHeight);
        CanvasHeight = Math.Round(Math.Max(((endMinutes - startMinutes) / 60.0) * HourHeight, 30));

        if (viewMode == CalendarViewMode.Week)
        {
            DayIndex = (int)(evt.StartTime.Date - viewStart.Date).TotalDays;
            if (DayIndex >= 0 && DayIndex < 7)
            {
                WidthPercentage = (100.0 / 7.0) - 1;
                CanvasWidth = 120;
            }
        }
        else
        {
            DayIndex = 0;
            CanvasLeft = 4;
            CanvasWidth = double.NaN; // NaN triggers auto-sizing
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
