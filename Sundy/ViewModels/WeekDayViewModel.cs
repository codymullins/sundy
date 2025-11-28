using System;

namespace Sundy.ViewModels;

public class WeekDayViewModel
{
    public WeekDayViewModel(DateTime date, bool isToday)
    {
        DayName = date.ToString("ddd").ToUpper();
        Date = date.Day.ToString();
        IsToday = isToday;
    }

    public string DayName { get; }
    public string Date { get; }
    public bool IsToday { get; }
    public string DateColor => IsToday ? "#1976D2" : "Black";
}