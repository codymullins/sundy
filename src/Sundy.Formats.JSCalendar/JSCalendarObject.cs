using System.Text.Json.Serialization;

namespace Sundy.Formats.JSCalendar;

/// <summary>
/// Base class for JSCalendar objects with common properties.
/// </summary>
public abstract record JSCalendarObject
{
    /// <summary>
    /// The type of this object (Event, Task, or Group).
    /// </summary>
    [JsonPropertyName("@type")]
    public required string Type { get; init; }

    /// <summary>
    /// A globally unique identifier for this object.
    /// </summary>
    [JsonPropertyName("uid")]
    public required string Uid { get; init; }

    /// <summary>
    /// The date and time this object was last modified.
    /// </summary>
    [JsonPropertyName("updated")]
    public required string Updated { get; init; }

    /// <summary>
    /// Relates this object to other JSCalendar objects.
    /// </summary>
    [JsonPropertyName("relatedTo")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, Relation>? RelatedTo { get; init; }

    /// <summary>
    /// The identifier for the product that last updated this object.
    /// </summary>
    [JsonPropertyName("prodId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ProdId { get; init; }

    /// <summary>
    /// The date and time this object was initially created.
    /// </summary>
    [JsonPropertyName("created")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Created { get; init; }

    /// <summary>
    /// The revision of the calendar object.
    /// </summary>
    [JsonPropertyName("sequence")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public uint? Sequence { get; init; }

    /// <summary>
    /// The iTIP method, in lowercase.
    /// </summary>
    [JsonPropertyName("method")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Method { get; init; }

    /// <summary>
    /// A short summary of the object.
    /// </summary>
    [JsonPropertyName("title")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Title { get; init; }

    /// <summary>
    /// A longer-form text description of the object.
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
    /// External resources associated with this object.
    /// </summary>
    [JsonPropertyName("links")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, Link>? Links { get; init; }

    /// <summary>
    /// The language tag that best describes the locale used for text.
    /// </summary>
    [JsonPropertyName("locale")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Locale { get; init; }

    /// <summary>
    /// A set of keywords or tags that relate to the object.
    /// </summary>
    [JsonPropertyName("keywords")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, bool>? Keywords { get; init; }

    /// <summary>
    /// A set of categories that relate to the calendar object.
    /// </summary>
    [JsonPropertyName("categories")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, bool>? Categories { get; init; }

    /// <summary>
    /// A color clients may use when displaying this calendar object.
    /// </summary>
    [JsonPropertyName("color")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Color { get; init; }
}
