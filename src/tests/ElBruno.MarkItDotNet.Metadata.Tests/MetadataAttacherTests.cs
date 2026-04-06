// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using ElBruno.MarkItDotNet.Chunking;
using ElBruno.MarkItDotNet.CoreModel;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Metadata.Tests;

public class MetadataAttacherTests
{
    #region AttachToDocument

    [Fact]
    public void AttachToDocument_SetsTitle()
    {
        var doc = new Document { Metadata = new DocumentMetadata() };
        var result = new MetadataResult { Title = "My Title" };

        var enriched = MetadataAttacher.AttachToDocument(doc, result);

        enriched.Metadata.Title.Should().Be("My Title");
    }

    [Fact]
    public void AttachToDocument_SetsAuthor()
    {
        var doc = new Document { Metadata = new DocumentMetadata() };
        var result = new MetadataResult { Author = "Bruno" };

        var enriched = MetadataAttacher.AttachToDocument(doc, result);

        enriched.Metadata.Author.Should().Be("Bruno");
    }

    [Fact]
    public void AttachToDocument_SetsWordCount()
    {
        var doc = new Document { Metadata = new DocumentMetadata() };
        var result = new MetadataResult { WordCount = 150 };

        var enriched = MetadataAttacher.AttachToDocument(doc, result);

        enriched.Metadata.WordCount.Should().Be(150);
    }

    [Fact]
    public void AttachToDocument_AddsLanguageToCustom()
    {
        var doc = new Document { Metadata = new DocumentMetadata() };
        var result = new MetadataResult { Language = "en" };

        var enriched = MetadataAttacher.AttachToDocument(doc, result);

        enriched.Metadata.Custom.Should().ContainKey("Language");
        enriched.Metadata.Custom["Language"].Should().Be("en");
    }

    [Fact]
    public void AttachToDocument_AddsDocumentTypeToCustom()
    {
        var doc = new Document { Metadata = new DocumentMetadata() };
        var result = new MetadataResult { DocumentType = DocumentType.Report };

        var enriched = MetadataAttacher.AttachToDocument(doc, result);

        enriched.Metadata.Custom.Should().ContainKey("DocumentType");
        enriched.Metadata.Custom["DocumentType"].Should().Be("Report");
    }

    [Fact]
    public void AttachToDocument_AddsTagsToCustom()
    {
        var doc = new Document { Metadata = new DocumentMetadata() };
        var result = new MetadataResult { Tags = ["tag1", "tag2"] };

        var enriched = MetadataAttacher.AttachToDocument(doc, result);

        enriched.Metadata.Custom.Should().ContainKey("Tags");
    }

    [Fact]
    public void AttachToDocument_PreservesExistingCustomMetadata()
    {
        var doc = new Document
        {
            Metadata = new DocumentMetadata
            {
                Custom = new Dictionary<string, object> { ["existing"] = "value" },
            },
        };
        var result = new MetadataResult { Language = "en" };

        var enriched = MetadataAttacher.AttachToDocument(doc, result);

        enriched.Metadata.Custom.Should().ContainKey("existing");
        enriched.Metadata.Custom["existing"].Should().Be("value");
    }

    [Fact]
    public void AttachToDocument_ThrowsForNullDocument()
    {
        var result = new MetadataResult();
        var action = () => MetadataAttacher.AttachToDocument(null!, result);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AttachToDocument_ThrowsForNullResult()
    {
        var doc = new Document();
        var action = () => MetadataAttacher.AttachToDocument(doc, null!);

        action.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region AttachToChunks

    [Fact]
    public void AttachToChunks_AddsDocumentTitleToChunks()
    {
        var chunks = new List<ChunkResult>
        {
            new() { Id = "c1", Content = "Hello" },
            new() { Id = "c2", Content = "World" },
        };
        var result = new MetadataResult { Title = "My Doc" };

        var enriched = MetadataAttacher.AttachToChunks(chunks, result);

        enriched.Should().HaveCount(2);
        enriched[0].Metadata.Should().ContainKey("DocumentTitle");
        enriched[0].Metadata["DocumentTitle"].Should().Be("My Doc");
        enriched[1].Metadata.Should().ContainKey("DocumentTitle");
    }

    [Fact]
    public void AttachToChunks_AddsAuthorAndLanguage()
    {
        var chunks = new List<ChunkResult> { new() { Id = "c1", Content = "text" } };
        var result = new MetadataResult { Author = "Bruno", Language = "en" };

        var enriched = MetadataAttacher.AttachToChunks(chunks, result);

        enriched[0].Metadata.Should().ContainKey("DocumentAuthor");
        enriched[0].Metadata["DocumentAuthor"].Should().Be("Bruno");
        enriched[0].Metadata.Should().ContainKey("DocumentLanguage");
        enriched[0].Metadata["DocumentLanguage"].Should().Be("en");
    }

    [Fact]
    public void AttachToChunks_AddsDocumentType()
    {
        var chunks = new List<ChunkResult> { new() { Id = "c1", Content = "text" } };
        var result = new MetadataResult { DocumentType = DocumentType.Article };

        var enriched = MetadataAttacher.AttachToChunks(chunks, result);

        enriched[0].Metadata.Should().ContainKey("DocumentType");
        enriched[0].Metadata["DocumentType"].Should().Be("Article");
    }

    [Fact]
    public void AttachToChunks_PreservesExistingChunkMetadata()
    {
        var chunks = new List<ChunkResult>
        {
            new()
            {
                Id = "c1",
                Content = "text",
                Metadata = new Dictionary<string, object> { ["existing"] = "value" },
            },
        };
        var result = new MetadataResult { Title = "Doc" };

        var enriched = MetadataAttacher.AttachToChunks(chunks, result);

        enriched[0].Metadata.Should().ContainKey("existing");
        enriched[0].Metadata["existing"].Should().Be("value");
        enriched[0].Metadata.Should().ContainKey("DocumentTitle");
    }

    [Fact]
    public void AttachToChunks_ReturnsEmptyListForEmptyInput()
    {
        var result = new MetadataResult { Title = "Doc" };

        var enriched = MetadataAttacher.AttachToChunks([], result);

        enriched.Should().BeEmpty();
    }

    [Fact]
    public void AttachToChunks_ThrowsForNullChunks()
    {
        var result = new MetadataResult();
        var action = () => MetadataAttacher.AttachToChunks(null!, result);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AttachToChunks_ThrowsForNullResult()
    {
        var action = () => MetadataAttacher.AttachToChunks([], null!);

        action.Should().Throw<ArgumentNullException>();
    }

    #endregion
}
