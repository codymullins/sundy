using System.Text.Json.Serialization;

namespace Sundy.Formats.JSCalendar;

/// <summary>
/// Represents a physical location.
/// </summary>
public record Location
{
    /// <summary>
    /// The type of this object. Must be "Location" if set.
    /// </summary>
    [JsonPropertyName("@type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Type { get; init; }

    /// <summary>
    /// The human-readable name or short description of the location.
    /// </summary>
    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; init; }

    /// <summary>
    /// A set of one or more location types that describe this location.
    /// </summary>
    [JsonPropertyName("locationTypes")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, bool>? LocationTypes { get; init; }

    /// <summary>
    /// A geo: URI for the location.
    /// </summary>
    [JsonPropertyName("coordinates")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Coordinates { get; init; }

    /// <summary>
    /// External resources associated with this location.
    /// </summary>
    [JsonPropertyName("links")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, Link>? Links { get; init; }
}

/// <summary>
/// Represents a virtual location such as a video conference or chat room.
/// </summary>
public record VirtualLocation
{
    /// <summary>
    /// The type of this object. Must be "VirtualLocation" if set.
    /// </summary>
    [JsonPropertyName("@type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Type { get; init; }

    /// <summary>
    /// The human-readable name or short description of the virtual location.
    /// </summary>
    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; init; }

    /// <summary>
    /// A URI that represents how to connect to this virtual location.
    /// </summary>
    [JsonPropertyName("uri")]
    public required string Uri { get; init; }

    /// <summary>
    /// A set of features supported by this virtual location.
    /// </summary>
    [JsonPropertyName("features")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<VirtualLocationFeature, bool>? Features { get; init; }
}
