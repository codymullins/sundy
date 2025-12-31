namespace Sundy.Core;

public class CalendarEvent
{
    public string? Id { get; set; }
    public string? CalendarId { get; set; }
    public string? Title { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public bool IsBlockingEvent { get; set; } // Created by Sundy as a block
    public string? SourceEventId { get; set; } // If blocking event, points to source

    // External calendar sync metadata

    /// <summary>
    /// External identifier for events synced from external sources.
    /// Format: "{provider}_{externalId}" (e.g., "outlook_AAMkAGI2...")
    /// </summary>
    public string? ExternalId { get; set; }

    /// <summary>
    /// Last modified time from external source (for change detection).
    /// </summary>
    public DateTimeOffset? ExternalModifiedAt { get; set; }

    // Sync metadata

    /// <summary>
    /// Server version when this entity was last synced.
    /// </summary>
    public long Version { get; set; }

    /// <summary>
    /// When this entity was last modified locally.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// Soft delete flag for sync. When true, entity is considered deleted.
    /// </summary>
    public bool IsDeleted { get; set; }
}