using System.Text.Json.Serialization;

namespace Sundy.Formats.JSCalendar;

/// <summary>
/// Base interface for alert triggers.
/// </summary>
public interface IAlertTrigger
{
    string? Type { get; }
}

/// <summary>
/// Triggers an alert at an offset from the event start or end time.
/// </summary>
public record OffsetTrigger : IAlertTrigger
{
    /// <summary>
    /// The type of this object. Must be "OffsetTrigger" if set.
    /// </summary>
    [JsonPropertyName("@type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Type { get; init; }

    /// <summary>
    /// The offset at which to trigger the alert.
    /// Negative durations signify alerts before, positive after.
    /// </summary>
    [JsonPropertyName("offset")]
    public required string Offset { get; init; }

    /// <summary>
    /// The time property that the alert offset is relative to.
    /// </summary>
    [JsonPropertyName("relativeTo")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public RelativeTo? RelativeTo { get; init; }
}

/// <summary>
/// Triggers an alert at a specific UTC date-time.
/// </summary>
public record AbsoluteTrigger : IAlertTrigger
{
    /// <summary>
    /// The type of this object. Must be "AbsoluteTrigger".
    /// </summary>
    [JsonPropertyName("@type")]
    public required string Type { get; init; }

    /// <summary>
    /// The specific UTC date-time when the alert is triggered.
    /// </summary>
    [JsonPropertyName("when")]
    public required string When { get; init; }
}

/// <summary>
/// Represents an unknown trigger type for future compatibility.
/// </summary>
public record UnknownTrigger : IAlertTrigger
{
    /// <summary>
    /// The type of this object (unrecognized type).
    /// </summary>
    [JsonPropertyName("@type")]
    public required string Type { get; init; }

    /// <summary>
    /// Additional properties for the unknown trigger type.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object>? ExtensionData { get; init; }
}

/// <summary>
/// Represents an alert/reminder for a calendar object.
/// </summary>
public record Alert
{
    /// <summary>
    /// The type of this object. Must be "Alert" if set.
    /// </summary>
    [JsonPropertyName("@type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Type { get; init; }

    /// <summary>
    /// Defines when to trigger the alert.
    /// </summary>
    [JsonPropertyName("trigger")]
    public required IAlertTrigger Trigger { get; init; }

    /// <summary>
    /// Records when an alert was last acknowledged.
    /// </summary>
    [JsonPropertyName("acknowledged")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Acknowledged { get; init; }

    /// <summary>
    /// Relates this alert to other alerts in the same JSCalendar object.
    /// </summary>
    [JsonPropertyName("relatedTo")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, Relation>? RelatedTo { get; init; }

    /// <summary>
    /// Describes how to alert the user.
    /// </summary>
    [JsonPropertyName("action")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public AlertAction? Action { get; init; }
}
