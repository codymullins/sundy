using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mediator;
using Sundy.Core;
using Sundy.Core.Commands;
using Sundy.Core.Queries;
using Sundy.Core.Meta;

namespace Sundy.Uno.ViewModels;

/// <summary>
/// ViewModel for creating and editing calendar events.
/// </summary>
public partial class EventEditViewModel(
    IMediator mediator,
    EventTimeService eventTimeService,
    Action? onSaved = null,
    Action? onCancelled = null)
    : ObservableObject
{
    private CalendarEvent? _originalEvent;
    private bool _isEditMode;

    public event EventHandler? CalendarSelected;
    public event EventHandler? SchedulerRequested;

    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private ObservableCollection<Calendar> _availableCalendars = [];
    [ObservableProperty] private Calendar? _selectedCalendar;
    [ObservableProperty] private DateTimeOffset _startDate = DateTime.Today;
    [ObservableProperty] private TimeSpan _startTime = TimeSpan.FromHours(9);
    [ObservableProperty] private DateTimeOffset _endDate = DateTime.Today;
    [ObservableProperty] private TimeSpan _endTime = TimeSpan.FromHours(10);
    [ObservableProperty] private bool _isAllDay;
    [ObservableProperty] private string _location = string.Empty;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private bool _isBlockingEvent;
    [ObservableProperty] private string _dialogTitle = "New Event";
    [ObservableProperty] private string _saveButtonText = "Create";

    public bool IsEditMode => _isEditMode;

    /// <summary>
    /// Formatted date display (e.g., "Monday, December 22, 2025")
    /// </summary>
    public string FormattedDate => StartDate.DateTime.ToString("dddd, MMMM d, yyyy");

    /// <summary>
    /// Formatted time range display (e.g., "11:00 PM → 12:00 AM")
    /// </summary>
    public string FormattedTimeRange
    {
        get
        {
            if (IsAllDay)
                return "All day";

            var startDateTime = StartDate.DateTime.Date.Add(StartTime);
            var endDateTime = EndDate.DateTime.Date.Add(EndTime);
            return $"{startDateTime:h:mm tt} → {endDateTime:h:mm tt}";
        }
    }

    /// <summary>
    /// Formatted duration display (e.g., "Duration: 1 hr")
    /// </summary>
    public string FormattedDuration
    {
        get
        {
            if (IsAllDay)
            {
                var days = (EndDate.Date - StartDate.Date).Days;
                return days <= 1 ? "All day" : $"Duration: {days} days";
            }

            var startDateTime = StartDate.DateTime.Date.Add(StartTime);
            var endDateTime = EndDate.DateTime.Date.Add(EndTime);
            var duration = endDateTime - startDateTime;

            if (duration.TotalMinutes < 60)
                return $"Duration: {(int)duration.TotalMinutes} min";
            if (duration.TotalHours < 24)
            {
                var hours = (int)duration.TotalHours;
                var minutes = duration.Minutes;
                if (minutes == 0)
                    return $"Duration: {hours} hr";
                return $"Duration: {hours} hr {minutes} min";
            }
            return $"Duration: {duration.TotalHours:F1} hrs";
        }
    }

    public async Task InitializeAsync(CalendarEvent? existingEvent = null, string? defaultCalendarId = null, DateOnly? defaultDate = null)
    {
        var calendars = await mediator.Send(new GetAllCalendarsQuery()).ConfigureAwait(false);
        AvailableCalendars = new ObservableCollection<Calendar>(calendars);

        if (existingEvent != null)
        {
            _isEditMode = true;
            _originalEvent = existingEvent;
            DialogTitle = "Edit Event";
            SaveButtonText = "Save";

            Title = existingEvent.Title ?? string.Empty;
            Location = existingEvent.Location ?? string.Empty;
            Description = existingEvent.Description ?? string.Empty;
            IsBlockingEvent = existingEvent.IsBlockingEvent;

            var (startDate, startTime) = eventTimeService.GetLocalDateTime(existingEvent.StartTime);
            var (endDate, endTime) = eventTimeService.GetLocalDateTime(existingEvent.EndTime);
            StartDate = startDate;
            StartTime = startTime;
            EndDate = endDate;
            EndTime = endTime;

            SelectedCalendar = AvailableCalendars.FirstOrDefault(c => c.Id == existingEvent.CalendarId);
        }
        else
        {
            _isEditMode = false;
            DialogTitle = "New Event";
            SaveButtonText = "Create";

            if (!string.IsNullOrEmpty(defaultCalendarId))
                SelectedCalendar = AvailableCalendars.FirstOrDefault(c => c.Id == defaultCalendarId);
            else
                SelectedCalendar = AvailableCalendars.FirstOrDefault();

            if (defaultDate.HasValue)
            {
                var startDateTime = defaultDate.Value.ToDateTime(TimeOnly.FromTimeSpan(TimeSpan.FromHours(9)));
                var endDateTime = defaultDate.Value.ToDateTime(TimeOnly.FromTimeSpan(TimeSpan.FromHours(10)));

                StartDate = startDateTime;
                StartTime = startDateTime.TimeOfDay;
                EndDate = endDateTime;
                EndTime = endDateTime.TimeOfDay;
            }
            else
            {
                var now = DateTime.Now;
                var nextHour = now.Date.AddHours(now.Hour + 1);
                StartDate = nextHour;
                StartTime = nextHour.TimeOfDay;
                EndDate = nextHour.AddHours(1);
                EndTime = nextHour.AddHours(1).TimeOfDay;
            }
        }

        OnPropertyChanged(nameof(IsEditMode));
        NotifyDateTimeChanged();
    }

    [RelayCommand(IncludeCancelCommand = true)]
    private async Task Save(CancellationToken ct)
    {
        if (SelectedCalendar == null || string.IsNullOrWhiteSpace(Title))
            return;

        var startDateTime = eventTimeService.CreateEventTime(StartDate.DateTime, StartTime);
        var endDateTime = eventTimeService.CreateEventTime(EndDate.DateTime, EndTime);

        if (endDateTime <= startDateTime)
            return;

        if (_isEditMode && _originalEvent != null)
        {
            _originalEvent.Title = Title;
            _originalEvent.StartTime = startDateTime;
            _originalEvent.EndTime = endDateTime;
            _originalEvent.Location = string.IsNullOrWhiteSpace(Location) ? null : Location;
            _originalEvent.Description = string.IsNullOrWhiteSpace(Description) ? null : Description;
            _originalEvent.CalendarId = SelectedCalendar.Id;
            await mediator.Send(new UpdateEventCommand(_originalEvent), ct).ConfigureAwait(false);
        }
        else
        {
            var newEvent = new CalendarEvent
            {
                Id = Guid.NewGuid().ToString(),
                CalendarId = SelectedCalendar.Id,
                Title = Title,
                StartTime = startDateTime,
                EndTime = endDateTime,
                Location = string.IsNullOrWhiteSpace(Location) ? null : Location,
                Description = string.IsNullOrWhiteSpace(Description) ? null : Description,
                IsBlockingEvent = false,
                SourceEventId = null
            };

            await mediator.Send(new CreateEventCommand(newEvent), ct).ConfigureAwait(false);
        }

        onSaved?.Invoke();
    }

    [RelayCommand]
    private async Task Delete()
    {
        if (_originalEvent?.Id == null) return;
        await mediator.Send(new DeleteEventCommand(_originalEvent.Id)).ConfigureAwait(false);
        onSaved?.Invoke();
    }

    [RelayCommand]
    private void Cancel() => onCancelled?.Invoke();

    [RelayCommand]
    private void SelectCalendar(Calendar calendar)
    {
        SelectedCalendar = calendar;
        CalendarSelected?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void OpenScheduler()
    {
        SchedulerRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called when the scheduler confirms new date/time values.
    /// </summary>
    public void ApplySchedulerSelection(DateTimeOffset selectedDate, TimeSpan startTime, TimeSpan endTime)
    {
        StartDate = selectedDate;
        StartTime = startTime;
        EndDate = selectedDate;
        EndTime = endTime;

        // If end time crosses midnight, adjust end date
        if (endTime < startTime)
        {
            EndDate = selectedDate.AddDays(1);
        }

        NotifyDateTimeChanged();
    }

    private void NotifyDateTimeChanged()
    {
        OnPropertyChanged(nameof(FormattedDate));
        OnPropertyChanged(nameof(FormattedTimeRange));
        OnPropertyChanged(nameof(FormattedDuration));
    }

    partial void OnIsAllDayChanged(bool value)
    {
        if (value)
        {
            StartTime = TimeSpan.Zero;
            EndTime = TimeSpan.Zero;
            EndDate = StartDate.AddDays(1);
        }
        else
        {
            EndDate = StartDate;
            if (StartTime == TimeSpan.Zero && EndTime == TimeSpan.Zero)
            {
                StartTime = TimeSpan.FromHours(9);
                EndTime = TimeSpan.FromHours(10);
            }
        }
        NotifyDateTimeChanged();
    }

    partial void OnStartDateChanged(DateTimeOffset value)
    {
        if (EndDate < value)
            EndDate = value;
        NotifyDateTimeChanged();
    }

    partial void OnStartTimeChanged(TimeSpan value)
    {
        if (StartDate.Date == EndDate.Date && EndTime <= value)
        {
            var newEndTime = value.Add(TimeSpan.FromHours(1));
            EndTime = newEndTime.TotalHours >= 24 ? TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59)) : newEndTime;
        }
        NotifyDateTimeChanged();
    }

    partial void OnEndDateChanged(DateTimeOffset value)
    {
        NotifyDateTimeChanged();
    }

    partial void OnEndTimeChanged(TimeSpan value)
    {
        NotifyDateTimeChanged();
    }
}
