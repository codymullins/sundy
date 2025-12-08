using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Sundy.ViewModels.Scheduler;

public partial class SchedulerViewModel : ObservableObject
{
    [ObservableProperty]
    private DateOnly _selectedDate = DateOnly.FromDateTime(DateTime.Today);

    [ObservableProperty]
    private TimeBlockViewModel _timeBlock = new();

    [ObservableProperty]
    private TimeOnly _currentTime = TimeOnly.FromDateTime(DateTime.Now);

    [ObservableProperty]
    private TimeOnly _dayStartTime = new(0, 0);

    [ObservableProperty]
    private TimeOnly _dayEndTime = new(23, 59);

    [ObservableProperty]
    private double _pixelsPerHour = 60.0;

    public double PixelsPerMinute => PixelsPerHour / 60.0;

    public double TotalHeight => 24 * PixelsPerHour;

    public ObservableCollection<DateOnly> WeekDays { get; } = new();

    public ObservableCollection<DayItemViewModel> DayItems { get; } = new();

    public ObservableCollection<HourLineViewModel> HourLines { get; } = new();

    public SchedulerViewModel()
    {
        UpdateWeekDays();
        InitializeHourLines();
        
        // Set default time block to current time + 1 hour rounded to next 30 min
        var now = DateTime.Now;
        var minutes = now.Minute < 30 ? 30 : 0;
        var hours = now.Minute < 30 ? now.Hour : now.Hour + 1;
        if (hours >= 23)
        {
            hours = 19; // Default evening slot if it's late
            minutes = 30;
        }
        TimeBlock.StartTime = new TimeOnly(hours, minutes);
        TimeBlock.EndTime = TimeBlock.StartTime.AddHours(1);
    }

    partial void OnSelectedDateChanged(DateOnly value)
    {
        UpdateWeekDays();
    }

    partial void OnPixelsPerHourChanged(double value)
    {
        OnPropertyChanged(nameof(PixelsPerMinute));
        OnPropertyChanged(nameof(TotalHeight));
        InitializeHourLines();
    }

    private void UpdateWeekDays()
    {
        WeekDays.Clear();
        DayItems.Clear();
        
        // Find the Sunday of the current week
        var daysFromSunday = (int)SelectedDate.DayOfWeek;
        var weekStart = SelectedDate.AddDays(-daysFromSunday);
        
        for (int i = 0; i < 7; i++)
        {
            var date = weekStart.AddDays(i);
            WeekDays.Add(date);
            DayItems.Add(new DayItemViewModel(date, date == SelectedDate));
        }
    }

    private void InitializeHourLines()
    {
        HourLines.Clear();
        for (int hour = 0; hour < 24; hour++)
        {
            HourLines.Add(new HourLineViewModel(hour, PixelsPerHour));
        }
    }

    public void UpdateCurrentTime()
    {
        CurrentTime = TimeOnly.FromDateTime(DateTime.Now);
    }

    public double TimeToY(TimeOnly time)
    {
        return (time.Hour * 60 + time.Minute) * PixelsPerMinute;
    }

    public TimeOnly YToTime(double y)
    {
        var totalMinutes = y / PixelsPerMinute;
        totalMinutes = Math.Clamp(totalMinutes, 0, 24 * 60 - 1);
        return new TimeOnly((int)totalMinutes / 60, (int)totalMinutes % 60);
    }

    [RelayCommand]
    private void SelectDate(DateOnly date)
    {
        foreach (var dayItem in DayItems)
        {
            dayItem.IsSelected = dayItem.Date == date;
        }
        SelectedDate = date;
    }

    [RelayCommand]
    private void NavigatePreviousWeek()
    {
        SelectedDate = SelectedDate.AddDays(-7);
    }

    [RelayCommand]
    private void NavigateNextWeek()
    {
        SelectedDate = SelectedDate.AddDays(7);
    }

    [RelayCommand]
    private void Confirm()
    {
        Confirmed?.Invoke(this, EventArgs.Empty);
    }
    
    [RelayCommand]
    private void Cancel()
    {
        Cancelled?.Invoke(this, EventArgs.Empty);
    }
    
    public event EventHandler? Confirmed;
    public event EventHandler? Cancelled;
}

public partial class HourLineViewModel : ObservableObject
{
    public int Hour { get; }
    
    public string Label { get; }
    
    public double YPosition { get; }
    
    public bool IsMajor => Hour % 3 == 0;

    public HourLineViewModel(int hour, double pixelsPerHour)
    {
        Hour = hour;
        YPosition = hour * pixelsPerHour;
        var time = new TimeOnly(hour, 0);
        Label = time.ToString("h tt").ToUpperInvariant();
    }
}

