using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mediator;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Serilog;
using Sundy.Core;
using Sundy.Core.Calendars.Outlook;
using Sundy.Core.Commands;
using Sundy.Core.Queries;
using Sundy.Core.Meta;

namespace Sundy.Uno.ViewModels;

/// <summary>
/// Main ViewModel for the Sundy calendar application.
/// Migrated from Avalonia - uses same CommunityToolkit.Mvvm pattern.
/// Updated to use ContentDialog instead of overlay flags.
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly EventTimeService _eventTimeService;
    private readonly OutlookCalendarProvider _outlookProvider;
    private readonly MicrosoftGraphAuthService _authService;
    private readonly Services.IClipboardService _clipboardService;
    private readonly Services.IXamlRootProvider _xamlRootProvider;

    public MainViewModel(
        IMediator mediator,
        CalendarViewModel calendarViewModel,
        EventTimeService eventTimeService,
        OutlookCalendarProvider outlookProvider,
        MicrosoftGraphAuthService authService,
        Services.IClipboardService clipboardService,
        Services.IXamlRootProvider xamlRootProvider)
    {
        _mediator = mediator;
        _eventTimeService = eventTimeService;
        _outlookProvider = outlookProvider;
        _authService = authService;
        _clipboardService = clipboardService;
        _xamlRootProvider = xamlRootProvider;

        CalendarViewModel = calendarViewModel;
        CalendarViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(CalendarViewModel.SelectedDate)
                or nameof(CalendarViewModel.ViewMode))
            {
                OnPropertyChanged(nameof(CurrentPeriodDisplay));
            }
            if (e.PropertyName == nameof(CalendarViewModel.ViewMode))
            {
                OnPropertyChanged(nameof(IsDayView));
                OnPropertyChanged(nameof(IsWeekView));
                OnPropertyChanged(nameof(IsMonthView));
            }
        };
        CalendarViewModel.EventEditRequested += async (_, evt) => await EditEvent(evt);
        CalendarViewModel.NewEventForDateRequested += async (_, date) => await CreateEventForDate(date);
    }

    [ObservableProperty] private CalendarViewModel _calendarViewModel = null!;
    [ObservableProperty] private ObservableCollection<CalendarListItemViewModel> _calendars = [];
    [ObservableProperty] private bool _isMobileLayout;
    [ObservableProperty] private bool _isSidebarOpen = true;
    [ObservableProperty] private double _currentWindowWidth;
    [ObservableProperty] private bool _isSidebarPanelVisible = true;

    public bool HasNoCalendars => Calendars.Count == 0;
    public bool IsSidebarVisible => !IsMobileLayout || IsSidebarOpen;
    public double SidebarTranslateX => !IsSidebarOpen ? -292 : 0;
    public bool IsDesktopSidebarVisible => !IsMobileLayout && IsSidebarPanelVisible;
    public double DesktopSidebarTranslateX => IsSidebarPanelVisible ? 0 : -266;

    // View mode properties for RadioButton binding
    public bool IsDayView
    {
        get => CalendarViewModel.ViewMode == CalendarViewMode.Day;
        set { if (value) CalendarViewModel.ViewMode = CalendarViewMode.Day; }
    }

    public bool IsWeekView
    {
        get => CalendarViewModel.ViewMode == CalendarViewMode.Week;
        set { if (value) CalendarViewModel.ViewMode = CalendarViewMode.Week; }
    }

    public bool IsMonthView
    {
        get => CalendarViewModel.ViewMode == CalendarViewMode.Month;
        set { if (value) CalendarViewModel.ViewMode = CalendarViewMode.Month; }
    }

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
            return $"{startOfWeek:MMM d} - {endOfWeek:d}, {endOfWeek:yyyy}";
        else if (startOfWeek.Year == endOfWeek.Year)
            return $"{startOfWeek:MMM d} - {endOfWeek:MMM d}, {endOfWeek:yyyy}";
        else
            return $"{startOfWeek:MMM d, yyyy} - {endOfWeek:MMM d, yyyy}";
    }

    partial void OnIsMobileLayoutChanged(bool value)
    {
        OnPropertyChanged(nameof(IsSidebarVisible));
        OnPropertyChanged(nameof(SidebarTranslateX));
        OnPropertyChanged(nameof(IsDesktopSidebarVisible));
    }

    partial void OnIsSidebarOpenChanged(bool value)
    {
        OnPropertyChanged(nameof(IsSidebarVisible));
        OnPropertyChanged(nameof(SidebarTranslateX));
    }

    partial void OnCurrentWindowWidthChanged(double value)
    {
        var willBeMobile = value < 768;
        if (willBeMobile && !IsMobileLayout)
            IsSidebarOpen = false;
        IsMobileLayout = willBeMobile;
    }

    partial void OnIsSidebarPanelVisibleChanged(bool value)
    {
        OnPropertyChanged(nameof(IsDesktopSidebarVisible));
        OnPropertyChanged(nameof(DesktopSidebarTranslateX));
    }

    public async Task InitializeAsync()
    {
        await _mediator.Send(new InitializeDatabaseCommand()).ConfigureAwait(false);
        await CalendarViewModel.LoadCalendarsAsync().ConfigureAwait(false);
        await LoadCalendarListAsync().ConfigureAwait(false);

        if (HasNoCalendars)
            await CreateNewCalendar().ConfigureAwait(false);

        await CalendarViewModel.RefreshViewAsync().ConfigureAwait(false);
    }

    private async Task LoadCalendarListAsync()
    {
        var calendars = await _mediator.Send(new GetAllCalendarsQuery()).ConfigureAwait(false);
        Calendars = new ObservableCollection<CalendarListItemViewModel>(
            calendars.Select(c => new CalendarListItemViewModel(c, OnCalendarVisibilityChanged)));
        OnPropertyChanged(nameof(HasNoCalendars));

        CalendarViewModel.VisibleCalendarIds = Calendars
            .Where(c => c.IsVisible)
            .Select(c => c.Id)
            .ToList();
    }

    private async void OnCalendarVisibilityChanged()
    {
        CalendarViewModel.VisibleCalendarIds = Calendars
            .Where(c => c.IsVisible)
            .Select(c => c.Id)
            .ToList();
        await CalendarViewModel.RefreshViewAsync().ConfigureAwait(false);
    }

    [RelayCommand]
    private void GoToToday() => CalendarViewModel.GoToTodayCommand.Execute(null);

    [RelayCommand]
    private void PreviousPeriod() => CalendarViewModel.PreviousPeriodCommand.Execute(null);

    [RelayCommand]
    private void NextPeriod() => CalendarViewModel.NextPeriodCommand.Execute(null);

    [RelayCommand]
    private async Task OpenSettings()
    {
        if (_xamlRootProvider.XamlRoot == null) return;

        var dialog = new ContentDialog
        {
            XamlRoot = _xamlRootProvider.XamlRoot
        };

        var settingsVm = new CalendarSettingsViewModel(
            _mediator,
            _outlookProvider,
            _authService,
            _clipboardService,
            onClosed: () => { dialog.Hide(); return Task.CompletedTask; });

        settingsVm.OutlookConnected += async (_, _) =>
        {
            await LoadCalendarListAsync().ConfigureAwait(false);
            await CalendarViewModel.LoadCalendarsAsync().ConfigureAwait(false);
            await CalendarViewModel.RefreshViewAsync().ConfigureAwait(false);
        };

        await settingsVm.LoadCalendarsAsync().ConfigureAwait(false);

        dialog.Content = new Views.CalendarSettingsView { DataContext = settingsVm };

        await dialog.ShowAsync();

        // Refresh after dialog closes
        await LoadCalendarListAsync().ConfigureAwait(false);
        await CalendarViewModel.LoadCalendarsAsync().ConfigureAwait(false);
        await CalendarViewModel.RefreshViewAsync().ConfigureAwait(false);
    }

    [RelayCommand]
    private async Task CreateEvent()
    {
        if (_xamlRootProvider.XamlRoot == null) return;
           var dialog = new ContentDialog
           {
               
               XamlRoot = _xamlRootProvider.XamlRoot
           };
        dialog.Padding = new Thickness(0);
        dialog.BorderThickness = new Thickness(0);
        var editVm = new EventEditViewModel(_mediator, _eventTimeService, onSaved: () => OnEventSaved(dialog), onCancelled: dialog.Hide);
        await editVm.InitializeAsync(existingEvent: null, defaultCalendarId: CalendarViewModel.SelectedCalendar?.Id).ConfigureAwait(false);
        dialog.Content = new Views.EventEditView { DataContext = editVm };
 

        await dialog.ShowAsync();
        await CalendarViewModel.RefreshViewAsync().ConfigureAwait(false);
    }

    private async Task CreateEventForDate(DateTime date)
    {
        if (_xamlRootProvider.XamlRoot == null) return;
        var dialog = new ContentDialog
        {
            XamlRoot = _xamlRootProvider.XamlRoot
        };
        dialog.Padding = new Thickness(0);
        dialog.BorderThickness = new Thickness(0);
        var editVm = new EventEditViewModel(_mediator, _eventTimeService, onSaved: () => OnEventSaved(dialog), onCancelled: dialog.Hide);
        dialog.Content = new Views.EventEditView { DataContext = editVm };

        await editVm.InitializeAsync(existingEvent: null, defaultCalendarId: CalendarViewModel.SelectedCalendar?.Id,
            defaultDate: DateOnly.FromDateTime(date)).ConfigureAwait(false);

        

        await dialog.ShowAsync();
        await CalendarViewModel.RefreshViewAsync().ConfigureAwait(false);
    }

    private async Task EditEvent(CalendarEvent evt)
    {
        if (_xamlRootProvider.XamlRoot == null) return;
           var dialog = new ContentDialog
        {
            XamlRoot = _xamlRootProvider.XamlRoot
        };
        dialog.Padding = new Thickness(0);
        dialog.BorderThickness = new Thickness(0);
        var editVm = new EventEditViewModel(_mediator, _eventTimeService, onSaved: () => OnEventSaved(dialog), onCancelled: dialog.Hide);
        dialog.Content = new Views.EventEditView { DataContext = editVm };

        await editVm.InitializeAsync(existingEvent: evt, defaultCalendarId: evt.CalendarId).ConfigureAwait(false);

     

        await dialog.ShowAsync();
        await CalendarViewModel.RefreshViewAsync().ConfigureAwait(false);
    }

    private async void OnEventSaved(ContentDialog? dialog)
    {
        dialog?.Hide();
        await CalendarViewModel.RefreshViewAsync().ConfigureAwait(false);
    }

    [RelayCommand]
    private async Task CreateNewCalendar()
    {
        try
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
        catch (Exception e)
        {
            Log.Error(e, "Error creating calendar");
        }
    }

    [RelayCommand]
    private void ToggleSidebar() => IsSidebarOpen = !IsSidebarOpen;

    [RelayCommand]
    private void CloseSidebar() => IsSidebarOpen = false;

    [RelayCommand]
    private void ToggleSidebarPanel() => IsSidebarPanelVisible = !IsSidebarPanelVisible;

    [RelayCommand]
    private void SetDayView()
    {
        CalendarViewModel.ViewMode = CalendarViewMode.Day;
        OnPropertyChanged(nameof(IsDayView));
        OnPropertyChanged(nameof(IsWeekView));
        OnPropertyChanged(nameof(IsMonthView));
    }

    [RelayCommand]
    private void SetWeekView()
    {
        CalendarViewModel.ViewMode = CalendarViewMode.Week;
        OnPropertyChanged(nameof(IsDayView));
        OnPropertyChanged(nameof(IsWeekView));
        OnPropertyChanged(nameof(IsMonthView));
    }

    [RelayCommand]
    private void SetMonthView()
    {
        CalendarViewModel.ViewMode = CalendarViewMode.Month;
        OnPropertyChanged(nameof(IsDayView));
        OnPropertyChanged(nameof(IsWeekView));
        OnPropertyChanged(nameof(IsMonthView));
    }

    /// <summary>
    /// Navigates to a specific date (used by macOS menu bar calendar).
    /// </summary>
    public void NavigateToDate(DateTime date)
    {
        CalendarViewModel.SelectedDate = date;
        CalendarViewModel.ViewMode = CalendarViewMode.Day;
        OnPropertyChanged(nameof(IsDayView));
        OnPropertyChanged(nameof(IsWeekView));
        OnPropertyChanged(nameof(IsMonthView));
        OnPropertyChanged(nameof(CurrentPeriodDisplay));
    }
}
