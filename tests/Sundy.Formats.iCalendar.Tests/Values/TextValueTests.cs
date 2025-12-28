using Sundy.Formats.iCalendar.Values;

namespace Sundy.Formats.iCalendar.Tests.Values;

/// <summary>
/// Tests for TEXT value escaping according to RFC 5545 Section 3.3.11
/// https://datatracker.ietf.org/doc/html/rfc5545#section-3.3.11
/// </summary>
public class TextValueTests
{
    [Fact]
    public void Unescape_BackslashBackslash_ReturnsBackslash()
    {
        // Arrange
        var input = "Text with \\\\ backslash";

        // Act
        var result = TextValue.Unescape(input);

        // Assert
        Assert.Equal("Text with \\ backslash", result);
    }

    [Fact]
    public void Unescape_BackslashN_ReturnsNewline()
    {
        // Arrange
        var input = "Line one\\nLine two";

        // Act
        var result = TextValue.Unescape(input);

        // Assert
        Assert.Equal("Line one\nLine two", result);
    }

    [Fact]
    public void Unescape_BackslashNUppercase_ReturnsNewline()
    {
        // Arrange - \N should also be treated as newline
        var input = "Line one\\NLine two";

        // Act
        var result = TextValue.Unescape(input);

        // Assert
        Assert.Equal("Line one\nLine two", result);
    }

    [Fact]
    public void Unescape_BackslashComma_ReturnsComma()
    {
        // Arrange
        var input = "Item 1\\, Item 2";

        // Act
        var result = TextValue.Unescape(input);

        // Assert
        Assert.Equal("Item 1, Item 2", result);
    }

    [Fact]
    public void Unescape_BackslashSemicolon_ReturnsSemicolon()
    {
        // Arrange
        var input = "Part 1\\; Part 2";

        // Act
        var result = TextValue.Unescape(input);

        // Assert
        Assert.Equal("Part 1; Part 2", result);
    }

    [Fact]
    public void Unescape_ColonNotEscaped_RemainsUnchanged()
    {
        // Arrange - colons should NOT be escaped in TEXT
        var input = "Time: 2:00 PM";

        // Act
        var result = TextValue.Unescape(input);

        // Assert
        Assert.Equal("Time: 2:00 PM", result);
    }

    [Fact]
    public void Unescape_MultipleEscapeSequences_UnescapesAll()
    {
        // Arrange
        var input = "Text with \\n newline\\, comma\\; semicolon\\\\ backslash";

        // Act
        var result = TextValue.Unescape(input);

        // Assert
        Assert.Equal("Text with \n newline, comma; semicolon\\ backslash", result);
    }

    [Fact]
    public void Unescape_RealWorldExampleFromBasicIcs_UnescapesCorrectly()
    {
        // Arrange - from Basic.ics line 17
        var input = "Hodgenville\\, Kentucky";

        // Act
        var result = TextValue.Unescape(input);

        // Assert
        Assert.Equal("Hodgenville, Kentucky", result);
    }

    [Fact]
    public void Unescape_DescriptionFromBasicIcs_UnescapesCorrectly()
    {
        // Arrange - from Basic.ics lines 19-20 (after unfolding)
        var input = "Born February 12\\, 1809\\nSixteenth President (1861-1865)\\n\\n\\n \\nhttp://AmericanHistoryCalendar.com";

        // Act
        var result = TextValue.Unescape(input);

        // Assert
        Assert.Equal("Born February 12, 1809\nSixteenth President (1861-1865)\n\n\n \nhttp://AmericanHistoryCalendar.com", result);
    }

    [Theory]
    [InlineData("\\\\", "\\")]
    [InlineData("\\n", "\n")]
    [InlineData("\\N", "\n")]
    [InlineData("\\,", ",")]
    [InlineData("\\;", ";")]
    [InlineData("Plain text", "Plain text")]
    [InlineData("", "")]
    public void Unescape_VariousInputs_ProducesExpectedOutput(string input, string expected)
    {
        // Act
        var result = TextValue.Unescape(input);

        // Assert
        Assert.Equal(expected, result);
    }



    [Fact]
    public void Escape_Backslash_EscapesToDoubleBackslash()
    {
        // Arrange
        var input = "Text with \\ backslash";

        // Act
        var result = TextValue.Escape(input);

        // Assert
        Assert.Equal("Text with \\\\ backslash", result);
    }

    [Fact]
    public void Escape_Newline_EscapesToBackslashN()
    {
        // Arrange
        var input = "Line one\nLine two";

        // Act
        var result = TextValue.Escape(input);

        // Assert
        Assert.Equal("Line one\\nLine two", result);
    }

    [Fact]
    public void Escape_Comma_EscapesToBackslashComma()
    {
        // Arrange
        var input = "Item 1, Item 2";

        // Act
        var result = TextValue.Escape(input);

        // Assert
        Assert.Equal("Item 1\\, Item 2", result);
    }

    [Fact]
    public void Escape_Semicolon_EscapesToBackslashSemicolon()
    {
        // Arrange
        var input = "Part 1; Part 2";

        // Act
        var result = TextValue.Escape(input);

        // Assert
        Assert.Equal("Part 1\\; Part 2", result);
    }

    [Fact]
    public void Escape_Colon_RemainsUnescaped()
    {
        // Arrange - colons must NOT be escaped
        var input = "Time: 2:00 PM";

        // Act
        var result = TextValue.Escape(input);

        // Assert
        Assert.Equal("Time: 2:00 PM", result);
    }

    [Fact]
    public void Escape_MultipleSpecialChars_EscapesAll()
    {
        // Arrange
        var input = "Text with \n newline, comma; semicolon\\ backslash";

        // Act
        var result = TextValue.Escape(input);

        // Assert
        Assert.Equal("Text with \\n newline\\, comma\\; semicolon\\\\ backslash", result);
    }

    [Theory]
    [InlineData("\\", "\\\\")]
    [InlineData("\n", "\\n")]
    [InlineData(",", "\\,")]
    [InlineData(";", "\\;")]
    [InlineData(":", ":")]  // NOT escaped
    [InlineData("Plain text", "Plain text")]
    [InlineData("", "")]
    public void Escape_VariousInputs_ProducesExpectedOutput(string input, string expected)
    {
        // Act
        var result = TextValue.Escape(input);

        // Assert
        Assert.Equal(expected, result);
    }



    [Fact]
    public void RoundTrip_UnescapeThenEscape_ReturnsOriginal()
    {
        // Arrange
        var original = "Text with \\n newline\\, comma\\; semicolon\\\\ backslash";

        // Act
        var unescaped = TextValue.Unescape(original);
        var escaped = TextValue.Escape(unescaped);

        // Assert
        Assert.Equal(original, escaped);
    }

    [Fact]
    public void RoundTrip_EscapeThenUnescape_ReturnsOriginal()
    {
        // Arrange
        var original = "Text with \n newline, comma; semicolon\\ backslash";

        // Act
        var escaped = TextValue.Escape(original);
        var unescaped = TextValue.Unescape(escaped);

        // Assert
        Assert.Equal(original, unescaped);
    }

    [Theory]
    [InlineData("Simple text")]
    [InlineData("Text with \n newlines")]
    [InlineData("Comma, separated, values")]
    [InlineData("Semicolon; separated; values")]
    [InlineData("Backslash \\ character")]
    [InlineData("Mixed: newline\n, comma, semicolon; backslash\\")]
    [InlineData("")]
    public void RoundTrip_VariousTexts_PreserveMeaning(string original)
    {
        // Act
        var escaped = TextValue.Escape(original);
        var unescaped = TextValue.Unescape(escaped);

        // Assert
        Assert.Equal(original, unescaped);
    }



    [Fact]
    public void Unescape_ConsecutiveBackslashes_UnescapesCorrectly()
    {
        // Arrange - \\n should be backslash + n, not newline
        var input = "\\\\n";

        // Act
        var result = TextValue.Unescape(input);

        // Assert
        Assert.Equal("\\n", result);
    }

    [Fact]
    public void Unescape_BackslashAtEnd_LeavesUnchanged()
    {
        // Arrange - trailing backslash with no character after it
        var input = "Text\\";

        // Act
        var result = TextValue.Unescape(input);

        // Assert
        // Per RFC, undefined escape sequences should be left as-is or handled gracefully
        Assert.Equal("Text\\", result);
    }

    [Fact]
    public void Unescape_UnknownEscapeSequence_LeavesUnchanged()
    {
        // Arrange - \x is not a defined escape sequence
        var input = "Text\\x";

        // Act
        var result = TextValue.Unescape(input);

        // Assert
        // Undefined sequences should be left as-is
        Assert.Equal("Text\\x", result);
    }

    [Fact]
    public void Escape_EmptyString_ReturnsEmpty()
    {
        // Act
        var result = TextValue.Escape("");

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void Unescape_EmptyString_ReturnsEmpty()
    {
        // Act
        var result = TextValue.Unescape("");

        // Assert
        Assert.Equal("", result);
    }

}
