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
/// Migrated from Avalonia - same logic preserved, scheduler integration simplified.
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
    }

    partial void OnStartDateChanged(DateTimeOffset value)
    {
        if (EndDate < value)
            EndDate = value;
    }

    partial void OnStartTimeChanged(TimeSpan value)
    {
        if (StartDate.Date == EndDate.Date && EndTime <= value)
        {
            var newEndTime = value.Add(TimeSpan.FromHours(1));
            EndTime = newEndTime.TotalHours >= 24 ? TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59)) : newEndTime;
        }
    }
}
