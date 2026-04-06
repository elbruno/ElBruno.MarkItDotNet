// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Chunking.Tests;

public class ChunkIdGeneratorTests
{
    [Fact]
    public void Generate_ReturnsDeterministicId()
    {
        var id1 = ChunkIdGenerator.Generate("doc-1", 0, "Hello world");
        var id2 = ChunkIdGenerator.Generate("doc-1", 0, "Hello world");

        id1.Should().Be(id2);
    }

    [Fact]
    public void Generate_SameContentSameId()
    {
        var content = "Some repeating content for testing.";
        var id1 = ChunkIdGenerator.Generate("doc-1", 5, content);
        var id2 = ChunkIdGenerator.Generate("doc-1", 5, content);

        id1.Should().Be(id2);
    }

    [Fact]
    public void Generate_DifferentContentDifferentId()
    {
        var id1 = ChunkIdGenerator.Generate("doc-1", 0, "Content A");
        var id2 = ChunkIdGenerator.Generate("doc-1", 0, "Content B");

        id1.Should().NotBe(id2);
    }

    [Fact]
    public void Generate_DifferentIndexDifferentId()
    {
        var id1 = ChunkIdGenerator.Generate("doc-1", 0, "Same content");
        var id2 = ChunkIdGenerator.Generate("doc-1", 1, "Same content");

        id1.Should().NotBe(id2);
    }

    [Fact]
    public void Generate_DifferentDocumentIdDifferentId()
    {
        var id1 = ChunkIdGenerator.Generate("doc-1", 0, "Same content");
        var id2 = ChunkIdGenerator.Generate("doc-2", 0, "Same content");

        id1.Should().NotBe(id2);
    }

    [Fact]
    public void Generate_IncludesDocumentIdAndChunkIndex()
    {
        var id = ChunkIdGenerator.Generate("my-doc", 3, "test content");

        id.Should().StartWith("my-doc-chunk-3-");
    }

    [Fact]
    public void Generate_HashPortionIsEightCharacters()
    {
        var id = ChunkIdGenerator.Generate("doc-1", 0, "test");
        var parts = id.Split('-');

        // Format: doc-1-chunk-0-{hash8}
        // After splitting: ["doc", "1", "chunk", "0", "{hash8}"]
        var hash = parts[^1];
        hash.Should().HaveLength(8);
    }
}
