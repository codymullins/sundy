using System.Text.Json.Serialization;

namespace Sundy.Formats.iCalendar;

/// <summary>
/// Parameters for an ATTENDEE property.
/// </summary>
public record AttendeeParameters
{
    /// <summary>
    /// The calendar user type.
    /// </summary>
    [JsonPropertyName("cutype")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public CalendarUserType? CalendarUserType { get; init; }

    /// <summary>
    /// The participation role.
    /// </summary>
    [JsonPropertyName("role")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ParticipantRole? Role { get; init; }

    /// <summary>
    /// The participation status.
    /// </summary>
    [JsonPropertyName("partstat")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ParticipantStatus? ParticipationStatus { get; init; }

    /// <summary>
    /// Whether a response is expected (RSVP).
    /// </summary>
    [JsonPropertyName("rsvp")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool Rsvp { get; init; }

    /// <summary>
    /// The calendar users to whom the attendee has delegated participation.
    /// </summary>
    [JsonPropertyName("delegatedTo")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? DelegatedTo { get; init; }

    /// <summary>
    /// The calendar users who have delegated participation to this attendee.
    /// </summary>
    [JsonPropertyName("delegatedFrom")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? DelegatedFrom { get; init; }

    /// <summary>
    /// The calendar user specified as a delegate for this attendee.
    /// </summary>
    [JsonPropertyName("sentBy")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SentBy { get; init; }

    /// <summary>
    /// The common or display name associated with the calendar user.
    /// </summary>
    [JsonPropertyName("cn")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CommonName { get; init; }

    /// <summary>
    /// The directory entry associated with the calendar user.
    /// </summary>
    [JsonPropertyName("dir")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DirectoryEntry { get; init; }

    /// <summary>
    /// The group or list membership of the calendar user.
    /// </summary>
    [JsonPropertyName("member")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Member { get; init; }

    /// <summary>
    /// The language for text values.
    /// </summary>
    [JsonPropertyName("language")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Language { get; init; }
}

/// <summary>
/// Represents an attendee with their calendar address and parameters.
/// </summary>
public record Attendee
{
    /// <summary>
    /// The calendar user address (usually mailto: URI).
    /// </summary>
    [JsonPropertyName("calendarAddress")]
    public required string CalendarAddress { get; init; }

    /// <summary>
    /// Parameters for this attendee.
    /// </summary>
    [JsonPropertyName("parameters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public AttendeeParameters? Parameters { get; init; }
}

/// <summary>
/// Parameters for an ORGANIZER property.
/// </summary>
public record OrganizerParameters
{
    /// <summary>
    /// The common or display name associated with the organizer.
    /// </summary>
    [JsonPropertyName("cn")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CommonName { get; init; }

    /// <summary>
    /// The directory entry associated with the organizer.
    /// </summary>
    [JsonPropertyName("dir")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DirectoryEntry { get; init; }

    /// <summary>
    /// The calendar user specified as a delegate for this organizer.
    /// </summary>
    [JsonPropertyName("sentBy")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SentBy { get; init; }

    /// <summary>
    /// The language for text values.
    /// </summary>
    [JsonPropertyName("language")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Language { get; init; }
}

/// <summary>
/// Represents an organizer with their calendar address and parameters.
/// </summary>
public record Organizer
{
    /// <summary>
    /// The calendar user address (usually mailto: URI).
    /// </summary>
    [JsonPropertyName("calendarAddress")]
    public required string CalendarAddress { get; init; }

    /// <summary>
    /// Parameters for this organizer.
    /// </summary>
    [JsonPropertyName("parameters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OrganizerParameters? Parameters { get; init; }
}

/// <summary>
/// Represents an attachment with URI or inline binary data.
/// </summary>
public record Attachment
{
    /// <summary>
    /// The URI of the attachment.
    /// </summary>
    [JsonPropertyName("uri")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Uri { get; init; }

    /// <summary>
    /// The inline binary data (base64 encoded).
    /// </summary>
    [JsonPropertyName("binary")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Binary { get; init; }

    /// <summary>
    /// The format type (MIME type) of the attachment.
    /// </summary>
    [JsonPropertyName("fmttype")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FormatType { get; init; }
}

/// <summary>
/// Represents a related-to reference.
/// </summary>
public record RelatedTo
{
    /// <summary>
    /// The unique identifier of the related component.
    /// </summary>
    [JsonPropertyName("uid")]
    public required string Uid { get; init; }

    /// <summary>
    /// The type of relationship.
    /// </summary>
    [JsonPropertyName("reltype")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public RelationshipType? RelationType { get; init; }
}
