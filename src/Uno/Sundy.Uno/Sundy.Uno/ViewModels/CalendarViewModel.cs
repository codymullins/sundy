using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mediator;
using Sundy.Core;
using Sundy.Core.Commands;
using Sundy.Core.Queries;

namespace Sundy.Uno.ViewModels;

public partial class CalendarViewModel : ObservableObject
{
    private readonly IMediator _mediator;
    private readonly object _refreshLock = new();
    private readonly SemaphoreSlim _refreshGate = new(1, 1);
    private bool _refreshQueued;
    private Task _refreshProcessing = Task.CompletedTask;

    public CalendarViewModel(IMediator mediator)
    {
        _mediator = mediator;
        InitializeTimeSlots();
    }

    public event EventHandler<CalendarEvent>? EventEditRequested;

    public event EventHandler<DateTime>? NewEventForDateRequested;

    [ObservableProperty] private ObservableCollection<Calendar> _calendars = new();

    [ObservableProperty] private ObservableCollection<EventViewModel> _visibleEvents = new();

    [ObservableProperty] private DateTime _selectedDate = DateTime.Today;

    [ObservableProperty] private Calendar? _selectedCalendar;

    [ObservableProperty] private CalendarViewMode _viewMode = CalendarViewMode.Month;

    [ObservableProperty] private ObservableCollection<MonthDayViewModel> _monthDays = new();

    [ObservableProperty] private ObservableCollection<TimeSlotViewModel> _timeSlots = new();

    [ObservableProperty] private ObservableCollection<WeekDayViewModel> _weekDays = new();

    [ObservableProperty] private EventViewModel? _selectedEvent;

    partial void OnSelectedEventChanged(EventViewModel? value)
    {
        if (value != null)
        {
            SelectionChanged?.Invoke(this, value);
        }
    }

    public event EventHandler<EventViewModel>? SelectionChanged;

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
        var cals = await _mediator.Send(new GetAllCalendarsQuery());
        Calendars = new ObservableCollection<Calendar>(cals);
    }

    public Task RefreshViewAsync()
    {
        lock (_refreshLock)
        {
            _refreshQueued = true;

            if (_refreshProcessing.IsCompleted)
            {
                _refreshProcessing = ProcessRefreshQueueAsync();
            }

            return _refreshProcessing;
        }
    }

    private async Task ProcessRefreshQueueAsync()
    {
        while (true)
        {
            lock (_refreshLock)
            {
                if (!_refreshQueued)
                {
                    _refreshProcessing = Task.CompletedTask;
                    return;
                }

                _refreshQueued = false;
            }

            await _refreshGate.WaitAsync().ConfigureAwait(false);
            try
            {
                await RefreshViewCoreAsync().ConfigureAwait(false);
            }
            finally
            {
                _refreshGate.Release();
            }
        }
    }

    private Task RefreshViewCoreAsync() =>
        ViewMode switch
        {
            CalendarViewMode.Month => LoadMonthViewAsync(),
            CalendarViewMode.Week => LoadWeekViewAsync(),
            CalendarViewMode.Day => LoadDayViewAsync(),
            _ => Task.CompletedTask
        };

    private async Task LoadMonthViewAsync()
    {
        var firstOfMonth = new DateTime(SelectedDate.Year, SelectedDate.Month, 1);
        var startDate = firstOfMonth.AddDays(-(int)firstOfMonth.DayOfWeek);
        var endDate = startDate.AddDays(42);

        var startRange = new DateTimeOffset(startDate, DateTimeOffset.Now.Offset);
        var endRange = new DateTimeOffset(endDate, DateTimeOffset.Now.Offset);

        var events = await _mediator.Send(new GetEventsInRangeQuery(
            startRange,
            endRange,
            SelectedCalendar?.Id));

        var calendarLookup = await _mediator.Send(new GetCalendarLookupQuery());

        var days = new ObservableCollection<MonthDayViewModel>();
        var dayIndex = 0;
        for (var date = startDate; date < endDate; date = date.AddDays(1))
        {
            var dayEvents = events
                .Where(e => e.StartTime.Date <= date && e.EndTime.Date >= date)
                .Select(e =>
                {
                    var eventVm = new EventViewModel(e, calendarLookup.GetValueOrDefault(e.CalendarId));
                    eventVm.EditRequested += OnEventEditRequested;
                    return eventVm;
                })
                .ToList();

            var isCurrentMonth = date.Month == SelectedDate.Month;
            var isToday = date.Date == DateTime.Today;

            days.Add(new MonthDayViewModel(date, isCurrentMonth, isToday, dayEvents, dayIndex, OnMonthDayEventSelected));
            dayIndex++;
        }

        MonthDays = days;
    }

    private void OnMonthDayEventSelected(MonthDayViewModel sourceDay, EventViewModel selectedEvent)
    {
        foreach (var day in MonthDays)
        {
            if (day != sourceDay && day.SelectedEvent != null)
            {
                day.SelectedEvent = null;
            }
        }

        if (SelectedEvent != null && SelectedEvent != selectedEvent)
        {
            SelectedEvent.IsSelected = false;
        }

        selectedEvent.IsSelected = true;
        SelectedEvent = selectedEvent;
    }

    private async Task LoadWeekViewAsync()
    {
        var startOfWeek = SelectedDate.AddDays(-(int)SelectedDate.DayOfWeek);
        var endOfWeek = startOfWeek.AddDays(7);

        var startRange = new DateTimeOffset(startOfWeek, DateTimeOffset.Now.Offset);
        var endRange = new DateTimeOffset(endOfWeek, DateTimeOffset.Now.Offset);

        var events = await _mediator.Send(new GetEventsInRangeQuery(
            startRange,
            endRange,
            SelectedCalendar?.Id));

        var calendarLookup = await _mediator.Send(new GetCalendarLookupQuery());

        var weekDays = new ObservableCollection<WeekDayViewModel>();
        for (int i = 0; i < 7; i++)
        {
            var date = startOfWeek.AddDays(i);
            weekDays.Add(new WeekDayViewModel(date, date.Date == DateTime.Today));
        }

        WeekDays = weekDays;

        var visibleEvents = events
            .Select(e =>
            {
                var eventVm = new EventViewModel(e, calendarLookup.GetValueOrDefault(e.CalendarId), startOfWeek,
                    ViewMode);
                eventVm.EditRequested += OnEventEditRequested;
                eventVm.Selected += OnEventSelected;
                return eventVm;
            })
            .ToList();

        VisibleEvents = new ObservableCollection<EventViewModel>(visibleEvents);
    }

    private async Task LoadDayViewAsync()
    {
        var start = SelectedDate.Date;
        var end = start.AddDays(1);

        var startRange = new DateTimeOffset(start, DateTimeOffset.Now.Offset);
        var endRange = new DateTimeOffset(end, DateTimeOffset.Now.Offset);

        var events = await _mediator.Send(new GetEventsInRangeQuery(
            startRange,
            endRange,
            SelectedCalendar?.Id));

        var calendarLookup = await _mediator.Send(new GetCalendarLookupQuery());

        var visibleEvents = events
            .Select(e =>
            {
                var eventVm = new EventViewModel(e, calendarLookup.GetValueOrDefault(e.CalendarId), start, ViewMode);
                eventVm.EditRequested += OnEventEditRequested;
                eventVm.Selected += OnEventSelected;
                return eventVm;
            })
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

        await _mediator.Send(new CreateEventCommand(newEvent), CancellationToken.None).ConfigureAwait(false);

        _ = RefreshViewAsync().ConfigureAwait(false);
    }

    [RelayCommand]
    private async Task UpdateEvent(CalendarEvent updatedEvent)
    {
        await _mediator.Send(new UpdateEventCommand(updatedEvent), CancellationToken.None)
            .ConfigureAwait(false);
        _ = RefreshViewAsync().ConfigureAwait(false);
    }

    [RelayCommand]
    private async Task DeleteEvent(CalendarEvent evt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(evt.Id);
        await _mediator.Send(new DeleteEventCommand(evt.Id), CancellationToken.None).ConfigureAwait(false);

        _ = RefreshViewAsync().ConfigureAwait(false);
    }

    [RelayCommand]
    private void RequestNewEventForDate(DateTime date)
    {
        NewEventForDateRequested?.Invoke(this, date);
    }

    private void OnEventEditRequested(object? sender, CalendarEvent evt)
    {
        EventEditRequested?.Invoke(this, evt);
    }

    private void OnEventSelected(object? sender, EventArgs e)
    {
        if (sender is EventViewModel eventVm)
        {
            if (SelectedEvent != null && SelectedEvent != eventVm)
            {
                SelectedEvent.IsSelected = false;
            }

            eventVm.IsSelected = true;
            SelectedEvent = eventVm;
        }
    }
}
