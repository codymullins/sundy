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
    private readonly Func<Task>? _onClosed;

    public CalendarSettingsViewModel(SundyDbContext db, Func<Task>? onClosed = null)
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

    [ObservableProperty]
    private bool _isDeleting;

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
        if (CalendarToDelete == null || IsDeleting) 
            return;

        IsDeleting = true;
        try
        {
            var calendarId = CalendarToDelete.Id;
            
            // Get all event IDs for this calendar first
            var eventIds = await _db.Events
                .Where(e => e.CalendarId == calendarId)
                .Select(e => e.Id)
                .ToListAsync();

            // Delete all blocking relationships involving this calendar or its events
            var blockingRelationships = await _db.BlockingRelationships
                .Where(br => br.SourceCalendarId == calendarId || 
                             eventIds.Contains(br.SourceEventId))
                .ToListAsync();
            
            if (blockingRelationships.Any())
            {
                _db.BlockingRelationships.RemoveRange(blockingRelationships);
            }

            // Delete all blocked events targeting this calendar or its events
            var blockedEvents = await _db.BlockedEvents
                .Where(be => be.TargetCalendarId == calendarId || 
                             eventIds.Contains(be.TargetEventId))
                .ToListAsync();
            
            if (blockedEvents.Any())
            {
                _db.BlockedEvents.RemoveRange(blockedEvents);
            }

            // Delete all events in this calendar
            var events = await _db.Events
                .Where(e => e.CalendarId == calendarId)
                .ToListAsync();
            
            if (events.Any())
            {
                _db.Events.RemoveRange(events);
            }

            // Find and delete the calendar itself
            var calendar = await _db.Calendars
                .FirstOrDefaultAsync(c => c.Id == calendarId);
            
            if (calendar != null)
            {
                _db.Calendars.Remove(calendar);
            }
            
            // Save all changes to the database
            await _db.SaveChangesAsync();

            // Remove from UI collection
            Calendars.Remove(CalendarToDelete);

            IsDeleteConfirmationOpen = false;
            CalendarToDelete = null;
        }
        catch (Exception ex)
        {
            // Log the error
            System.Diagnostics.Debug.WriteLine($"Error deleting calendar: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
        }
        finally
        {
            IsDeleting = false;
            IsDeleteConfirmationOpen = false;
            CalendarToDelete = null;
        }
    }

    [RelayCommand]
    private void CancelDelete()
    {
        IsDeleteConfirmationOpen = false;
        CalendarToDelete = null;
    }

    [RelayCommand]
    private async Task Close()
    {
        if (_onClosed != null)
        {
            await _onClosed();
        }
    }
    
    public async Task LoadCalendarsAsync()
    {
        var cals = await _db.Calendars.ToListAsync();
        Calendars = new ObservableCollection<CalendarItemViewModel>(
            cals.Select(c => new CalendarItemViewModel(c, _db, RequestDeleteCalendar)));
    }
}

