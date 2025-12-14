using System.Text.Json.Serialization;

namespace Sundy.Formats.iCalendar;

/// <summary>
/// Base record for all iCalendar components with common properties.
/// </summary>
public abstract record ComponentBase
{
    /// <summary>
    /// The unique identifier for the component.
    /// </summary>
    [JsonPropertyName("uid")]
    public required string Uid { get; init; }

    /// <summary>
    /// The date and time the component was created.
    /// </summary>
    [JsonPropertyName("created")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Created { get; init; }

    /// <summary>
    /// The date and time the component was last modified.
    /// </summary>
    [JsonPropertyName("lastModified")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LastModified { get; init; }

    /// <summary>
    /// The date and time stamp for the component.
    /// </summary>
    [JsonPropertyName("dtstamp")]
    public required string DateTimeStamp { get; init; }

    /// <summary>
    /// The revision sequence number.
    /// </summary>
    [JsonPropertyName("sequence")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Sequence { get; init; }

    /// <summary>
    /// A short summary or subject for the component.
    /// </summary>
    [JsonPropertyName("summary")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Summary { get; init; }

    /// <summary>
    /// A more complete description of the component.
    /// </summary>
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; init; }

    /// <summary>
    /// Comments associated with the component.
    /// </summary>
    [JsonPropertyName("comment")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Comment { get; init; }

    /// <summary>
    /// The organizer of the component.
    /// </summary>
    [JsonPropertyName("organizer")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Organizer? Organizer { get; init; }

    /// <summary>
    /// The URL associated with the component.
    /// </summary>
    [JsonPropertyName("url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Url { get; init; }

    /// <summary>
    /// Attachments for the component.
    /// </summary>
    [JsonPropertyName("attach")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Attachment[]? Attachments { get; init; }

    /// <summary>
    /// Categories for the component.
    /// </summary>
    [JsonPropertyName("categories")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Categories { get; init; }

    /// <summary>
    /// The access classification.
    /// </summary>
    [JsonPropertyName("class")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Classification? Class { get; init; }

    /// <summary>
    /// Related components.
    /// </summary>
    [JsonPropertyName("relatedTo")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public RelatedTo[]? RelatedTo { get; init; }

    /// <summary>
    /// Request status information.
    /// </summary>
    [JsonPropertyName("requestStatus")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public RequestStatus[]? RequestStatus { get; init; }
}
