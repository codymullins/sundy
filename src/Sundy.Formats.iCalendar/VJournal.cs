using System.Text.Json.Serialization;

namespace Sundy.Formats.iCalendar;

/// <summary>
/// Represents a VJOURNAL component - a journal entry or note.
/// </summary>
public record VJournal : ComponentBase
{
    /// <summary>
    /// The start date/time of the journal entry.
    /// </summary>
    [JsonPropertyName("dtstart")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DateStart { get; init; }

    /// <summary>
    /// The status of the journal entry.
    /// </summary>
    [JsonPropertyName("status")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JournalStatus? Status { get; init; }

    /// <summary>
    /// Contact information for the journal entry.
    /// </summary>
    [JsonPropertyName("contact")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Contact { get; init; }

    /// <summary>
    /// Recurrence rule for the journal entry.
    /// </summary>
    [JsonPropertyName("rrule")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public RecurrenceRule? RecurrenceRule { get; init; }

    /// <summary>
    /// Recurrence date-times for the journal entry.
    /// </summary>
    [JsonPropertyName("rdate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? RecurrenceDates { get; init; }

    /// <summary>
    /// Exception date-times for the recurrence.
    /// </summary>
    [JsonPropertyName("exdate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? ExceptionDates { get; init; }

    /// <summary>
    /// Exception rules for the recurrence.
    /// </summary>
    [JsonPropertyName("exrule")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public RecurrenceRule[]? ExceptionRules { get; init; }

    /// <summary>
    /// The recurrence ID for an instance of a recurring journal entry.
    /// </summary>
    [JsonPropertyName("recurrenceId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RecurrenceId { get; init; }

    /// <summary>
    /// Range parameter for recurrence ID.
    /// </summary>
    [JsonPropertyName("range")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Range? Range { get; init; }
}
