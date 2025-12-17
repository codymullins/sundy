namespace Sundy.Core.Meta;

public class FixedTimeZoneProvider(TimeZoneInfo timeZone) : ITimeZoneProvider
{
    public TimeZoneInfo GetCurrentTimeZone() => timeZone;

    public DateTimeOffset ToLocal(DateTimeOffset utc)
    {
        return TimeZoneInfo.ConvertTime(utc, timeZone);
    }

    public DateTimeOffset ToUtc(DateTimeOffset local)
    {
        return TimeZoneInfo.ConvertTimeToUtc(local.DateTime, timeZone);
    }
}
