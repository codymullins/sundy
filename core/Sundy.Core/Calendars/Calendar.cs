namespace Sundy.Core;

/// <summary>
/// Represents a calendar in the system.
/// </summary>
public class Calendar
{
    public required string Id { get; set; }
    public required string Name { get; set; }

    /// <summary>
    /// Custom display name override. When set, this is shown in the UI instead of Name.
    /// For external calendars, Name contains the original name from the provider.
    /// </summary>
    public string? DisplayName { get; set; }

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

    /// <summary>
    /// For external calendars (Microsoft, Google), the ID of the connected account.
    /// Null for local calendars.
    /// </summary>
    public string? ExternalAccountId { get; set; }

    /// <summary>
    /// Whether this calendar is hidden from the sidebar by default.
    /// Used to hide system calendars like Birthdays, Holidays.
    /// </summary>
    public bool IsHidden { get; set; }

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