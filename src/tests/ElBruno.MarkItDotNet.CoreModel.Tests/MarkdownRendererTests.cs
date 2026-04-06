// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.CoreModel.Tests;

public class MarkdownRendererTests
{
    [Fact]
    public void Render_DocumentWithTitle_RendersH1()
    {
        var doc = new Document { Metadata = new DocumentMetadata { Title = "My Doc" } };

        var md = MarkdownRenderer.Render(doc);

        md.Should().StartWith("# My Doc");
    }

    [Fact]
    public void Render_Heading_UsesCorrectLevel()
    {
        var doc = new Document
        {
            Sections =
            [
                new DocumentSection
                {
                    Heading = new HeadingBlock { Text = "Section One", Level = 2 }
                }
            ]
        };

        var md = MarkdownRenderer.Render(doc);

        md.Should().Contain("## Section One");
    }

    [Fact]
    public void Render_Paragraph_OutputsTextWithBlankLine()
    {
        var doc = new Document
        {
            Sections =
            [
                new DocumentSection
                {
                    Blocks = [new ParagraphBlock { Text = "Some text here." }]
                }
            ]
        };

        var md = MarkdownRenderer.Render(doc);

        md.Should().Contain("Some text here.");
    }

    [Fact]
    public void Render_Table_ProducesMarkdownTable()
    {
        var doc = new Document
        {
            Sections =
            [
                new DocumentSection
                {
                    Blocks =
                    [
                        new TableBlock
                        {
                            Headers = ["Name", "Age"],
                            Rows = [["Alice", "30"], ["Bob", "25"]]
                        }
                    ]
                }
            ]
        };

        var md = MarkdownRenderer.Render(doc);

        md.Should().Contain("| Name | Age |");
        md.Should().Contain("| --- | --- |");
        md.Should().Contain("| Alice | 30 |");
        md.Should().Contain("| Bob | 25 |");
    }

    [Fact]
    public void Render_Figure_ProducesImageSyntax()
    {
        var doc = new Document
        {
            Sections =
            [
                new DocumentSection
                {
                    Blocks =
                    [
                        new FigureBlock
                        {
                            AltText = "A cat",
                            ImagePath = "images/cat.png",
                            Caption = "Figure 1: A cat"
                        }
                    ]
                }
            ]
        };

        var md = MarkdownRenderer.Render(doc);

        md.Should().Contain("![A cat](images/cat.png)");
        md.Should().Contain("*Figure 1: A cat*");
    }

    [Fact]
    public void Render_UnorderedList_UsesDashMarker()
    {
        var doc = new Document
        {
            Sections =
            [
                new DocumentSection
                {
                    Blocks =
                    [
                        new ListBlock
                        {
                            IsOrdered = false,
                            Items =
                            [
                                new ListItemBlock { Text = "Apple" },
                                new ListItemBlock { Text = "Banana" }
                            ]
                        }
                    ]
                }
            ]
        };

        var md = MarkdownRenderer.Render(doc);

        md.Should().Contain("- Apple");
        md.Should().Contain("- Banana");
    }

    [Fact]
    public void Render_OrderedList_UsesNumberedMarker()
    {
        var doc = new Document
        {
            Sections =
            [
                new DocumentSection
                {
                    Blocks =
                    [
                        new ListBlock
                        {
                            IsOrdered = true,
                            Items =
                            [
                                new ListItemBlock { Text = "First" },
                                new ListItemBlock { Text = "Second" }
                            ]
                        }
                    ]
                }
            ]
        };

        var md = MarkdownRenderer.Render(doc);

        md.Should().Contain("1. First");
        md.Should().Contain("2. Second");
    }

    [Fact]
    public void Render_NestedListItems_IndentsSubItems()
    {
        var doc = new Document
        {
            Sections =
            [
                new DocumentSection
                {
                    Blocks =
                    [
                        new ListBlock
                        {
                            IsOrdered = false,
                            Items =
                            [
                                new ListItemBlock
                                {
                                    Text = "Parent",
                                    SubItems =
                                    [
                                        new ListItemBlock { Text = "Child" }
                                    ]
                                }
                            ]
                        }
                    ]
                }
            ]
        };

        var md = MarkdownRenderer.Render(doc);

        md.Should().Contain("- Parent");
        md.Should().Contain("  - Child");
    }

    [Fact]
    public void Render_NestedSections_RendersRecursively()
    {
        var doc = new Document
        {
            Sections =
            [
                new DocumentSection
                {
                    Heading = new HeadingBlock { Text = "Top", Level = 1 },
                    SubSections =
                    [
                        new DocumentSection
                        {
                            Heading = new HeadingBlock { Text = "Nested", Level = 2 },
                            Blocks = [new ParagraphBlock { Text = "Content here." }]
                        }
                    ]
                }
            ]
        };

        var md = MarkdownRenderer.Render(doc);

        md.Should().Contain("# Top");
        md.Should().Contain("## Nested");
        md.Should().Contain("Content here.");
    }

    [Fact]
    public void Render_EmptyTable_ProducesNothing()
    {
        var doc = new Document
        {
            Sections =
            [
                new DocumentSection
                {
                    Blocks = [new TableBlock()]
                }
            ]
        };

        var md = MarkdownRenderer.Render(doc);

        md.Trim().Should().BeEmpty();
    }
}
