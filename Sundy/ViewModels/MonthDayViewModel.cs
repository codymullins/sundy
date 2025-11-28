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

    public string Background => IsToday ? "#E3F2FD" : (IsCurrentMonth ? "White" : "#F5F5F5");
    public string DayForeground => IsCurrentMonth ? (IsToday ? "#1976D2" : "Black") : "#999";
}