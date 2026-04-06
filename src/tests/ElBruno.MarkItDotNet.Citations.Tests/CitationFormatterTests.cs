using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Citations.Tests;

public class CitationFormatterTests
{
    [Fact]
    public void Format_WithAllFields_IncludesAllParts()
    {
        var citation = new CitationReference
        {
            FilePath = "document.pdf",
            PageNumber = 3,
            SectionTitle = "Introduction",
            BlockId = "b1"
        };

        var result = CitationFormatter.Format(citation);

        result.Should().Contain("document.pdf");
        result.Should().Contain("Page 3");
        result.Should().Contain("Section 'Introduction'");
        result.Should().Contain("Block b1");
    }

    [Fact]
    public void Format_WithOnlyFilePath_ReturnsFilePath()
    {
        var citation = new CitationReference { FilePath = "notes.md" };

        var result = CitationFormatter.Format(citation);

        result.Should().Be("notes.md");
    }

    [Fact]
    public void Format_WithNoFields_ReturnsUnknownSource()
    {
        var citation = new CitationReference();

        var result = CitationFormatter.Format(citation);

        result.Should().Be("Unknown source");
    }

    [Fact]
    public void Format_HeadingPath_UsedWhenNoSectionTitle()
    {
        var citation = new CitationReference
        {
            FilePath = "doc.pdf",
            HeadingPath = "Chapter 1 > Intro"
        };

        var result = CitationFormatter.Format(citation);

        result.Should().Contain("Section 'Chapter 1 > Intro'");
    }

    [Fact]
    public void Format_SectionTitle_TakesPriorityOverHeadingPath()
    {
        var citation = new CitationReference
        {
            SectionTitle = "My Section",
            HeadingPath = "Chapter 1 > My Section"
        };

        var result = CitationFormatter.Format(citation);

        result.Should().Contain("Section 'My Section'");
        result.Should().NotContain("Chapter 1 > My Section");
    }

    [Fact]
    public void FormatShort_WithFileAndPage_ReturnsCompactForm()
    {
        var citation = new CitationReference
        {
            FilePath = "document.pdf",
            PageNumber = 3
        };

        var result = CitationFormatter.FormatShort(citation);

        result.Should().Be("document.pdf p.3");
    }

    [Fact]
    public void FormatShort_WithNoFields_ReturnsUnknown()
    {
        var citation = new CitationReference();

        var result = CitationFormatter.FormatShort(citation);

        result.Should().Be("Unknown");
    }

    [Fact]
    public void FormatMarkdown_WithPageNumber_CreatesPageAnchor()
    {
        var citation = new CitationReference
        {
            FilePath = "document.pdf",
            PageNumber = 3
        };

        var result = CitationFormatter.FormatMarkdown(citation);

        result.Should().Be("[document.pdf p.3](#page-3)");
    }

    [Fact]
    public void FormatMarkdown_WithSectionTitle_CreatesSectionAnchor()
    {
        var citation = new CitationReference
        {
            FilePath = "doc.md",
            SectionTitle = "Getting Started"
        };

        var result = CitationFormatter.FormatMarkdown(citation);

        result.Should().Contain("#getting-started");
        result.Should().StartWith("[");
        result.Should().EndWith(")");
    }

    [Fact]
    public void FormatMarkdown_WithBlockIdOnly_CreatesBlockAnchor()
    {
        var citation = new CitationReference
        {
            BlockId = "abc-123"
        };

        var result = CitationFormatter.FormatMarkdown(citation);

        result.Should().Contain("#block-abc-123");
    }
}
