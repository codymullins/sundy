namespace Sundy.Core.Meta;

public interface ITimeZoneProvider
{
    TimeZoneInfo GetCurrentTimeZone();
    DateTimeOffset ToLocal(DateTimeOffset utc);
    DateTimeOffset ToUtc(DateTimeOffset local);
}
