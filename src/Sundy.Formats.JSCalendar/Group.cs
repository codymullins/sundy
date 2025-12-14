using System.Text.Json.Serialization;

namespace Sundy.Formats.JSCalendar;

/// <summary>
/// Represents a collection of Event and/or Task objects.
/// </summary>
public record Group : JSCalendarObject
{
    /// <summary>
    /// A collection of group members (Events and Tasks).
    /// </summary>
    [JsonPropertyName("entries")]
    public required JSCalendarObject[] Entries { get; init; }

    /// <summary>
    /// The source from which updated versions of this group may be retrieved.
    /// </summary>
    [JsonPropertyName("source")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Source { get; init; }
}
