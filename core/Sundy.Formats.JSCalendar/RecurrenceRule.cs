using System.Text.Json.Serialization;

namespace Sundy.Formats.JSCalendar;

/// <summary>
/// Represents a day of the week with an optional occurrence index.
/// </summary>
public record NDay
{
    /// <summary>
    /// The type of this object. Must be "NDay" if set.
    /// </summary>
    [JsonPropertyName("@type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Type { get; init; }

    /// <summary>
    /// A day of the week on which to repeat.
    /// </summary>
    [JsonPropertyName("day")]
    public required DayOfWeek Day { get; init; }

    /// <summary>
    /// Represents only a specific instance within the recurrence period.
    /// Positive values count from start, negative from end.
    /// </summary>
    [JsonPropertyName("nthOfPeriod")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? NthOfPeriod { get; init; }
}

/// <summary>
/// Represents a recurrence rule for recurring calendar objects.
/// </summary>
public record RecurrenceRule
{
    /// <summary>
    /// The type of this object. Must be "RecurrenceRule" if set.
    /// </summary>
    [JsonPropertyName("@type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Type { get; init; }

    /// <summary>
    /// The time span covered by each iteration of this recurrence rule.
    /// </summary>
    [JsonPropertyName("frequency")]
    public required RecurrenceFrequency Frequency { get; init; }

    /// <summary>
    /// The interval of iteration periods at which the recurrence repeats.
    /// </summary>
    [JsonPropertyName("interval")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public uint? Interval { get; init; }

    /// <summary>
    /// The calendar system in which this recurrence rule operates.
    /// </summary>
    [JsonPropertyName("rscale")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Rscale { get; init; }

    /// <summary>
    /// The behavior to use when expansion produces invalid dates.
    /// </summary>
    [JsonPropertyName("skip")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SkipBehavior? Skip { get; init; }

    /// <summary>
    /// The day on which the week is considered to start.
    /// </summary>
    [JsonPropertyName("firstDayOfWeek")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DayOfWeek? FirstDayOfWeek { get; init; }

    /// <summary>
    /// Days of the week on which to repeat.
    /// </summary>
    [JsonPropertyName("byDay")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public NDay[]? ByDay { get; init; }

    /// <summary>
    /// Days of the month on which to repeat.
    /// </summary>
    [JsonPropertyName("byMonthDay")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int[]? ByMonthDay { get; init; }

    /// <summary>
    /// Months in which to repeat.
    /// </summary>
    [JsonPropertyName("byMonth")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? ByMonth { get; init; }

    /// <summary>
    /// Days of the year on which to repeat.
    /// </summary>
    [JsonPropertyName("byYearDay")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int[]? ByYearDay { get; init; }

    /// <summary>
    /// Weeks of the year in which to repeat.
    /// </summary>
    [JsonPropertyName("byWeekNo")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int[]? ByWeekNo { get; init; }

    /// <summary>
    /// Hours of the day in which to repeat (0-23).
    /// </summary>
    [JsonPropertyName("byHour")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public uint[]? ByHour { get; init; }

    /// <summary>
    /// Minutes of the hour in which to repeat (0-59).
    /// </summary>
    [JsonPropertyName("byMinute")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public uint[]? ByMinute { get; init; }

    /// <summary>
    /// Seconds of the minute in which to repeat (0-60).
    /// </summary>
    [JsonPropertyName("bySecond")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public uint[]? BySecond { get; init; }

    /// <summary>
    /// Occurrences within the recurrence interval to include.
    /// </summary>
    [JsonPropertyName("bySetPosition")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int[]? BySetPosition { get; init; }

    /// <summary>
    /// The number of occurrences at which to range-bound the recurrence.
    /// </summary>
    [JsonPropertyName("count")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public uint? Count { get; init; }

    /// <summary>
    /// The date-time at which to finish recurring.
    /// </summary>
    [JsonPropertyName("until")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Until { get; init; }
}
