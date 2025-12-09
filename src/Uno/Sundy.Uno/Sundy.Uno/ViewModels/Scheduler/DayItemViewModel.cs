using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Sundy.Uno.ViewModels.Scheduler;

public partial class DayItemViewModel : ObservableObject
{
    public DateOnly Date { get; }
    
    public string DayOfWeekShort { get; }
    
    public int DayNumber { get; }
    
    [ObservableProperty]
    private bool _isSelected;
    
    public bool IsToday { get; }

    public DayItemViewModel(DateOnly date, bool isSelected = false)
    {
        Date = date;
        DayOfWeekShort = date.DayOfWeek.ToString()[..1].ToUpperInvariant();
        DayNumber = date.Day;
        IsSelected = isSelected;
        IsToday = date == DateOnly.FromDateTime(DateTime.Today);
    }
}
