using System.Text.Json.Serialization;

namespace Sundy.Formats.iCalendar;

/// <summary>
/// Represents a free/busy time period with type.
/// </summary>
public record FreeBusyPeriod
{
    /// <summary>
    /// The time period.
    /// </summary>
    [JsonPropertyName("period")]
    public required Period Period { get; init; }

    /// <summary>
    /// The free/busy time type.
    /// </summary>
    [JsonPropertyName("fbtype")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public FreeBusyTimeType? FreeBusyType { get; init; }
}

/// <summary>
/// Represents a VFREEBUSY component - free/busy time information.
/// </summary>
public record VFreeBusy
{
    /// <summary>
    /// The unique identifier for this free/busy information.
    /// </summary>
    [JsonPropertyName("uid")]
    public required string Uid { get; init; }

    /// <summary>
    /// The date and time stamp for this component.
    /// </summary>
    [JsonPropertyName("dtstamp")]
    public required string DateTimeStamp { get; init; }

    /// <summary>
    /// The start date/time of the free/busy period.
    /// </summary>
    [JsonPropertyName("dtstart")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DateStart { get; init; }

    /// <summary>
    /// The end date/time of the free/busy period.
    /// </summary>
    [JsonPropertyName("dtend")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DateEnd { get; init; }

    /// <summary>
    /// The organizer of the free/busy request.
    /// </summary>
    [JsonPropertyName("organizer")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Organizer? Organizer { get; init; }

    /// <summary>
    /// Attendees for the free/busy request.
    /// </summary>
    [JsonPropertyName("attendee")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Attendee[]? Attendees { get; init; }

    /// <summary>
    /// The URL associated with this free/busy information.
    /// </summary>
    [JsonPropertyName("url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Url { get; init; }

    /// <summary>
    /// Comments for this free/busy information.
    /// </summary>
    [JsonPropertyName("comment")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Comment { get; init; }

    /// <summary>
    /// Contact information for this free/busy information.
    /// </summary>
    [JsonPropertyName("contact")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Contact { get; init; }

    /// <summary>
    /// Request status information.
    /// </summary>
    [JsonPropertyName("requestStatus")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public RequestStatus[]? RequestStatus { get; init; }

    /// <summary>
    /// Free/busy time periods.
    /// </summary>
    [JsonPropertyName("freebusy")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public FreeBusyPeriod[]? FreeBusyPeriods { get; init; }
}
