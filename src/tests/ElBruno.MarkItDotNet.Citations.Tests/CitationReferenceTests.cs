using ElBruno.MarkItDotNet.CoreModel;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Citations.Tests;

public class CitationReferenceTests
{
    [Fact]
    public void Default_Mode_Is_Exact()
    {
        var citation = new CitationReference();

        citation.Mode.Should().Be(CitationMode.Exact);
    }

    [Fact]
    public void AllProperties_CanBeSet()
    {
        var span = new SpanReference { Offset = 10, Length = 50 };
        var citation = new CitationReference
        {
            FilePath = "doc.pdf",
            PageNumber = 3,
            SectionTitle = "Introduction",
            HeadingPath = "Chapter 1 > Introduction",
            BlockId = "block-42",
            Span = span,
            Mode = CitationMode.Exact
        };

        citation.FilePath.Should().Be("doc.pdf");
        citation.PageNumber.Should().Be(3);
        citation.SectionTitle.Should().Be("Introduction");
        citation.HeadingPath.Should().Be("Chapter 1 > Introduction");
        citation.BlockId.Should().Be("block-42");
        citation.Span.Should().Be(span);
        citation.Mode.Should().Be(CitationMode.Exact);
    }

    [Fact]
    public void NullableProperties_DefaultToNull()
    {
        var citation = new CitationReference();

        citation.FilePath.Should().BeNull();
        citation.PageNumber.Should().BeNull();
        citation.SectionTitle.Should().BeNull();
        citation.HeadingPath.Should().BeNull();
        citation.BlockId.Should().BeNull();
        citation.Span.Should().BeNull();
    }

    [Fact]
    public void CoarseMode_CanBeSet()
    {
        var citation = new CitationReference { Mode = CitationMode.Coarse };

        citation.Mode.Should().Be(CitationMode.Coarse);
    }

    [Fact]
    public void Record_Equality_Works()
    {
        var citation1 = new CitationReference
        {
            FilePath = "doc.pdf",
            PageNumber = 1,
            Mode = CitationMode.Exact
        };
        var citation2 = new CitationReference
        {
            FilePath = "doc.pdf",
            PageNumber = 1,
            Mode = CitationMode.Exact
        };

        citation1.Should().Be(citation2);
    }

    [Fact]
    public void Record_With_Expression_Creates_Modified_Copy()
    {
        var original = new CitationReference
        {
            FilePath = "doc.pdf",
            PageNumber = 1,
            Mode = CitationMode.Exact
        };

        var modified = original with { PageNumber = 5, Mode = CitationMode.Coarse };

        modified.FilePath.Should().Be("doc.pdf");
        modified.PageNumber.Should().Be(5);
        modified.Mode.Should().Be(CitationMode.Coarse);
    }

    [Fact]
    public void Span_Can_Be_Assigned()
    {
        var span = new SpanReference { Offset = 0, Length = 100 };
        var citation = new CitationReference { Span = span };

        citation.Span!.Offset.Should().Be(0);
        citation.Span.Length.Should().Be(100);
    }
}
