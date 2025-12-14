using System.Text.Json.Serialization;

namespace Sundy.Formats.JSCalendar;

/// <summary>
/// Represents an external resource associated with a calendar object.
/// </summary>
public record Link
{
    /// <summary>
    /// The type of this object. Must be "Link" if set.
    /// </summary>
    [JsonPropertyName("@type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Type { get; init; }

    /// <summary>
    /// A URI from which the resource may be fetched.
    /// </summary>
    [JsonPropertyName("href")]
    public required string Href { get; init; }

    /// <summary>
    /// The media type of the resource, if known.
    /// </summary>
    [JsonPropertyName("contentType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ContentType { get; init; }

    /// <summary>
    /// The size in octets of the resource when fully decoded, if known.
    /// </summary>
    [JsonPropertyName("size")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public uint? Size { get; init; }

    /// <summary>
    /// The relation of the linked resource to the object.
    /// </summary>
    [JsonPropertyName("rel")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Rel { get; init; }

    /// <summary>
    /// A set of intended purposes of a link to an image.
    /// </summary>
    [JsonPropertyName("display")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<LinkDisplay, bool>? Display { get; init; }

    /// <summary>
    /// A human-readable, plain-text description of the resource.
    /// </summary>
    [JsonPropertyName("title")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Title { get; init; }
}
