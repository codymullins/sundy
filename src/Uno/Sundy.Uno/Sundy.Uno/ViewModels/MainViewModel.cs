using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mediator;
using Sundy.Core;
using Sundy.Core.Commands;
using Sundy.Core.Queries;
using Sundy.Core.System;

namespace Sundy.Uno.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly EventTimeService _eventTimeService;

    public MainViewModel(
        IMediator mediator,
        CalendarViewModel calendarViewModel,
        EventTimeService eventTimeService)
    {
        _mediator = mediator;
        _eventTimeService = eventTimeService;

        CalendarViewModel = calendarViewModel;
        CalendarViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(CalendarViewModel.SelectedDate)
                or nameof(CalendarViewModel.ViewMode))
            {
                OnPropertyChanged(nameof(CurrentPeriodDisplay));
            }
        };
        CalendarViewModel.EventEditRequested += async (_, evt) => await EditEvent(evt);
        CalendarViewModel.NewEventForDateRequested += async (_, date) => await CreateEventForDate(date);
    }

    [ObservableProperty]
    private CalendarViewModel _calendarViewModel = null!;

    [ObservableProperty]
    private ObservableCollection<CalendarListItemViewModel> _calendars = new();

    [ObservableProperty]
    private bool _isEventDialogOpen;

    [ObservableProperty]
    private EventEditViewModel? _eventEditViewModel;

    [ObservableProperty]
    private bool _isSettingsDialogOpen;

    [ObservableProperty]
    private CalendarSettingsViewModel? _calendarSettingsViewModel;

    public bool HasNoCalendars => Calendars.Count == 0;

    public int ViewModeIndex
    {
        get => CalendarViewModel.ViewMode switch
        {
            CalendarViewMode.Day => 0,
            CalendarViewMode.Week => 1,
            CalendarViewMode.Month => 2,
            _ => 2
        };
        set
        {
            var newMode = value switch
            {
                0 => CalendarViewMode.Day,
                1 => CalendarViewMode.Week,
                2 => CalendarViewMode.Month,
                _ => CalendarViewMode.Month
            };

            if (CalendarViewModel.ViewMode != newMode)
            {
                CalendarViewModel.ViewMode = newMode;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentPeriodDisplay));
            }
        }
    }

    public string CurrentPeriodDisplay
    {
        get
        {
            var date = CalendarViewModel.SelectedDate;
            return CalendarViewModel.ViewMode switch
            {
                CalendarViewMode.Month => date.ToString("MMMM yyyy"),
                CalendarViewMode.Week => GetWeekRangeDisplay(date),
                CalendarViewMode.Day => date.ToString("dddd, MMMM d, yyyy"),
                _ => date.ToString("MMMM yyyy")
            };
        }
    }

    private static string GetWeekRangeDisplay(DateTime date)
    {
        var startOfWeek = date.AddDays(-(int)date.DayOfWeek);
        var endOfWeek = startOfWeek.AddDays(6);

        if (startOfWeek.Month == endOfWeek.Month)
        {
            return $"{startOfWeek:MMM d} - {endOfWeek:d}, {endOfWeek:yyyy}";
        }
        else if (startOfWeek.Year == endOfWeek.Year)
        {
            return $"{startOfWeek:MMM d} - {endOfWeek:MMM d}, {endOfWeek:yyyy}";
        }
        else
        {
            return $"{startOfWeek:MMM d, yyyy} - {endOfWeek:MMM d, yyyy}";
        }
    }

    public async Task InitializeAsync()
    {
        await CalendarViewModel.LoadCalendarsAsync().ConfigureAwait(false);
        await LoadCalendarListAsync().ConfigureAwait(false);
        await CalendarViewModel.RefreshViewAsync().ConfigureAwait(false);
    }

    private async Task LoadCalendarListAsync()
    {
        var calendars = await _mediator.Send(new GetAllCalendarsQuery());

        Calendars.Clear();
        Calendars.AddRange(calendars.Select(c => new CalendarListItemViewModel(c, OnCalendarVisibilityChanged)));
        OnPropertyChanged(nameof(Calendars));
        OnPropertyChanged(nameof(HasNoCalendars));
    }

    private async void OnCalendarVisibilityChanged()
    {
        await CalendarViewModel.RefreshViewAsync().ConfigureAwait(false);
    }

    [RelayCommand]
    private void GoToToday()
    {
        CalendarViewModel.GoToTodayCommand.Execute(null);
    }

    [RelayCommand]
    private void PreviousPeriod()
    {
        CalendarViewModel.PreviousPeriodCommand.Execute(null);
    }

    [RelayCommand]
    private void NextPeriod()
    {
        CalendarViewModel.NextPeriodCommand.Execute(null);
    }

    [RelayCommand]
    private async Task OpenSettings()
    {
        var settingsVm = new CalendarSettingsViewModel(
            _mediator,
            onClosed: async () =>
            {
                IsSettingsDialogOpen = false;
                CalendarSettingsViewModel = null;
                await LoadCalendarListAsync().ConfigureAwait(false);
                await CalendarViewModel.LoadCalendarsAsync().ConfigureAwait(false);
                await CalendarViewModel.RefreshViewAsync().ConfigureAwait(false);
            });

        await settingsVm.LoadCalendarsAsync().ConfigureAwait(false);
        CalendarSettingsViewModel = settingsVm;
        IsSettingsDialogOpen = true;
    }

    [RelayCommand]
    private async Task CreateEvent()
    {
        var editVm = new EventEditViewModel(
            _mediator,
            _eventTimeService,
            onSaved: OnEventSaved,
            onCancelled: () =>
            {
                IsEventDialogOpen = false;
                EventEditViewModel = null;
            });

        await editVm.InitializeAsync(
            existingEvent: null,
            defaultCalendarId: CalendarViewModel.SelectedCalendar?.Id).ConfigureAwait(false);

        EventEditViewModel = editVm;
        IsEventDialogOpen = true;
    }

    private async Task CreateEventForDate(DateTime date)
    {
        var editVm = new EventEditViewModel(
            _mediator,
            _eventTimeService,
            onSaved: OnEventSaved,
            onCancelled: () =>
            {
                IsEventDialogOpen = false;
                EventEditViewModel = null;
            });

        await editVm.InitializeAsync(
            existingEvent: null,
            defaultCalendarId: CalendarViewModel.SelectedCalendar?.Id,
            defaultDate: DateOnly.FromDateTime(date)).ConfigureAwait(false);

        EventEditViewModel = editVm;
        IsEventDialogOpen = true;
    }

    private async Task EditEvent(CalendarEvent evt)
    {
        var editVm = new EventEditViewModel(
            _mediator,
            _eventTimeService,
            onSaved: OnEventSaved,
            onCancelled: () =>
            {
                IsEventDialogOpen = false;
                EventEditViewModel = null;
            });

        await editVm.InitializeAsync(
            existingEvent: evt,
            defaultCalendarId: evt.CalendarId).ConfigureAwait(false);

        EventEditViewModel = editVm;
        IsEventDialogOpen = true;
    }

    private async void OnEventSaved()
    {
        IsEventDialogOpen = false;
        EventEditViewModel = null;
        await CalendarViewModel.RefreshViewAsync().ConfigureAwait(false);
    }

    [RelayCommand]
    private void CloseEventDialog()
    {
        IsEventDialogOpen = false;
        EventEditViewModel = null;
    }

    [RelayCommand]
    private async Task CloseSettingsDialog()
    {
        try
        {
            IsSettingsDialogOpen = false;
            CalendarSettingsViewModel = null;
            await LoadCalendarListAsync().ConfigureAwait(false);
            await CalendarViewModel.LoadCalendarsAsync().ConfigureAwait(false);
            await CalendarViewModel.RefreshViewAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error closing settings: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task CreateNewCalendar()
    {
        var calendar = new Calendar
        {
            Id = Guid.NewGuid().ToString(),
            Name = "My Calendar",
            Color = "#4A90E2",
            Type = CalendarType.Local,
            EnableBlocking = true,
            ReceiveBlocks = true
        };
        await _mediator.Send(new CreateCalendarCommand(calendar)).ConfigureAwait(false);

        await LoadCalendarListAsync().ConfigureAwait(false);
        await CalendarViewModel.LoadCalendarsAsync().ConfigureAwait(false);
        await CalendarViewModel.RefreshViewAsync().ConfigureAwait(false);
    }
}

public partial class CalendarListItemViewModel(Calendar calendar, Action? onVisibilityChanged = null) : ObservableObject
{
    public string Name => calendar.Name;
    public string Color => calendar.Color;

    public bool IsVisible
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                onVisibilityChanged?.Invoke();
            }
        }
    } = true;
}
