using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Sundy.Core;

namespace Sundy.ViewModels;

public partial class CalendarSettingsViewModel(SundyDbContext db, Func<Task>? onClosed = null) : ObservableObject
{
    [ObservableProperty] private ObservableCollection<CalendarItemViewModel> _calendars = [];

    [ObservableProperty] private bool _isDeleteConfirmationOpen;

    [ObservableProperty] private CalendarItemViewModel? _calendarToDelete;

    [ObservableProperty] private string _newCalendarName = string.Empty;

    [ObservableProperty] private bool _isDeleting;

    [ObservableProperty] private bool _isResetConfirmationOpen;

    [ObservableProperty] private bool _isResettingDatabase;

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

        db.Calendars.Add(calendar);
        await db.SaveChangesAsync();

        Calendars.Add(new CalendarItemViewModel(calendar, RequestDeleteCalendar));
        NewCalendarName = string.Empty;
    }

    [RelayCommand]
    private async Task RequestDeleteCalendar(CalendarItemViewModel calendar, CancellationToken ct)
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
            var eventIds = await db.Events
                .Where(e => e.CalendarId == calendarId)
                .Select(e => e.Id)
                .ToListAsync();

            // Delete all blocking relationships involving this calendar or its events
            var blockingRelationships = await db.BlockingRelationships
                .Where(br => br.SourceCalendarId == calendarId ||
                             eventIds.Contains(br.SourceEventId))
                .ToListAsync();

            if (blockingRelationships.Any())
            {
                db.BlockingRelationships.RemoveRange(blockingRelationships);
            }

            // Delete all blocked events targeting this calendar or its events
            var blockedEvents = await db.BlockedEvents
                .Where(be => be.TargetCalendarId == calendarId ||
                             eventIds.Contains(be.TargetEventId))
                .ToListAsync();

            if (blockedEvents.Any())
            {
                db.BlockedEvents.RemoveRange(blockedEvents);
            }

            // Delete all events in this calendar
            var events = await db.Events
                .Where(e => e.CalendarId == calendarId)
                .ToListAsync();

            if (events.Any())
            {
                db.Events.RemoveRange(events);
            }

            // Find and delete the calendar itself
            var calendar = await db.Calendars
                .FirstOrDefaultAsync(c => c.Id == calendarId);

            if (calendar != null)
            {
                db.Calendars.Remove(calendar);
            }

            // Save all changes to the database
            await db.SaveChangesAsync();

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
        if (onClosed != null)
        {
            await onClosed();
        }
    }

    [RelayCommand]
    private void ResetDatabase()
    {
        IsResetConfirmationOpen = true;
    }

    [RelayCommand]
    private async Task ConfirmReset()
    {
        if (IsResettingDatabase)
            return;

        IsResettingDatabase = true;
        try
        {
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();
            Calendars.Clear();

            IsResetConfirmationOpen = false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error resetting database: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
        }
        finally
        {
            IsResettingDatabase = false;
        }
    }

    [RelayCommand]
    private void CancelReset()
    {
        IsResetConfirmationOpen = false;
    }

    public async Task LoadCalendarsAsync()
    {
        var cals = await db.Calendars.ToListAsync();
        Calendars = new ObservableCollection<CalendarItemViewModel>(cals.Select(p =>
            new CalendarItemViewModel(p, RequestDeleteCalendar)));
    }
}