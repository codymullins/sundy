using System;
using System.Collections.ObjectModel;
using Sundy.Core;

namespace Sundy.ViewModels.DesignData;

public class DesignEventEditViewModel : EventEditViewModel
{
    public DesignEventEditViewModel() : base(null!, null!)
    {
        Title = "Team Standup";
        Location = "Conference Room A";
        Description = "Daily team standup meeting to discuss progress and blockers.";
        
        AvailableCalendars = new ObservableCollection<CalendarItemViewModel>
        {
            new CalendarItemViewModel(
                new Calendar 
                { 
                    Id = "1", 
                    Name = "Work", 
                    Color = "#4A90E2",
                    EnableBlocking = true,
                    ReceiveBlocks = true
                }, 
                null!),
            new CalendarItemViewModel(
                new Calendar 
                { 
                    Id = "2", 
                    Name = "Personal", 
                    Color = "#E74C3C",
                    EnableBlocking = true,
                    ReceiveBlocks = true
                }, 
                null!)
        };
        
        SelectedCalendar = AvailableCalendars[0];
        
        var tomorrow = DateTime.Today.AddDays(1);
        StartDate = new DateTimeOffset(tomorrow.AddHours(9));
        StartTime = TimeSpan.FromHours(9);
        EndDate = new DateTimeOffset(tomorrow.AddHours(10));
        EndTime = TimeSpan.FromHours(10);
        
        DialogTitle = "Edit Event";
        SaveButtonText = "Save";
        IsAllDay = false;
        IsBlockingEvent = false;
    }
}

// Design ViewModel for blocking event
public class DesignBlockingEventEditViewModel : EventEditViewModel
{
    public DesignBlockingEventEditViewModel() : base(null!, null!)
    {
        Title = "🔒 Busy (Team Standup)";
        
        AvailableCalendars = new ObservableCollection<CalendarItemViewModel>
        {
            new CalendarItemViewModel(
                new Calendar 
                { 
                    Id = "2", 
                    Name = "Personal", 
                    Color = "#E74C3C",
                    EnableBlocking = true,
                    ReceiveBlocks = true
                }, 
                null!)
        };
        
        SelectedCalendar = AvailableCalendars[0];
        
        var tomorrow = DateTime.Today.AddDays(1);
        StartDate = new DateTimeOffset(tomorrow.AddHours(9));
        StartTime = TimeSpan.FromHours(9);
        EndDate = new DateTimeOffset(tomorrow.AddHours(10));
        EndTime = TimeSpan.FromHours(10);
        
        DialogTitle = "Edit Event";
        SaveButtonText = "Save";
        IsAllDay = false;
        IsBlockingEvent = true;
        BlockingSourceText = "This is an automatically created blocking event from 'Team Standup' on Work Calendar";
    }
}

public class DesignCalendarSettingsViewModel : CalendarSettingsViewModel
{
    public DesignCalendarSettingsViewModel() : base(null!)
    {
        Calendars = new ObservableCollection<CalendarItemViewModel>
        {
            new CalendarItemViewModel(
                new Calendar 
                { 
                    Id = "1", 
                    Name = "Work", 
                    Color = "#4A90E2",
                    EnableBlocking = true,
                    ReceiveBlocks = true
                }, 
                null!),
            new CalendarItemViewModel(
                new Calendar 
                { 
                    Id = "2", 
                    Name = "Personal", 
                    Color = "#E74C3C",
                    EnableBlocking = true,
                    ReceiveBlocks = false
                }, 
                null!)
        };
    }
}

public class DesignCalendarViewModel : CalendarViewModel
{
    public DesignCalendarViewModel() : base(null!, null!)
    {
        // Sample data for designer
        Calendars = new ObservableCollection<Calendar>
        {
            new Calendar 
            { 
                Id = "1", 
                Name = "Work", 
                Color = "#4A90E2",
                EnableBlocking = true,
                ReceiveBlocks = true
            },
            new Calendar 
            { 
                Id = "2", 
                Name = "Personal", 
                Color = "#E74C3C",
                EnableBlocking = true,
                ReceiveBlocks = true
            }
        };
        
        // VisibleEvents = new ObservableCollection<eve>
        // {
        //     new CalendarEvent
        //     {
        //         Id = "1",
        //         CalendarId = "1",
        //         Title = "Team Meeting",
        //         StartTime = DateTime.Today.AddHours(10),
        //         EndTime = DateTime.Today.AddHours(11),
        //         IsBlockingEvent = false
        //     },
        //     new CalendarEvent
        //     {
        //         Id = "2",
        //         CalendarId = "2",
        //         Title = "🔒 Busy (Team Meeting)",
        //         StartTime = DateTime.Today.AddHours(10),
        //         EndTime = DateTime.Today.AddHours(11),
        //         IsBlockingEvent = true
        //     }
        // };
        //
        SelectedDate = DateTime.Today;
    }
}