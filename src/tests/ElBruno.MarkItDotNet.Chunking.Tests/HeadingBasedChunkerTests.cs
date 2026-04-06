// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Chunking.Tests;

public class HeadingBasedChunkerTests
{
    private readonly HeadingBasedChunker _chunker = new();

    [Fact]
    public void Chunk_SimpleDocument_EachSectionBecomesAChunk()
    {
        var document = TestDocumentBuilder.CreateSimpleDocument();

        var chunks = _chunker.Chunk(document);

        chunks.Should().HaveCount(3);
        chunks[0].Content.Should().Contain("Introduction");
        chunks[1].Content.Should().Contain("Main Content");
        chunks[2].Content.Should().Contain("Conclusion");
    }

    [Fact]
    public void Chunk_SimpleDocument_HeadingPathIsCorrect()
    {
        var document = TestDocumentBuilder.CreateSimpleDocument();

        var chunks = _chunker.Chunk(document);

        chunks[0].HeadingPath.Should().Be("Introduction");
        chunks[1].HeadingPath.Should().Be("Main Content");
        chunks[2].HeadingPath.Should().Be("Conclusion");
    }

    [Fact]
    public void Chunk_NestedSections_HeadingPathShowsHierarchy()
    {
        var document = TestDocumentBuilder.CreateDocumentWithNestedSections();

        var chunks = _chunker.Chunk(document);

        chunks.Should().HaveCountGreaterThanOrEqualTo(4);
        chunks[0].HeadingPath.Should().Be("Chapter 1");
        chunks[1].HeadingPath.Should().Be("Chapter 1 > Section 1.1");
        chunks[2].HeadingPath.Should().Be("Chapter 1 > Section 1.1 > Subsection 1.1.1");
        chunks[3].HeadingPath.Should().Be("Chapter 1 > Section 1.2");
    }

    [Fact]
    public void Chunk_NestedSections_SubSectionsProduceSeparateChunks()
    {
        var document = TestDocumentBuilder.CreateDocumentWithNestedSections();

        var chunks = _chunker.Chunk(document);

        chunks.Should().HaveCount(4);
        chunks[2].Content.Should().Contain("Deeply nested content");
    }

    [Fact]
    public void Chunk_DocumentWithTables_TablesArePreservedWhole()
    {
        var document = TestDocumentBuilder.CreateDocumentWithTables();

        var chunks = _chunker.Chunk(document);

        chunks.Should().HaveCount(1);
        var content = chunks[0].Content;
        content.Should().Contain("Quarter | Revenue | Profit");
        content.Should().Contain("Q1 | $100M | $20M");
        content.Should().Contain("Q4 | $130M | $30M");
    }

    [Fact]
    public void Chunk_EmptySection_IsSkipped()
    {
        var document = TestDocumentBuilder.CreateDocumentWithEmptySection();

        var chunks = _chunker.Chunk(document);

        chunks.Should().HaveCount(1);
        chunks[0].Content.Should().Contain("Non-Empty");
    }

    [Fact]
    public void Chunk_SimpleDocument_ChunkIndicesAreSequential()
    {
        var document = TestDocumentBuilder.CreateSimpleDocument();

        var chunks = _chunker.Chunk(document);

        for (var i = 0; i < chunks.Count; i++)
        {
            chunks[i].Index.Should().Be(i);
        }
    }

    [Fact]
    public void Chunk_DocumentWithFigures_FiguresAreRendered()
    {
        var document = TestDocumentBuilder.CreateDocumentWithFigures();

        var chunks = _chunker.Chunk(document);

        chunks.Should().HaveCount(1);
        chunks[0].Content.Should().Contain("[Figure: System Architecture]");
    }
}
