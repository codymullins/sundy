using System;

namespace Sundy.Uno.ViewModels;

public class WeekDayViewModel
{
    public WeekDayViewModel(DateTime date, bool isToday)
    {
        DayName = date.ToString("ddd").ToUpper();
        Date = date.Day.ToString();
        IsToday = isToday;
        FullDate = date;
    }

    public string DayName { get; }
    public string Date { get; }
    public bool IsToday { get; }
    public DateTime FullDate { get; }
    
    public string TodayBackground => IsToday ? "#6366F1" : "Transparent";
    public string TodayForeground => IsToday ? "White" : "#B0B0B0";
}
