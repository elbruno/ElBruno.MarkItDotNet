// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.VectorData.Tests;

public class VectorRecordTests
{
    [Fact]
    public void DefaultValues_AreSetCorrectly()
    {
        var record = new VectorRecord();

        record.Id.Should().BeEmpty();
        record.Content.Should().BeEmpty();
        record.Embedding.Should().BeNull();
        record.DocumentId.Should().BeNull();
        record.DocumentTitle.Should().BeNull();
        record.HeadingPath.Should().BeNull();
        record.PageNumber.Should().BeNull();
        record.FilePath.Should().BeNull();
        record.ChunkIndex.Should().Be(0);
        record.Tags.Should().BeEmpty();
        record.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void AllProperties_CanBeSet()
    {
        var embedding = new ReadOnlyMemory<float>([1.0f, 2.0f, 3.0f]);
        var tags = new List<string> { "tag1", "tag2" };
        var metadata = new Dictionary<string, object> { ["key"] = "value" };

        var record = new VectorRecord
        {
            Id = "chunk-1",
            Content = "Hello world",
            Embedding = embedding,
            DocumentId = "doc-1",
            DocumentTitle = "Test Document",
            HeadingPath = "Chapter 1 > Section 1",
            PageNumber = 5,
            FilePath = "/docs/test.md",
            ChunkIndex = 3,
            Tags = tags,
            Metadata = metadata,
        };

        record.Id.Should().Be("chunk-1");
        record.Content.Should().Be("Hello world");
        record.Embedding!.Value.ToArray().Should().BeEquivalentTo([1.0f, 2.0f, 3.0f]);
        record.DocumentId.Should().Be("doc-1");
        record.DocumentTitle.Should().Be("Test Document");
        record.HeadingPath.Should().Be("Chapter 1 > Section 1");
        record.PageNumber.Should().Be(5);
        record.FilePath.Should().Be("/docs/test.md");
        record.ChunkIndex.Should().Be(3);
        record.Tags.Should().BeEquivalentTo(["tag1", "tag2"]);
        record.Metadata.Should().ContainKey("key").WhoseValue.Should().Be("value");
    }

    [Fact]
    public void Record_SupportsWithExpression()
    {
        var original = new VectorRecord
        {
            Id = "chunk-1",
            Content = "Original content",
            ChunkIndex = 0,
        };

        var modified = original with { Content = "Modified content", ChunkIndex = 1 };

        modified.Id.Should().Be("chunk-1");
        modified.Content.Should().Be("Modified content");
        modified.ChunkIndex.Should().Be(1);
    }

    [Fact]
    public void Record_ValueEquality_Works()
    {
        var tags = new List<string> { "tag1" };
        var metadata = new Dictionary<string, object> { ["key"] = "value" };

        var record1 = new VectorRecord { Id = "chunk-1", Content = "Hello", ChunkIndex = 0, Tags = tags, Metadata = metadata };
        var record2 = new VectorRecord { Id = "chunk-1", Content = "Hello", ChunkIndex = 0, Tags = tags, Metadata = metadata };

        record1.Should().Be(record2);
    }
}
