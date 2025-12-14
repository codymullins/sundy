using System.Text.Json.Serialization;

namespace Sundy.Formats.JSCalendar;

/// <summary>
/// Represents a participant in a calendar event or task.
/// </summary>
public record Participant
{
    /// <summary>
    /// The type of this object. Must be "Participant" if set.
    /// </summary>
    [JsonPropertyName("@type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Type { get; init; }

    /// <summary>
    /// The display name of the participant.
    /// </summary>
    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; init; }

    /// <summary>
    /// The email address to use to contact the participant.
    /// </summary>
    [JsonPropertyName("email")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Email { get; init; }

    /// <summary>
    /// A description of this participant.
    /// </summary>
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; init; }

    /// <summary>
    /// The media type of the contents of the description property.
    /// </summary>
    [JsonPropertyName("descriptionContentType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DescriptionContentType { get; init; }

    /// <summary>
    /// A URI that globally identifies this participant.
    /// </summary>
    [JsonPropertyName("calendarAddress")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CalendarAddress { get; init; }

    /// <summary>
    /// What kind of entity this participant is.
    /// </summary>
    [JsonPropertyName("kind")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ParticipantKind? Kind { get; init; }

    /// <summary>
    /// The roles that this participant fulfills.
    /// </summary>
    [JsonPropertyName("roles")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<ParticipantRole, bool>? Roles { get; init; }

    /// <summary>
    /// The participation status of this participant.
    /// </summary>
    [JsonPropertyName("participationStatus")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ParticipationStatus? ParticipationStatus { get; init; }

    /// <summary>
    /// Whether the organizer is expecting a reply from the participant.
    /// </summary>
    [JsonPropertyName("expectReply")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? ExpectReply { get; init; }

    /// <summary>
    /// The email address in the "From" header that last updated this participant via iMIP.
    /// </summary>
    [JsonPropertyName("sentBy")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SentBy { get; init; }

    /// <summary>
    /// The set of participants that this participant has delegated to.
    /// </summary>
    [JsonPropertyName("delegatedTo")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, bool>? DelegatedTo { get; init; }

    /// <summary>
    /// The set of participants that this participant is acting as a delegate for.
    /// </summary>
    [JsonPropertyName("delegatedFrom")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, bool>? DelegatedFrom { get; init; }

    /// <summary>
    /// The set of group participants that caused this participant to be invited.
    /// </summary>
    [JsonPropertyName("memberOf")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, bool>? MemberOf { get; init; }

    /// <summary>
    /// External resources associated with this participant.
    /// </summary>
    [JsonPropertyName("links")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, Link>? Links { get; init; }

    /// <summary>
    /// The progress of the participant for a task.
    /// </summary>
    [JsonPropertyName("progress")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ProgressStatus? Progress { get; init; }

    /// <summary>
    /// The percent completion of the participant for a task (0-100).
    /// </summary>
    [JsonPropertyName("percentComplete")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public uint? PercentComplete { get; init; }
}
