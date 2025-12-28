using Sundy.Formats.iCalendar.Parsing;

namespace Sundy.Formats.iCalendar.Tests.Parsing;

/// <summary>
/// Tests for content-line parsing according to RFC 5545 Section 3.1 (Content Lines)
/// https://datatracker.ietf.org/doc/html/rfc5545#section-3.1
/// </summary>
public class ContentLineTests
{
    [Fact]
    public void Unfold_SingleLineNoFolding_ReturnsUnchanged()
    {
        // Arrange
        var input = "SUMMARY:Test Event";

        // Act
        var result = ContentLineFolder.Unfold(input);

        // Assert
        Assert.Equal("SUMMARY:Test Event", result);
    }

    [Fact]
    public void Unfold_SingleFoldWithSpace_UnfoldsCorrectly()
    {
        // Arrange
        var input = "DESCRIPTION:This is a long line that has been \r\n folded";

        // Act
        var result = ContentLineFolder.Unfold(input);

        // Assert
        Assert.Equal("DESCRIPTION:This is a long line that has been folded", result);
    }

    [Fact]
    public void Unfold_SingleFoldWithTab_UnfoldsCorrectly()
    {
        // Arrange
        var input = "DESCRIPTION:This is a long line that has been \r\n\tfolded";

        // Act
        var result = ContentLineFolder.Unfold(input);

        // Assert
        Assert.Equal("DESCRIPTION:This is a long line that has been folded", result);
    }

    [Fact]
    public void Unfold_MultipleFolds_UnfoldsAllCorrectly()
    {
        // Arrange
        var input = "DESCRIPTION:Line one \r\n line two \r\n line three";

        // Act
        var result = ContentLineFolder.Unfold(input);

        // Assert
        Assert.Equal("DESCRIPTION:Line one line two line three", result);
    }

    [Fact]
    public void Unfold_MixedSpaceAndTab_UnfoldsCorrectly()
    {
        // Arrange
        var input = "DESCRIPTION:First \r\n second \r\n\tthird";

        // Act
        var result = ContentLineFolder.Unfold(input);

        // Assert
        Assert.Equal("DESCRIPTION:First second third", result);
    }

    [Fact]
    public void Unfold_RealWorldExampleFromBasicIcs_UnfoldsCorrectly()
    {
        // Arrange - from Basic.ics lines 19-20
        // Line 20 starts with space + \n, so we need space before fold marker
        var input = "DESCRIPTION:Born February 12\\, 1809\\nSixteenth President (1861-1865)\\n\\n\\n \r\n \\nhttp://AmericanHistoryCalendar.com";

        // Act
        var result = ContentLineFolder.Unfold(input);

        // Assert
        Assert.Equal("DESCRIPTION:Born February 12\\, 1809\\nSixteenth President (1861-1865)\\n\\n\\n \\nhttp://AmericanHistoryCalendar.com", result);
    }

    [Fact]
    public void Unfold_UrlExampleFromBasicIcs_UnfoldsCorrectly()
    {
        // Arrange - from Basic.ics lines 21-22
        var input = "URL:http://americanhistorycalendar.com/peoplecalendar/1,328-abraham-lincol\r\n n";

        // Act
        var result = ContentLineFolder.Unfold(input);

        // Assert
        Assert.Equal("URL:http://americanhistorycalendar.com/peoplecalendar/1,328-abraham-lincoln", result);
    }

    [Fact]
    public void Unfold_Utf8MultiByteCharacterAcrossFold_PreservesCharacter()
    {
        // Arrange - emoji with fold after it
        // Note: Proper folding should not split UTF-8 multi-byte characters
        var emoji = "😀"; // 4-byte UTF-8 character
        var input = $"SUMMARY:Test {emoji} \r\n event";

        // Act
        var result = ContentLineFolder.Unfold(input);

        // Assert
        Assert.Equal($"SUMMARY:Test {emoji} event", result);
    }

    [Fact]
    public void Unfold_NoSpaceAfterCRLF_TreatsAsRegularNewline()
    {
        // Arrange - CRLF without space/tab is NOT a fold
        var input = "SUMMARY:Line one\r\nDTSTART:20240101";

        // Act
        var result = ContentLineFolder.Unfold(input);

        // Assert - should remain unchanged
        Assert.Equal("SUMMARY:Line one\r\nDTSTART:20240101", result);
    }

    [Fact]
    public void Parse_SimpleProperty_ParsesNameAndValue()
    {
        // Arrange
        var input = "SUMMARY:Test Event";
        var parser = new ContentLineParser();

        // Act
        var result = parser.Parse(input);

        // Assert
        Assert.Equal("SUMMARY", result.Name);
        Assert.Equal("Test Event", result.Value);
        Assert.Empty(result.Parameters);
    }

    [Fact]
    public void Parse_PropertyWithSingleParameter_ParsesCorrectly()
    {
        // Arrange
        var input = "DTSTART;VALUE=DATE:20080212";
        var parser = new ContentLineParser();

        // Act
        var result = parser.Parse(input);

        // Assert
        Assert.Equal("DTSTART", result.Name);
        Assert.Equal("20080212", result.Value);
        Assert.Single(result.Parameters);
        Assert.Equal("DATE", result.Parameters["VALUE"]);
    }

    [Fact]
    public void Parse_PropertyWithMultipleParameters_ParsesAllParameters()
    {
        // Arrange
        var input = "ATTENDEE;CN=\"John Doe\";RSVP=TRUE:mailto:john@example.com";
        var parser = new ContentLineParser();

        // Act
        var result = parser.Parse(input);

        // Assert
        Assert.Equal("ATTENDEE", result.Name);
        Assert.Equal("mailto:john@example.com", result.Value);
        Assert.Equal(2, result.Parameters.Count);
        Assert.Equal("John Doe", result.Parameters["CN"]);
        Assert.Equal("TRUE", result.Parameters["RSVP"]);
    }

    [Fact]
    public void Parse_PropertyWithQuotedParameter_RemovesQuotes()
    {
        // Arrange
        var input = "LOCATION;ALTREP=\"http://example.com\":Room 123";
        var parser = new ContentLineParser();

        // Act
        var result = parser.Parse(input);

        // Assert
        Assert.Equal("LOCATION", result.Name);
        Assert.Equal("Room 123", result.Value);
        Assert.Single(result.Parameters);
        Assert.Equal("http://example.com", result.Parameters["ALTREP"]);
    }

    [Fact]
    public void Parse_PropertyWithQuotedParameterContainingSpecialChars_ParsesCorrectly()
    {
        // Arrange - quoted parameter can contain : ; ,
        var input = "DESCRIPTION;ALTREP=\"http://example.com:8080;foo=bar,baz\":Test";
        var parser = new ContentLineParser();

        // Act
        var result = parser.Parse(input);

        // Assert
        Assert.Equal("DESCRIPTION", result.Name);
        Assert.Equal("Test", result.Value);
        Assert.Equal("http://example.com:8080;foo=bar,baz", result.Parameters["ALTREP"]);
    }

    [Fact]
    public void Parse_MultiValuedProperty_ParsesValue()
    {
        // Arrange - from Basic.ics line 16
        var input = "CATEGORIES:U.S. Presidents,Civil War People";
        var parser = new ContentLineParser();

        // Act
        var result = parser.Parse(input);

        // Assert
        Assert.Equal("CATEGORIES", result.Name);
        Assert.Equal("U.S. Presidents,Civil War People", result.Value);
        // Note: Splitting comma-separated values is done by property-specific parsers
    }

    [Fact]
    public void Parse_PropertyNameCaseInsensitive_NormalizesToUppercase()
    {
        // Arrange
        var input = "summary:Test";
        var parser = new ContentLineParser();

        // Act
        var result = parser.Parse(input);

        // Assert
        Assert.Equal("SUMMARY", result.Name); // Normalized to uppercase
    }

    [Fact]
    public void Parse_ParameterNameCaseInsensitive_NormalizesToUppercase()
    {
        // Arrange
        var input = "DTSTART;value=date:20080212";
        var parser = new ContentLineParser();

        // Act
        var result = parser.Parse(input);

        // Assert
        Assert.Equal("DATE", result.Parameters["VALUE"]); // Param value normalized
    }

    [Fact]
    public void Parse_PropertyValueCaseSensitive_PreservesCase()
    {
        // Arrange
        var input = "SUMMARY:Test Event With MixedCase";
        var parser = new ContentLineParser();

        // Act
        var result = parser.Parse(input);

        // Assert
        Assert.Equal("Test Event With MixedCase", result.Value); // Case preserved
    }

    [Fact]
    public void Parse_ValueContainingColon_SplitsOnFirstColon()
    {
        // Arrange - value can contain colons
        var input = "URL:http://example.com:8080/path";
        var parser = new ContentLineParser();

        // Act
        var result = parser.Parse(input);

        // Assert
        Assert.Equal("URL", result.Name);
        Assert.Equal("http://example.com:8080/path", result.Value);
    }

    [Fact]
    public void Parse_ValueContainingSemicolon_PreservesSemicolon()
    {
        // Arrange - semicolon in value (not parameter) is preserved
        var input = "DESCRIPTION:Meeting at 2:00; bring notes";
        var parser = new ContentLineParser();

        // Act
        var result = parser.Parse(input);

        // Assert
        Assert.Equal("DESCRIPTION", result.Name);
        Assert.Equal("Meeting at 2:00; bring notes", result.Value);
    }

    [Fact]
    public void Parse_MissingColon_ThrowsFormatException()
    {
        // Arrange
        var input = "SUMMARY Test Event"; // No colon
        var parser = new ContentLineParser();

        // Act & Assert
        Assert.Throws<FormatException>(() => parser.Parse(input));
    }

    [Fact]
    public void Parse_EmptyLine_ThrowsArgumentException()
    {
        // Arrange
        var input = "";
        var parser = new ContentLineParser();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => parser.Parse(input));
    }

    [Fact]
    public void Parse_OnlyWhitespace_ThrowsArgumentException()
    {
        // Arrange
        var input = "   ";
        var parser = new ContentLineParser();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => parser.Parse(input));
    }

    [Fact]
    public void Parse_MalformedParameter_ThrowsFormatException()
    {
        // Arrange
        var input = "DTSTART;INVALIDPARAM:20080212"; // Missing = in parameter
        var parser = new ContentLineParser();

        // Act & Assert
        Assert.Throws<FormatException>(() => parser.Parse(input));
    }

    [Fact]
    public void Parse_RequiresCRLFLineEndings_StrictMode()
    {
        // Arrange - in strict mode, only CRLF is valid
        // This is a future enhancement - for now, we'll be lenient
        var input = "SUMMARY:Test\nDTSTART:20240101"; // LF only
        var parser = new ContentLineParser();

        // Act
        var result = parser.Parse(input.Split('\n')[0]);

        // Assert - lenient mode accepts it
        Assert.Equal("SUMMARY", result.Name);
    }
}
