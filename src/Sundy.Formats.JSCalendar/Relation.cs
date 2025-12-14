using System.Text.Json.Serialization;

namespace Sundy.Formats.JSCalendar;

/// <summary>
/// Defines the relation to other objects using a set of relation types.
/// </summary>
public record Relation
{
    /// <summary>
    /// The type of this object. Must be "Relation" if set.
    /// </summary>
    [JsonPropertyName("@type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Type { get; init; }

    /// <summary>
    /// Describes how the linked object is related to the linking object.
    /// Empty dictionary represents a "parent" relation unless defined differently.
    /// </summary>
    [JsonPropertyName("relation")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<RelationType, bool>? RelationTypes { get; init; }
}
