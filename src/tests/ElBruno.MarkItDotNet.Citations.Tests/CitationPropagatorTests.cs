using ElBruno.MarkItDotNet.CoreModel;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Citations.Tests;

public class CitationPropagatorTests
{
    [Fact]
    public void PropagateToChunks_WithSources_CreatesExactCitations()
    {
        var document = new Document
        {
            Id = "doc1",
            Source = new SourceReference { FilePath = "report.pdf" },
            Sections = new List<DocumentSection>()
        };

        var chunks = new List<ChunkInfo>
        {
            new ChunkInfo
            {
                ChunkId = "chunk-1",
                Content = "Some content",
                Sources = new List<SourceReference>
                {
                    new SourceReference
                    {
                        FilePath = "report.pdf",
                        PageNumber = 1,
                        HeadingPath = "Introduction"
                    }
                }
            }
        };

        var result = CitationPropagator.PropagateToChunks(document, chunks);

        result.Should().HaveCount(1);
        result[0].ChunkId.Should().Be("chunk-1");
        result[0].Citations.Should().HaveCount(1);
        result[0].Citations[0].FilePath.Should().Be("report.pdf");
        result[0].Citations[0].PageNumber.Should().Be(1);
        result[0].Citations[0].Mode.Should().Be(CitationMode.Exact);
    }

    [Fact]
    public void PropagateToChunks_WithoutSources_CreatesCoarseCitations()
    {
        var document = new Document
        {
            Id = "doc1",
            Source = new SourceReference { FilePath = "report.pdf" },
            Sections = new List<DocumentSection>()
        };

        var chunks = new List<ChunkInfo>
        {
            new ChunkInfo
            {
                ChunkId = "chunk-2",
                Content = "Content without sources",
                HeadingPath = "Chapter 1"
            }
        };

        var result = CitationPropagator.PropagateToChunks(document, chunks);

        result.Should().HaveCount(1);
        result[0].Citations.Should().HaveCount(1);
        result[0].Citations[0].FilePath.Should().Be("report.pdf");
        result[0].Citations[0].HeadingPath.Should().Be("Chapter 1");
        result[0].Citations[0].Mode.Should().Be(CitationMode.Coarse);
    }

    [Fact]
    public void PropagateToChunks_MultipleChunks_ReturnsOneSetPerChunk()
    {
        var document = new Document
        {
            Id = "doc1",
            Source = new SourceReference { FilePath = "doc.pdf" },
            Sections = new List<DocumentSection>()
        };

        var chunks = new List<ChunkInfo>
        {
            new ChunkInfo { ChunkId = "c1", Content = "A" },
            new ChunkInfo { ChunkId = "c2", Content = "B" },
            new ChunkInfo { ChunkId = "c3", Content = "C" }
        };

        var result = CitationPropagator.PropagateToChunks(document, chunks);

        result.Should().HaveCount(3);
        result.Select(r => r.ChunkId).Should().BeEquivalentTo("c1", "c2", "c3");
    }

    [Fact]
    public void PropagateToChunks_SourceWithoutFilePath_FallsBackToDocumentFilePath()
    {
        var document = new Document
        {
            Id = "doc1",
            Source = new SourceReference { FilePath = "main.pdf" },
            Sections = new List<DocumentSection>()
        };

        var chunks = new List<ChunkInfo>
        {
            new ChunkInfo
            {
                ChunkId = "chunk-fb",
                Content = "Content",
                Sources = new List<SourceReference>
                {
                    new SourceReference { PageNumber = 7 }
                }
            }
        };

        var result = CitationPropagator.PropagateToChunks(document, chunks);

        result[0].Citations[0].FilePath.Should().Be("main.pdf");
        result[0].Citations[0].PageNumber.Should().Be(7);
    }

    [Fact]
    public void PropagateToChunks_ChunkWithMultipleSources_CreatesMultipleCitations()
    {
        var document = new Document
        {
            Id = "doc1",
            Source = new SourceReference { FilePath = "doc.pdf" },
            Sections = new List<DocumentSection>()
        };

        var chunks = new List<ChunkInfo>
        {
            new ChunkInfo
            {
                ChunkId = "multi",
                Content = "Spanning content",
                Sources = new List<SourceReference>
                {
                    new SourceReference { PageNumber = 1 },
                    new SourceReference { PageNumber = 2 }
                }
            }
        };

        var result = CitationPropagator.PropagateToChunks(document, chunks);

        result[0].Citations.Should().HaveCount(2);
        result[0].Citations[0].PageNumber.Should().Be(1);
        result[0].Citations[1].PageNumber.Should().Be(2);
    }

    [Fact]
    public void PropagateToChunks_EmptyChunkList_ReturnsEmptyList()
    {
        var document = new Document
        {
            Id = "doc1",
            Source = new SourceReference { FilePath = "doc.pdf" },
            Sections = new List<DocumentSection>()
        };

        var result = CitationPropagator.PropagateToChunks(document, new List<ChunkInfo>());

        result.Should().BeEmpty();
    }

    [Fact]
    public void PropagateToChunks_NullDocument_ThrowsArgumentNullException()
    {
        var act = () => CitationPropagator.PropagateToChunks(null!, new List<ChunkInfo>());

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void PropagateToChunks_HeadingPathFallsBackToChunkHeadingPath()
    {
        var document = new Document
        {
            Id = "doc1",
            Source = new SourceReference { FilePath = "doc.pdf" },
            Sections = new List<DocumentSection>()
        };

        var chunks = new List<ChunkInfo>
        {
            new ChunkInfo
            {
                ChunkId = "hp-test",
                Content = "Content",
                HeadingPath = "Chapter 3 > Summary",
                Sources = new List<SourceReference>
                {
                    new SourceReference { PageNumber = 10 }
                }
            }
        };

        var result = CitationPropagator.PropagateToChunks(document, chunks);

        result[0].Citations[0].HeadingPath.Should().Be("Chapter 3 > Summary");
    }
}
