using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mediator;
using Serilog;
using Sundy.Core;
using Sundy.Core.Commands;
using Sundy.Core.Queries;

namespace Sundy.ViewModels;

public partial class CalendarSettingsViewModel(IMediator mediator, Func<Task>? onClosed = null) : ObservableObject
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

        await mediator.Send(new CreateCalendarCommand(calendar)).ConfigureAwait(false);

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

            await mediator.Send(new DeleteCalendarCommand(calendarId)).ConfigureAwait(false);

            // Remove from UI collection
            Calendars.Remove(CalendarToDelete);

            IsDeleteConfirmationOpen = false;
            CalendarToDelete = null;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error deleting calendar {CalendarId}", CalendarToDelete?.Id);
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
            await mediator.Send(new ResetDatabaseCommand());
            Calendars.Clear();

            IsResetConfirmationOpen = false;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error resetting database");
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
        var cals = await mediator.Send(new GetAllCalendarsQuery());
        Calendars = new ObservableCollection<CalendarItemViewModel>(cals.Select(p =>
            new CalendarItemViewModel(p, RequestDeleteCalendar)));
    }
}