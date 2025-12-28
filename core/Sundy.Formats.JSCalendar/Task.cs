using System.Text.Json.Serialization;

namespace Sundy.Formats.JSCalendar;

/// <summary>
/// Represents a task or to-do item.
/// </summary>
public record Task : JSCalendarObject
{
    /// <summary>
    /// The date/time the task is due in the task's time zone.
    /// </summary>
    [JsonPropertyName("due")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Due { get; init; }

    /// <summary>
    /// The date/time the task should start in the task's time zone.
    /// </summary>
    [JsonPropertyName("start")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Start { get; init; }

    /// <summary>
    /// The time zone in which the task is scheduled.
    /// </summary>
    [JsonPropertyName("timeZone")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TimeZone { get; init; }

    /// <summary>
    /// The estimated positive duration of time the task takes to complete.
    /// </summary>
    [JsonPropertyName("estimatedDuration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? EstimatedDuration { get; init; }

    /// <summary>
    /// Indicates that time is not important to display to the user.
    /// </summary>
    [JsonPropertyName("showWithoutTime")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? ShowWithoutTime { get; init; }

    /// <summary>
    /// Physical locations associated with the task.
    /// </summary>
    [JsonPropertyName("locations")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, Location>? Locations { get; init; }

    /// <summary>
    /// Identifies which location is the main location for the task.
    /// </summary>
    [JsonPropertyName("mainLocationId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? MainLocationId { get; init; }

    /// <summary>
    /// Virtual locations associated with the task.
    /// </summary>
    [JsonPropertyName("virtualLocations")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, VirtualLocation>? VirtualLocations { get; init; }

    /// <summary>
    /// The recurrence id if this is a single occurrence of a recurring task.
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
    /// The recurrence rule for this task.
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
    /// The percent completion of the task overall (0-100).
    /// </summary>
    [JsonPropertyName("percentComplete")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public uint? PercentComplete { get; init; }

    /// <summary>
    /// The progress of this task.
    /// </summary>
    [JsonPropertyName("progress")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ProgressStatus? Progress { get; init; }

    /// <summary>
    /// The priority of the task (0-9).
    /// </summary>
    [JsonPropertyName("priority")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Priority { get; init; }

    /// <summary>
    /// How this task should be treated when calculating free-busy state.
    /// </summary>
    [JsonPropertyName("freeBusyStatus")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public FreeBusyStatus? FreeBusyStatus { get; init; }

    /// <summary>
    /// Privacy classification of the task.
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
    /// Participants in the task.
    /// </summary>
    [JsonPropertyName("participants")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, Participant>? Participants { get; init; }

    /// <summary>
    /// Alerts/reminders for the task.
    /// </summary>
    [JsonPropertyName("alerts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, Alert>? Alerts { get; init; }
}
