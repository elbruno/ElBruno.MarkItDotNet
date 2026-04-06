// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.CoreModel.Tests;

public class DocumentBlockTests
{
    [Fact]
    public void ParagraphBlock_HasDefaultEmptyText()
    {
        var block = new ParagraphBlock();

        block.Text.Should().BeEmpty();
        block.Id.Should().BeEmpty();
    }

    [Fact]
    public void ParagraphBlock_CanSetText()
    {
        var block = new ParagraphBlock { Text = "Hello world" };

        block.Text.Should().Be("Hello world");
    }

    [Fact]
    public void HeadingBlock_HasTextAndLevel()
    {
        var block = new HeadingBlock { Text = "Title", Level = 2 };

        block.Text.Should().Be("Title");
        block.Level.Should().Be(2);
    }

    [Fact]
    public void HeadingBlock_DefaultLevel_IsZero()
    {
        var block = new HeadingBlock();

        block.Level.Should().Be(0);
    }

    [Fact]
    public void TableBlock_HasHeadersAndRows()
    {
        var block = new TableBlock
        {
            Headers = ["Name", "Age"],
            Rows = [["Alice", "30"], ["Bob", "25"]]
        };

        block.Headers.Should().HaveCount(2);
        block.Rows.Should().HaveCount(2);
        block.Rows[0][0].Should().Be("Alice");
    }

    [Fact]
    public void FigureBlock_HasAltTextCaptionAndPath()
    {
        var block = new FigureBlock
        {
            AltText = "A cat",
            Caption = "Figure 1: A cat",
            ImagePath = "images/cat.png"
        };

        block.AltText.Should().Be("A cat");
        block.Caption.Should().Be("Figure 1: A cat");
        block.ImagePath.Should().Be("images/cat.png");
    }

    [Fact]
    public void FigureBlock_NullableProperties_DefaultToNull()
    {
        var block = new FigureBlock();

        block.AltText.Should().BeNull();
        block.Caption.Should().BeNull();
        block.ImagePath.Should().BeNull();
    }

    [Fact]
    public void ListBlock_SupportsOrderedAndUnordered()
    {
        var ordered = new ListBlock { IsOrdered = true, Items = [new ListItemBlock { Text = "First" }] };
        var unordered = new ListBlock { IsOrdered = false, Items = [new ListItemBlock { Text = "Item" }] };

        ordered.IsOrdered.Should().BeTrue();
        unordered.IsOrdered.Should().BeFalse();
    }

    [Fact]
    public void ListItemBlock_SupportsNestedSubItems()
    {
        var item = new ListItemBlock
        {
            Text = "Parent",
            SubItems =
            [
                new ListItemBlock { Text = "Child 1" },
                new ListItemBlock { Text = "Child 2" }
            ]
        };

        item.SubItems.Should().HaveCount(2);
        item.SubItems[0].Text.Should().Be("Child 1");
    }

    [Fact]
    public void DocumentBlock_PropertiesDictionary_CanStoreValues()
    {
        var block = new ParagraphBlock
        {
            Properties = new Dictionary<string, object> { ["bold"] = true }
        };

        block.Properties.Should().ContainKey("bold");
    }

    [Fact]
    public void DocumentBlock_Source_CanBeAttached()
    {
        var block = new ParagraphBlock
        {
            Source = new SourceReference
            {
                FilePath = "doc.md",
                PageNumber = 3,
                Span = new SpanReference { Offset = 10, Length = 50 }
            }
        };

        block.Source.Should().NotBeNull();
        block.Source!.FilePath.Should().Be("doc.md");
        block.Source.PageNumber.Should().Be(3);
        block.Source.Span!.Offset.Should().Be(10);
        block.Source.Span.Length.Should().Be(50);
    }
}
