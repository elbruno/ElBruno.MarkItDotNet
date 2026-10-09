// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using FluentAssertions;
using Microsoft.Extensions.DataIngestion;
using Xunit;
using Core = ElBruno.MarkItDotNet.CoreModel;

namespace ElBruno.MarkItDotNet.DataIngestion.Tests;

public sealed class DocumentDataIngestionExtensionsTests
{
    [Fact]
    public void ToIngestionDocument_PreservesStructureAndSourceMetadata()
    {
        var document = new Core.Document
        {
            Id = "doc-123",
            Metadata = new Core.DocumentMetadata
            {
                Title = "API guide",
                Author = "Bruno",
                SourceFormat = "markdown",
                WordCount = 12,
                Custom = new Dictionary<string, object> { ["category"] = "guide" }
            },
            Source = new Core.SourceReference { FilePath = "docs/api.md" },
            Sections =
            [
                new Core.DocumentSection
                {
                    Id = "section-1",
                    Heading = new Core.HeadingBlock
                    {
                        Id = "heading-1",
                        Text = "Introduction",
                        Level = 2,
                        OriginalMarkdown = "## Introduction"
                    },
                    Blocks =
                    [
                        new Core.ParagraphBlock
                        {
                            Id = "paragraph-1",
                            Text = "Welcome.",
                            OriginalMarkdown = "Welcome.",
                            Source = new Core.SourceReference
                            {
                                FilePath = "docs/api.md",
                                Span = new Core.SpanReference { Offset = 15, Length = 8 }
                            }
                        },
                        new Core.TableBlock
                        {
                            Id = "table-1",
                            Headers = ["Name", "Value"],
                            Rows = [["Mode", "Fast"]],
                            OriginalMarkdown = "| Name | Value |\n| --- | --- |\n| Mode | Fast |"
                        }
                    ],
                    SubSections =
                    [
                        new Core.DocumentSection
                        {
                            Id = "section-2",
                            Heading = new Core.HeadingBlock
                            {
                                Id = "heading-2",
                                Text = "Details",
                                Level = 3,
                                OriginalMarkdown = "### Details"
                            }
                        }
                    ]
                }
            ]
        };

        var result = document.ToIngestionDocument();
        var root = result.Sections.Should().ContainSingle().Subject;
        var content = result.EnumerateContent().ToArray();

        result.Identifier.Should().Be(document.Id);
        root.Metadata["markitdotnet.title"].Should().Be("API guide");
        root.Metadata["markitdotnet.custom.category"].Should().Be("guide");
        root.Elements.OfType<IngestionDocumentSection>().Should().ContainSingle();
        content.OfType<IngestionDocumentHeader>().Select(header => header.Level)
            .Should().ContainInOrder(1, 2, 3);
        content.OfType<IngestionDocumentParagraph>().Should().ContainSingle(paragraph =>
            paragraph.Text == "Welcome."
            && (string)paragraph.Metadata["markitdotnet.blockId"]! == "paragraph-1");

        var table = content.OfType<IngestionDocumentTable>().Should().ContainSingle().Subject;
        table.Cells.GetLength(0).Should().Be(2);
        table.Cells.GetLength(1).Should().Be(2);
        table.Cells[0, 0]!.Text.Should().Be("Name");
        table.Cells[1, 1]!.Text.Should().Be("Fast");
    }

    [Fact]
    public void ToIngestionDocument_UsesMarkdownRenderingForProgrammaticallyCreatedBlocks()
    {
        var document = new Core.Document
        {
            Id = "generated",
            Sections =
            [
                new Core.DocumentSection
                {
                    Id = "section",
                    Blocks =
                    [
                        new Core.CodeBlock { Id = "code", Content = "Console.WriteLine(1);", Language = "csharp" },
                        new Core.QuoteBlock
                        {
                            Id = "quote",
                            Blocks = [new Core.ParagraphBlock { Id = "quote-paragraph", Text = "Quoted." }]
                        }
                    ]
                }
            ]
        };

        var elements = document.ToIngestionDocument().EnumerateContent().ToArray();

        elements.OfType<IngestionDocumentParagraph>().Should().ContainSingle(paragraph =>
            paragraph.Text == "Console.WriteLine(1);"
            && paragraph.GetMarkdown().Contains("```csharp", StringComparison.Ordinal)
            && (string)paragraph.Metadata["markitdotnet.codeLanguage"]! == "csharp");
        elements.OfType<IngestionDocumentParagraph>().Should().ContainSingle(paragraph =>
            paragraph.GetMarkdown().Contains("> Quoted.", StringComparison.Ordinal)
            && (string)paragraph.Metadata["markitdotnet.blockType"]!
                == nameof(Core.QuoteBlock));
    }

    [Fact]
    public void ToIngestionDocument_MapsFiguresAndRetainsListMarkdown()
    {
        var document = new Core.Document
        {
            Id = "rich-blocks",
            Sections =
            [
                new Core.DocumentSection
                {
                    Id = "section",
                    Blocks =
                    [
                        new Core.FigureBlock
                        {
                            Id = "figure",
                            AltText = "A diagram",
                            ImagePath = "diagram.png",
                            OriginalMarkdown = "![A diagram](diagram.png)"
                        },
                        new Core.ListBlock
                        {
                            Id = "tasks",
                            Items = [new Core.ListItemBlock { Id = "task", Text = "ship", IsChecked = true }],
                            OriginalMarkdown = "- [x] ship"
                        }
                    ]
                }
            ]
        };

        var elements = document.ToIngestionDocument().EnumerateContent().ToArray();

        elements.OfType<IngestionDocumentImage>().Should().ContainSingle(image =>
            image.AlternativeText == "A diagram"
            && image.GetMarkdown() == "![A diagram](diagram.png)");
        elements.OfType<IngestionDocumentParagraph>().Should().ContainSingle(paragraph =>
            paragraph.GetMarkdown() == "- [x] ship"
            && (string)paragraph.Metadata["markitdotnet.blockType"]! == nameof(Core.ListBlock));
    }

    [Fact]
    public void ToIngestionDocument_RejectsDocumentsWithoutAnIdentifier()
    {
        var document = new Core.Document();

        var act = () => document.ToIngestionDocument();

        act.Should().Throw<ArgumentException>();
    }
}
