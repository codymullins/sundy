namespace Sundy.Core;

public class BlockedEvent
{
    public string? Id { get; set; }
    public required string TargetCalendarId { get; set; }
    public required string TargetEventId { get; set; }
}