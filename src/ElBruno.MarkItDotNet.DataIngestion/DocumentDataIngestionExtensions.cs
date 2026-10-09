// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.DataIngestion;
using Core = ElBruno.MarkItDotNet.CoreModel;

namespace ElBruno.MarkItDotNet.DataIngestion;

/// <summary>
/// Converts MarkItDotNet documents into Microsoft.Extensions.DataIngestion documents.
/// </summary>
public static class DocumentDataIngestionExtensions
{
    private const string MetadataPrefix = "markitdotnet.";

    /// <summary>
    /// Converts a CoreModel document into a Microsoft.Extensions.DataIngestion document.
    /// </summary>
    /// <param name="document">The document to convert.</param>
    /// <returns>A structured ingestion document.</returns>
    public static IngestionDocument ToIngestionDocument(this Core.Document document)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrWhiteSpace(document.Id);

        var result = new IngestionDocument(document.Id);
        var root = new IngestionDocumentSection();
        AddDocumentMetadata(root.Metadata, document);

        if (!string.IsNullOrWhiteSpace(document.Metadata.Title))
        {
            var title = new IngestionDocumentHeader($"# {document.Metadata.Title}")
            {
                Text = document.Metadata.Title,
                Level = 1
            };
            title.Metadata[$"{MetadataPrefix}elementType"] = "documentTitle";
            root.Elements.Add(title);
        }

        foreach (var section in document.Sections)
        {
            root.Elements.Add(MapSection(section));
        }

        result.Sections.Add(root);
        return result;
    }

    private static IngestionDocumentSection MapSection(Core.DocumentSection source)
    {
        var section = new IngestionDocumentSection();
        section.Metadata[$"{MetadataPrefix}sectionId"] = source.Id;

        if (source.Heading is not null)
        {
            var heading = MapBlock(source.Heading);
            if (heading is not null)
            {
                section.Elements.Add(heading);
            }
        }

        foreach (var block in source.Blocks)
        {
            var mapped = MapBlock(block);
            if (mapped is not null)
            {
                section.Elements.Add(mapped);
            }
        }

        foreach (var child in source.SubSections)
        {
            section.Elements.Add(MapSection(child));
        }

        return section;
    }

    private static IngestionDocumentElement? MapBlock(Core.DocumentBlock block)
    {
        var markdown = block.OriginalMarkdown ?? RenderBlock(block);
        if (string.IsNullOrEmpty(markdown))
        {
            return null;
        }

        IngestionDocumentElement element = block switch
        {
            Core.HeadingBlock heading => new IngestionDocumentHeader(markdown)
            {
                Text = heading.Text,
                Level = Math.Clamp(heading.Level, 1, 10)
            },
            Core.ParagraphBlock paragraph => new IngestionDocumentParagraph(markdown)
            {
                Text = paragraph.Text
            },
            Core.TableBlock table => new IngestionDocumentTable(markdown, MapTableCells(table)),
            Core.FigureBlock figure => new IngestionDocumentImage(markdown)
            {
                AlternativeText = figure.AltText
            },
            Core.CodeBlock code => new IngestionDocumentParagraph(markdown)
            {
                Text = code.Content
            },
            _ => new IngestionDocumentParagraph(markdown)
            {
                Text = markdown
            }
        };

        AddBlockMetadata(element, block);
        if (block is Core.CodeBlock { Language: not null } codeBlock)
        {
            element.Metadata[$"{MetadataPrefix}codeLanguage"] = codeBlock.Language;
        }

        return element;
    }

    private static IngestionDocumentElement?[,] MapTableCells(Core.TableBlock table)
    {
        var columnCount = Math.Max(
            table.Headers.Count,
            table.Rows.Count == 0 ? 0 : table.Rows.Max(row => row.Count));
        var cells = new IngestionDocumentElement?[table.Rows.Count + 1, columnCount];

        for (var column = 0; column < table.Headers.Count; column++)
        {
            cells[0, column] = CreateCell(table.Headers[column]);
        }

        for (var row = 0; row < table.Rows.Count; row++)
        {
            for (var column = 0; column < table.Rows[row].Count; column++)
            {
                cells[row + 1, column] = CreateCell(table.Rows[row][column]);
            }
        }

        return cells;
    }

    private static IngestionDocumentElement? CreateCell(string text) =>
        string.IsNullOrEmpty(text)
            ? null
            : new IngestionDocumentParagraph(text) { Text = text };

    private static void AddDocumentMetadata(
        IDictionary<string, object?> metadata,
        Core.Document document)
    {
        metadata[$"{MetadataPrefix}documentId"] = document.Id;
        AddIfNotNull(metadata, $"{MetadataPrefix}title", document.Metadata.Title);
        AddIfNotNull(metadata, $"{MetadataPrefix}author", document.Metadata.Author);
        AddIfNotNull(metadata, $"{MetadataPrefix}sourceFormat", document.Metadata.SourceFormat);
        AddIfNotNull(metadata, $"{MetadataPrefix}createdAt", document.Metadata.CreatedAt);
        AddIfNotNull(metadata, $"{MetadataPrefix}modifiedAt", document.Metadata.ModifiedAt);
        AddIfNotNull(metadata, $"{MetadataPrefix}pageCount", document.Metadata.PageCount);
        AddIfNotNull(metadata, $"{MetadataPrefix}wordCount", document.Metadata.WordCount);
        AddIfNotNull(metadata, $"{MetadataPrefix}sourcePath", document.Source?.FilePath);

        foreach (var (key, value) in document.Metadata.Custom)
        {
            metadata[$"{MetadataPrefix}custom.{key}"] = value;
        }
    }

    private static void AddBlockMetadata(IngestionDocumentElement element, Core.DocumentBlock block)
    {
        element.Metadata[$"{MetadataPrefix}blockId"] = block.Id;
        element.Metadata[$"{MetadataPrefix}blockType"] = block.GetType().Name;
        element.PageNumber = block.Source?.PageNumber;

        if (block.Source is { } source)
        {
            AddIfNotNull(element.Metadata, $"{MetadataPrefix}sourcePath", source.FilePath);
            AddIfNotNull(element.Metadata, $"{MetadataPrefix}headingPath", source.HeadingPath);
            if (source.Span is { } span)
            {
                element.Metadata[$"{MetadataPrefix}sourceOffset"] = span.Offset;
                element.Metadata[$"{MetadataPrefix}sourceLength"] = span.Length;
            }
        }

        foreach (var (key, value) in block.Properties)
        {
            element.Metadata[$"{MetadataPrefix}property.{key}"] = value;
        }
    }

    private static void AddIfNotNull(IDictionary<string, object?> metadata, string key, object? value)
    {
        if (value is not null)
        {
            metadata[key] = value;
        }
    }

    private static string RenderBlock(Core.DocumentBlock block) =>
        Core.MarkdownRenderer.Render(new Core.Document
        {
            Id = "data-ingestion-adapter",
            Sections = [new Core.DocumentSection { Blocks = [block] }]
        }).Trim();
}
