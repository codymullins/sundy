namespace Sundy.Core;

public class BlockingRelationship
{
    public string Id { get; set; }
    public string SourceCalendarId { get; set; }
    public string SourceEventId { get; set; }
    public List<BlockedEvent> BlockedEvents { get; set; } = new();
}