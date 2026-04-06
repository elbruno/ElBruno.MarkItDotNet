using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using ElBruno.MarkItDotNet.CoreModel;
using Drawing = DocumentFormat.OpenXml.Wordprocessing.Drawing;

namespace ElBruno.MarkItDotNet.Converters;

/// <summary>
/// Converts DOCX files to the CoreModel <see cref="Document"/> representation using OpenXml.
/// Parses headings, paragraphs, tables, lists, and images into structured blocks.
/// </summary>
public class DocxCoreModelMapper : IStructuredConverter
{
    /// <inheritdoc />
    public bool CanHandle(string filePath)
    {
        var ext = Path.GetExtension(filePath);
        return ext.Equals(".docx", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public Task<CoreModel.Document> ConvertToDocumentAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        using var stream = File.OpenRead(filePath);
        return ConvertToDocumentAsync(stream, Path.GetFileName(filePath), cancellationToken);
    }

    /// <inheritdoc />
    public Task<CoreModel.Document> ConvertToDocumentAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        using var doc = WordprocessingDocument.Open(stream, false);
        var body = doc.MainDocumentPart?.Document?.Body;
        if (body is null)
        {
            return Task.FromResult(CreateEmptyDocument(fileName));
        }

        var mainPart = doc.MainDocumentPart!;
        var orderedNumIds = DetectOrderedNumberings(mainPart);
        var totalWordCount = 0;

        var sections = new List<DocumentSection>();
        var currentBlocks = new List<DocumentBlock>();
        HeadingBlock? currentHeading = null;

        // Stack for heading hierarchy: (level, section-in-progress)
        var sectionStack = new Stack<(int Level, HeadingBlock? Heading, List<DocumentBlock> Blocks, List<DocumentSection> SubSections)>();

        foreach (var element in body.Elements())
        {
            cancellationToken.ThrowIfCancellationRequested();

            switch (element)
            {
                case Paragraph paragraph:
                {
                    var styleId = paragraph.ParagraphProperties?.ParagraphStyleId?.Val?.Value;
                    if (styleId is "FootnoteText" or "FootnoteReference")
                        continue;

                    var headingLevel = GetHeadingLevel(styleId);
                    var text = GetPlainText(paragraph);

                    if (headingLevel > 0 && !string.IsNullOrWhiteSpace(text))
                    {
                        totalWordCount += CountWords(text);

                        // Flush current section before starting new one
                        FlushCurrentSection(ref currentHeading, ref currentBlocks, sections, sectionStack, headingLevel);

                        currentHeading = new HeadingBlock
                        {
                            Id = Guid.NewGuid().ToString("N"),
                            Text = text,
                            Level = headingLevel
                        };
                        currentBlocks = [];
                    }
                    else
                    {
                        // Check for list items
                        var numProps = paragraph.ParagraphProperties?.NumberingProperties;
                        if (numProps is not null && !string.IsNullOrWhiteSpace(text))
                        {
                            totalWordCount += CountWords(text);
                            var numId = numProps.NumberingId?.Val?.Value ?? 0;
                            var isOrdered = orderedNumIds.Contains(numId);

                            // Try to merge with previous ListBlock
                            var listItem = new ListItemBlock
                            {
                                Id = Guid.NewGuid().ToString("N"),
                                Text = text,
                                SubItems = Array.Empty<ListItemBlock>()
                            };

                            var lastBlock = currentBlocks.Count > 0 ? currentBlocks[^1] : null;
                            if (lastBlock is ListBlock existingList && existingList.IsOrdered == isOrdered)
                            {
                                // Add to existing list
                                var items = existingList.Items.ToList();
                                items.Add(listItem);
                                currentBlocks[^1] = new ListBlock
                                {
                                    Id = existingList.Id,
                                    IsOrdered = isOrdered,
                                    Items = items.AsReadOnly()
                                };
                            }
                            else
                            {
                                currentBlocks.Add(new ListBlock
                                {
                                    Id = Guid.NewGuid().ToString("N"),
                                    IsOrdered = isOrdered,
                                    Items = new List<ListItemBlock> { listItem }.AsReadOnly()
                                });
                            }
                        }
                        else if (!string.IsNullOrWhiteSpace(text))
                        {
                            totalWordCount += CountWords(text);

                            // Check for images
                            if (paragraph.Descendants<Drawing>().Any())
                            {
                                currentBlocks.Add(new FigureBlock
                                {
                                    Id = Guid.NewGuid().ToString("N"),
                                    AltText = "image",
                                    Caption = null,
                                    ImagePath = "embedded-image"
                                });
                            }

                            currentBlocks.Add(new ParagraphBlock
                            {
                                Id = Guid.NewGuid().ToString("N"),
                                Text = text
                            });
                        }
                        else if (paragraph.Descendants<Drawing>().Any())
                        {
                            // Image-only paragraph
                            currentBlocks.Add(new FigureBlock
                            {
                                Id = Guid.NewGuid().ToString("N"),
                                AltText = "image",
                                Caption = null,
                                ImagePath = "embedded-image"
                            });
                        }
                    }
                    break;
                }

                case Table table:
                {
                    var (tableBlock, wordCount) = ConvertTable(table);
                    if (tableBlock is not null)
                    {
                        totalWordCount += wordCount;
                        currentBlocks.Add(tableBlock);
                    }
                    break;
                }
            }
        }

        // Flush remaining content
        FlushAllSections(ref currentHeading, ref currentBlocks, sections, sectionStack);

        var metadata = new DocumentMetadata
        {
            SourceFormat = ".docx",
            WordCount = totalWordCount
        };

        var result = new CoreModel.Document
        {
            Id = Guid.NewGuid().ToString("N"),
            Sections = sections.AsReadOnly(),
            Metadata = metadata,
            Source = new CoreModel.SourceReference { FilePath = fileName }
        };

        return Task.FromResult(result);
    }

    private static void FlushCurrentSection(
        ref HeadingBlock? currentHeading,
        ref List<DocumentBlock> currentBlocks,
        List<DocumentSection> sections,
        Stack<(int Level, HeadingBlock? Heading, List<DocumentBlock> Blocks, List<DocumentSection> SubSections)> sectionStack,
        int newHeadingLevel)
    {
        if (currentHeading is null && currentBlocks.Count == 0)
            return;

        var section = new DocumentSection
        {
            Id = Guid.NewGuid().ToString("N"),
            Heading = currentHeading,
            Blocks = currentBlocks.AsReadOnly(),
            SubSections = Array.Empty<DocumentSection>()
        };

        // Determine where to place this section based on heading hierarchy
        while (sectionStack.Count > 0 && sectionStack.Peek().Level >= (currentHeading?.Level ?? int.MaxValue))
        {
            var parent = sectionStack.Pop();
            var parentSubSections = parent.SubSections;
            parentSubSections.Add(section);
            section = new DocumentSection
            {
                Id = Guid.NewGuid().ToString("N"),
                Heading = parent.Heading,
                Blocks = parent.Blocks.AsReadOnly(),
                SubSections = parentSubSections.AsReadOnly()
            };
        }

        if (currentHeading is not null)
        {
            sectionStack.Push((currentHeading.Level, section.Heading, section.Blocks.ToList(), section.SubSections.ToList()));
        }
        else
        {
            sections.Add(section);
        }

        currentHeading = null;
        currentBlocks = [];
    }

    private static void FlushAllSections(
        ref HeadingBlock? currentHeading,
        ref List<DocumentBlock> currentBlocks,
        List<DocumentSection> sections,
        Stack<(int Level, HeadingBlock? Heading, List<DocumentBlock> Blocks, List<DocumentSection> SubSections)> sectionStack)
    {
        // Flush any remaining blocks into a section
        if (currentHeading is not null || currentBlocks.Count > 0)
        {
            var section = new DocumentSection
            {
                Id = Guid.NewGuid().ToString("N"),
                Heading = currentHeading,
                Blocks = currentBlocks.AsReadOnly(),
                SubSections = Array.Empty<DocumentSection>()
            };

            if (sectionStack.Count > 0)
            {
                var parent = sectionStack.Pop();
                parent.SubSections.Add(section);
                sectionStack.Push((parent.Level, parent.Heading, parent.Blocks, parent.SubSections));
            }
            else
            {
                sections.Add(section);
            }
        }

        // Collapse the stack
        while (sectionStack.Count > 0)
        {
            var current = sectionStack.Pop();
            var section = new DocumentSection
            {
                Id = Guid.NewGuid().ToString("N"),
                Heading = current.Heading,
                Blocks = current.Blocks.AsReadOnly(),
                SubSections = current.SubSections.AsReadOnly()
            };

            if (sectionStack.Count > 0)
            {
                var parent = sectionStack.Pop();
                parent.SubSections.Add(section);
                sectionStack.Push((parent.Level, parent.Heading, parent.Blocks, parent.SubSections));
            }
            else
            {
                sections.Add(section);
            }
        }
    }

    private static (TableBlock? Block, int WordCount) ConvertTable(Table table)
    {
        var rows = table.Elements<TableRow>().ToList();
        if (rows.Count == 0)
            return (null, 0);

        var wordCount = 0;
        var headerCells = rows[0].Elements<TableCell>().ToList();
        var headers = new List<string>();
        foreach (var cell in headerCells)
        {
            var text = GetCellText(cell);
            headers.Add(text);
            wordCount += CountWords(text);
        }

        var dataRows = new List<IReadOnlyList<string>>();
        for (var i = 1; i < rows.Count; i++)
        {
            var cells = rows[i].Elements<TableCell>().ToList();
            var row = new List<string>();
            foreach (var cell in cells)
            {
                var text = GetCellText(cell);
                row.Add(text);
                wordCount += CountWords(text);
            }
            dataRows.Add(row.AsReadOnly());
        }

        var block = new TableBlock
        {
            Id = Guid.NewGuid().ToString("N"),
            Headers = headers.AsReadOnly(),
            Rows = dataRows.AsReadOnly()
        };

        return (block, wordCount);
    }

    private static string GetPlainText(Paragraph paragraph)
    {
        return string.Concat(
            paragraph.Elements<Run>()
                .SelectMany(r => r.Elements<Text>())
                .Select(t => t.Text));
    }

    private static string GetCellText(TableCell cell)
    {
        return string.Join(" ", cell.Elements<Paragraph>()
            .Select(p => string.Concat(p.Elements<Run>()
                .SelectMany(r => r.Elements<Text>())
                .Select(t => t.Text))))
            .Trim();
    }

    private static int GetHeadingLevel(string? styleId)
    {
        if (string.IsNullOrEmpty(styleId))
            return 0;

        if (styleId.StartsWith("Heading", StringComparison.OrdinalIgnoreCase) &&
            int.TryParse(styleId.AsSpan(7), out var level) &&
            level is >= 1 and <= 6)
        {
            return level;
        }

        return 0;
    }

    private static HashSet<int> DetectOrderedNumberings(MainDocumentPart mainPart)
    {
        var orderedIds = new HashSet<int>();
        var numberingPart = mainPart.NumberingDefinitionsPart;
        if (numberingPart?.Numbering is null)
            return orderedIds;

        var abstractNums = numberingPart.Numbering.Elements<AbstractNum>().ToList();
        var numberingInstances = numberingPart.Numbering.Elements<NumberingInstance>().ToList();

        foreach (var numInstance in numberingInstances)
        {
            var numId = numInstance.NumberID?.Value ?? 0;
            var abstractNumId = numInstance.AbstractNumId?.Val?.Value ?? -1;
            var abstractNum = abstractNums.FirstOrDefault(a => a.AbstractNumberId?.Value == abstractNumId);
            if (abstractNum is null) continue;

            var firstLevel = abstractNum.Elements<Level>().FirstOrDefault(l => l.LevelIndex?.Value == 0);
            var format = firstLevel?.NumberingFormat?.Val?.Value;
            if (format == NumberFormatValues.Decimal ||
                format == NumberFormatValues.UpperLetter ||
                format == NumberFormatValues.LowerLetter ||
                format == NumberFormatValues.UpperRoman ||
                format == NumberFormatValues.LowerRoman)
            {
                orderedIds.Add(numId);
            }
        }

        return orderedIds;
    }

    private static CoreModel.Document CreateEmptyDocument(string fileName)
    {
        return new CoreModel.Document
        {
            Id = Guid.NewGuid().ToString("N"),
            Sections = Array.Empty<DocumentSection>(),
            Metadata = new DocumentMetadata { SourceFormat = ".docx" },
            Source = new CoreModel.SourceReference { FilePath = fileName }
        };
    }

    private static int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        var count = 0;
        var inWord = false;
        foreach (var ch in text)
        {
            if (char.IsWhiteSpace(ch))
                inWord = false;
            else if (!inWord)
            {
                inWord = true;
                count++;
            }
        }
        return count;
    }
}
