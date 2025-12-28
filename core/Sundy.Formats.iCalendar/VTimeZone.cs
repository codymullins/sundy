using System.Text.Json.Serialization;

namespace Sundy.Formats.iCalendar;

/// <summary>
/// Base record for time zone sub-components (Standard and Daylight).
/// </summary>
public abstract record TimeZoneComponent
{
    /// <summary>
    /// The start date/time for this time zone observance.
    /// </summary>
    [JsonPropertyName("dtstart")]
    public required string DateStart { get; init; }

    /// <summary>
    /// The offset from UTC for this time zone observance.
    /// </summary>
    [JsonPropertyName("tzoffsetto")]
    public required UTCOffset OffsetTo { get; init; }

    /// <summary>
    /// The offset from UTC that was in effect before this observance.
    /// </summary>
    [JsonPropertyName("tzoffsetfrom")]
    public required UTCOffset OffsetFrom { get; init; }

    /// <summary>
    /// The customary name for this time zone observance.
    /// </summary>
    [JsonPropertyName("tzname")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? TimeZoneName { get; init; }

    /// <summary>
    /// Recurrence rule for this observance.
    /// </summary>
    [JsonPropertyName("rrule")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public RecurrenceRule? RecurrenceRule { get; init; }

    /// <summary>
    /// Recurrence date-times for this observance.
    /// </summary>
    [JsonPropertyName("rdate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? RecurrenceDates { get; init; }

    /// <summary>
    /// Comments for this observance.
    /// </summary>
    [JsonPropertyName("comment")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Comment { get; init; }
}

/// <summary>
/// Represents a STANDARD sub-component of VTIMEZONE.
/// </summary>
public record Standard : TimeZoneComponent
{
}

/// <summary>
/// Represents a DAYLIGHT sub-component of VTIMEZONE.
/// </summary>
public record Daylight : TimeZoneComponent
{
}

/// <summary>
/// Represents a VTIMEZONE component - time zone definition.
/// </summary>
public record VTimeZone
{
    /// <summary>
    /// The time zone identifier.
    /// </summary>
    [JsonPropertyName("tzid")]
    public required string TimeZoneId { get; init; }

    /// <summary>
    /// The last modified date/time for this time zone definition.
    /// </summary>
    [JsonPropertyName("lastModified")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LastModified { get; init; }

    /// <summary>
    /// The URL from which this time zone definition was obtained.
    /// </summary>
    [JsonPropertyName("tzurl")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TimeZoneUrl { get; init; }

    /// <summary>
    /// Standard time components (when daylight saving is not in effect).
    /// </summary>
    [JsonPropertyName("standard")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Standard[]? StandardComponents { get; init; }

    /// <summary>
    /// Daylight saving time components.
    /// </summary>
    [JsonPropertyName("daylight")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Daylight[]? DaylightComponents { get; init; }
}
