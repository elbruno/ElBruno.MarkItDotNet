// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.CoreModel.Tests;

public class DocumentTests
{
    [Fact]
    public void DefaultDocument_HasEmptyId()
    {
        var doc = new Document();

        doc.Id.Should().BeEmpty();
    }

    [Fact]
    public void DefaultDocument_HasEmptySections()
    {
        var doc = new Document();

        doc.Sections.Should().BeEmpty();
    }

    [Fact]
    public void DefaultDocument_HasDefaultMetadata()
    {
        var doc = new Document();

        doc.Metadata.Should().NotBeNull();
        doc.Metadata.Title.Should().BeNull();
    }

    [Fact]
    public void DefaultDocument_HasNullSource()
    {
        var doc = new Document();

        doc.Source.Should().BeNull();
    }

    [Fact]
    public void Document_CanBeCreatedWithProperties()
    {
        var doc = new Document
        {
            Id = "doc-1",
            Metadata = new DocumentMetadata { Title = "Test" },
            Source = new SourceReference { FilePath = "/test.md" }
        };

        doc.Id.Should().Be("doc-1");
        doc.Metadata.Title.Should().Be("Test");
        doc.Source!.FilePath.Should().Be("/test.md");
    }

    [Fact]
    public void Document_WithExpression_CreatesModifiedCopy()
    {
        var original = new Document { Id = "orig" };
        var modified = original with { Id = "copy" };

        original.Id.Should().Be("orig");
        modified.Id.Should().Be("copy");
    }

    [Fact]
    public void Document_WithSections_PreservesSectionOrder()
    {
        var doc = new Document
        {
            Sections =
            [
                new DocumentSection { Id = "s1" },
                new DocumentSection { Id = "s2" },
                new DocumentSection { Id = "s3" }
            ]
        };

        doc.Sections.Should().HaveCount(3);
        doc.Sections[0].Id.Should().Be("s1");
        doc.Sections[2].Id.Should().Be("s3");
    }

    [Fact]
    public void Document_Equality_ComparesIdAndMetadata()
    {
        var sections = new List<DocumentSection>();
        var metadata = new DocumentMetadata();
        var doc1 = new Document { Id = "same", Sections = sections, Metadata = metadata };
        var doc2 = new Document { Id = "same", Sections = sections, Metadata = metadata };

        doc1.Should().Be(doc2);
    }

    [Fact]
    public void DocumentMetadata_CustomDictionary_CanStoreValues()
    {
        var meta = new DocumentMetadata
        {
            Title = "Test",
            Custom = new Dictionary<string, object> { ["key1"] = "value1" }
        };

        meta.Custom.Should().ContainKey("key1");
        meta.Custom["key1"].Should().Be("value1");
    }

    [Fact]
    public void DocumentSection_SupportsNestedSubSections()
    {
        var section = new DocumentSection
        {
            Id = "parent",
            SubSections =
            [
                new DocumentSection
                {
                    Id = "child",
                    Heading = new HeadingBlock { Text = "Sub", Level = 2 }
                }
            ]
        };

        section.SubSections.Should().HaveCount(1);
        section.SubSections[0].Heading!.Text.Should().Be("Sub");
    }
}
