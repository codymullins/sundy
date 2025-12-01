using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Sundy.Core;

namespace Sundy.ViewModels;

// Main calendar view
public partial class CalendarViewModel : ObservableObject
{
    private readonly BlockingEngine _blockingEngine;
    private readonly SundyDbContext _db;

    public CalendarViewModel(BlockingEngine blockingEngine, SundyDbContext db)
    {
        _blockingEngine = blockingEngine;
        _db = db;
        InitializeTimeSlots();
    }

    [ObservableProperty]
    private ObservableCollection<Calendar> _calendars = new();

    [ObservableProperty]
    private ObservableCollection<EventViewModel> _visibleEvents = new();

    [ObservableProperty]
    private DateTime _selectedDate = DateTime.Today;

    [ObservableProperty]
    private Calendar? _selectedCalendar;

    [ObservableProperty]
    private CalendarViewMode _viewMode = CalendarViewMode.Month;

    [ObservableProperty]
    private ObservableCollection<MonthDayViewModel> _monthDays = new();

    [ObservableProperty]
    private ObservableCollection<TimeSlotViewModel> _timeSlots = new();

    [ObservableProperty]
    private ObservableCollection<WeekDayViewModel> _weekDays = new();

    // View mode helpers
    public bool IsMonthView => ViewMode == CalendarViewMode.Month;
    public bool IsWeekView => ViewMode == CalendarViewMode.Week;
    public bool IsDayView => ViewMode == CalendarViewMode.Day;

    partial void OnViewModeChanged(CalendarViewMode value)
    {
        OnPropertyChanged(nameof(IsMonthView));
        OnPropertyChanged(nameof(IsWeekView));
        OnPropertyChanged(nameof(IsDayView));
        _ = RefreshViewAsync();
    }

    partial void OnSelectedDateChanged(DateTime value)
    {
        _ = RefreshViewAsync();
    }

    private void InitializeTimeSlots()
    {
        TimeSlots.Clear();
        for (int hour = 0; hour < 24; hour++)
        {
            TimeSlots.Add(new TimeSlotViewModel(hour));
        }
    }

    public async Task LoadCalendarsAsync()
    {
        var cals = await _db.Calendars.ToListAsync();
        Calendars = new ObservableCollection<Calendar>(cals);
    }

    public async Task RefreshViewAsync()
    {
        switch (ViewMode)
        {
            case CalendarViewMode.Month:
                await LoadMonthViewAsync();
                break;
            case CalendarViewMode.Week:
                await LoadWeekViewAsync();
                break;
            case CalendarViewMode.Day:
                await LoadDayViewAsync();
                break;
        }
    }

    private async Task LoadMonthViewAsync()
    {
        var firstOfMonth = new DateTime(SelectedDate.Year, SelectedDate.Month, 1);
        var lastOfMonth = firstOfMonth.AddMonths(1).AddDays(-1);

        // Find the first Sunday on or before the first of the month
        var startDate = firstOfMonth.AddDays(-(int)firstOfMonth.DayOfWeek);
        // Always show 6 weeks (42 days) for consistent grid
        var endDate = startDate.AddDays(42);

        var events = await _db.Events
             .Where(e => e.StartTime < endDate && e.EndTime > startDate)
             .ToListAsync();

        var calendarLookup = await _db.Calendars.ToDictionaryAsync(c => c.Id);

        var days = new ObservableCollection<MonthDayViewModel>();
        for (var date = startDate; date < endDate; date = date.AddDays(1))
        {
            var dayEvents = events
                .Where(e => e.StartTime.Date <= date && e.EndTime.Date >= date)
                .Select(e => new EventViewModel(e, calendarLookup.GetValueOrDefault(e.CalendarId)))
                .ToList();

            var isCurrentMonth = date.Month == SelectedDate.Month;
            var isToday = date.Date == DateTime.Today;

            days.Add(new MonthDayViewModel(date, isCurrentMonth, isToday, dayEvents));
        }

        MonthDays = days;
    }

    private async Task LoadWeekViewAsync()
    {
        // Find Sunday of the current week
        var startOfWeek = SelectedDate.AddDays(-(int)SelectedDate.DayOfWeek);
        var endOfWeek = startOfWeek.AddDays(7);

        var events = await _db.Events
            .Where(e => e.StartTime < endOfWeek && e.EndTime > startOfWeek)
            .ToListAsync();

        var calendarLookup = await _db.Calendars.ToDictionaryAsync(c => c.Id);

        // Build week day headers
        var weekDays = new ObservableCollection<WeekDayViewModel>();
        for (int i = 0; i < 7; i++)
        {
            var date = startOfWeek.AddDays(i);
            weekDays.Add(new WeekDayViewModel(date, date.Date == DateTime.Today));
        }
        WeekDays = weekDays;

        // Build positioned events
        var visibleEvents = events
            .Select(e => new EventViewModel(e, calendarLookup.GetValueOrDefault(e.CalendarId), startOfWeek, ViewMode))
            .ToList();

        VisibleEvents = new ObservableCollection<EventViewModel>(visibleEvents);
    }

    private async Task LoadDayViewAsync()
    {
        var start = SelectedDate.Date;
        var end = start.AddDays(1);

        var events = await _db.Events
            .Where(e => e.StartTime < end && e.EndTime > start)
            .ToListAsync();

        var calendarLookup = await _db.Calendars.ToDictionaryAsync(c => c.Id);

        var visibleEvents = events
            .Select(e => new EventViewModel(e, calendarLookup.GetValueOrDefault(e.CalendarId), start, ViewMode))
            .ToList();

        VisibleEvents = new ObservableCollection<EventViewModel>(visibleEvents);
    }

    [RelayCommand]
    private void NextPeriod()
    {
        SelectedDate = ViewMode switch
        {
            CalendarViewMode.Month => SelectedDate.AddMonths(1),
            CalendarViewMode.Week => SelectedDate.AddDays(7),
            CalendarViewMode.Day => SelectedDate.AddDays(1),
            _ => SelectedDate
        };
    }

    [RelayCommand]
    private void PreviousPeriod()
    {
        SelectedDate = ViewMode switch
        {
            CalendarViewMode.Month => SelectedDate.AddMonths(-1),
            CalendarViewMode.Week => SelectedDate.AddDays(-7),
            CalendarViewMode.Day => SelectedDate.AddDays(-1),
            _ => SelectedDate
        };
    }

    [RelayCommand]
    private void GoToToday()
    {
        SelectedDate = DateTime.Today;
    }

    [RelayCommand]
    private async Task CreateEvent(CalendarEvent newEvent)
    {
        if (SelectedCalendar == null) return;

        await _blockingEngine.CreateEventWithBlockingAsync(
            SelectedCalendar.Id,
            newEvent);

        await RefreshViewAsync();
    }

    [RelayCommand]
    private async Task UpdateEvent(CalendarEvent updatedEvent)
    {
        await _blockingEngine.UpdateEventWithBlockingAsync(
            updatedEvent.CalendarId,
            updatedEvent);

        await RefreshViewAsync();
    }

    [RelayCommand]
    private async Task DeleteEvent(CalendarEvent evt)
    {
        await _blockingEngine.DeleteEventWithBlockingAsync(
            evt.CalendarId,
            evt.Id);

        await RefreshViewAsync();
    }
}