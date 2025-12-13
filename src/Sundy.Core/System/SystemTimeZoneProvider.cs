namespace Sundy.Core.System;

public class SystemTimeZoneProvider : ITimeZoneProvider
{
    public TimeZoneInfo GetCurrentTimeZone() => TimeZoneInfo.Local;

    public DateTimeOffset ToLocal(DateTimeOffset utc)
    {
        return TimeZoneInfo.ConvertTime(utc, TimeZoneInfo.Local);
    }

    public DateTimeOffset ToUtc(DateTimeOffset local)
    {
        return local.ToUniversalTime();
    }
}
