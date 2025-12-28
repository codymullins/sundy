namespace Sundy.Core.Calendars.Outlook;

/// <summary>
/// Information about an Outlook calendar.
/// </summary>
public record OutlookCalendarInfo
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Color { get; set; }
    public bool IsDefault { get; set; }
}
