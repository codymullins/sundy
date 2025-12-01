﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sundy.Core;

namespace Sundy.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly SundyDbContext _db;
    private readonly BlockingEngine _blockingEngine;
    private readonly ILogger<MainViewModel> _logger;

    public MainViewModel(
        SundyDbContext db,
        BlockingEngine blockingEngine,
        ILogger<MainViewModel> logger)
    {
        _db = db;
        _blockingEngine = blockingEngine;
        _logger = logger;

        CalendarViewModel = new CalendarViewModel(blockingEngine, db);
        CalendarViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(CalendarViewModel.SelectedDate) 
                or nameof(CalendarViewModel.ViewMode))
            {
                OnPropertyChanged(nameof(CurrentPeriodDisplay));
            }
        };
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

    public bool HasNoCalendars => Calendars?.Count == 0;

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
        await CalendarViewModel.LoadCalendarsAsync();
        await LoadCalendarListAsync();
        await CalendarViewModel.RefreshViewAsync();
    }

    private async Task LoadCalendarListAsync()
    {
        var calendars = await _db.Calendars.ToListAsync();
        Calendars = new ObservableCollection<CalendarListItemViewModel>(
            calendars.Select(c => new CalendarListItemViewModel(c, OnCalendarVisibilityChanged)));
        OnPropertyChanged(nameof(HasNoCalendars));
    }

    private async void OnCalendarVisibilityChanged()
    {
        await CalendarViewModel.RefreshViewAsync();
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
            _db,
            onClosed: async () =>
            {
                IsSettingsDialogOpen = false;
                CalendarSettingsViewModel = null;
                await LoadCalendarListAsync();
                await CalendarViewModel.LoadCalendarsAsync();
                await CalendarViewModel.RefreshViewAsync();
            });
        
        await settingsVm.LoadCalendarsAsync();
        CalendarSettingsViewModel = settingsVm;
        IsSettingsDialogOpen = true;
    }

    [RelayCommand]
    private async Task CreateEvent()
    {
        var editVm = new EventEditViewModel(
            _db,
            _blockingEngine,
            onSaved: OnEventSaved,
            onCancelled: () =>
            {
                IsEventDialogOpen = false;
                EventEditViewModel = null;
            });
        
        await editVm.InitializeAsync(
            existingEvent: null,
            defaultCalendarId: CalendarViewModel.SelectedCalendar?.Id);
        
        EventEditViewModel = editVm;
        IsEventDialogOpen = true;
    }

    private async void OnEventSaved()
    {
        IsEventDialogOpen = false;
        EventEditViewModel = null;
        await CalendarViewModel.RefreshViewAsync();
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
            await LoadCalendarListAsync();
            await CalendarViewModel.LoadCalendarsAsync();
            await CalendarViewModel.RefreshViewAsync();
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
            Color = "#4A90E2", // Default blue
            Type = CalendarType.Local,
            EnableBlocking = true,
            ReceiveBlocks = true
        };
        
        _db.Calendars.Add(calendar);
        await _db.SaveChangesAsync();
        
        await LoadCalendarListAsync();
        await CalendarViewModel.LoadCalendarsAsync();
        await CalendarViewModel.RefreshViewAsync();
    }
}

// ViewModel for calendar items in the sidebar list
public partial class CalendarListItemViewModel : ObservableObject
{
    private readonly Calendar _calendar;
    private readonly Action? _onVisibilityChanged;
    private bool _isVisible = true;

    public CalendarListItemViewModel(Calendar calendar, Action? onVisibilityChanged = null)
    {
        _calendar = calendar;
        _onVisibilityChanged = onVisibilityChanged;
    }

    public string Id => _calendar.Id;
    public string Name => _calendar.Name;
    public string Color => _calendar.Color;

    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            if (SetProperty(ref _isVisible, value))
            {
                _onVisibilityChanged?.Invoke();
            }
        }
    }
}