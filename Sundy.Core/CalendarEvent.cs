namespace Sundy.Core;

public class CalendarEvent
{
    public string Id { get; set; }
    public string CalendarId { get; set; }
    public string Title { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public bool IsBlockingEvent { get; set; } // Created by Sundy as a block
    public string? SourceEventId { get; set; } // If blocking event, points to source
}