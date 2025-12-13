using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mediator;
using Sundy.Core;
using Sundy.Core.Commands;
using Sundy.Core.Queries;
using Sundy.Core.System;
using Sundy.ViewModels.Scheduler;

namespace Sundy.ViewModels;

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
    [ObservableProperty] private SchedulerViewModel _scheduler = new();

    [ObservableProperty] private string _title = string.Empty;

    [ObservableProperty] private ObservableCollection<Calendar> _availableCalendars = [];

    [ObservableProperty] private Calendar? _selectedCalendar;

    [ObservableProperty] private DateTime _startDate = DateTime.Today;

    [ObservableProperty] private TimeSpan _startTime = TimeSpan.FromHours(9);

    [ObservableProperty] private DateTime _endDate = DateTime.Today;

    [ObservableProperty] private TimeSpan _endTime = TimeSpan.FromHours(10);

    [ObservableProperty] private bool _isAllDay;

    [ObservableProperty] private string _location = string.Empty;

    [ObservableProperty] private string _description = string.Empty;

    [ObservableProperty] private bool _isBlockingEvent;

    [ObservableProperty] private string _blockingSourceText = string.Empty;

    [ObservableProperty] private string _dialogTitle = "New Event";

    [ObservableProperty] private string _saveButtonText = "Create";

    public bool IsEditMode => _isEditMode;

    public async Task InitializeAsync(CalendarEvent? existingEvent = null, string? defaultCalendarId = null, DateOnly? defaultDate = null)
    {
        // Load available calendars
        var calendars = await mediator.Send(new GetAllCalendarsQuery()).ConfigureAwait(false);
        AvailableCalendars = new ObservableCollection<Calendar>(calendars);

        if (existingEvent != null)
        {
            // Edit mode
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

            // Set selected calendar
            SelectedCalendar = AvailableCalendars
                .FirstOrDefault(c => c.Id == existingEvent.CalendarId);
        }
        else
        {
            // Create mode
            _isEditMode = false;
            DialogTitle = "New Event";
            SaveButtonText = "Create";

            // Set default calendar
            if (!string.IsNullOrEmpty(defaultCalendarId))
            {
                SelectedCalendar = AvailableCalendars
                    .FirstOrDefault(c => c.Id == defaultCalendarId);
            }
            else
            {
                SelectedCalendar = AvailableCalendars.FirstOrDefault();
            }

            if (defaultDate.HasValue)
            {
                // Use provided date with default times (9-10 AM)
                var startDateTime = defaultDate.Value.ToDateTime(TimeOnly.FromTimeSpan(TimeSpan.FromHours(9)));
                var endDateTime = defaultDate.Value.ToDateTime(TimeOnly.FromTimeSpan(TimeSpan.FromHours(10)));

                StartDate = startDateTime;
                StartTime = startDateTime.TimeOfDay;
                EndDate = endDateTime;
                EndTime = endDateTime.TimeOfDay;
            }
            else
            {
                // Default to 1 hour duration starting next hour
                var now = DateTime.Now;
                var nextHour = now.Date.AddHours(now.Hour + 1);
                StartDate = nextHour;
                StartTime = nextHour.TimeOfDay;
                EndDate = nextHour.AddHours(1);
                EndTime = nextHour.AddHours(1).TimeOfDay;
            }
        }

        OnPropertyChanged(nameof(IsEditMode));
    }

    [RelayCommand(IncludeCancelCommand = true)]
    private async Task Save(CancellationToken ct)
    {
        if (SelectedCalendar == null)
        {
            // TODO: Show validation error
            return;
        }

        if (string.IsNullOrWhiteSpace(Title))
        {
            // TODO: Show validation error
            return;
        }

        var startDateTime = eventTimeService.CreateEventTime(StartDate, StartTime);
        var endDateTime = eventTimeService.CreateEventTime(EndDate, EndTime);

        if (endDateTime <= startDateTime)
        {
            // TODO: Show validation error - end must be after start
            return;
        }

        if (_isEditMode && _originalEvent != null)
        {
            // Update existing event
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
            // Create new event
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

        // TODO: Show confirmation dialog

        await mediator.Send(new DeleteEventCommand(_originalEvent.Id)).ConfigureAwait(false);

        onSaved?.Invoke();
    }

    [RelayCommand]
    private void Cancel()
    {
        onCancelled?.Invoke();
    }

    [RelayCommand]
    private void SelectCalendar(Calendar calendar)
    {
        SelectedCalendar = calendar;
        CalendarSelected?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private async Task OpenScheduler()
    {
        // Initialize scheduler with current event date/time
        Scheduler.SelectedDate = DateOnly.FromDateTime(StartDate);
        Scheduler.TimeBlock.StartTime = TimeSpanToTimeOnly(StartTime);
        Scheduler.TimeBlock.EndTime = TimeSpanToTimeOnly(EndTime);

        if (OnSchedulerOpenRequested != null)
        {
            await OnSchedulerOpenRequested(this);
        }
    }

    public void ApplySchedulerSelection()
    {
        // Apply the scheduler selection back to the event
        var selectedDateTime = Scheduler.SelectedDate.ToDateTime(TimeOnly.MinValue);
        StartDate = selectedDateTime;
        StartTime = Scheduler.TimeBlock.StartTime.ToTimeSpan();
        
        // Check if the time block spans midnight
        if (Scheduler.TimeBlock.EndTime < Scheduler.TimeBlock.StartTime)
        {
            // End time is next day
            EndDate = StartDate.AddDays(1);
        }
        else
        {
            // Same day
            EndDate = StartDate;
        }
        EndTime = Scheduler.TimeBlock.EndTime.ToTimeSpan();
    }

    public Func<EventEditViewModel, Task>? OnSchedulerOpenRequested { get; set; }

    partial void OnIsAllDayChanged(bool value)
    {
        if (value)
        {
            // Set to full day (midnight to midnight next day)
            StartTime = TimeSpan.Zero;
            EndTime = TimeSpan.Zero; // Midnight
            EndDate = StartDate.AddDays(1); // Next day

            // Update scheduler display
            Scheduler.TimeBlock.StartTime = TimeSpanToTimeOnly(StartTime);
            Scheduler.TimeBlock.EndTime = new TimeOnly(23, 59); // Show as end of day in UI
        }
        else
        {
            // Reset to same day with 1 hour duration
            EndDate = StartDate;
            if (StartTime == TimeSpan.Zero && EndTime == TimeSpan.Zero)
            {
                StartTime = TimeSpan.FromHours(9);
                EndTime = TimeSpan.FromHours(10);
            }

            // Update scheduler display
            Scheduler.TimeBlock.StartTime = TimeSpanToTimeOnly(StartTime);
            Scheduler.TimeBlock.EndTime = TimeSpanToTimeOnly(EndTime);
        }
    }

    partial void OnStartDateChanged(DateTime value)
    {
        // If end date is before start date, adjust it
        if (EndDate < value)
        {
            EndDate = value;
        }

        // Update scheduler display
        Scheduler.SelectedDate = DateOnly.FromDateTime(value);
    }

    partial void OnStartTimeChanged(TimeSpan value)
    {
        // If end time is before start time on same day, adjust it
        if (StartDate.Date == EndDate.Date && EndTime <= value)
        {
            var newEndTime = value.Add(TimeSpan.FromHours(1));
            // Clamp to max valid TimeOnly value if it exceeds 24 hours
            EndTime = newEndTime.TotalHours >= 24 ? TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59)) : newEndTime;
        }

        // Update scheduler display - safely convert TimeSpan to TimeOnly
        Scheduler.TimeBlock.StartTime = TimeSpanToTimeOnly(value);
    }

    partial void OnEndTimeChanged(TimeSpan value)
    {
        // Update scheduler display - safely convert TimeSpan to TimeOnly
        Scheduler.TimeBlock.EndTime = TimeSpanToTimeOnly(value);
    }

    private static TimeOnly TimeSpanToTimeOnly(TimeSpan value)
    {
        // Clamp TimeSpan to valid TimeOnly range (0 to 23:59:59)
        if (value.TotalHours >= 24)
            return new TimeOnly(23, 59, 59);
        if (value < TimeSpan.Zero)
            return TimeOnly.MinValue;
        return TimeOnly.FromTimeSpan(value);
    }
}
