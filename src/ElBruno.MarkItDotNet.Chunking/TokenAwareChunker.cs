// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using System.Text;
using System.Text.RegularExpressions;
using ElBruno.MarkItDotNet.CoreModel;

namespace ElBruno.MarkItDotNet.Chunking;

/// <summary>
/// A chunking strategy that respects token limits by splitting at natural boundaries
/// (paragraph and sentence) while staying within the configured <see cref="ChunkingOptions.MaxChunkSize"/> tokens.
/// </summary>
public partial class TokenAwareChunker : IChunkingStrategy
{
    /// <inheritdoc />
    public string Name => "token-aware";

    /// <inheritdoc />
    public IReadOnlyList<ChunkResult> Chunk(Document document, ChunkingOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(document);

        options ??= new ChunkingOptions();
        var tokenCounter = options.TokenCounter ?? DefaultTokenCounter;
        var allBlocks = FlattenBlocks(document);
        var results = new List<ChunkResult>();
        var index = 0;

        var currentParts = new List<string>();
        var currentTokens = 0;
        var currentSources = new List<SourceReference>();
        string? currentHeadingPath = null;
        var overlapParts = new List<string>();

        foreach (var (block, headingPath) in allBlocks)
        {
            var blockText = RenderBlock(block, options);
            if (string.IsNullOrWhiteSpace(blockText))
            {
                continue;
            }

            var blockTokens = tokenCounter(blockText);
            currentHeadingPath = headingPath;

            var isAtomic = (block is TableBlock && options.PreserveTableAtomicity)
                        || (block is FigureBlock && options.PreserveFigureAtomicity);

            // If this block alone exceeds MaxChunkSize and is not atomic, split at sentence boundaries
            if (!isAtomic && blockTokens > options.MaxChunkSize)
            {
                // Flush current content first
                if (currentParts.Count > 0)
                {
                    var content = string.Join("\n", currentParts).TrimEnd();
                    overlapParts = ExtractOverlapParts(currentParts, options.OverlapSize, tokenCounter);
                    EmitChunk(document.Id, content, headingPath, currentSources, results, ref index);
                    currentParts = [];
                    currentTokens = 0;
                    currentSources = [];
                }

                // Split by sentences
                var sentences = SplitIntoSentences(blockText);
                var sentenceParts = new List<string>();
                var sentenceTokens = 0;

                // Apply overlap from previous chunk
                if (overlapParts.Count > 0)
                {
                    sentenceParts.AddRange(overlapParts);
                    sentenceTokens = overlapParts.Sum(p => tokenCounter(p));
                    overlapParts = [];
                }

                foreach (var sentence in sentences)
                {
                    var sTokens = tokenCounter(sentence);

                    if (sentenceParts.Count > 0 && sentenceTokens + sTokens > options.MaxChunkSize)
                    {
                        var content = string.Join(" ", sentenceParts).TrimEnd();
                        overlapParts = ExtractOverlapParts(sentenceParts, options.OverlapSize, tokenCounter);
                        var sources = block.Source is not null ? new List<SourceReference> { block.Source } : [];
                        EmitChunk(document.Id, content, headingPath, sources, results, ref index);
                        sentenceParts = [.. overlapParts];
                        sentenceTokens = sentenceParts.Sum(p => tokenCounter(p));
                        overlapParts = [];
                    }

                    sentenceParts.Add(sentence);
                    sentenceTokens += sTokens;
                }

                if (sentenceParts.Count > 0)
                {
                    currentParts = sentenceParts;
                    currentTokens = sentenceTokens;
                    if (block.Source is not null)
                    {
                        currentSources.Add(block.Source);
                    }
                }

                continue;
            }

            // Would adding this block exceed the limit?
            if (currentParts.Count > 0 && currentTokens + blockTokens > options.MaxChunkSize)
            {
                var content = string.Join("\n", currentParts).TrimEnd();
                overlapParts = ExtractOverlapParts(currentParts, options.OverlapSize, tokenCounter);
                EmitChunk(document.Id, content, headingPath, currentSources, results, ref index);
                currentParts = [.. overlapParts];
                currentTokens = overlapParts.Sum(p => tokenCounter(p));
                currentSources = [];
                overlapParts = [];
            }

            currentParts.Add(blockText);
            currentTokens += blockTokens;

            if (block.Source is not null)
            {
                currentSources.Add(block.Source);
            }
        }

        // Flush remaining
        if (currentParts.Count > 0)
        {
            var finalContent = string.Join("\n", currentParts).TrimEnd();
            if (!string.IsNullOrWhiteSpace(finalContent))
            {
                EmitChunk(document.Id, finalContent, currentHeadingPath, currentSources, results, ref index);
            }
        }

        return results;
    }

    /// <summary>
    /// Default token counter that estimates tokens by counting whitespace-separated words.
    /// </summary>
    public static int DefaultTokenCounter(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        return WordSplitRegex().Split(text.Trim()).Length;
    }

    private static List<string> ExtractOverlapParts(
        List<string> parts,
        int overlapTokens,
        Func<string, int> tokenCounter)
    {
        if (overlapTokens <= 0 || parts.Count == 0)
        {
            return [];
        }

        var overlap = new List<string>();
        var tokens = 0;

        for (var i = parts.Count - 1; i >= 0; i--)
        {
            var partTokens = tokenCounter(parts[i]);
            if (tokens + partTokens > overlapTokens && overlap.Count > 0)
            {
                break;
            }

            overlap.Insert(0, parts[i]);
            tokens += partTokens;
        }

        return overlap;
    }

    private static List<string> SplitIntoSentences(string text)
    {
        var sentences = SentenceSplitRegex().Split(text)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList();

        return sentences.Count > 0 ? sentences : [text];
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

    [GeneratedRegex(@"\s+")]
    private static partial Regex WordSplitRegex();

    [GeneratedRegex(@"(?<=[.!?])\s+")]
    private static partial Regex SentenceSplitRegex();
}
