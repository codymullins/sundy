namespace Sundy.Formats.iCalendar.Parsing;

/// <summary>
/// Represents a parsed iCalendar content line according to RFC 5545 Section 3.1
/// Format: name *(";" param) ":" value CRLF
/// </summary>
public class ContentLine
{
    /// <summary>
    /// The property name (case-insensitive, normalized to uppercase)
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The property value (case-sensitive)
    /// </summary>
    public string Value { get; init; } = string.Empty;

    /// <summary>
    /// Property parameters as key-value pairs
    /// Keys are normalized to uppercase (case-insensitive)
    /// </summary>
    public Dictionary<string, string> Parameters { get; init; } = new();

    public ContentLine()
    {
    }

    public ContentLine(string name, string value)
    {
        Name = name;
        Value = value;
    }

    public ContentLine(string name, string value, Dictionary<string, string> parameters)
    {
        Name = name;
        Value = value;
        Parameters = parameters;
    }
}
