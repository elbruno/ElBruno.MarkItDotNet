using Azure.AI.DocumentIntelligence;
using ElBruno.MarkItDotNet.CoreModel;
using CoreModelSection = ElBruno.MarkItDotNet.CoreModel.DocumentSection;

namespace ElBruno.MarkItDotNet.DocumentIntelligence;

/// <summary>
/// Maps Azure Document Intelligence <see cref="AnalyzeResult"/> to the CoreModel <see cref="Document"/>.
/// Converts paragraphs, tables, and figures into structured document blocks organized by heading hierarchy.
/// </summary>
public static class DocumentIntelligenceMapper
{
    /// <summary>
    /// Maps an <see cref="AnalyzeResult"/> from Azure Document Intelligence to a CoreModel <see cref="Document"/>.
    /// </summary>
    /// <param name="result">The Azure Document Intelligence analysis result.</param>
    /// <param name="filePath">Optional source file path to include in source references.</param>
    /// <returns>A structured <see cref="Document"/> representing the analyzed content.</returns>
    public static Document MapToDocument(AnalyzeResult result, string? filePath = null)
    {
        ArgumentNullException.ThrowIfNull(result);

        var positionedElements = CollectPositionedElements(result, filePath);
        positionedElements.Sort((a, b) => a.Offset.CompareTo(b.Offset));

        var sections = BuildSections(positionedElements);
        var metadata = BuildMetadata(result, filePath);
        var source = filePath is not null ? new SourceReference { FilePath = filePath } : null;

        return new Document
        {
            Id = Guid.NewGuid().ToString(),
            Sections = sections,
            Metadata = metadata,
            Source = source
        };
    }

    private static List<PositionedElement> CollectPositionedElements(AnalyzeResult result, string? filePath)
    {
        var elements = new List<PositionedElement>();

        if (result.Paragraphs is not null)
        {
            foreach (var paragraph in result.Paragraphs)
            {
                var offset = GetOffset(paragraph.Spans);
                var pageNumber = GetPageNumber(paragraph.BoundingRegions);

                if (paragraph.Role == ParagraphRole.PageHeader ||
                    paragraph.Role == ParagraphRole.PageFooter ||
                    paragraph.Role == ParagraphRole.PageNumber)
                {
                    continue;
                }

                if (paragraph.Role == ParagraphRole.Title || paragraph.Role == ParagraphRole.SectionHeading)
                {
                    var level = paragraph.Role == ParagraphRole.Title ? 1 : 2;
                    var heading = new HeadingBlock
                    {
                        Id = Guid.NewGuid().ToString(),
                        Text = paragraph.Content,
                        Level = level,
                        Source = CreateSourceReference(filePath, pageNumber, paragraph.Spans)
                    };
                    elements.Add(new PositionedElement(offset, heading, IsHeading: true));
                }
                else
                {
                    var block = new ParagraphBlock
                    {
                        Id = Guid.NewGuid().ToString(),
                        Text = paragraph.Content,
                        Source = CreateSourceReference(filePath, pageNumber, paragraph.Spans)
                    };
                    elements.Add(new PositionedElement(offset, block, IsHeading: false));
                }
            }
        }

        if (result.Tables is not null)
        {
            foreach (var table in result.Tables)
            {
                var offset = GetOffset(table.Spans);
                var pageNumber = GetPageNumber(table.BoundingRegions);
                var tableBlock = MapTable(table, filePath, pageNumber);
                elements.Add(new PositionedElement(offset, tableBlock, IsHeading: false));
            }
        }

        if (result.Figures is not null)
        {
            foreach (var figure in result.Figures)
            {
                var offset = GetOffset(figure.Spans);
                var pageNumber = GetPageNumber(figure.BoundingRegions);
                var figureBlock = MapFigure(figure, filePath, pageNumber);
                elements.Add(new PositionedElement(offset, figureBlock, IsHeading: false));
            }
        }

        return elements;
    }

    private static TableBlock MapTable(DocumentTable table, string? filePath, int? pageNumber)
    {
        var headers = new List<string>();
        var rows = new List<IReadOnlyList<string>>();

        // Extract headers from cells with kind == ColumnHeader or from the first row
        var hasColumnHeaders = table.Cells.Any(c => c.Kind == DocumentTableCellKind.ColumnHeader);

        if (hasColumnHeaders)
        {
            var headerCells = table.Cells
                .Where(c => c.Kind == DocumentTableCellKind.ColumnHeader)
                .OrderBy(c => c.ColumnIndex)
                .ToList();

            headers.AddRange(headerCells.Select(c => c.Content));

            // Get max row index of header cells to know where data starts
            var dataStartRow = headerCells.Max(c => c.RowIndex) + 1;

            for (var row = dataStartRow; row < table.RowCount; row++)
            {
                var rowCells = new string[table.ColumnCount];
                foreach (var cell in table.Cells.Where(c => c.RowIndex == row))
                {
                    if (cell.ColumnIndex < table.ColumnCount)
                    {
                        rowCells[cell.ColumnIndex] = cell.Content;
                    }
                }
                rows.Add(rowCells.Select(c => c ?? string.Empty).ToArray());
            }
        }
        else
        {
            // Use first row as headers
            var firstRowCells = table.Cells
                .Where(c => c.RowIndex == 0)
                .OrderBy(c => c.ColumnIndex)
                .ToList();

            headers.AddRange(firstRowCells.Select(c => c.Content));

            for (var row = 1; row < table.RowCount; row++)
            {
                var rowCells = new string[table.ColumnCount];
                foreach (var cell in table.Cells.Where(c => c.RowIndex == row))
                {
                    if (cell.ColumnIndex < table.ColumnCount)
                    {
                        rowCells[cell.ColumnIndex] = cell.Content;
                    }
                }
                rows.Add(rowCells.Select(c => c ?? string.Empty).ToArray());
            }
        }

        var properties = new Dictionary<string, object>
        {
            ["RowCount"] = table.RowCount,
            ["ColumnCount"] = table.ColumnCount
        };

        if (table.BoundingRegions is { Count: > 0 })
        {
            properties["BoundingRegions"] = table.BoundingRegions
                .Select(r => new { r.PageNumber, r.Polygon })
                .ToArray();
        }

        return new TableBlock
        {
            Id = Guid.NewGuid().ToString(),
            Headers = headers.AsReadOnly(),
            Rows = rows.AsReadOnly(),
            Source = CreateSourceReference(filePath, pageNumber, table.Spans),
            Properties = properties
        };
    }

    private static FigureBlock MapFigure(DocumentFigure figure, string? filePath, int? pageNumber)
    {
        var properties = new Dictionary<string, object>();
        if (figure.BoundingRegions is { Count: > 0 })
        {
            properties["BoundingRegions"] = figure.BoundingRegions
                .Select(r => new { r.PageNumber, r.Polygon })
                .ToArray();
        }

        return new FigureBlock
        {
            Id = Guid.NewGuid().ToString(),
            Caption = figure.Caption?.Content,
            Source = CreateSourceReference(filePath, pageNumber, figure.Spans),
            Properties = properties
        };
    }

    private static List<CoreModelSection> BuildSections(List<PositionedElement> elements)
    {
        var sections = new List<CoreModelSection>();
        HeadingBlock? currentHeading = null;
        var currentBlocks = new List<DocumentBlock>();

        foreach (var element in elements)
        {
            if (element.IsHeading)
            {
                // Close the current section before starting a new one
                if (currentHeading is not null || currentBlocks.Count > 0)
                {
                    sections.Add(CreateSection(currentHeading, currentBlocks));
                    currentBlocks = [];
                }
                currentHeading = (HeadingBlock)element.Block;
            }
            else
            {
                currentBlocks.Add(element.Block);
            }
        }

        // Close the final section
        if (currentHeading is not null || currentBlocks.Count > 0)
        {
            sections.Add(CreateSection(currentHeading, currentBlocks));
        }

        // Ensure at least one empty section if the document has no content
        if (sections.Count == 0)
        {
            sections.Add(CreateSection(null, []));
        }

        return sections;
    }

    private static CoreModelSection CreateSection(HeadingBlock? heading, List<DocumentBlock> blocks)
    {
        return new CoreModelSection
        {
            Id = Guid.NewGuid().ToString(),
            Heading = heading,
            Blocks = blocks.AsReadOnly()
        };
    }

    private static DocumentMetadata BuildMetadata(AnalyzeResult result, string? filePath)
    {
        var pageCount = result.Pages?.Count;
        var sourceFormat = filePath is not null
            ? Path.GetExtension(filePath).TrimStart('.').ToUpperInvariant()
            : null;

        // Estimate word count from the full content
        var wordCount = !string.IsNullOrWhiteSpace(result.Content)
            ? result.Content.Split([' ', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries).Length
            : (int?)null;

        return new DocumentMetadata
        {
            SourceFormat = sourceFormat,
            PageCount = pageCount,
            WordCount = wordCount
        };
    }

    private static SourceReference CreateSourceReference(
        string? filePath,
        int? pageNumber,
        IReadOnlyList<DocumentSpan>? spans)
    {
        SpanReference? spanRef = null;
        if (spans is { Count: > 0 })
        {
            var firstSpan = spans[0];
            spanRef = new SpanReference
            {
                Offset = firstSpan.Offset,
                Length = firstSpan.Length
            };
        }

        return new SourceReference
        {
            FilePath = filePath,
            PageNumber = pageNumber,
            Span = spanRef
        };
    }

    private static int GetOffset(IReadOnlyList<DocumentSpan>? spans) =>
        spans is { Count: > 0 } ? spans[0].Offset : 0;

    private static int? GetPageNumber(IReadOnlyList<BoundingRegion>? regions) =>
        regions is { Count: > 0 } ? regions[0].PageNumber : null;

    private sealed record PositionedElement(int Offset, DocumentBlock Block, bool IsHeading);
}
