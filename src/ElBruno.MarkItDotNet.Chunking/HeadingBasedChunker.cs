// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using ElBruno.MarkItDotNet.CoreModel;

namespace ElBruno.MarkItDotNet.Chunking;

/// <summary>
/// A chunking strategy that splits a document at heading boundaries.
/// Each section (and sub-section) becomes a separate chunk, with heading text
/// prepended and a heading path tracking the hierarchy.
/// </summary>
public class HeadingBasedChunker : IChunkingStrategy
{
    /// <inheritdoc />
    public string Name => "heading-based";

    /// <inheritdoc />
    public IReadOnlyList<ChunkResult> Chunk(Document document, ChunkingOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(document);

        options ??= new ChunkingOptions();
        var results = new List<ChunkResult>();
        var index = 0;

        foreach (var section in document.Sections)
        {
            ChunkSection(document.Id, section, parentHeadingPath: null, results, ref index, options);
        }

        return results;
    }

    private static void ChunkSection(
        string documentId,
        DocumentSection section,
        string? parentHeadingPath,
        List<ChunkResult> results,
        ref int index,
        ChunkingOptions options)
    {
        var headingText = section.Heading?.Text;
        var headingPath = BuildHeadingPath(parentHeadingPath, headingText);

        var contentBuilder = new System.Text.StringBuilder();
        var sources = new List<SourceReference>();

        if (!string.IsNullOrEmpty(headingText))
        {
            contentBuilder.AppendLine(headingText);
        }

        foreach (var block in section.Blocks)
        {
            var blockText = RenderBlock(block, options);
            if (!string.IsNullOrEmpty(blockText))
            {
                contentBuilder.AppendLine(blockText);
            }

            if (block.Source is not null)
            {
                sources.Add(block.Source);
            }
        }

        var content = contentBuilder.ToString().TrimEnd();

        if (!string.IsNullOrWhiteSpace(content))
        {
            var chunkId = ChunkIdGenerator.Generate(documentId, index, content);
            results.Add(new ChunkResult
            {
                Id = chunkId,
                Content = content,
                Index = index,
                Sources = sources,
                HeadingPath = headingPath,
            });
            index++;
        }

        foreach (var subSection in section.SubSections)
        {
            ChunkSection(documentId, subSection, headingPath, results, ref index, options);
        }
    }

    private static string? BuildHeadingPath(string? parentPath, string? headingText)
    {
        if (string.IsNullOrEmpty(headingText))
        {
            return parentPath;
        }

        return string.IsNullOrEmpty(parentPath)
            ? headingText
            : $"{parentPath} > {headingText}";
    }

    private static string RenderBlock(DocumentBlock block, ChunkingOptions options)
    {
        return block switch
        {
            ParagraphBlock paragraph => paragraph.Text,
            HeadingBlock heading => heading.Text,
            TableBlock table when options.PreserveTableAtomicity => RenderTable(table),
            TableBlock table => RenderTable(table),
            FigureBlock figure when options.PreserveFigureAtomicity => RenderFigure(figure),
            FigureBlock figure => RenderFigure(figure),
            ListBlock list => RenderList(list),
            _ => string.Empty,
        };
    }

    private static string RenderTable(TableBlock table)
    {
        var builder = new System.Text.StringBuilder();

        if (table.Headers.Count > 0)
        {
            builder.AppendLine(string.Join(" | ", table.Headers));
        }

        foreach (var row in table.Rows)
        {
            builder.AppendLine(string.Join(" | ", row));
        }

        return builder.ToString().TrimEnd();
    }

    private static string RenderFigure(FigureBlock figure)
    {
        var parts = new List<string>();
        if (!string.IsNullOrEmpty(figure.Caption))
        {
            parts.Add($"[Figure: {figure.Caption}]");
        }
        else if (!string.IsNullOrEmpty(figure.AltText))
        {
            parts.Add($"[Figure: {figure.AltText}]");
        }
        else
        {
            parts.Add("[Figure]");
        }

        return string.Join(" ", parts);
    }

    private static string RenderList(ListBlock list)
    {
        var builder = new System.Text.StringBuilder();
        RenderListItems(builder, list.Items, list.IsOrdered, depth: 0);
        return builder.ToString().TrimEnd();
    }

    private static void RenderListItems(
        System.Text.StringBuilder builder,
        IReadOnlyList<ListItemBlock> items,
        bool isOrdered,
        int depth)
    {
        var indent = new string(' ', depth * 2);
        for (var i = 0; i < items.Count; i++)
        {
            var bullet = isOrdered ? $"{i + 1}." : "-";
            builder.AppendLine($"{indent}{bullet} {items[i].Text}");

            if (items[i].SubItems.Count > 0)
            {
                RenderListItems(builder, items[i].SubItems, isOrdered, depth + 1);
            }
        }
    }
}
