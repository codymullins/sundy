using System.Text;

namespace Sundy.Formats.iCalendar.Parsing;

/// <summary>
/// Handles line folding and unfolding according to RFC 5545 Section 3.1
/// Lines longer than 75 octets should be folded by inserting CRLF + (SPACE | TAB)
/// https://datatracker.ietf.org/doc/html/rfc5545#section-3.1
/// </summary>
public static class ContentLineFolder
{
    private const int MaxLineLength = 75; // octets (bytes), not characters

    /// <summary>
    /// Unfolds a content line by removing CRLF followed by space or tab
    /// </summary>
    /// <param name="input">The potentially folded content line</param>
    /// <returns>The unfolded content line</returns>
    public static string Unfold(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var sb = new StringBuilder(input.Length);
        var i = 0;

        while (i < input.Length)
        {
            // Check for CRLF followed by space or tab
            if (i + 2 < input.Length &&
                input[i] == '\r' &&
                input[i + 1] == '\n' &&
                (input[i + 2] == ' ' || input[i + 2] == '\t'))
            {
                // Skip CRLF and the space/tab
                i += 3;
            }
            else
            {
                sb.Append(input[i]);
                i++;
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Folds a content line at 75 octets (bytes) by inserting CRLF + SPACE
    /// Takes care not to split UTF-8 multi-byte characters
    /// </summary>
    /// <param name="input">The unfolded content line</param>
    /// <returns>The folded content line</returns>
    public static string Fold(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var bytes = Encoding.UTF8.GetBytes(input);
        if (bytes.Length <= MaxLineLength)
        {
            return input; // No folding needed
        }

        var result = new StringBuilder();
        var currentLineBytes = 0;
        var charIndex = 0;

        while (charIndex < input.Length)
        {
            var c = input[charIndex];
            var charBytes = Encoding.UTF8.GetByteCount(new[] { c });

            // Check if adding this character would exceed the limit
            if (currentLineBytes + charBytes > MaxLineLength && currentLineBytes > 0)
            {
                // Insert fold: CRLF + SPACE
                result.Append("\r\n ");
                currentLineBytes = 1; // The leading space counts
            }

            result.Append(c);
            currentLineBytes += charBytes;
            charIndex++;
        }

        return result.ToString();
    }

    /// <summary>
    /// Splits a multi-line string into individual content lines
    /// Handles unfolding of each line
    /// </summary>
    /// <param name="input">Multi-line iCalendar content</param>
    /// <returns>Individual unfolded content lines</returns>
    public static IEnumerable<string> SplitAndUnfold(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            yield break;
        }

        var lines = new List<string>();
        var currentLine = new StringBuilder();
        var i = 0;

        while (i < input.Length)
        {
            if (i + 1 < input.Length && input[i] == '\r' && input[i + 1] == '\n')
            {
                // Found CRLF
                i += 2;

                // Check if next line is a continuation (starts with space or tab)
                if (i < input.Length && (input[i] == ' ' || input[i] == '\t'))
                {
                    // Skip the space/tab and continue building current line
                    i++;
                }
                else
                {
                    // End of current line
                    if (currentLine.Length > 0)
                    {
                        yield return currentLine.ToString();
                        currentLine.Clear();
                    }
                }
            }
            else
            {
                currentLine.Append(input[i]);
                i++;
            }
        }

        // Return the last line if any
        if (currentLine.Length > 0)
        {
            yield return currentLine.ToString();
        }
    }
}
