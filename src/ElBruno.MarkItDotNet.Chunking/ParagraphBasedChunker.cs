// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using System.Text;
using ElBruno.MarkItDotNet.CoreModel;

namespace ElBruno.MarkItDotNet.Chunking;

/// <summary>
/// A chunking strategy that splits a document at paragraph boundaries,
/// accumulating content until the maximum chunk size is reached.
/// Tables and figures can be preserved atomically.
/// </summary>
public class ParagraphBasedChunker : IChunkingStrategy
{
    /// <inheritdoc />
    public string Name => "paragraph-based";

    /// <inheritdoc />
    public IReadOnlyList<ChunkResult> Chunk(Document document, ChunkingOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(document);

        options ??= new ChunkingOptions();
        var allBlocks = FlattenBlocks(document);
        var results = new List<ChunkResult>();
        var index = 0;

        var currentContent = new StringBuilder();
        var currentSources = new List<SourceReference>();
        var previousOverlap = string.Empty;

        foreach (var (block, headingPath) in allBlocks)
        {
            var blockText = RenderBlock(block, options);
            if (string.IsNullOrWhiteSpace(blockText))
            {
                continue;
            }

            var isAtomic = (block is TableBlock && options.PreserveTableAtomicity)
                        || (block is FigureBlock && options.PreserveFigureAtomicity);

            // If adding this block exceeds MaxChunkSize, flush current chunk first
            if (currentContent.Length > 0
                && currentContent.Length + blockText.Length + 1 > options.MaxChunkSize)
            {
                var content = currentContent.ToString().TrimEnd();
                previousOverlap = ExtractOverlap(content, options.OverlapSize);
                EmitChunk(document.Id, content, headingPath, currentSources, results, ref index);
                currentContent.Clear();
                currentSources = [];

                // Apply overlap from previous chunk
                if (!string.IsNullOrEmpty(previousOverlap))
                {
                    currentContent.Append(previousOverlap);
                    currentContent.AppendLine();
                }
            }

            // If the block itself exceeds MaxChunkSize and is not atomic, split it
            if (!isAtomic && blockText.Length > options.MaxChunkSize)
            {
                // Flush anything we have first
                if (currentContent.Length > 0)
                {
                    var content = currentContent.ToString().TrimEnd();
                    previousOverlap = ExtractOverlap(content, options.OverlapSize);
                    EmitChunk(document.Id, content, headingPath, currentSources, results, ref index);
                    currentContent.Clear();
                    currentSources = [];
                }

                // Split oversized block
                var remaining = blockText;
                while (remaining.Length > 0)
                {
                    var chunkSize = Math.Min(options.MaxChunkSize, remaining.Length);
                    var chunk = remaining[..chunkSize];
                    remaining = remaining[chunkSize..];

                    if (!string.IsNullOrEmpty(previousOverlap) && results.Count > 0)
                    {
                        chunk = previousOverlap + "\n" + chunk;
                    }

                    previousOverlap = ExtractOverlap(chunk, options.OverlapSize);

                    var sources = block.Source is not null ? new List<SourceReference> { block.Source } : [];
                    EmitChunk(document.Id, chunk.TrimEnd(), headingPath, sources, results, ref index);
                }

                continue;
            }

            // Atomic blocks that exceed size: emit as their own chunk
            if (isAtomic && currentContent.Length > 0
                && currentContent.Length + blockText.Length + 1 > options.MaxChunkSize)
            {
                var content = currentContent.ToString().TrimEnd();
                previousOverlap = ExtractOverlap(content, options.OverlapSize);
                EmitChunk(document.Id, content, headingPath, currentSources, results, ref index);
                currentContent.Clear();
                currentSources = [];
            }

            if (currentContent.Length > 0)
            {
                currentContent.AppendLine();
            }

            currentContent.Append(blockText);

            if (block.Source is not null)
            {
                currentSources.Add(block.Source);
            }
        }

        // Flush remaining content
        if (currentContent.Length > 0)
        {
            var finalContent = currentContent.ToString().TrimEnd();
            if (!string.IsNullOrWhiteSpace(finalContent))
            {
                EmitChunk(document.Id, finalContent, null, currentSources, results, ref index);
            }
        }

        return results;
    }

    private static void EmitChunk(
        string documentId,
        string content,
        string? headingPath,
        List<SourceReference> sources,
        List<ChunkResult> results,
        ref int index)
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

    private static string ExtractOverlap(string content, int overlapSize)
    {
        if (overlapSize <= 0 || content.Length <= overlapSize)
        {
            return content.Length <= overlapSize ? content : string.Empty;
        }

        return content[^overlapSize..];
    }

    private static List<(DocumentBlock Block, string? HeadingPath)> FlattenBlocks(Document document)
    {
        var blocks = new List<(DocumentBlock, string?)>();

        foreach (var section in document.Sections)
        {
            FlattenSection(section, parentHeadingPath: null, blocks);
        }

        return blocks;
    }

    private static void FlattenSection(
        DocumentSection section,
        string? parentHeadingPath,
        List<(DocumentBlock, string?)> blocks)
    {
        var headingText = section.Heading?.Text;
        var headingPath = string.IsNullOrEmpty(headingText)
            ? parentHeadingPath
            : (string.IsNullOrEmpty(parentHeadingPath) ? headingText : $"{parentHeadingPath} > {headingText}");

        foreach (var block in section.Blocks)
        {
            blocks.Add((block, headingPath));
        }

        foreach (var subSection in section.SubSections)
        {
            FlattenSection(subSection, headingPath, blocks);
        }
    }

    private static string RenderBlock(DocumentBlock block, ChunkingOptions options)
    {
        return block switch
        {
            ParagraphBlock paragraph => paragraph.Text,
            HeadingBlock heading => heading.Text,
            TableBlock table => RenderTable(table),
            FigureBlock figure => RenderFigure(figure),
            ListBlock list => RenderList(list),
            _ => string.Empty,
        };
    }

    private static string RenderTable(TableBlock table)
    {
        var builder = new StringBuilder();

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
        if (!string.IsNullOrEmpty(figure.Caption))
        {
            return $"[Figure: {figure.Caption}]";
        }

        if (!string.IsNullOrEmpty(figure.AltText))
        {
            return $"[Figure: {figure.AltText}]";
        }

        return "[Figure]";
    }

    private static string RenderList(ListBlock list)
    {
        var builder = new StringBuilder();
        RenderListItems(builder, list.Items, list.IsOrdered, depth: 0);
        return builder.ToString().TrimEnd();
    }

    private static void RenderListItems(
        StringBuilder builder,
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
