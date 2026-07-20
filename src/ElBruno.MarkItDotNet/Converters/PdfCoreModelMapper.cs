using ElBruno.MarkItDotNet.CoreModel;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace ElBruno.MarkItDotNet.Converters;

/// <summary>
/// Converts PDF files to the CoreModel <see cref="Document"/> representation using PdfPig.
/// Extracts words per page, groups into lines/paragraphs, detects headings by font size ratio.
/// </summary>
public class PdfCoreModelMapper : IStructuredConverter
{
    private const double LineYTolerance = 3.0;
    private const double ParagraphSpacingFactor = 1.5;
    private const double HeadingMinimumSizeRatio = 1.15;
    private const double HeadingMinimumBoldSizeRatio = 1.05;
    private const int MaxHeadingLevel = 3;

    /// <inheritdoc />
    public bool CanHandle(string filePath)
    {
        var ext = Path.GetExtension(filePath);
        return ext.Equals(".pdf", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public Task<Document> ConvertToDocumentAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        using var stream = File.OpenRead(filePath);
        return ConvertToDocumentAsync(stream, Path.GetFileName(filePath), cancellationToken);
    }

    /// <inheritdoc />
    public Task<Document> ConvertToDocumentAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        using var document = PdfDocument.Open(stream);
        var pageCount = document.NumberOfPages;
        var totalWordCount = 0;

        var sections = new List<DocumentSection>();
        DocumentSection? currentSection = null;
        var currentBlocks = new List<DocumentBlock>();
        HeadingBlock? currentHeading = null;

        for (var pageNum = 1; pageNum <= pageCount; pageNum++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var page = document.GetPage(pageNum);
            var words = page.GetWords().ToList();
            if (words.Count == 0)
                continue;

            var lines = GroupWordsIntoLines(words);
            if (lines.Count == 0)
                continue;

            var bodyFontSize = DetectBodyFontSize(lines);
            var paragraphs = GroupLinesIntoParagraphs(lines);
            var headingLevelMap = BuildHeadingLevelMap(paragraphs, bodyFontSize);

            foreach (var para in paragraphs)
            {
                if (string.IsNullOrWhiteSpace(para.Text))
                    continue;

                var wordCount = CountWords(para.Text);
                totalWordCount += wordCount;

                var source = new SourceReference
                {
                    PageNumber = pageNum
                };

                var roundedFontSize = Math.Round(para.FontSize, 1);
                if (headingLevelMap.TryGetValue(roundedFontSize, out var level))
                {
                    // Flush current section
                    if (currentHeading is not null || currentBlocks.Count > 0)
                    {
                        sections.Add(new DocumentSection
                        {
                            Id = Guid.NewGuid().ToString("N"),
                            Heading = currentHeading,
                            Blocks = currentBlocks.AsReadOnly(),
                            SubSections = Array.Empty<DocumentSection>()
                        });
                        currentBlocks = [];
                    }

                    currentHeading = new HeadingBlock
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        Text = para.Text,
                        Level = level,
                        Source = source
                    };
                }
                else
                {
                    currentBlocks.Add(new ParagraphBlock
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        Text = para.Text,
                        Source = source
                    });
                }
            }
        }

        // Flush final section
        if (currentHeading is not null || currentBlocks.Count > 0)
        {
            sections.Add(new DocumentSection
            {
                Id = Guid.NewGuid().ToString("N"),
                Heading = currentHeading,
                Blocks = currentBlocks.AsReadOnly(),
                SubSections = Array.Empty<DocumentSection>()
            });
        }

        var metadata = new DocumentMetadata
        {
            SourceFormat = ".pdf",
            PageCount = pageCount,
            WordCount = totalWordCount
        };

        var result = new Document
        {
            Id = Guid.NewGuid().ToString("N"),
            Sections = sections.AsReadOnly(),
            Metadata = metadata,
            Source = new SourceReference { FilePath = fileName }
        };

        return Task.FromResult(result);
    }

    private static List<TextLine> GroupWordsIntoLines(List<Word> words)
    {
        var sorted = words
            .OrderByDescending(w => w.BoundingBox.Bottom)
            .ThenBy(w => w.BoundingBox.Left)
            .ToList();

        var lines = new List<TextLine>();
        var currentLineWords = new List<Word>();
        var currentY = double.MaxValue;

        foreach (var word in sorted)
        {
            var wordY = word.BoundingBox.Bottom;

            if (currentLineWords.Count == 0 || Math.Abs(wordY - currentY) <= LineYTolerance)
            {
                currentLineWords.Add(word);
                currentY = currentLineWords.Count == 1
                    ? wordY
                    : currentLineWords.Average(w => w.BoundingBox.Bottom);
            }
            else
            {
                lines.Add(CreateTextLine(currentLineWords));
                currentLineWords = [word];
                currentY = wordY;
            }
        }

        if (currentLineWords.Count > 0)
            lines.Add(CreateTextLine(currentLineWords));

        return lines;
    }

    private static TextLine CreateTextLine(List<Word> words)
    {
        var sorted = words.OrderBy(w => w.BoundingBox.Left).ToList();
        var text = string.Join(" ", sorted.Select(w => w.Text));
        var letters = sorted.SelectMany(w => w.Letters).ToList();
        var avgFontSize = letters
            .Select(l => l.PointSize)
            .DefaultIfEmpty(0)
            .Average();
        var boldRatio = letters.Count == 0
            ? 0
            : letters.Count(l => IsBoldFontName(l.FontName)) / (double)letters.Count;
        var y = sorted.Average(w => w.BoundingBox.Bottom);
        var height = sorted.Max(w => w.BoundingBox.Height);

        return new TextLine(text, y, height, avgFontSize, boldRatio);
    }

    private static double DetectBodyFontSize(List<TextLine> lines)
    {
        var fontSizes = lines
            .Where(l => l.FontSize > 0)
            .GroupBy(l => Math.Round(l.FontSize, 1))
            .OrderByDescending(g => g.Sum(l => l.Text.Length))
            .ToList();

        return fontSizes.Count > 0 ? fontSizes[0].Key : 0;
    }

    private static List<ParagraphInfo> GroupLinesIntoParagraphs(List<TextLine> lines)
    {
        var paragraphs = new List<ParagraphInfo>();
        if (lines.Count == 0)
            return paragraphs;

        var currentLines = new List<TextLine> { lines[0] };

        for (var i = 1; i < lines.Count; i++)
        {
            var prevLine = lines[i - 1];
            var currentLine = lines[i];

            var spacing = prevLine.Y - currentLine.Y;
            var avgHeight = (prevLine.Height + currentLine.Height) / 2;
            var isNewParagraph = avgHeight > 0 && spacing > avgHeight * ParagraphSpacingFactor;
            var fontSizeChanged = Math.Abs(prevLine.FontSize - currentLine.FontSize) > 1.0;

            if (isNewParagraph || fontSizeChanged)
            {
                paragraphs.Add(CreateParagraph(currentLines));
                currentLines = [currentLine];
            }
            else
            {
                currentLines.Add(currentLine);
            }
        }

        if (currentLines.Count > 0)
            paragraphs.Add(CreateParagraph(currentLines));

        return paragraphs;
    }

    private static ParagraphInfo CreateParagraph(List<TextLine> lines)
    {
        var text = string.Join(" ", lines.Select(l => l.Text));
        var avgFontSize = lines.Average(l => l.FontSize);
        var avgBoldRatio = lines.Average(l => l.BoldRatio);

        return new ParagraphInfo(text, avgFontSize, avgBoldRatio, lines.Count);
    }

    private static Dictionary<double, int> BuildHeadingLevelMap(List<ParagraphInfo> paragraphs, double bodyFontSize)
    {
        if (bodyFontSize <= 0 || paragraphs.Count == 0)
            return [];

        var roundedBodyFontSize = Math.Round(bodyFontSize, 1);
        var headingSizes = paragraphs
            .Where(p => IsHeadingCandidate(p, bodyFontSize))
            .Select(p => Math.Round(p.FontSize, 1))
            .Where(size => size > roundedBodyFontSize)
            .Distinct()
            .OrderByDescending(size => size)
            .Take(MaxHeadingLevel)
            .ToList();

        if (headingSizes.Count == 0)
            return [];

        return headingSizes
            .Select((size, index) => new { Size = size, Level = index + 1 })
            .ToDictionary(x => x.Size, x => x.Level);
    }

    private static bool IsHeadingCandidate(ParagraphInfo paragraph, double bodyFontSize)
    {
        if (paragraph.LineCount > 3 || bodyFontSize <= 0 || paragraph.FontSize <= 0)
            return false;

        var isLargeBySize = paragraph.FontSize >= bodyFontSize * HeadingMinimumSizeRatio;
        var isBoldAndLarger = paragraph.BoldRatio >= 0.6 &&
                              paragraph.FontSize >= bodyFontSize * HeadingMinimumBoldSizeRatio;

        return isLargeBySize || isBoldAndLarger;
    }

    private static bool IsBoldFontName(string? fontName)
    {
        if (string.IsNullOrWhiteSpace(fontName))
            return false;

        var normalized = fontName.ToLowerInvariant();
        return normalized.Contains("bold", StringComparison.Ordinal) ||
               normalized.Contains("black", StringComparison.Ordinal) ||
               normalized.Contains("heavy", StringComparison.Ordinal) ||
               normalized.Contains("semibold", StringComparison.Ordinal) ||
               normalized.Contains("demi", StringComparison.Ordinal);
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

    private sealed record TextLine(string Text, double Y, double Height, double FontSize, double BoldRatio);
    private sealed record ParagraphInfo(string Text, double FontSize, double BoldRatio, int LineCount);
}
