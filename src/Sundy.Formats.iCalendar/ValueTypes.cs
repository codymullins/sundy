using System.Text.Json.Serialization;

namespace Sundy.Formats.iCalendar;

/// <summary>
/// Represents a duration value in iCalendar format (ISO 8601).
/// </summary>
public record Duration
{
    /// <summary>
    /// The duration string in ISO 8601 format (e.g., "PT1H30M", "P15DT5H").
    /// </summary>
    [JsonPropertyName("value")]
    public required string Value { get; init; }

    /// <summary>
    /// Indicates whether the duration is negative.
    /// </summary>
    [JsonPropertyName("isNegative")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool IsNegative { get; init; }
}

/// <summary>
/// Represents a period of time with explicit start and end.
/// </summary>
public record Period
{
    /// <summary>
    /// The start date-time of the period.
    /// </summary>
    [JsonPropertyName("start")]
    public required string Start { get; init; }

    /// <summary>
    /// The end date-time of the period (mutually exclusive with Duration).
    /// </summary>
    [JsonPropertyName("end")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? End { get; init; }

    /// <summary>
    /// The duration of the period (mutually exclusive with End).
    /// </summary>
    [JsonPropertyName("duration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Duration? DurationValue { get; init; }
}

/// <summary>
/// Represents a weekday with an optional occurrence specifier.
/// </summary>
public record WeekdayNum
{
    /// <summary>
    /// The occurrence number (e.g., +1 for first, -1 for last, null for all).
    /// </summary>
    [JsonPropertyName("occurrence")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Occurrence { get; init; }

    /// <summary>
    /// The day of the week.
    /// </summary>
    [JsonPropertyName("day")]
    public required Weekday Day { get; init; }
}

/// <summary>
/// Represents a recurrence rule (RRULE).
/// </summary>
public record RecurrenceRule
{
    /// <summary>
    /// The frequency of recurrence.
    /// </summary>
    [JsonPropertyName("freq")]
    public required RecurrenceFrequency Frequency { get; init; }

    /// <summary>
    /// The interval between recurrences.
    /// </summary>
    [JsonPropertyName("interval")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Interval { get; init; }

    /// <summary>
    /// The count of occurrences.
    /// </summary>
    [JsonPropertyName("count")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Count { get; init; }

    /// <summary>
    /// The end date-time for the recurrence.
    /// </summary>
    [JsonPropertyName("until")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Until { get; init; }

    /// <summary>
    /// List of days of the week.
    /// </summary>
    [JsonPropertyName("byday")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public WeekdayNum[]? ByDay { get; init; }

    /// <summary>
    /// List of months of the year (1-12).
    /// </summary>
    [JsonPropertyName("bymonth")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int[]? ByMonth { get; init; }

    /// <summary>
    /// List of days of the month (-31 to 31, excluding 0).
    /// </summary>
    [JsonPropertyName("bymonthday")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int[]? ByMonthDay { get; init; }

    /// <summary>
    /// List of days of the year (-366 to 366, excluding 0).
    /// </summary>
    [JsonPropertyName("byyearday")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int[]? ByYearDay { get; init; }

    /// <summary>
    /// List of weeks of the year (-53 to 53, excluding 0).
    /// </summary>
    [JsonPropertyName("byweekno")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int[]? ByWeekNo { get; init; }

    /// <summary>
    /// List of hours of the day (0-23).
    /// </summary>
    [JsonPropertyName("byhour")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int[]? ByHour { get; init; }

    /// <summary>
    /// List of minutes within an hour (0-59).
    /// </summary>
    [JsonPropertyName("byminute")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int[]? ByMinute { get; init; }

    /// <summary>
    /// List of seconds within a minute (0-60).
    /// </summary>
    [JsonPropertyName("bysecond")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int[]? BySecond { get; init; }

    /// <summary>
    /// List of ordinals specifying positions within a set (-366 to 366, excluding 0).
    /// </summary>
    [JsonPropertyName("bysetpos")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int[]? BySetPos { get; init; }

    /// <summary>
    /// The day on which the workweek starts.
    /// </summary>
    [JsonPropertyName("wkst")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Weekday? WeekStart { get; init; }
}

/// <summary>
/// Represents a UTC offset for time zones.
/// </summary>
public record UTCOffset
{
    /// <summary>
    /// The offset string in format "+/-HHMM" or "+/-HHMMSS".
    /// </summary>
    [JsonPropertyName("value")]
    public required string Value { get; init; }
}

/// <summary>
/// Represents geographic coordinates.
/// </summary>
public record GeoCoordinate
{
    /// <summary>
    /// Latitude in decimal degrees.
    /// </summary>
    [JsonPropertyName("latitude")]
    public required double Latitude { get; init; }

    /// <summary>
    /// Longitude in decimal degrees.
    /// </summary>
    [JsonPropertyName("longitude")]
    public required double Longitude { get; init; }
}

/// <summary>
/// Represents a trigger for an alarm (can be duration or absolute time).
/// </summary>
public record Trigger
{
    /// <summary>
    /// The duration before/after the event start (when relative).
    /// </summary>
    [JsonPropertyName("duration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Duration? DurationValue { get; init; }

    /// <summary>
    /// The absolute date-time for the trigger (when absolute).
    /// </summary>
    [JsonPropertyName("dateTime")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DateTime { get; init; }

    /// <summary>
    /// For duration triggers, whether to trigger relative to end (true) or start (false).
    /// </summary>
    [JsonPropertyName("relatedToEnd")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool RelatedToEnd { get; init; }
}

/// <summary>
/// Represents a request status code.
/// </summary>
public record RequestStatus
{
    /// <summary>
    /// The status code (e.g., "2.0", "3.1").
    /// </summary>
    [JsonPropertyName("code")]
    public required string Code { get; init; }

    /// <summary>
    /// The status description.
    /// </summary>
    [JsonPropertyName("description")]
    public required string Description { get; init; }

    /// <summary>
    /// Additional exception data.
    /// </summary>
    [JsonPropertyName("exceptionData")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ExceptionData { get; init; }
}
