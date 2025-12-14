using System.Text.Json.Serialization;

namespace Sundy.Formats.iCalendar;

/// <summary>
/// Represents a VEVENT component - a scheduled event.
/// </summary>
public record VEvent : ComponentBase
{
    /// <summary>
    /// The start date/time of the event.
    /// </summary>
    [JsonPropertyName("dtstart")]
    public required string DateStart { get; init; }

    /// <summary>
    /// The end date/time of the event (mutually exclusive with Duration).
    /// </summary>
    [JsonPropertyName("dtend")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DateEnd { get; init; }

    /// <summary>
    /// The duration of the event (mutually exclusive with DateEnd).
    /// </summary>
    [JsonPropertyName("duration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Duration? Duration { get; init; }

    /// <summary>
    /// The status of the event.
    /// </summary>
    [JsonPropertyName("status")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EventStatus? Status { get; init; }

    /// <summary>
    /// The time transparency of the event.
    /// </summary>
    [JsonPropertyName("transp")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TimeTransparency? Transparency { get; init; }

    /// <summary>
    /// Geographic position for the event.
    /// </summary>
    [JsonPropertyName("geo")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public GeoCoordinate? Geo { get; init; }

    /// <summary>
    /// The venue or location of the event.
    /// </summary>
    [JsonPropertyName("location")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Location { get; init; }

    /// <summary>
    /// The priority of the event (0-9, with 0 being undefined).
    /// </summary>
    [JsonPropertyName("priority")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Priority { get; init; }

    /// <summary>
    /// Resources required for the event.
    /// </summary>
    [JsonPropertyName("resources")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Resources { get; init; }

    /// <summary>
    /// Attendees of the event.
    /// </summary>
    [JsonPropertyName("attendee")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Attendee[]? Attendees { get; init; }

    /// <summary>
    /// Contact information for the event.
    /// </summary>
    [JsonPropertyName("contact")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Contact { get; init; }

    /// <summary>
    /// Recurrence rule for the event.
    /// </summary>
    [JsonPropertyName("rrule")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public RecurrenceRule? RecurrenceRule { get; init; }

    /// <summary>
    /// Recurrence date-times for the event.
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
    /// The recurrence ID for an instance of a recurring event.
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
    /// Alarms associated with this event.
    /// </summary>
    [JsonPropertyName("valarm")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public VAlarm[]? Alarms { get; init; }
}
