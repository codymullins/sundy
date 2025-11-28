namespace Sundy.Core;

// Calendar entity
public class Calendar
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Color { get; set; }
    public CalendarType Type { get; set; } // Local, Google, Microsoft
    public bool EnableBlocking { get; set; } // Should create blocks on other calendars
    public bool ReceiveBlocks { get; set; }  // Should receive blocks from other calendars
}

// Event entity

// Blocking relationship tracking