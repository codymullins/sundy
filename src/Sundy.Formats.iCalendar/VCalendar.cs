using System.Text.Json.Serialization;

namespace Sundy.Formats.iCalendar;

/// <summary>
/// Represents a VCALENDAR component - the top-level iCalendar object.
/// </summary>
public record VCalendar
{
    /// <summary>
    /// The product identifier that created this iCalendar object.
    /// </summary>
    [JsonPropertyName("prodid")]
    public required string ProductId { get; init; }

    /// <summary>
    /// The iCalendar specification version (should be "2.0").
    /// </summary>
    [JsonPropertyName("version")]
    public required string Version { get; init; }

    /// <summary>
    /// The calendar scale (typically "GREGORIAN").
    /// </summary>
    [JsonPropertyName("calscale")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CalendarScale { get; init; }

    /// <summary>
    /// The iTIP method (e.g., "PUBLISH", "REQUEST", "REPLY").
    /// </summary>
    [JsonPropertyName("method")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Method { get; init; }

    /// <summary>
    /// The name of the calendar.
    /// </summary>
    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; init; }

    /// <summary>
    /// The description of the calendar.
    /// </summary>
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; init; }

    /// <summary>
    /// The last modified date/time of the calendar.
    /// </summary>
    [JsonPropertyName("lastModified")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LastModified { get; init; }

    /// <summary>
    /// The URL where the calendar can be refreshed.
    /// </summary>
    [JsonPropertyName("url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Url { get; init; }

    /// <summary>
    /// The refresh interval for the calendar.
    /// </summary>
    [JsonPropertyName("refreshInterval")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Duration? RefreshInterval { get; init; }

    /// <summary>
    /// The source URL from which this calendar was retrieved.
    /// </summary>
    [JsonPropertyName("source")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Source { get; init; }

    /// <summary>
    /// The color to use when displaying this calendar.
    /// </summary>
    [JsonPropertyName("color")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Color { get; init; }

    /// <summary>
    /// Categories for the calendar.
    /// </summary>
    [JsonPropertyName("categories")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Categories { get; init; }

    /// <summary>
    /// Time zone definitions used in this calendar.
    /// </summary>
    [JsonPropertyName("vtimezone")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public VTimeZone[]? TimeZones { get; init; }

    /// <summary>
    /// Events in this calendar.
    /// </summary>
    [JsonPropertyName("vevent")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public VEvent[]? Events { get; init; }

    /// <summary>
    /// To-dos in this calendar.
    /// </summary>
    [JsonPropertyName("vtodo")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public VTodo[]? Todos { get; init; }

    /// <summary>
    /// Journal entries in this calendar.
    /// </summary>
    [JsonPropertyName("vjournal")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public VJournal[]? Journals { get; init; }

    /// <summary>
    /// Free/busy time information in this calendar.
    /// </summary>
    [JsonPropertyName("vfreebusy")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public VFreeBusy[]? FreeBusy { get; init; }
}
