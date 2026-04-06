// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using ElBruno.MarkItDotNet.Chunking;
using ElBruno.MarkItDotNet.Citations;
using ElBruno.MarkItDotNet.CoreModel;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.AzureSearch.Tests;

public class DefaultSearchDocumentMapperTests
{
    private readonly DefaultSearchDocumentMapper _mapper = new();

    [Fact]
    public void Map_BasicChunk_MapsIdAndContent()
    {
        var chunk = new ChunkResult
        {
            Id = "chunk-1",
            Content = "Hello world",
            Index = 0,
        };

        var result = _mapper.Map(chunk);

        result.Id.Should().Be("chunk-1");
        result.Content.Should().Be("Hello world");
        result.ChunkIndex.Should().Be(0);
    }

    [Fact]
    public void Map_WithDocument_MapsTitleAndDocumentId()
    {
        var chunk = new ChunkResult { Id = "c1", Content = "text", Index = 1 };
        var document = new Document
        {
            Id = "doc-42",
            Metadata = new DocumentMetadata { Title = "My Document" },
        };

        var result = _mapper.Map(chunk, document);

        result.Title.Should().Be("My Document");
        result.DocumentId.Should().Be("doc-42");
    }

    [Fact]
    public void Map_WithCitation_MapsCitationText()
    {
        var chunk = new ChunkResult { Id = "c1", Content = "text", Index = 0 };
        var citation = new CitationReference
        {
            FilePath = "docs/readme.md",
            PageNumber = 3,
            HeadingPath = "Introduction",
        };

        var result = _mapper.Map(chunk, citation: citation);

        result.CitationText.Should().NotBeNullOrEmpty();
        result.FilePath.Should().Be("docs/readme.md");
        result.PageNumber.Should().Be(3);
    }

    [Fact]
    public void Map_WithCitation_MapsHeadingPathFromCitation()
    {
        var chunk = new ChunkResult { Id = "c1", Content = "text", Index = 0 };
        var citation = new CitationReference
        {
            HeadingPath = "Chapter 1 > Section 1.1",
        };

        var result = _mapper.Map(chunk, citation: citation);

        result.HeadingPath.Should().Be("Chapter 1 > Section 1.1");
    }

    [Fact]
    public void Map_ChunkHeadingPathTakesPrecedence()
    {
        var chunk = new ChunkResult
        {
            Id = "c1",
            Content = "text",
            Index = 0,
            HeadingPath = "Chunk Heading",
        };
        var citation = new CitationReference
        {
            HeadingPath = "Citation Heading",
        };

        var result = _mapper.Map(chunk, citation: citation);

        result.HeadingPath.Should().Be("Chunk Heading");
    }

    [Fact]
    public void Map_WithMetadata_SerializesMetadataAsJson()
    {
        var chunk = new ChunkResult
        {
            Id = "c1",
            Content = "text",
            Index = 0,
            Metadata = new Dictionary<string, object> { ["key"] = "value" },
        };

        var result = _mapper.Map(chunk);

        result.Metadata.Should().Contain("key");
        result.Metadata.Should().Contain("value");
    }

    [Fact]
    public void Map_WithoutMetadata_MetadataIsNull()
    {
        var chunk = new ChunkResult { Id = "c1", Content = "text", Index = 0 };

        var result = _mapper.Map(chunk);

        result.Metadata.Should().BeNull();
    }

    [Fact]
    public void Map_WithDocumentSource_MapsFilePath()
    {
        var chunk = new ChunkResult { Id = "c1", Content = "text", Index = 0 };
        var document = new Document
        {
            Id = "d1",
            Source = new SourceReference { FilePath = "path/to/file.md" },
        };

        var result = _mapper.Map(chunk, document);

        result.FilePath.Should().Be("path/to/file.md");
    }

    [Fact]
    public void Map_CitationFilePathTakesPrecedenceOverDocument()
    {
        var chunk = new ChunkResult { Id = "c1", Content = "text", Index = 0 };
        var document = new Document
        {
            Id = "d1",
            Source = new SourceReference { FilePath = "doc-path.md" },
        };
        var citation = new CitationReference { FilePath = "citation-path.md" };

        var result = _mapper.Map(chunk, document, citation);

        result.FilePath.Should().Be("citation-path.md");
    }

    [Fact]
    public void Map_SetsLastUpdated()
    {
        var before = DateTimeOffset.UtcNow;
        var chunk = new ChunkResult { Id = "c1", Content = "text", Index = 0 };

        var result = _mapper.Map(chunk);

        result.LastUpdated.Should().BeOnOrAfter(before);
        result.LastUpdated.Should().BeOnOrBefore(DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Map_NullChunk_Throws()
    {
        var act = () => _mapper.Map(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Map_WithoutCitation_CitationTextIsNull()
    {
        var chunk = new ChunkResult { Id = "c1", Content = "text", Index = 0 };

        var result = _mapper.Map(chunk);

        result.CitationText.Should().BeNull();
    }

    [Fact]
    public void Map_VectorFieldDefaultsToNull()
    {
        var chunk = new ChunkResult { Id = "c1", Content = "text", Index = 0 };

        var result = _mapper.Map(chunk);

        result.ContentVector.Should().BeNull();
    }

    [Fact]
    public void Map_TagsDefaultsToEmpty()
    {
        var chunk = new ChunkResult { Id = "c1", Content = "text", Index = 0 };

        var result = _mapper.Map(chunk);

        result.Tags.Should().BeEmpty();
    }
}
