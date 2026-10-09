// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using System.Text;
using CoreModel = ElBruno.MarkItDotNet.CoreModel;
using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Extensions.TaskLists;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using ModelCodeBlock = ElBruno.MarkItDotNet.CoreModel.CodeBlock;
using ModelHeadingBlock = ElBruno.MarkItDotNet.CoreModel.HeadingBlock;
using ModelListBlock = ElBruno.MarkItDotNet.CoreModel.ListBlock;
using ModelListItemBlock = ElBruno.MarkItDotNet.CoreModel.ListItemBlock;
using ModelParagraphBlock = ElBruno.MarkItDotNet.CoreModel.ParagraphBlock;
using ModelQuoteBlock = ElBruno.MarkItDotNet.CoreModel.QuoteBlock;
using ModelTableBlock = ElBruno.MarkItDotNet.CoreModel.TableBlock;
using MarkdownHeadingBlock = Markdig.Syntax.HeadingBlock;
using MarkdownListBlock = Markdig.Syntax.ListBlock;
using MarkdownListItemBlock = Markdig.Syntax.ListItemBlock;
using MarkdownParagraphBlock = Markdig.Syntax.ParagraphBlock;
using MarkdownQuoteBlock = Markdig.Syntax.QuoteBlock;
using MarkdigMarkdown = Markdig.Markdown;

namespace ElBruno.MarkItDotNet.Markdown;

/// <summary>
/// Parses CommonMark and Markdig's advanced Markdown extensions into the structured document model.
/// </summary>
public static class MarkdownDocumentParser
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    /// <summary>
    /// Parses Markdown into a structured document while retaining source locations and unsupported
    /// block syntax verbatim.
    /// </summary>
    /// <param name="markdown">Markdown source text.</param>
    /// <param name="sourcePath">Optional source path used for document and block references.</param>
    /// <returns>A structured document.</returns>
    public static CoreModel.Document Parse(string markdown, string? sourcePath = null)
    {
        ArgumentNullException.ThrowIfNull(markdown);

        var parsed = MarkdigMarkdown.Parse(markdown, Pipeline);
        var root = new SectionBuilder(null, 0, 0);
        var sectionStack = new Stack<SectionBuilder>();
        sectionStack.Push(root);
        string? title = null;
        var blockPosition = 0;

        foreach (var block in parsed)
        {
            if (block is MarkdownHeadingBlock heading)
            {
                var headingText = GetInlineText(heading.Inline);
                if (title is null && heading.Level == 1 && sectionStack.Count == 1 && root.Blocks.Count == 0)
                {
                    title = headingText;
                    continue;
                }

                while (sectionStack.Count > 1 && sectionStack.Peek().Level >= heading.Level)
                {
                    sectionStack.Pop();
                }

                var headingBlock = new ModelHeadingBlock
                {
                    Id = CoreModel.DocumentIdGenerator.ForBlock("heading", headingText, blockPosition++),
                    Text = headingText,
                    Level = heading.Level,
                    Source = CreateSource(sourcePath, heading.Span),
                    OriginalMarkdown = GetSourceSlice(markdown, heading.Span)
                };
                var section = new SectionBuilder(headingBlock, heading.Level, blockPosition);
                sectionStack.Peek().Children.Add(section);
                sectionStack.Push(section);
                continue;
            }

            var mapped = MapBlock(block, markdown, sourcePath, ref blockPosition);
            if (mapped is not null)
            {
                sectionStack.Peek().Blocks.Add(mapped);
            }
        }

        var sections = root.Children.Select(ToDocumentSection).ToArray();
        if (root.Blocks.Count > 0)
        {
            sections = [ToDocumentSection(root)];
        }
        else if (sections.Length == 0)
        {
            sections = [ToDocumentSection(root)];
        }

        var documentId = CoreModel.DocumentIdGenerator.Generate(sourcePath ?? string.Empty, markdown);
        return new CoreModel.Document
        {
            Id = documentId,
            Sections = sections,
            Metadata = new CoreModel.DocumentMetadata
            {
                Title = title,
                SourceFormat = "markdown",
                WordCount = CountWords(markdown)
            },
            Source = sourcePath is null ? null : new CoreModel.SourceReference { FilePath = sourcePath }
        };
    }

    private static CoreModel.DocumentSection ToDocumentSection(SectionBuilder builder) => new()
    {
        Id = CoreModel.DocumentIdGenerator.ForBlock("section", builder.Heading?.Text ?? string.Empty, builder.Position),
        Heading = builder.Heading,
        Blocks = builder.Blocks.ToArray(),
        SubSections = builder.Children.Select(ToDocumentSection).ToArray()
    };

    private static CoreModel.DocumentBlock? MapBlock(
        Block block,
        string source,
        string? sourcePath,
        ref int position)
    {
        var blockPosition = position++;
        var sourceReference = CreateSource(sourcePath, block.Span);

        switch (block)
        {
            case MarkdownParagraphBlock paragraph:
            {
                var text = GetInlineText(paragraph.Inline);
                return new ModelParagraphBlock
                {
                    Id = CoreModel.DocumentIdGenerator.ForBlock("paragraph", text, blockPosition),
                    Text = text,
                    Source = sourceReference,
                    OriginalMarkdown = GetSourceSlice(source, paragraph.Span)
                };
            }
            case Table table:
                return MapTable(table, source, blockPosition, sourceReference);
            case MarkdownListBlock list:
                return MapList(list, source, sourcePath, blockPosition, ref position);
            case MarkdownQuoteBlock quote:
            {
                var quoteBlocks = new List<CoreModel.DocumentBlock>();
                foreach (var child in quote)
                {
                    var childBlock = MapBlock(child, source, sourcePath, ref position);
                    if (childBlock is not null)
                    {
                        quoteBlocks.Add(childBlock);
                    }
                }

                return new ModelQuoteBlock
                {
                    Id = CoreModel.DocumentIdGenerator.ForBlock("quote", GetSourceSlice(source, quote.Span), blockPosition),
                    Blocks = quoteBlocks,
                    Source = sourceReference,
                    OriginalMarkdown = GetSourceSlice(source, quote.Span)
                };
            }
            case Markdig.Syntax.CodeBlock code:
            {
                var fencedCode = code as FencedCodeBlock;
                var content = code.Lines.ToString();
                var language = fencedCode?.Info?.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                return new ModelCodeBlock
                {
                    Id = CoreModel.DocumentIdGenerator.ForBlock("code", content, blockPosition),
                    Content = content.TrimEnd('\r', '\n'),
                    Language = language,
                    Source = sourceReference,
                    OriginalMarkdown = GetSourceSlice(source, code.Span)
                };
            }
            default:
            {
                var raw = GetSourceSlice(source, block.Span);
                return new CoreModel.RawMarkdownBlock
                {
                    Id = CoreModel.DocumentIdGenerator.ForBlock("rawMarkdown", raw, blockPosition),
                    Content = raw,
                    BlockType = block.GetType().Name,
                    Source = sourceReference,
                    OriginalMarkdown = raw
                };
            }
        }
    }

    private static ModelTableBlock MapTable(
        Table table,
        string markdown,
        int position,
        CoreModel.SourceReference? source)
    {
        var rows = table.OfType<TableRow>().ToList();
        if (rows.Count == 0)
        {
            return new ModelTableBlock
            {
                Id = CoreModel.DocumentIdGenerator.ForBlock("table", string.Empty, position),
                Source = source
            };
        }

        var headerCells = rows[0].OfType<TableCell>().Select(GetBlockText).ToArray();
        var dataRows = rows.Skip(1)
            .Select(row => (IReadOnlyList<string>)row.OfType<TableCell>().Select(GetBlockText).ToArray())
            .ToArray();
        var properties = new Dictionary<string, object>();
        if (table.ColumnDefinitions.Count > 0)
        {
            properties["ColumnAlignments"] = table.ColumnDefinitions
                .Select(column => column.Alignment?.ToString() ?? "None")
                .ToArray();
        }

        return new ModelTableBlock
        {
            Id = CoreModel.DocumentIdGenerator.ForBlock("table", string.Join("|", headerCells), position),
            Headers = headerCells,
            Rows = dataRows,
            Properties = properties,
            Source = source,
            OriginalMarkdown = GetSourceSlice(markdown, table.Span)
        };
    }

    private static ModelListBlock MapList(
        MarkdownListBlock list,
        string source,
        string? sourcePath,
        int position,
        ref int nextPosition)
    {
        var items = new List<ModelListItemBlock>();
        foreach (var item in list.OfType<MarkdownListItemBlock>())
        {
            items.Add(MapListItem(item, source, sourcePath, ref nextPosition));
        }

        return new ModelListBlock
        {
            Id = CoreModel.DocumentIdGenerator.ForBlock("list", string.Join("|", items.Select(item => item.Text)), position),
            IsOrdered = list.IsOrdered,
            Items = items.ToArray(),
            Source = CreateSource(sourcePath, list.Span),
            OriginalMarkdown = GetSourceSlice(source, list.Span)
        };
    }

    private static ModelListItemBlock MapListItem(
        MarkdownListItemBlock item,
        string source,
        string? sourcePath,
        ref int nextPosition)
    {
        var texts = new List<string>();
        var subItems = new List<ModelListItemBlock>();
        bool? isChecked = null;

        foreach (var child in item)
        {
            if (child is MarkdownParagraphBlock paragraph)
            {
                var task = FindTaskList(paragraph.Inline);
                if (task is not null)
                {
                    isChecked = task.Checked;
                }

                var text = GetInlineText(paragraph.Inline);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    texts.Add(text.Trim());
                }
            }
            else if (child is MarkdownListBlock nestedList)
            {
                foreach (var nested in nestedList.OfType<MarkdownListItemBlock>())
                {
                    subItems.Add(MapListItem(nested, source, sourcePath, ref nextPosition));
                }
            }
            else
            {
                var nestedBlock = MapBlock(child, source, sourcePath, ref nextPosition);
                if (nestedBlock is CoreModel.ParagraphBlock nestedParagraph)
                {
                    texts.Add(nestedParagraph.Text);
                }
            }
        }

        var textValue = string.Join(Environment.NewLine, texts);
        var itemPosition = nextPosition++;
        return new ModelListItemBlock
        {
            Id = CoreModel.DocumentIdGenerator.ForBlock("listItem", textValue, itemPosition),
            Text = textValue,
            SubItems = subItems,
            IsChecked = isChecked,
            Source = CreateSource(sourcePath, item.Span)
        };
    }

    private static TaskList? FindTaskList(ContainerInline? inline)
    {
        if (inline is null)
        {
            return null;
        }

        foreach (var child in inline)
        {
            if (child is TaskList task)
            {
                return task;
            }

            if (child is ContainerInline container && FindTaskList(container) is { } nestedTask)
            {
                return nestedTask;
            }
        }

        return null;
    }

    private static string GetBlockText(Block block) => block switch
    {
        LeafBlock leaf => GetInlineText(leaf.Inline),
        ContainerBlock container => string.Join(string.Empty, container.Select(GetBlockText)),
        _ => string.Empty
    };

    private static string GetInlineText(ContainerInline? inline)
    {
        if (inline is null)
        {
            return string.Empty;
        }

        var result = new StringBuilder();
        foreach (var child in inline)
        {
            switch (child)
            {
                case LiteralInline literal:
                    result.Append(literal.Content);
                    break;
                case CodeInline code:
                    result.Append(code.Content);
                    break;
                case LineBreakInline:
                    result.AppendLine();
                    break;
                case HtmlInline html:
                    result.Append(html.Tag);
                    break;
                case TaskList:
                    break;
                case ContainerInline nested:
                    result.Append(GetInlineText(nested));
                    break;
                default:
                    result.Append(child);
                    break;
            }
        }

        return result.ToString();
    }

    private static CoreModel.SourceReference? CreateSource(string? sourcePath, SourceSpan span)
    {
        if (sourcePath is null && span.Start < 0)
        {
            return null;
        }

        return new CoreModel.SourceReference
        {
            FilePath = sourcePath,
            Span = span.Start < 0
                ? null
                : new CoreModel.SpanReference { Offset = span.Start, Length = span.Length }
        };
    }

    private static string GetSourceSlice(string source, SourceSpan span)
    {
        if (span.Start < 0 || span.Length <= 0 || span.Start >= source.Length)
        {
            return string.Empty;
        }

        return source.Substring(span.Start, Math.Min(span.Length, source.Length - span.Start));
    }

    private static int CountWords(string text) =>
        text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;

    private sealed class SectionBuilder(CoreModel.HeadingBlock? heading, int level, int position)
    {
        public CoreModel.HeadingBlock? Heading { get; } = heading;
        public int Level { get; } = level;
        public int Position { get; } = position;
        public List<CoreModel.DocumentBlock> Blocks { get; } = [];
        public List<SectionBuilder> Children { get; } = [];
    }
}
