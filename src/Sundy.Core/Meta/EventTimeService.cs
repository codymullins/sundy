namespace Sundy.Core.Meta;

public class EventTimeService(ITimeZoneProvider timeZoneProvider)
{
    public DateTimeOffset CreateEventTime(DateTime localDate, TimeSpan localTime)
    {
        var localDateTime = new DateTime(
            localDate.Year,
            localDate.Month,
            localDate.Day,
            localTime.Hours,
            localTime.Minutes,
            localTime.Seconds,
            DateTimeKind.Unspecified);

        var tz = timeZoneProvider.GetCurrentTimeZone();
        var offset = tz.GetUtcOffset(localDateTime);
        return new DateTimeOffset(localDateTime, offset);
    }

    public (DateTime Date, TimeSpan Time) GetLocalDateTime(DateTimeOffset utcTime)
    {
        var local = timeZoneProvider.ToLocal(utcTime);
        return (local.DateTime.Date, local.TimeOfDay);
    }

    public DateTime GetLocalDate(DateTimeOffset utcTime)
    {
        return timeZoneProvider.ToLocal(utcTime).DateTime.Date;
    }

    public TimeSpan GetLocalTime(DateTimeOffset utcTime)
    {
        return timeZoneProvider.ToLocal(utcTime).TimeOfDay;
    }
}
