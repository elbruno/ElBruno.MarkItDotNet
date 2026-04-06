using ElBruno.MarkItDotNet.CoreModel;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Citations.Tests;

public class CitationSerializerTests
{
    [Fact]
    public void RoundTrip_PreservesAllFields()
    {
        var original = new CitationSet
        {
            ChunkId = "chunk-42",
            Citations = new List<CitationReference>
            {
                new CitationReference
                {
                    FilePath = "report.pdf",
                    PageNumber = 5,
                    SectionTitle = "Findings",
                    HeadingPath = "Chapter 2 > Findings",
                    BlockId = "block-7",
                    Span = new SpanReference { Offset = 100, Length = 50 },
                    Mode = CitationMode.Exact
                }
            }
        };

        var json = CitationSerializer.Serialize(original);
        var deserialized = CitationSerializer.Deserialize(json);

        deserialized.ChunkId.Should().Be("chunk-42");
        deserialized.Citations.Should().HaveCount(1);

        var citation = deserialized.Citations[0];
        citation.FilePath.Should().Be("report.pdf");
        citation.PageNumber.Should().Be(5);
        citation.SectionTitle.Should().Be("Findings");
        citation.HeadingPath.Should().Be("Chapter 2 > Findings");
        citation.BlockId.Should().Be("block-7");
        citation.Span.Should().NotBeNull();
        citation.Span!.Offset.Should().Be(100);
        citation.Span.Length.Should().Be(50);
        citation.Mode.Should().Be(CitationMode.Exact);
    }

    [Fact]
    public void RoundTrip_EmptyCitations_Preserved()
    {
        var original = new CitationSet
        {
            ChunkId = "empty-chunk",
            Citations = new List<CitationReference>()
        };

        var json = CitationSerializer.Serialize(original);
        var deserialized = CitationSerializer.Deserialize(json);

        deserialized.ChunkId.Should().Be("empty-chunk");
        deserialized.Citations.Should().BeEmpty();
    }

    [Fact]
    public void Serialize_UsesCamelCaseNaming()
    {
        var set = new CitationSet
        {
            ChunkId = "test",
            Citations = new List<CitationReference>
            {
                new CitationReference
                {
                    FilePath = "doc.pdf",
                    PageNumber = 1
                }
            }
        };

        var json = CitationSerializer.Serialize(set);

        json.Should().Contain("\"chunkId\"");
        json.Should().Contain("\"filePath\"");
        json.Should().Contain("\"pageNumber\"");
        json.Should().NotContain("\"ChunkId\"");
        json.Should().NotContain("\"FilePath\"");
    }

    [Fact]
    public void Serialize_OmitsNullFields()
    {
        var set = new CitationSet
        {
            ChunkId = "test",
            Citations = new List<CitationReference>
            {
                new CitationReference { FilePath = "doc.pdf" }
            }
        };

        var json = CitationSerializer.Serialize(set);

        json.Should().NotContain("\"pageNumber\"");
        json.Should().NotContain("\"sectionTitle\"");
        json.Should().NotContain("\"span\"");
    }

    [Fact]
    public void RoundTrip_CoarseMode_Preserved()
    {
        var original = new CitationSet
        {
            ChunkId = "coarse-test",
            Citations = new List<CitationReference>
            {
                new CitationReference
                {
                    FilePath = "notes.md",
                    Mode = CitationMode.Coarse
                }
            }
        };

        var json = CitationSerializer.Serialize(original);
        var deserialized = CitationSerializer.Deserialize(json);

        deserialized.Citations[0].Mode.Should().Be(CitationMode.Coarse);
    }

    [Fact]
    public void RoundTrip_MultipleCitations_AllPreserved()
    {
        var original = new CitationSet
        {
            ChunkId = "multi",
            Citations = new List<CitationReference>
            {
                new CitationReference { FilePath = "a.pdf", PageNumber = 1 },
                new CitationReference { FilePath = "b.pdf", PageNumber = 2 },
                new CitationReference { FilePath = "c.pdf", PageNumber = 3 }
            }
        };

        var json = CitationSerializer.Serialize(original);
        var deserialized = CitationSerializer.Deserialize(json);

        deserialized.Citations.Should().HaveCount(3);
        deserialized.Citations[0].FilePath.Should().Be("a.pdf");
        deserialized.Citations[1].FilePath.Should().Be("b.pdf");
        deserialized.Citations[2].FilePath.Should().Be("c.pdf");
    }

    [Fact]
    public void Deserialize_NullInput_ThrowsArgumentNullException()
    {
        var act = () => CitationSerializer.Deserialize(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Serialize_NullInput_ThrowsArgumentNullException()
    {
        var act = () => CitationSerializer.Serialize(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
