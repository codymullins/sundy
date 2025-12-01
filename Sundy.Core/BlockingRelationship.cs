namespace Sundy.Core;

public class BlockingRelationship
{
    public required string Id { get; set; }
    public required string SourceCalendarId { get; set; }
    public required string SourceEventId { get; set; }
    public List<BlockedEvent> BlockedEvents { get; set; } = [];
}