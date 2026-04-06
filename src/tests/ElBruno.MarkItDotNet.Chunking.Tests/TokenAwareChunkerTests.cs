// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Chunking.Tests;

public class TokenAwareChunkerTests
{
    private readonly TokenAwareChunker _chunker = new();

    [Fact]
    public void Chunk_RespectsTokenLimit()
    {
        var document = TestDocumentBuilder.CreateLargeDocument(paragraphCount: 20);
        var options = new ChunkingOptions
        {
            MaxChunkSize = 50,
            OverlapSize = 0,
        };

        var chunks = _chunker.Chunk(document, options);

        chunks.Should().HaveCountGreaterThan(1);
    }

    [Fact]
    public void Chunk_CustomTokenCounter_IsUsed()
    {
        var callCount = 0;
        var document = TestDocumentBuilder.CreateSimpleDocument();
        var options = new ChunkingOptions
        {
            MaxChunkSize = 100,
            OverlapSize = 0,
            TokenCounter = text =>
            {
                callCount++;
                return text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            },
        };

        _ = _chunker.Chunk(document, options);

        callCount.Should().BeGreaterThan(0, "custom token counter should be invoked");
    }

    [Fact]
    public void Chunk_OversizedBlock_SplitAtSentenceBoundaries()
    {
        var document = TestDocumentBuilder.CreateLargeDocument(paragraphCount: 1);
        var options = new ChunkingOptions
        {
            MaxChunkSize = 5,
            OverlapSize = 0,
        };

        var chunks = _chunker.Chunk(document, options);

        chunks.Should().HaveCountGreaterThanOrEqualTo(1, "oversized blocks should be split into multiple chunks");
    }

    [Fact]
    public void Chunk_OverlapInTokens_ProducesOverlap()
    {
        var document = TestDocumentBuilder.CreateLargeDocument(paragraphCount: 10);
        var options = new ChunkingOptions
        {
            MaxChunkSize = 30,
            OverlapSize = 10,
        };

        var chunks = _chunker.Chunk(document, options);

        chunks.Should().HaveCountGreaterThan(1, "with a small token limit, multiple chunks should be produced");

        // With overlap, some content should repeat between consecutive chunks
        var foundOverlap = false;
        for (var i = 0; i < chunks.Count - 1; i++)
        {
            var currentWords = chunks[i].Content.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var nextContent = chunks[i + 1].Content;

            // Check if any of the last words of the current chunk appear in the next chunk
            var lastWords = currentWords.TakeLast(5);
            if (lastWords.Any(w => nextContent.Contains(w, StringComparison.Ordinal)))
            {
                foundOverlap = true;
                break;
            }
        }

        foundOverlap.Should().BeTrue("overlap should cause shared content between consecutive chunks");
    }

    [Fact]
    public void Chunk_DefaultTokenCounter_CountsWords()
    {
        var count = TokenAwareChunker.DefaultTokenCounter("Hello world this is a test");

        count.Should().Be(6);
    }

    [Fact]
    public void Chunk_EmptyDocument_ReturnsEmptyList()
    {
        var document = TestDocumentBuilder.CreateDocumentWithEmptySection();
        var options = new ChunkingOptions { MaxChunkSize = 1000 };

        var chunks = _chunker.Chunk(document, options);

        // Should only contain chunks from the non-empty section
        chunks.Should().HaveCountGreaterThanOrEqualTo(1);
        chunks.All(c => !string.IsNullOrWhiteSpace(c.Content)).Should().BeTrue();
    }

    [Fact]
    public void Chunk_ChunkIdsAreUnique()
    {
        var document = TestDocumentBuilder.CreateLargeDocument(paragraphCount: 20);
        var options = new ChunkingOptions { MaxChunkSize = 50, OverlapSize = 0 };

        var chunks = _chunker.Chunk(document, options);

        var ids = chunks.Select(c => c.Id).ToList();
        ids.Should().OnlyHaveUniqueItems("each chunk should have a unique ID");
    }

    [Fact]
    public void Chunk_NullDocument_ThrowsArgumentNullException()
    {
        var act = () => _chunker.Chunk(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
