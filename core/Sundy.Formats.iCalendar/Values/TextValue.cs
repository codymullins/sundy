using System.Text;

namespace Sundy.Formats.iCalendar.Values;

/// <summary>
/// Handles TEXT value escaping and unescaping according to RFC 5545 Section 3.3.11
/// https://datatracker.ietf.org/doc/html/rfc5545#section-3.3.11
///
/// Escape sequences in TEXT values:
/// - \\ → backslash
/// - \n or \N → newline
/// - \, → comma
/// - \; → semicolon
/// - Colon (:) is NOT escaped
/// </summary>
public static class TextValue
{
    /// <summary>
    /// Unescapes a TEXT value by replacing escape sequences with their actual characters
    /// </summary>
    /// <param name="input">The escaped TEXT value</param>
    /// <returns>The unescaped TEXT value</returns>
    public static string Unescape(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var result = new StringBuilder(input.Length);
        var i = 0;

        while (i < input.Length)
        {
            if (input[i] == '\\' && i + 1 < input.Length)
            {
                // Check the next character after backslash
                var nextChar = input[i + 1];

                switch (nextChar)
                {
                    case '\\':
                        result.Append('\\');
                        i += 2;
                        break;
                    case 'n':
                    case 'N':
                        result.Append('\n');
                        i += 2;
                        break;
                    case ',':
                        result.Append(',');
                        i += 2;
                        break;
                    case ';':
                        result.Append(';');
                        i += 2;
                        break;
                    default:
                        // Unknown escape sequence - leave as-is
                        result.Append('\\');
                        result.Append(nextChar);
                        i += 2;
                        break;
                }
            }
            else
            {
                result.Append(input[i]);
                i++;
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Escapes a TEXT value by replacing special characters with their escape sequences
    /// </summary>
    /// <param name="input">The unescaped TEXT value</param>
    /// <returns>The escaped TEXT value</returns>
    public static string Escape(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var result = new StringBuilder(input.Length * 2); // Pre-allocate for potential escapes

        foreach (var c in input)
        {
            switch (c)
            {
                case '\\':
                    result.Append("\\\\");
                    break;
                case '\n':
                    result.Append("\\n");
                    break;
                case ',':
                    result.Append("\\,");
                    break;
                case ';':
                    result.Append("\\;");
                    break;
                // Note: Colon (:) is NOT escaped in TEXT values
                default:
                    result.Append(c);
                    break;
            }
        }

        return result.ToString();
    }
}
