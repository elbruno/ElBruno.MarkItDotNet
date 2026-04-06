// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.AzureSearch.Tests;

public class SearchDocumentTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var doc = new SearchDocument();

        doc.Id.Should().BeEmpty();
        doc.Content.Should().BeEmpty();
        doc.Title.Should().BeNull();
        doc.HeadingPath.Should().BeNull();
        doc.FilePath.Should().BeNull();
        doc.PageNumber.Should().BeNull();
        doc.ChunkIndex.Should().Be(0);
        doc.DocumentId.Should().BeNull();
        doc.ContentVector.Should().BeNull();
        doc.Tags.Should().BeEmpty();
        doc.Metadata.Should().BeNull();
        doc.CitationText.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithAllProperties_SetsValues()
    {
        var vector = new ReadOnlyMemory<float>(new float[] { 0.1f, 0.2f, 0.3f });
        var now = DateTimeOffset.UtcNow;

        var doc = new SearchDocument
        {
            Id = "doc-1",
            Content = "Sample content",
            Title = "Sample Title",
            HeadingPath = "Chapter 1 > Section 1",
            FilePath = "path/to/file.md",
            PageNumber = 5,
            ChunkIndex = 2,
            DocumentId = "parent-doc",
            ContentVector = vector,
            Tags = new List<string> { "tag1", "tag2" },
            Metadata = """{"key":"value"}""",
            CitationText = "file.md, Page 5",
            LastUpdated = now,
        };

        doc.Id.Should().Be("doc-1");
        doc.Content.Should().Be("Sample content");
        doc.Title.Should().Be("Sample Title");
        doc.HeadingPath.Should().Be("Chapter 1 > Section 1");
        doc.FilePath.Should().Be("path/to/file.md");
        doc.PageNumber.Should().Be(5);
        doc.ChunkIndex.Should().Be(2);
        doc.DocumentId.Should().Be("parent-doc");
        doc.ContentVector.Should().NotBeNull();
        doc.ContentVector!.Value.Length.Should().Be(3);
        doc.Tags.Should().HaveCount(2);
        doc.Metadata.Should().Contain("key");
        doc.CitationText.Should().Be("file.md, Page 5");
        doc.LastUpdated.Should().Be(now);
    }

    [Fact]
    public void Record_WithExpression_CreatesModifiedCopy()
    {
        var original = new SearchDocument
        {
            Id = "doc-1",
            Content = "Original content",
            ChunkIndex = 0,
        };

        var modified = original with { Content = "Modified content", ChunkIndex = 1 };

        modified.Id.Should().Be("doc-1");
        modified.Content.Should().Be("Modified content");
        modified.ChunkIndex.Should().Be(1);
        original.Content.Should().Be("Original content");
    }

    [Fact]
    public void Record_Equality_ComparesValueProperties()
    {
        var now = DateTimeOffset.UtcNow;
        var tags = new List<string> { "tag1" };

        var doc1 = new SearchDocument
        {
            Id = "doc-1",
            Content = "Same content",
            Tags = tags,
            LastUpdated = now,
        };

        var doc2 = new SearchDocument
        {
            Id = "doc-1",
            Content = "Same content",
            Tags = tags,
            LastUpdated = now,
        };

        doc1.Should().Be(doc2);
    }

    [Fact]
    public void LastUpdated_DefaultsToUtcNow()
    {
        var before = DateTimeOffset.UtcNow;
        var doc = new SearchDocument();
        var after = DateTimeOffset.UtcNow;

        doc.LastUpdated.Should().BeOnOrAfter(before);
        doc.LastUpdated.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void Tags_CanBeModified()
    {
        var doc = new SearchDocument();

        doc.Tags.Add("tag1");
        doc.Tags.Add("tag2");

        doc.Tags.Should().HaveCount(2);
        doc.Tags.Should().Contain("tag1");
        doc.Tags.Should().Contain("tag2");
    }

    [Fact]
    public void UploadResult_DefaultValues()
    {
        var result = new UploadResult();

        result.SuccessCount.Should().Be(0);
        result.FailureCount.Should().Be(0);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void UploadResult_WithValues()
    {
        var result = new UploadResult
        {
            SuccessCount = 10,
            FailureCount = 2,
            Errors = new List<string> { "Error 1", "Error 2" },
        };

        result.SuccessCount.Should().Be(10);
        result.FailureCount.Should().Be(2);
        result.Errors.Should().HaveCount(2);
    }
}
