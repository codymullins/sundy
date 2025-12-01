using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Sundy.Core;
using Sundy.ViewModels.Scheduler;

namespace Sundy.ViewModels;

public partial class EventEditViewModel(
    SundyDbContext db,
    BlockingEngine blockingEngine,
    Action? onSaved = null,
    Action? onCancelled = null)
    : ObservableObject
{
    private CalendarEvent? _originalEvent;
    private bool _isEditMode;

    public event EventHandler? CalendarSelected;
    [ObservableProperty] private SchedulerViewModel _scheduler = new();

// Sync the scheduler's TimeBlock with your event times
    partial void OnSchedulerChanged(SchedulerViewModel value)
    {
        // When scheduler selection changes, update the event times
        value.TimeBlock.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(TimeBlockViewModel.StartTime))
                StartTime = value.TimeBlock.StartTime.ToTimeSpan();
            if (e.PropertyName == nameof(TimeBlockViewModel.EndTime))
                EndTime = value.TimeBlock.EndTime.ToTimeSpan();
        };
    }

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

    public async Task InitializeAsync(CalendarEvent? existingEvent = null, string? defaultCalendarId = null)
    {
        // Load available calendars
        var calendars = await db.Calendars.ToListAsync();
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

            StartDate = existingEvent.StartTime;
            StartTime = existingEvent.StartTime.TimeOfDay;
            EndDate = existingEvent.EndTime;
            EndTime = existingEvent.EndTime.TimeOfDay;

            // Set selected calendar
            SelectedCalendar = AvailableCalendars
                .FirstOrDefault(c => c.Id == existingEvent.CalendarId);

            // If this is a blocking event, show source info
            if (existingEvent.IsBlockingEvent && !string.IsNullOrEmpty(existingEvent.SourceEventId))
            {
                var sourceEvent = await db.Events.FindAsync(existingEvent.SourceEventId);
                if (sourceEvent != null)
                {
                    var sourceCalendar = await db.Calendars.FindAsync(sourceEvent.CalendarId);
                    BlockingSourceText =
                        $"This is an automatically created blocking event from '{sourceEvent.Title}' on {sourceCalendar?.Name ?? "Unknown Calendar"}";
                }
            }
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

            // Default to 1 hour duration
            var now = DateTime.Now;
            var nextHour = new DateTime(now.Year, now.Month, now.Day, now.Hour + 1, 0, 0);
            StartDate = new DateTimeOffset(nextHour);
            StartTime = nextHour.TimeOfDay;
            EndDate = new DateTimeOffset(nextHour.AddHours(1));
            EndTime = nextHour.AddHours(1).TimeOfDay;
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

        var startDateTime = CombineDateAndTime(StartDate, StartTime);
        var endDateTime = CombineDateAndTime(EndDate, EndTime);

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

            db.Events.Add(newEvent);
        }

        await db.SaveChangesAsync(ct);

        onSaved?.Invoke();
    }

    [RelayCommand]
    private async Task Delete()
    {
        if (_originalEvent == null) return;

        // TODO: Show confirmation dialog

        db.Events.Remove(_originalEvent);
        await db.SaveChangesAsync();

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
    private void OpenScheduler()
    {
        SchedulerOpenRequested?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? SchedulerOpenRequested;

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
            // Set to full day
            StartTime = TimeSpan.Zero;
            EndTime = TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59));
        }
    }

    partial void OnStartDateChanged(DateTimeOffset value)
    {
        // If end date is before start date, adjust it
        if (EndDate < value)
        {
            EndDate = value;
        }
    }

    partial void OnStartTimeChanged(TimeSpan value)
    {
        // If end time is before start time on same day, adjust it
        if (StartDate.Date == EndDate.Date && EndTime <= value)
        {
            EndTime = value.Add(TimeSpan.FromHours(1));
        }
    }
}