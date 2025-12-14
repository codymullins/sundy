using System.Text.Json.Serialization;

namespace Sundy.Formats.iCalendar;

/// <summary>
/// Represents a VTODO component - a to-do or task.
/// </summary>
public record VTodo : ComponentBase
{
    /// <summary>
    /// The start date/time of the to-do.
    /// </summary>
    [JsonPropertyName("dtstart")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DateStart { get; init; }

    /// <summary>
    /// The due date/time of the to-do.
    /// </summary>
    [JsonPropertyName("due")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Due { get; init; }

    /// <summary>
    /// The duration of the to-do (mutually exclusive with Due when DateStart is specified).
    /// </summary>
    [JsonPropertyName("duration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Duration? Duration { get; init; }

    /// <summary>
    /// The date/time when the to-do was completed.
    /// </summary>
    [JsonPropertyName("completed")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Completed { get; init; }

    /// <summary>
    /// The percent completion of the to-do (0-100).
    /// </summary>
    [JsonPropertyName("percentComplete")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? PercentComplete { get; init; }

    /// <summary>
    /// The status of the to-do.
    /// </summary>
    [JsonPropertyName("status")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TodoStatus? Status { get; init; }

    /// <summary>
    /// Geographic position for the to-do.
    /// </summary>
    [JsonPropertyName("geo")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public GeoCoordinate? Geo { get; init; }

    /// <summary>
    /// The venue or location of the to-do.
    /// </summary>
    [JsonPropertyName("location")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Location { get; init; }

    /// <summary>
    /// The priority of the to-do (0-9, with 0 being undefined).
    /// </summary>
    [JsonPropertyName("priority")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Priority { get; init; }

    /// <summary>
    /// Resources required for the to-do.
    /// </summary>
    [JsonPropertyName("resources")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Resources { get; init; }

    /// <summary>
    /// Attendees of the to-do.
    /// </summary>
    [JsonPropertyName("attendee")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Attendee[]? Attendees { get; init; }

    /// <summary>
    /// Contact information for the to-do.
    /// </summary>
    [JsonPropertyName("contact")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Contact { get; init; }

    /// <summary>
    /// Recurrence rule for the to-do.
    /// </summary>
    [JsonPropertyName("rrule")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public RecurrenceRule? RecurrenceRule { get; init; }

    /// <summary>
    /// Recurrence date-times for the to-do.
    /// </summary>
    [JsonPropertyName("rdate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? RecurrenceDates { get; init; }

    /// <summary>
    /// Exception date-times for the recurrence.
    /// </summary>
    [JsonPropertyName("exdate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? ExceptionDates { get; init; }

    /// <summary>
    /// Exception rules for the recurrence.
    /// </summary>
    [JsonPropertyName("exrule")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public RecurrenceRule[]? ExceptionRules { get; init; }

    /// <summary>
    /// The recurrence ID for an instance of a recurring to-do.
    /// </summary>
    [JsonPropertyName("recurrenceId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RecurrenceId { get; init; }

    /// <summary>
    /// Range parameter for recurrence ID.
    /// </summary>
    [JsonPropertyName("range")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Range? Range { get; init; }

    /// <summary>
    /// Alarms associated with this to-do.
    /// </summary>
    [JsonPropertyName("valarm")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public VAlarm[]? Alarms { get; init; }
}
