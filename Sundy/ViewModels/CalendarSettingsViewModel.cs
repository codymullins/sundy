using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Sundy.Core;

namespace Sundy.ViewModels;

public partial class CalendarSettingsViewModel : ObservableObject
{
    private readonly SundyDbContext _db;
    private readonly Action? _onClosed;

    public CalendarSettingsViewModel(SundyDbContext db, Action? onClosed = null)
    {
        _db = db;
        _onClosed = onClosed;
    }

    [ObservableProperty]
    private ObservableCollection<CalendarItemViewModel> _calendars = new();

    [ObservableProperty]
    private bool _isDeleteConfirmationOpen;

    [ObservableProperty]
    private CalendarItemViewModel? _calendarToDelete;

    [ObservableProperty]
    private string _newCalendarName = string.Empty;

    [RelayCommand]
    private async Task CreateCalendar()
    {
        if (string.IsNullOrWhiteSpace(NewCalendarName))
            return;

        var calendar = new Calendar
        {
            Id = Guid.NewGuid().ToString(),
            Name = NewCalendarName.Trim(),
            Color = "#4A90E2", // Default blue
            Type = CalendarType.Local,
            EnableBlocking = true,
            ReceiveBlocks = true
        };
        
        _db.Calendars.Add(calendar);
        await _db.SaveChangesAsync();
        
        Calendars.Add(new CalendarItemViewModel(calendar, _db, RequestDeleteCalendar));
        NewCalendarName = string.Empty;
    }

    private async Task RequestDeleteCalendar(CalendarItemViewModel calendar)
    {
        CalendarToDelete = calendar;
        IsDeleteConfirmationOpen = true;
    }

    [RelayCommand]
    private async Task ConfirmDelete()
    {
        if (CalendarToDelete == null) return;

        // Find and remove the calendar from the database
        var calendar = await _db.Calendars.FindAsync(CalendarToDelete.Id);
        if (calendar != null)
        {
            // Also delete all events in this calendar
            var events = await _db.Events
                .Where(e => e.CalendarId == calendar.Id)
                .ToListAsync();
            _db.Events.RemoveRange(events);

            // Delete all blocking relationships for this calendar
            var blockingRelationships = await _db.BlockingRelationships
                .Where(br => br.SourceCalendarId == calendar.Id)
                .ToListAsync();
            _db.BlockingRelationships.RemoveRange(blockingRelationships);

            // Delete all blocked events for this calendar
            var blockedEvents = await _db.BlockedEvents
                .Where(be => be.TargetCalendarId == calendar.Id)
                .ToListAsync();
            _db.BlockedEvents.RemoveRange(blockedEvents);

            _db.Calendars.Remove(calendar);
            await _db.SaveChangesAsync();

            Calendars.Remove(CalendarToDelete);
        }

        IsDeleteConfirmationOpen = false;
        CalendarToDelete = null;
    }

    [RelayCommand]
    private void CancelDelete()
    {
        IsDeleteConfirmationOpen = false;
        CalendarToDelete = null;
    }

    [RelayCommand]
    private void Close()
    {
        _onClosed?.Invoke();
    }
    
    public async Task LoadCalendarsAsync()
    {
        var cals = await _db.Calendars.ToListAsync();
        Calendars = new ObservableCollection<CalendarItemViewModel>(
            cals.Select(c => new CalendarItemViewModel(c, _db, RequestDeleteCalendar)));
    }
}

