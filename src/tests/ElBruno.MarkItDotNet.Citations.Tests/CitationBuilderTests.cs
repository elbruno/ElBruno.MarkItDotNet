using ElBruno.MarkItDotNet.CoreModel;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Citations.Tests;

public class CitationBuilderTests
{
    [Fact]
    public void FromBlock_WithSourceReference_CreatesExactCitation()
    {
        var block = new ParagraphBlock
        {
            Id = "p1",
            Text = "Hello world",
            Source = new SourceReference
            {
                FilePath = "test.pdf",
                PageNumber = 2,
                HeadingPath = "Chapter 1",
                Span = new SpanReference { Offset = 10, Length = 20 }
            }
        };

        var citation = CitationBuilder.FromBlock(block, "fallback.pdf", "Fallback Heading");

        citation.FilePath.Should().Be("test.pdf");
        citation.PageNumber.Should().Be(2);
        citation.HeadingPath.Should().Be("Chapter 1");
        citation.BlockId.Should().Be("p1");
        citation.Span.Should().NotBeNull();
        citation.Span!.Offset.Should().Be(10);
        citation.Span.Length.Should().Be(20);
        citation.Mode.Should().Be(CitationMode.Exact);
    }

    [Fact]
    public void FromBlock_WithoutSourceReference_CreatesCoarseCitation()
    {
        var block = new ParagraphBlock
        {
            Id = "p2",
            Text = "No source info"
        };

        var citation = CitationBuilder.FromBlock(block, "file.md", "Some Heading");

        citation.FilePath.Should().Be("file.md");
        citation.HeadingPath.Should().Be("Some Heading");
        citation.BlockId.Should().Be("p2");
        citation.PageNumber.Should().BeNull();
        citation.Span.Should().BeNull();
        citation.Mode.Should().Be(CitationMode.Coarse);
    }

    [Fact]
    public void FromBlock_SourceWithoutFilePath_FallsBackToProvidedFilePath()
    {
        var block = new ParagraphBlock
        {
            Id = "p3",
            Text = "Content",
            Source = new SourceReference { PageNumber = 5 }
        };

        var citation = CitationBuilder.FromBlock(block, "fallback.pdf", null);

        citation.FilePath.Should().Be("fallback.pdf");
        citation.PageNumber.Should().Be(5);
        citation.Mode.Should().Be(CitationMode.Exact);
    }

    [Fact]
    public void FromSection_ExtractsCitationsForAllBlocks()
    {
        var section = new DocumentSection
        {
            Id = "s1",
            Heading = new HeadingBlock { Id = "h1", Text = "Introduction", Level = 1 },
            Blocks = new List<DocumentBlock>
            {
                new ParagraphBlock { Id = "p1", Text = "First paragraph" },
                new ParagraphBlock { Id = "p2", Text = "Second paragraph" }
            }
        };

        var citations = CitationBuilder.FromSection(section, "doc.pdf");

        citations.Should().HaveCount(2);
        citations[0].BlockId.Should().Be("p1");
        citations[1].BlockId.Should().Be("p2");
        citations.Should().AllSatisfy(c => c.FilePath.Should().Be("doc.pdf"));
        citations.Should().AllSatisfy(c => c.HeadingPath.Should().Be("Introduction"));
    }

    [Fact]
    public void FromSection_IncludesSubSectionBlocks()
    {
        var section = new DocumentSection
        {
            Id = "s1",
            Heading = new HeadingBlock { Id = "h1", Text = "Chapter", Level = 1 },
            Blocks = new List<DocumentBlock>
            {
                new ParagraphBlock { Id = "p1", Text = "Intro" }
            },
            SubSections = new List<DocumentSection>
            {
                new DocumentSection
                {
                    Id = "s2",
                    Heading = new HeadingBlock { Id = "h2", Text = "Details", Level = 2 },
                    Blocks = new List<DocumentBlock>
                    {
                        new ParagraphBlock { Id = "p2", Text = "Detail text" }
                    }
                }
            }
        };

        var citations = CitationBuilder.FromSection(section, "doc.pdf");

        citations.Should().HaveCount(2);
        citations[0].HeadingPath.Should().Be("Chapter");
        citations[1].HeadingPath.Should().Be("Chapter > Details");
    }

    [Fact]
    public void FromDocument_ExtractsAllSectionCitations()
    {
        var document = new Document
        {
            Id = "doc1",
            Source = new SourceReference { FilePath = "report.pdf" },
            Sections = new List<DocumentSection>
            {
                new DocumentSection
                {
                    Id = "s1",
                    Heading = new HeadingBlock { Id = "h1", Text = "Section A", Level = 1 },
                    Blocks = new List<DocumentBlock>
                    {
                        new ParagraphBlock { Id = "p1", Text = "Content A" }
                    }
                },
                new DocumentSection
                {
                    Id = "s2",
                    Heading = new HeadingBlock { Id = "h2", Text = "Section B", Level = 1 },
                    Blocks = new List<DocumentBlock>
                    {
                        new ParagraphBlock { Id = "p2", Text = "Content B" }
                    }
                }
            }
        };

        var citations = CitationBuilder.FromDocument(document);

        citations.Should().HaveCount(2);
        citations[0].HeadingPath.Should().Be("Section A");
        citations[1].HeadingPath.Should().Be("Section B");
        citations.Should().AllSatisfy(c => c.FilePath.Should().Be("report.pdf"));
    }

    [Fact]
    public void FromDocument_WithNoSections_ReturnsEmpty()
    {
        var document = new Document
        {
            Id = "empty",
            Source = new SourceReference { FilePath = "empty.pdf" },
            Sections = new List<DocumentSection>()
        };

        var citations = CitationBuilder.FromDocument(document);

        citations.Should().BeEmpty();
    }

    [Fact]
    public void FromBlock_NullBlock_ThrowsArgumentNullException()
    {
        var act = () => CitationBuilder.FromBlock(null!, "file.pdf", null);

        act.Should().Throw<ArgumentNullException>();
    }
}
