using System.Text.Json.Serialization;

namespace Sundy.Formats.iCalendar;

/// <summary>
/// Represents a VALARM component - an alarm or reminder.
/// </summary>
public record VAlarm
{
    /// <summary>
    /// The action to invoke when the alarm is triggered.
    /// </summary>
    [JsonPropertyName("action")]
    public required AlarmAction Action { get; init; }

    /// <summary>
    /// The trigger that defines when the alarm will be activated.
    /// </summary>
    [JsonPropertyName("trigger")]
    public required Trigger Trigger { get; init; }

    /// <summary>
    /// The duration of the alarm (for snoozing).
    /// </summary>
    [JsonPropertyName("duration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Duration? Duration { get; init; }

    /// <summary>
    /// The number of times the alarm should repeat.
    /// </summary>
    [JsonPropertyName("repeat")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Repeat { get; init; }

    /// <summary>
    /// The description text for DISPLAY or EMAIL alarms.
    /// </summary>
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; init; }

    /// <summary>
    /// The summary/subject for EMAIL alarms.
    /// </summary>
    [JsonPropertyName("summary")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Summary { get; init; }

    /// <summary>
    /// The attendees for EMAIL alarms.
    /// </summary>
    [JsonPropertyName("attendee")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Attendee[]? Attendees { get; init; }

    /// <summary>
    /// Attachments for the alarm (e.g., sound file for AUDIO alarms).
    /// </summary>
    [JsonPropertyName("attach")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Attachment[]? Attachments { get; init; }
}
