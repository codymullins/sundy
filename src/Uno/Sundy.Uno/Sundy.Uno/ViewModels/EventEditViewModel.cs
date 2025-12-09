using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mediator;
using Sundy.Core;
using Sundy.Core.Commands;
using Sundy.Core.Queries;
using Sundy.Uno.ViewModels.Scheduler;

namespace Sundy.Uno.ViewModels;

public partial class EventEditViewModel(
    IMediator mediator,
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

    [ObservableProperty] private DateTimeOffset _startDate = DateTimeOffset.Now.Date;

    [ObservableProperty] private TimeSpan _startTime = TimeSpan.FromHours(9);

    [ObservableProperty] private DateTimeOffset _endDate = DateTimeOffset.Now.Date;

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

            StartDate = existingEvent.StartTime;
            StartTime = existingEvent.StartTime.TimeOfDay;
            EndDate = existingEvent.EndTime;
            EndTime = existingEvent.EndTime.TimeOfDay;

            SelectedCalendar = AvailableCalendars
                .FirstOrDefault(c => c.Id == existingEvent.CalendarId);
        }
        else
        {
            _isEditMode = false;
            DialogTitle = "New Event";
            SaveButtonText = "Create";

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
                var startDateTime = defaultDate.Value.ToDateTime(TimeOnly.FromTimeSpan(TimeSpan.FromHours(9)));
                var endDateTime = defaultDate.Value.ToDateTime(TimeOnly.FromTimeSpan(TimeSpan.FromHours(10)));

                StartDate = new DateTimeOffset(startDateTime);
                StartTime = startDateTime.TimeOfDay;
                EndDate = new DateTimeOffset(endDateTime);
                EndTime = endDateTime.TimeOfDay;
            }
            else
            {
                var now = DateTime.Now;
                var nextHour = now.Date.AddHours(now.Hour + 1);
                StartDate = new DateTimeOffset(nextHour);
                StartTime = nextHour.TimeOfDay;
                EndDate = new DateTimeOffset(nextHour.AddHours(1));
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
            return;
        }

        if (string.IsNullOrWhiteSpace(Title))
        {
            return;
        }

        var startDateTime = CombineDateAndTime(StartDate, StartTime);
        var endDateTime = CombineDateAndTime(EndDate, EndTime);

        if (endDateTime <= startDateTime)
        {
            return;
        }

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
        Scheduler.SelectedDate = DateOnly.FromDateTime(StartDate.DateTime);
        Scheduler.TimeBlock.StartTime = TimeSpanToTimeOnly(StartTime);
        Scheduler.TimeBlock.EndTime = TimeSpanToTimeOnly(EndTime);

        if (OnSchedulerOpenRequested != null)
        {
            await OnSchedulerOpenRequested(this);
        }
    }

    public void ApplySchedulerSelection()
    {
        var selectedDateTime = Scheduler.SelectedDate.ToDateTime(TimeOnly.MinValue);
        StartDate = new DateTimeOffset(selectedDateTime, StartDate.Offset);
        StartTime = Scheduler.TimeBlock.StartTime.ToTimeSpan();
        
        if (Scheduler.TimeBlock.EndTime < Scheduler.TimeBlock.StartTime)
        {
            EndDate = StartDate.AddDays(1);
        }
        else
        {
            EndDate = StartDate;
        }
        EndTime = Scheduler.TimeBlock.EndTime.ToTimeSpan();
    }

    public Func<EventEditViewModel, Task>? OnSchedulerOpenRequested { get; set; }

    private static DateTime CombineDateAndTime(DateTimeOffset date, TimeSpan time)
    {
        return new DateTime(
            date.Year,
            date.Month,
            date.Day,
            time.Hours,
            time.Minutes,
            0,
            DateTimeKind.Local);
    }

    partial void OnIsAllDayChanged(bool value)
    {
        if (value)
        {
            StartTime = TimeSpan.Zero;
            EndTime = TimeSpan.Zero;
            EndDate = StartDate.AddDays(1);

            Scheduler.TimeBlock.StartTime = TimeSpanToTimeOnly(StartTime);
            Scheduler.TimeBlock.EndTime = new TimeOnly(23, 59);
        }
        else
        {
            EndDate = StartDate;
            if (StartTime == TimeSpan.Zero && EndTime == TimeSpan.Zero)
            {
                StartTime = TimeSpan.FromHours(9);
                EndTime = TimeSpan.FromHours(10);
            }

            Scheduler.TimeBlock.StartTime = TimeSpanToTimeOnly(StartTime);
            Scheduler.TimeBlock.EndTime = TimeSpanToTimeOnly(EndTime);
        }
    }

    partial void OnStartDateChanged(DateTimeOffset value)
    {
        if (EndDate < value)
        {
            EndDate = value;
        }

        Scheduler.SelectedDate = DateOnly.FromDateTime(value.DateTime);
    }

    partial void OnStartTimeChanged(TimeSpan value)
    {
        if (StartDate.Date == EndDate.Date && EndTime <= value)
        {
            var newEndTime = value.Add(TimeSpan.FromHours(1));
            EndTime = newEndTime.TotalHours >= 24 ? TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59)) : newEndTime;
        }

        Scheduler.TimeBlock.StartTime = TimeSpanToTimeOnly(value);
    }

    partial void OnEndTimeChanged(TimeSpan value)
    {
        Scheduler.TimeBlock.EndTime = TimeSpanToTimeOnly(value);
    }

    private static TimeOnly TimeSpanToTimeOnly(TimeSpan value)
    {
        if (value.TotalHours >= 24)
            return new TimeOnly(23, 59, 59);
        if (value < TimeSpan.Zero)
            return TimeOnly.MinValue;
        return TimeOnly.FromTimeSpan(value);
    }
}
