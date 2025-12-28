using System.Text;

namespace Sundy.Formats.iCalendar.Parsing;

/// <summary>
/// Parses iCalendar content lines according to RFC 5545 Section 3.1
/// Format: name *(";" param) ":" value CRLF
/// https://datatracker.ietf.org/doc/html/rfc5545#section-3.1
/// </summary>
public class ContentLineParser
{
    /// <summary>
    /// Parses a single unfolded content line
    /// </summary>
    /// <param name="line">The unfolded content line</param>
    /// <returns>Parsed ContentLine object</returns>
    /// <exception cref="ArgumentException">If line is null or whitespace</exception>
    /// <exception cref="FormatException">If line format is invalid</exception>
    public ContentLine Parse(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            throw new ArgumentException("Content line cannot be null or whitespace", nameof(line));
        }

        // Find the first colon outside of quotes (separates params from value)
        var colonIndex = FindFirstUnquotedChar(line, ':');
        if (colonIndex == -1)
        {
            throw new FormatException("Content line must contain a colon separating name/params from value");
        }

        var nameAndParams = line.Substring(0, colonIndex);
        var value = line.Substring(colonIndex + 1);

        // Parse name and parameters
        var (name, parameters) = ParseNameAndParameters(nameAndParams);

        return new ContentLine(name, value, parameters);
    }

    /// <summary>
    /// Finds the first occurrence of a character that's not inside quotes
    /// </summary>
    private int FindFirstUnquotedChar(string input, char target)
    {
        var inQuotes = false;

        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == target && !inQuotes)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Parses the name and parameters portion (before the colon)
    /// Format: name *(";" param)
    /// </summary>
    private (string name, Dictionary<string, string> parameters) ParseNameAndParameters(string input)
    {
        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Split by semicolon (but respect quotes)
        var parts = SplitRespectingQuotes(input, ';');

        if (parts.Count == 0)
        {
            throw new FormatException("Invalid content line: no property name found");
        }

        // First part is the property name
        var name = parts[0].Trim().ToUpperInvariant();

        // Remaining parts are parameters
        for (var i = 1; i < parts.Count; i++)
        {
            var param = parts[i].Trim();
            var equalsIndex = param.IndexOf('=');

            if (equalsIndex == -1)
            {
                throw new FormatException($"Invalid parameter format: '{param}'. Expected 'NAME=VALUE'");
            }

            var paramName = param.Substring(0, equalsIndex).Trim().ToUpperInvariant();
            var paramValue = param.Substring(equalsIndex + 1).Trim();

            // Check if value is quoted
            var wasQuoted = paramValue.StartsWith('"') && paramValue.EndsWith('"') && paramValue.Length >= 2;

            // Remove quotes if present
            if (wasQuoted)
            {
                paramValue = paramValue.Substring(1, paramValue.Length - 2);
            }

            // Only normalize enumerated values (non-quoted) to uppercase
            // Quoted values preserve their case (e.g., CN="John Doe", ALTREP="http://...")
            if (!wasQuoted)
            {
                paramValue = paramValue.ToUpperInvariant();
            }

            parameters[paramName] = paramValue;
        }

        return (name, parameters);
    }

    /// <summary>
    /// Splits a string by delimiter while respecting quoted strings
    /// Quoted strings can contain the delimiter without being split
    /// </summary>
    private List<string> SplitRespectingQuotes(string input, char delimiter)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
                current.Append(c);
            }
            else if (c == delimiter && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        // Add the last part
        if (current.Length > 0)
        {
            result.Add(current.ToString());
        }

        return result;
    }
}
