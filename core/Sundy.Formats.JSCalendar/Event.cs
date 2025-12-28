using System.Text.Json.Serialization;

namespace Sundy.Formats.JSCalendar;

/// <summary>
/// Represents a scheduled event on a calendar.
/// </summary>
public record Event : JSCalendarObject
{
    /// <summary>
    /// The date/time the event starts in the event's time zone.
    /// </summary>
    [JsonPropertyName("start")]
    public required string Start { get; init; }

    /// <summary>
    /// The zero or positive duration of the event.
    /// </summary>
    [JsonPropertyName("duration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Duration { get; init; }

    /// <summary>
    /// The time zone in which the event is scheduled.
    /// </summary>
    [JsonPropertyName("timeZone")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TimeZone { get; init; }

    /// <summary>
    /// The time zone in which this event ends (for transcontinental events).
    /// </summary>
    [JsonPropertyName("endTimeZone")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? EndTimeZone { get; init; }

    /// <summary>
    /// Indicates that time is not important to display to the user.
    /// </summary>
    [JsonPropertyName("showWithoutTime")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? ShowWithoutTime { get; init; }

    /// <summary>
    /// Physical locations associated with the event.
    /// </summary>
    [JsonPropertyName("locations")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, Location>? Locations { get; init; }

    /// <summary>
    /// Identifies which location is the main location for the event.
    /// </summary>
    [JsonPropertyName("mainLocationId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? MainLocationId { get; init; }

    /// <summary>
    /// Virtual locations associated with the event.
    /// </summary>
    [JsonPropertyName("virtualLocations")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, VirtualLocation>? VirtualLocations { get; init; }

    /// <summary>
    /// The recurrence id if this is a single occurrence of a recurring event.
    /// </summary>
    [JsonPropertyName("recurrenceId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RecurrenceId { get; init; }

    /// <summary>
    /// The time zone of the main object this recurrence instance belongs to.
    /// </summary>
    [JsonPropertyName("recurrenceIdTimeZone")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RecurrenceIdTimeZone { get; init; }

    /// <summary>
    /// The recurrence rule for this event.
    /// </summary>
    [JsonPropertyName("recurrenceRule")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public RecurrenceRule? RecurrenceRule { get; init; }

    /// <summary>
    /// Maps recurrence ids to overridden properties.
    /// </summary>
    [JsonPropertyName("recurrenceOverrides")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, Dictionary<string, object>>? RecurrenceOverrides { get; init; }

    /// <summary>
    /// The scheduling status of the event.
    /// </summary>
    [JsonPropertyName("status")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EventStatus? Status { get; init; }

    /// <summary>
    /// The priority of the event (0-9).
    /// </summary>
    [JsonPropertyName("priority")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Priority { get; init; }

    /// <summary>
    /// How this event should be treated when calculating free-busy state.
    /// </summary>
    [JsonPropertyName("freeBusyStatus")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public FreeBusyStatus? FreeBusyStatus { get; init; }

    /// <summary>
    /// Privacy classification of the event.
    /// </summary>
    [JsonPropertyName("privacy")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Privacy? Privacy { get; init; }

    /// <summary>
    /// The calendar address of the organizer.
    /// </summary>
    [JsonPropertyName("organizerCalendarAddress")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? OrganizerCalendarAddress { get; init; }

    /// <summary>
    /// Participants in the event.
    /// </summary>
    [JsonPropertyName("participants")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, Participant>? Participants { get; init; }

    /// <summary>
    /// Alerts/reminders for the event.
    /// </summary>
    [JsonPropertyName("alerts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, Alert>? Alerts { get; init; }
}
