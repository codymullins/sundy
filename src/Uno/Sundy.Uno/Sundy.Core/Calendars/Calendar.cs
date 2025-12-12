namespace Sundy.Core;

/// <summary>
/// Represents a calendar in the system.
/// </summary>
public class Calendar
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Color { get; set; }
    public CalendarType Type { get; set; }
    
    /// <summary>
    /// Should create blocks on other calendars
    /// </summary>
    public bool EnableBlocking { get; set; }
    
    /// <summary>
    /// Should receive blocks from other calendars
    /// </summary>
    public bool ReceiveBlocks { get; set; }
}