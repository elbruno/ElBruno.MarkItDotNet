// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using ElBruno.MarkItDotNet.Chunking;
using ElBruno.MarkItDotNet.CoreModel;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.VectorData.Tests;

public class DefaultVectorRecordMapperTests
{
    private readonly DefaultVectorRecordMapper _mapper = new();

    [Fact]
    public void MapChunk_BasicChunk_MapsAllFields()
    {
        var chunk = new ChunkResult
        {
            Id = "doc-1-chunk-0-abc12345",
            Content = "This is test content.",
            Index = 0,
            HeadingPath = "Introduction > Overview",
            Sources =
            [
                new SourceReference { FilePath = "/docs/test.md", PageNumber = 1 },
            ],
            Metadata = new Dictionary<string, object> { ["lang"] = "en" },
        };

        var result = _mapper.MapChunk(chunk);

        result.Id.Should().Be("doc-1-chunk-0-abc12345");
        result.Content.Should().Be("This is test content.");
        result.ChunkIndex.Should().Be(0);
        result.HeadingPath.Should().Be("Introduction > Overview");
        result.PageNumber.Should().Be(1);
        result.FilePath.Should().Be("/docs/test.md");
        result.DocumentId.Should().BeNull();
        result.DocumentTitle.Should().BeNull();
        result.Embedding.Should().BeNull();
        result.Metadata.Should().ContainKey("lang").WhoseValue.Should().Be("en");
    }

    [Fact]
    public void MapChunk_WithDocument_ExtractsDocumentMetadata()
    {
        var chunk = new ChunkResult
        {
            Id = "chunk-1",
            Content = "Content",
            Index = 2,
            Sources = [],
        };

        var document = new Document
        {
            Id = "doc-42",
            Metadata = new DocumentMetadata
            {
                Title = "My Document",
                SourceFormat = "pdf",
            },
            Source = new SourceReference { FilePath = "/path/to/doc.pdf" },
        };

        var result = _mapper.MapChunk(chunk, document);

        result.DocumentId.Should().Be("doc-42");
        result.DocumentTitle.Should().Be("My Document");
        result.FilePath.Should().Be("/path/to/doc.pdf");
        result.Tags.Should().Contain("format:pdf");
    }

    [Fact]
    public void MapChunk_ChunkSourceTakesPrecedenceOverDocumentSource()
    {
        var chunk = new ChunkResult
        {
            Id = "chunk-1",
            Content = "Content",
            Index = 0,
            Sources = [new SourceReference { FilePath = "/chunk/source.md" }],
        };

        var document = new Document
        {
            Id = "doc-1",
            Source = new SourceReference { FilePath = "/doc/source.md" },
        };

        var result = _mapper.MapChunk(chunk, document);

        result.FilePath.Should().Be("/chunk/source.md");
    }

    [Fact]
    public void MapChunk_NoSources_FilePathFallsToDocumentSource()
    {
        var chunk = new ChunkResult
        {
            Id = "chunk-1",
            Content = "Content",
            Index = 0,
            Sources = [],
        };

        var document = new Document
        {
            Id = "doc-1",
            Source = new SourceReference { FilePath = "/doc/fallback.md" },
        };

        var result = _mapper.MapChunk(chunk, document);

        result.FilePath.Should().Be("/doc/fallback.md");
    }

    [Fact]
    public void MapChunk_WithHeadingPath_AddsHasHeadingTag()
    {
        var chunk = new ChunkResult
        {
            Id = "chunk-1",
            Content = "Content",
            Index = 0,
            HeadingPath = "Chapter 1",
            Sources = [],
        };

        var result = _mapper.MapChunk(chunk);

        result.Tags.Should().Contain("has-heading");
    }

    [Fact]
    public void MapChunk_WithPageNumber_AddsPageTag()
    {
        var chunk = new ChunkResult
        {
            Id = "chunk-1",
            Content = "Content",
            Index = 0,
            Sources = [new SourceReference { PageNumber = 7 }],
        };

        var result = _mapper.MapChunk(chunk);

        result.Tags.Should().Contain("page:7");
    }

    [Fact]
    public void MapChunk_MinimalChunk_ProducesValidRecord()
    {
        var chunk = new ChunkResult
        {
            Id = "minimal",
            Content = "Just text",
            Index = 0,
            Sources = [],
        };

        var result = _mapper.MapChunk(chunk);

        result.Id.Should().Be("minimal");
        result.Content.Should().Be("Just text");
        result.Tags.Should().BeEmpty();
        result.Metadata.Should().BeEmpty();
        result.PageNumber.Should().BeNull();
        result.FilePath.Should().BeNull();
    }

    [Fact]
    public void MapChunk_NullChunk_ThrowsArgumentNullException()
    {
        var act = () => _mapper.MapChunk(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void MapChunk_EmptyMetadata_CopiesAsEmptyDictionary()
    {
        var chunk = new ChunkResult
        {
            Id = "chunk-1",
            Content = "Content",
            Index = 0,
            Sources = [],
            Metadata = new Dictionary<string, object>(),
        };

        var result = _mapper.MapChunk(chunk);

        result.Metadata.Should().BeEmpty();
    }
}
