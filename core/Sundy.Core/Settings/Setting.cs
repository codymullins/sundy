namespace Sundy.Core.Settings;

/// <summary>
/// Represents an app setting that can be synced.
/// </summary>
public class Setting
{
    /// <summary>
    /// The setting key (unique identifier).
    /// </summary>
    public required string Key { get; set; }

    /// <summary>
    /// The setting value (stored as string, can be JSON for complex types).
    /// </summary>
    public required string Value { get; set; }

    // Sync metadata

    /// <summary>
    /// Server version when this setting was last synced.
    /// </summary>
    public long Version { get; set; }

    /// <summary>
    /// When this setting was last modified locally.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// Soft delete flag for sync. When true, setting is considered deleted.
    /// </summary>
    public bool IsDeleted { get; set; }
}
