using ElBruno.MarkItDotNet.CoreModel;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Markdown.Tests;

public class MarkdownDocumentParserTests
{
    [Fact]
    public void Parse_MapsTitleHeadingsAndSourceLocations()
    {
        const string markdown = "# Guide\n\nIntroduction.\n\n## Setup\n\nInstall it.";

        var document = MarkdownDocumentParser.Parse(markdown, "guide.md");

        document.Metadata.Title.Should().Be("Guide");
        document.Source!.FilePath.Should().Be("guide.md");
        document.Sections.Should().ContainSingle();
        document.Sections[0].Blocks.Should().ContainSingle()
            .Which.As<ParagraphBlock>().Text.Should().Be("Introduction.");
        document.Sections[0].SubSections.Should().ContainSingle();
        document.Sections[0].SubSections[0].Heading!.Text.Should().Be("Setup");
        document.Sections[0].SubSections[0].Heading!.Source!.Span!.Offset.Should().Be(markdown.IndexOf("## Setup", StringComparison.Ordinal));
        document.Sections[0].SubSections[0].Blocks.Should().ContainSingle()
            .Which.As<ParagraphBlock>().Text.Should().Be("Install it.");
        document.Sections[0].SubSections[0].Blocks[0].OriginalMarkdown.Should().Be("Install it.");
    }

    [Fact]
    public void Parse_MapsTablesCodeQuotesAndTaskLists()
    {
        const string markdown = """
            # Guide

            ## Content

            | Name | Count |
            | :--- | ---: |
            | Apples | 3 |

            ```csharp
            Console.WriteLine("hello");
            ```

            > A quoted paragraph.

            - [x] Complete
            - [ ] Pending
            """;

        var document = MarkdownDocumentParser.Parse(markdown);
        var blocks = document.Sections[0].Blocks;

        blocks.OfType<TableBlock>().Should().ContainSingle().Which.Rows[0]
            .Should().Equal("Apples", "3");
        blocks.OfType<CodeBlock>().Should().ContainSingle().Which.Language.Should().Be("csharp");
        blocks.OfType<QuoteBlock>().Should().ContainSingle()
            .Which.Blocks.OfType<ParagraphBlock>().Should().ContainSingle()
            .Which.Text.Should().Be("A quoted paragraph.");

        var listItems = blocks.OfType<ListBlock>().Single().Items;
        listItems.Select(item => item.IsChecked).Should().Equal(true, false);
        listItems.Select(item => item.Text).Should().Equal("Complete", "Pending");
    }

    [Fact]
    public void Parse_PreservesInlineMarkdownAndUnsupportedBlocks()
    {
        const string markdown = "# Guide\n\nA **bold** [link](https://example.com).\n\n---";

        var document = MarkdownDocumentParser.Parse(markdown);
        var section = document.Sections.Single();

        section.Blocks[0].Should().BeOfType<ParagraphBlock>()
            .Which.OriginalMarkdown.Should().Be("A **bold** [link](https://example.com).");
        section.Blocks[1].Should().BeOfType<RawMarkdownBlock>()
            .Which.Content.Should().Be("---");
    }

    [Fact]
    public void Parse_ProducesStableDocumentIdForSameContentAndSource()
    {
        const string markdown = "# Stable";

        MarkdownDocumentParser.Parse(markdown, "stable.md").Id
            .Should().Be(MarkdownDocumentParser.Parse(markdown, "stable.md").Id);
    }

    [Fact]
    public void Parse_RejectsNullInput()
    {
        var act = () => MarkdownDocumentParser.Parse(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
