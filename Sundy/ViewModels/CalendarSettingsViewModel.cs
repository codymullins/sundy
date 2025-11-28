using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Sundy.Core;

namespace Sundy.ViewModels;

public partial class CalendarSettingsViewModel(SundyDbContext db) : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<CalendarItemViewModel> _calendars = new();

    [RelayCommand]
    private async Task CreateCalendar(string name)
    {
        var calendar = new Calendar
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Color = "#4A90E2", // Default blue
            Type = CalendarType.Local,
            EnableBlocking = true,
            ReceiveBlocks = true
        };
        
        db.Calendars.Add(calendar);
        await db.SaveChangesAsync();
        
        Calendars.Add(new CalendarItemViewModel(calendar, db));
    }
    
    public async Task LoadCalendarsAsync()
    {
        var cals = await db.Calendars.ToListAsync();
        Calendars = new ObservableCollection<CalendarItemViewModel>(
            cals.Select(c => new CalendarItemViewModel(c, db)));
    }
}