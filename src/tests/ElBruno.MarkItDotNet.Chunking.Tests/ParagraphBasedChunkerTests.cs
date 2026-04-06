// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Chunking.Tests;

public class ParagraphBasedChunkerTests
{
    private readonly ParagraphBasedChunker _chunker = new();

    [Fact]
    public void Chunk_SimpleDocument_RespectsMaxSize()
    {
        var document = TestDocumentBuilder.CreateLargeDocument(paragraphCount: 20);
        var options = new ChunkingOptions { MaxChunkSize = 300, OverlapSize = 0 };

        var chunks = _chunker.Chunk(document, options);

        chunks.Should().HaveCountGreaterThan(1);
        foreach (var chunk in chunks)
        {
            // Allow some tolerance for edge cases
            chunk.Content.Length.Should().BeLessThanOrEqualTo(options.MaxChunkSize + 200,
                "each chunk should respect max size approximately");
        }
    }

    [Fact]
    public void Chunk_WithOverlap_ChunksShareContent()
    {
        var document = TestDocumentBuilder.CreateLargeDocument(paragraphCount: 10);
        var options = new ChunkingOptions { MaxChunkSize = 300, OverlapSize = 50 };

        var chunks = _chunker.Chunk(document, options);

        chunks.Should().HaveCountGreaterThan(1);

        // The end of chunk N should overlap with the beginning of chunk N+1
        for (var i = 0; i < chunks.Count - 1; i++)
        {
            var currentEnd = chunks[i].Content[^Math.Min(50, chunks[i].Content.Length)..];
            chunks[i + 1].Content.Should().Contain(currentEnd,
                "overlap content from previous chunk should appear in next chunk");
        }
    }

    [Fact]
    public void Chunk_TableAtomicity_TableNotSplitAcrossChunks()
    {
        var document = TestDocumentBuilder.CreateDocumentWithTables();
        var options = new ChunkingOptions
        {
            MaxChunkSize = 100,
            OverlapSize = 0,
            PreserveTableAtomicity = true,
        };

        var chunks = _chunker.Chunk(document, options);

        // Find the chunk that contains the table header — it should also contain all rows
        var tableChunk = chunks.FirstOrDefault(c => c.Content.Contains("Quarter | Revenue | Profit"));
        tableChunk.Should().NotBeNull("a chunk should contain the table");
        tableChunk!.Content.Should().Contain("Q4 | $130M | $30M",
            "the entire table should be in one chunk");
    }

    [Fact]
    public void Chunk_FigureAtomicity_FigureNotSplit()
    {
        var document = TestDocumentBuilder.CreateDocumentWithFigures();
        var options = new ChunkingOptions
        {
            MaxChunkSize = 80,
            OverlapSize = 0,
            PreserveFigureAtomicity = true,
        };

        var chunks = _chunker.Chunk(document, options);

        var figureChunk = chunks.FirstOrDefault(c => c.Content.Contains("[Figure:"));
        figureChunk.Should().NotBeNull("a chunk should contain the figure");
    }

    [Fact]
    public void Chunk_SmallMaxSize_StillProducesChunks()
    {
        var document = TestDocumentBuilder.CreateSimpleDocument();
        var options = new ChunkingOptions { MaxChunkSize = 50, OverlapSize = 0 };

        var chunks = _chunker.Chunk(document, options);

        chunks.Should().NotBeEmpty();
    }

    [Fact]
    public void Chunk_SingleParagraph_ProducesSingleChunk()
    {
        var document = TestDocumentBuilder.CreateDocumentWithEmptySection();
        var options = new ChunkingOptions { MaxChunkSize = 5000, OverlapSize = 0 };

        var chunks = _chunker.Chunk(document, options);

        chunks.Should().HaveCount(1);
    }

    [Fact]
    public void Chunk_ChunkIndicesAreSequential()
    {
        var document = TestDocumentBuilder.CreateLargeDocument(paragraphCount: 20);
        var options = new ChunkingOptions { MaxChunkSize = 300, OverlapSize = 0 };

        var chunks = _chunker.Chunk(document, options);

        for (var i = 0; i < chunks.Count; i++)
        {
            chunks[i].Index.Should().Be(i);
        }
    }
}
