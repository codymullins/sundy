namespace Sundy.Test.TestHelpers;

public static class DateTimeTestHelpers
{
    public static void AssertDateTimeOffsetEqual(
        DateTimeOffset expected,
        DateTimeOffset actual,
        TimeSpan? tolerance = null)
    {
        var actualTolerance = tolerance ?? TimeSpan.FromMilliseconds(100);
        var difference = (expected - actual).Duration();

        Assert.True(
            difference <= actualTolerance,
            $"Expected: {expected:O}, Actual: {actual:O}, Difference: {difference.TotalMilliseconds}ms");
    }

    public static DateTime CreateMidnightDateTime(DateTime date)
    {
        return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Local);
    }

    public static DateTime CreateDateTimeAtTime(DateTime date, int hour, int minute, int second = 0)
    {
        return new DateTime(date.Year, date.Month, date.Day, hour, minute, second, DateTimeKind.Local);
    }

    public static DateTime GetWeekStartSunday(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Sunday)) % 7;
        return date.AddDays(-diff).Date;
    }

    public static DateTime GetWeekEndSaturday(DateTime date)
    {
        return GetWeekStartSunday(date).AddDays(6);
    }

    public static DateTime GetMonthViewStartDate(DateTime date)
    {
        // Get first day of month
        var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);

        // Get the Sunday before or equal to the first day
        return GetWeekStartSunday(firstDayOfMonth);
    }

    public static List<DateTime> GetMonthViewDates(DateTime date)
    {
        var startDate = GetMonthViewStartDate(date);
        var dates = new List<DateTime>();

        for (int i = 0; i < 42; i++) // 6 weeks * 7 days
        {
            dates.Add(startDate.AddDays(i));
        }

        return dates;
    }

    public static bool IsTimeEqual(TimeOnly time1, TimeOnly time2, TimeSpan? tolerance = null)
    {
        var actualTolerance = tolerance ?? TimeSpan.FromSeconds(1);
        var diff = (time1.ToTimeSpan() - time2.ToTimeSpan()).Duration();
        return diff <= actualTolerance;
    }
}
