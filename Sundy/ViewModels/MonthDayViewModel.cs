using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Sundy.ViewModels;

public partial class MonthDayViewModel : ObservableObject
{
    public MonthDayViewModel(DateTime date, bool isCurrentMonth, bool isToday, List<EventViewModel> events)
    {
        Date = date;
        Day = date.Day.ToString();
        IsCurrentMonth = isCurrentMonth;
        IsToday = isToday;
        Events = new ObservableCollection<EventViewModel>(events);
    }

    public DateTime Date { get; }
    public string Day { get; }
    public bool IsCurrentMonth { get; }
    public bool IsToday { get; }
    public ObservableCollection<EventViewModel> Events { get; }

    // Return null to use the default background from theme resources
    // Today highlighting will be done in XAML using DynamicResource
    public string? Background => IsCurrentMonth ? null : "#00000000"; // Transparent for non-current month
    public string? DayForeground => null; // Let XAML handle foreground colors
}