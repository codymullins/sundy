namespace Sundy.Core.System;

public interface ITimeZoneProvider
{
    TimeZoneInfo GetCurrentTimeZone();
    DateTimeOffset ToLocal(DateTimeOffset utc);
    DateTimeOffset ToUtc(DateTimeOffset local);
}
