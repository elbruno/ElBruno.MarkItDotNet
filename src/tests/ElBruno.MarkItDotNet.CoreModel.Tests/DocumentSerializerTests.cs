// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.CoreModel.Tests;

public class DocumentSerializerTests
{
    [Fact]
    public void Serialize_EmptyDocument_ProducesValidJson()
    {
        var doc = new Document { Id = "test-1" };

        var json = DocumentSerializer.Serialize(doc);

        json.Should().Contain("\"id\"");
        json.Should().Contain("\"test-1\"");
    }

    [Fact]
    public void RoundTrip_EmptyDocument_PreservesId()
    {
        var original = new Document { Id = "round-trip" };

        var json = DocumentSerializer.Serialize(original);
        var restored = DocumentSerializer.Deserialize(json);

        restored.Should().NotBeNull();
        restored!.Id.Should().Be("round-trip");
    }

    [Fact]
    public void RoundTrip_DocumentWithParagraph_PreservesContent()
    {
        var original = new Document
        {
            Id = "doc-para",
            Sections =
            [
                new DocumentSection
                {
                    Id = "s1",
                    Blocks = [new ParagraphBlock { Id = "p1", Text = "Hello world" }]
                }
            ]
        };

        var json = DocumentSerializer.Serialize(original);
        var restored = DocumentSerializer.Deserialize(json);

        restored!.Sections[0].Blocks[0].Should().BeOfType<ParagraphBlock>();
        ((ParagraphBlock)restored.Sections[0].Blocks[0]).Text.Should().Be("Hello world");
    }

    [Fact]
    public void RoundTrip_DocumentWithHeading_PreservesLevel()
    {
        var original = new Document
        {
            Id = "doc-heading",
            Sections =
            [
                new DocumentSection
                {
                    Id = "s1",
                    Heading = new HeadingBlock { Id = "h1", Text = "Title", Level = 2 }
                }
            ]
        };

        var json = DocumentSerializer.Serialize(original);
        var restored = DocumentSerializer.Deserialize(json);

        restored!.Sections[0].Heading.Should().NotBeNull();
        restored.Sections[0].Heading!.Level.Should().Be(2);
        restored.Sections[0].Heading!.Text.Should().Be("Title");
    }

    [Fact]
    public void RoundTrip_DocumentWithTable_PreservesStructure()
    {
        var original = new Document
        {
            Id = "doc-table",
            Sections =
            [
                new DocumentSection
                {
                    Id = "s1",
                    Blocks =
                    [
                        new TableBlock
                        {
                            Id = "t1",
                            Headers = ["Name", "Age"],
                            Rows = [["Alice", "30"]]
                        }
                    ]
                }
            ]
        };

        var json = DocumentSerializer.Serialize(original);
        var restored = DocumentSerializer.Deserialize(json);

        var table = restored!.Sections[0].Blocks[0].Should().BeOfType<TableBlock>().Subject;
        table.Headers.Should().HaveCount(2);
        table.Rows.Should().HaveCount(1);
    }

    [Fact]
    public void Serialize_UseCamelCaseNaming()
    {
        var doc = new Document
        {
            Metadata = new DocumentMetadata { SourceFormat = "pdf" }
        };

        var json = DocumentSerializer.Serialize(doc);

        json.Should().Contain("\"sourceFormat\"");
        json.Should().NotContain("\"SourceFormat\"");
    }

    [Fact]
    public void RoundTrip_DocumentWithFigure_PreservesProperties()
    {
        var original = new Document
        {
            Id = "doc-fig",
            Sections =
            [
                new DocumentSection
                {
                    Id = "s1",
                    Blocks =
                    [
                        new FigureBlock
                        {
                            Id = "f1",
                            AltText = "Logo",
                            Caption = "Figure 1",
                            ImagePath = "img/logo.png"
                        }
                    ]
                }
            ]
        };

        var json = DocumentSerializer.Serialize(original);
        var restored = DocumentSerializer.Deserialize(json);

        var figure = restored!.Sections[0].Blocks[0].Should().BeOfType<FigureBlock>().Subject;
        figure.AltText.Should().Be("Logo");
        figure.ImagePath.Should().Be("img/logo.png");
    }

    [Fact]
    public void RoundTrip_DocumentWithList_PreservesItems()
    {
        var original = new Document
        {
            Id = "doc-list",
            Sections =
            [
                new DocumentSection
                {
                    Id = "s1",
                    Blocks =
                    [
                        new ListBlock
                        {
                            Id = "l1",
                            IsOrdered = true,
                            Items =
                            [
                                new ListItemBlock { Id = "li1", Text = "First" },
                                new ListItemBlock { Id = "li2", Text = "Second" }
                            ]
                        }
                    ]
                }
            ]
        };

        var json = DocumentSerializer.Serialize(original);
        var restored = DocumentSerializer.Deserialize(json);

        var list = restored!.Sections[0].Blocks[0].Should().BeOfType<ListBlock>().Subject;
        list.IsOrdered.Should().BeTrue();
        list.Items.Should().HaveCount(2);
    }

    [Fact]
    public void Serialize_PolymorphicType_IncludesDiscriminator()
    {
        var doc = new Document
        {
            Id = "poly",
            Sections =
            [
                new DocumentSection
                {
                    Id = "s1",
                    Blocks = [new ParagraphBlock { Text = "test" }]
                }
            ]
        };

        var json = DocumentSerializer.Serialize(doc);

        json.Should().Contain("\"$type\"");
        json.Should().Contain("\"paragraph\"");
    }

    [Fact]
    public void RoundTrip_Metadata_PreservesAllFields()
    {
        var now = DateTimeOffset.UtcNow;
        var original = new Document
        {
            Id = "meta",
            Metadata = new DocumentMetadata
            {
                Title = "Test Doc",
                Author = "Bruno",
                SourceFormat = "docx",
                CreatedAt = now,
                PageCount = 5,
                WordCount = 1000
            }
        };

        var json = DocumentSerializer.Serialize(original);
        var restored = DocumentSerializer.Deserialize(json);

        restored!.Metadata.Title.Should().Be("Test Doc");
        restored.Metadata.Author.Should().Be("Bruno");
        restored.Metadata.PageCount.Should().Be(5);
    }
}
