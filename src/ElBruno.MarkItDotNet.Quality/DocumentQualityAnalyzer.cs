// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using System.Text.RegularExpressions;
using ElBruno.MarkItDotNet.CoreModel;

namespace ElBruno.MarkItDotNet.Quality;

/// <summary>
/// Default implementation of <see cref="IQualityAnalyzer"/> that performs heuristic-based
/// quality analysis on a <see cref="Document"/>.
/// </summary>
public partial class DocumentQualityAnalyzer : IQualityAnalyzer
{
    private static readonly HashSet<char> Vowels = ['a', 'e', 'i', 'o', 'u', 'A', 'E', 'I', 'O', 'U'];

    private readonly QualityAnalyzerOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentQualityAnalyzer"/> class
    /// with the specified options.
    /// </summary>
    /// <param name="options">Configuration options; if null, defaults are used.</param>
    public DocumentQualityAnalyzer(QualityAnalyzerOptions? options = null)
    {
        _options = options ?? new QualityAnalyzerOptions();
    }

    /// <inheritdoc />
    public QualityReport Analyze(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var issues = new List<QualityIssue>();
        var allBlocks = CollectAllBlocks(document);
        var paragraphs = allBlocks.OfType<ParagraphBlock>().ToList();
        var headings = CollectAllHeadings(document);
        var tables = allBlocks.OfType<TableBlock>().ToList();

        double textDensity = ComputeTextDensity(allBlocks, paragraphs, issues);
        double emptyBlockRatio = ComputeEmptyBlockRatio(paragraphs, issues);
        double duplicateLineRatio = ComputeDuplicateLineRatio(paragraphs, issues);
        double ocrSuspicionScore = ComputeOcrSuspicionScore(paragraphs, issues);
        int tableWarningCount = ComputeTableWarnings(tables, issues);
        double readingOrderScore = ComputeReadingOrderScore(headings, issues);
        double headingConsistencyScore = ComputeHeadingConsistencyScore(headings, issues);
        double averageBlockLength = ComputeAverageBlockLength(allBlocks);

        var metrics = new QualityMetrics(
            TextDensity: textDensity,
            AverageBlockLength: averageBlockLength,
            EmptyBlockRatio: emptyBlockRatio,
            DuplicateLineRatio: duplicateLineRatio,
            TableWarningCount: tableWarningCount,
            OcrSuspicionScore: ocrSuspicionScore,
            ReadingOrderScore: readingOrderScore,
            HeadingConsistencyScore: headingConsistencyScore);

        double overallScore = ComputeOverallScore(metrics);
        var suggestedAction = DetermineAction(overallScore);

        return new QualityReport(overallScore, issues, metrics, suggestedAction);
    }

    private double ComputeOverallScore(QualityMetrics metrics)
    {
        // Each metric contributes to the overall score.
        // TextDensity, ReadingOrderScore, HeadingConsistencyScore are "higher is better".
        // OcrSuspicionScore, EmptyBlockRatio, DuplicateLineRatio are "lower is better" (invert).
        double score =
            (_options.TextDensityWeight * metrics.TextDensity) +
            (_options.OcrSuspicionWeight * (1.0 - metrics.OcrSuspicionScore)) +
            (_options.DuplicateLineWeight * (1.0 - metrics.DuplicateLineRatio)) +
            (_options.EmptyBlockWeight * (1.0 - metrics.EmptyBlockRatio)) +
            (_options.ReadingOrderWeight * metrics.ReadingOrderScore) +
            (_options.HeadingConsistencyWeight * metrics.HeadingConsistencyScore);

        return Math.Clamp(score, 0.0, 1.0);
    }

    private QualityAction DetermineAction(double overallScore)
    {
        if (overallScore <= _options.RejectThreshold)
        {
            return QualityAction.Reject;
        }

        if (overallScore <= _options.FallbackToDocumentIntelligenceThreshold)
        {
            return QualityAction.FallbackToDocumentIntelligence;
        }

        if (overallScore <= _options.FallbackToOcrThreshold)
        {
            return QualityAction.FallbackToOcr;
        }

        if (overallScore <= _options.ReviewThreshold)
        {
            return QualityAction.Review;
        }

        return QualityAction.None;
    }

    #region Metric Computations

    private double ComputeTextDensity(
        List<DocumentBlock> allBlocks,
        List<ParagraphBlock> paragraphs,
        List<QualityIssue> issues)
    {
        if (allBlocks.Count == 0)
        {
            issues.Add(new QualityIssue("NO_BLOCKS", QualitySeverity.Warning, "Document contains no blocks."));
            return 0.0;
        }

        int textRichCount = paragraphs.Count(p =>
            p.Text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length >= _options.MinWordsForTextRich);

        double density = (double)textRichCount / allBlocks.Count;

        if (density < 0.3)
        {
            issues.Add(new QualityIssue(
                "LOW_TEXT_DENSITY",
                QualitySeverity.Warning,
                $"Text density is low ({density:P0}). Only {textRichCount}/{allBlocks.Count} blocks have ≥{_options.MinWordsForTextRich} words."));
        }

        return density;
    }

    private double ComputeEmptyBlockRatio(List<ParagraphBlock> paragraphs, List<QualityIssue> issues)
    {
        if (paragraphs.Count == 0)
        {
            return 0.0;
        }

        var emptyBlocks = paragraphs.Where(p => string.IsNullOrWhiteSpace(p.Text)).ToList();
        double ratio = (double)emptyBlocks.Count / paragraphs.Count;

        foreach (var block in emptyBlocks)
        {
            issues.Add(new QualityIssue(
                "EMPTY_BLOCK",
                QualitySeverity.Info,
                "Paragraph block contains empty or whitespace-only text.",
                BlockId: block.Id));
        }

        return ratio;
    }

    private static double ComputeDuplicateLineRatio(List<ParagraphBlock> paragraphs, List<QualityIssue> issues)
    {
        if (paragraphs.Count <= 1)
        {
            return 0.0;
        }

        var nonEmpty = paragraphs.Where(p => !string.IsNullOrWhiteSpace(p.Text)).ToList();
        if (nonEmpty.Count <= 1)
        {
            return 0.0;
        }

        var textGroups = nonEmpty.GroupBy(p => p.Text.Trim()).Where(g => g.Count() > 1).ToList();
        int duplicateCount = textGroups.Sum(g => g.Count() - 1);

        foreach (var group in textGroups)
        {
            foreach (var block in group.Skip(1))
            {
                issues.Add(new QualityIssue(
                    "DUPLICATE_LINE",
                    QualitySeverity.Warning,
                    $"Duplicate paragraph text: \"{Truncate(block.Text, 60)}\"",
                    BlockId: block.Id));
            }
        }

        return (double)duplicateCount / nonEmpty.Count;
    }

    private double ComputeOcrSuspicionScore(List<ParagraphBlock> paragraphs, List<QualityIssue> issues)
    {
        if (paragraphs.Count == 0)
        {
            return 0.0;
        }

        int suspiciousCount = 0;
        var nonEmpty = paragraphs.Where(p => !string.IsNullOrWhiteSpace(p.Text)).ToList();

        if (nonEmpty.Count == 0)
        {
            return 0.0;
        }

        foreach (var paragraph in nonEmpty)
        {
            if (IsGarbledText(paragraph.Text))
            {
                suspiciousCount++;
                issues.Add(new QualityIssue(
                    "OCR_GARBLED",
                    QualitySeverity.Error,
                    $"Text appears garbled (possible OCR artifact): \"{Truncate(paragraph.Text, 60)}\"",
                    BlockId: paragraph.Id));
            }
        }

        return (double)suspiciousCount / nonEmpty.Count;
    }

    private static int ComputeTableWarnings(List<TableBlock> tables, List<QualityIssue> issues)
    {
        int warningCount = 0;

        foreach (var table in tables)
        {
            // Single-column table
            if (table.Headers.Count <= 1 && table.Rows.All(r => r.Count <= 1))
            {
                issues.Add(new QualityIssue(
                    "TABLE_SINGLE_COLUMN",
                    QualitySeverity.Warning,
                    "Table has only a single column, which may indicate a parsing issue.",
                    BlockId: table.Id));
                warningCount++;
            }

            // All-empty cells
            bool allEmpty = table.Headers.All(string.IsNullOrWhiteSpace) &&
                            table.Rows.All(r => r.All(string.IsNullOrWhiteSpace));
            if (allEmpty)
            {
                issues.Add(new QualityIssue(
                    "TABLE_ALL_EMPTY",
                    QualitySeverity.Error,
                    "Table contains only empty cells.",
                    BlockId: table.Id));
                warningCount++;
            }

            // Mismatched row lengths
            int expectedColumns = table.Headers.Count;
            foreach (var row in table.Rows)
            {
                if (row.Count != expectedColumns)
                {
                    issues.Add(new QualityIssue(
                        "TABLE_ROW_MISMATCH",
                        QualitySeverity.Warning,
                        $"Table row has {row.Count} cells but header has {expectedColumns} columns.",
                        BlockId: table.Id));
                    warningCount++;
                    break; // Report once per table
                }
            }
        }

        return warningCount;
    }

    private static double ComputeReadingOrderScore(List<HeadingBlock> headings, List<QualityIssue> issues)
    {
        if (headings.Count <= 1)
        {
            return 1.0;
        }

        int violations = 0;
        for (int i = 1; i < headings.Count; i++)
        {
            int jump = headings[i].Level - headings[i - 1].Level;
            // A jump of more than 1 level down (e.g., H1→H4) is a violation
            if (jump > 1)
            {
                violations++;
                issues.Add(new QualityIssue(
                    "HEADING_ORDER_SKIP",
                    QualitySeverity.Warning,
                    $"Heading level jumps from H{headings[i - 1].Level} to H{headings[i].Level} (skipped {jump - 1} level(s)).",
                    BlockId: headings[i].Id));
            }
        }

        return 1.0 - Math.Min(1.0, (double)violations / (headings.Count - 1));
    }

    private static double ComputeHeadingConsistencyScore(List<HeadingBlock> headings, List<QualityIssue> issues)
    {
        if (headings.Count <= 1)
        {
            return 1.0;
        }

        // Group headings by level and check capitalization consistency within each level
        var levelGroups = headings
            .Where(h => !string.IsNullOrWhiteSpace(h.Text))
            .GroupBy(h => h.Level)
            .Where(g => g.Count() > 1)
            .ToList();

        if (levelGroups.Count == 0)
        {
            return 1.0;
        }

        int inconsistentLevels = 0;
        foreach (var group in levelGroups)
        {
            var patterns = group.Select(h => GetCapitalizationPattern(h.Text)).Distinct().ToList();
            if (patterns.Count > 1)
            {
                inconsistentLevels++;
                issues.Add(new QualityIssue(
                    "HEADING_INCONSISTENT_CAPS",
                    QualitySeverity.Info,
                    $"H{group.Key} headings have inconsistent capitalization patterns."));
            }
        }

        return 1.0 - ((double)inconsistentLevels / levelGroups.Count);
    }

    private static double ComputeAverageBlockLength(List<DocumentBlock> allBlocks)
    {
        if (allBlocks.Count == 0)
        {
            return 0.0;
        }

        double totalLength = allBlocks.Sum(b => GetBlockTextLength(b));
        return totalLength / allBlocks.Count;
    }

    #endregion

    #region Helpers

    private static List<DocumentBlock> CollectAllBlocks(Document document)
    {
        var blocks = new List<DocumentBlock>();
        foreach (var section in document.Sections)
        {
            CollectBlocksFromSection(section, blocks);
        }

        return blocks;
    }

    private static void CollectBlocksFromSection(DocumentSection section, List<DocumentBlock> blocks)
    {
        if (section.Heading is not null)
        {
            blocks.Add(section.Heading);
        }

        foreach (var block in section.Blocks)
        {
            blocks.Add(block);
        }

        foreach (var sub in section.SubSections)
        {
            CollectBlocksFromSection(sub, blocks);
        }
    }

    private static List<HeadingBlock> CollectAllHeadings(Document document)
    {
        var headings = new List<HeadingBlock>();
        foreach (var section in document.Sections)
        {
            CollectHeadingsFromSection(section, headings);
        }

        return headings;
    }

    private static void CollectHeadingsFromSection(DocumentSection section, List<HeadingBlock> headings)
    {
        if (section.Heading is not null)
        {
            headings.Add(section.Heading);
        }

        foreach (var sub in section.SubSections)
        {
            CollectHeadingsFromSection(sub, headings);
        }
    }

    private bool IsGarbledText(string text)
    {
        var words = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
        {
            return false;
        }

        int garbledWords = 0;
        foreach (var word in words)
        {
            if (word.Length < 2)
            {
                continue;
            }

            // Check for excessive special characters
            int specialCount = word.Count(c => !char.IsLetterOrDigit(c) && c != '-' && c != '\'');
            if ((double)specialCount / word.Length >= _options.SpecialCharRatioThreshold)
            {
                garbledWords++;
                continue;
            }

            // Check for no vowels (only in alphabetic words of sufficient length)
            if (word.Length >= 4 && word.All(char.IsLetter) && !word.Any(c => Vowels.Contains(c)))
            {
                garbledWords++;
            }
        }

        // Consider it garbled if more than 40% of the words are suspicious
        return words.Length > 0 && (double)garbledWords / words.Length > 0.4;
    }

    private static string GetCapitalizationPattern(string text)
    {
        string trimmed = text.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return "empty";
        }

        // Check if all words start with uppercase (Title Case)
        var words = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        bool allTitleCase = words.All(w => w.Length == 0 || char.IsUpper(w[0]));
        bool allUpperCase = trimmed.Equals(trimmed.ToUpperInvariant(), StringComparison.Ordinal);
        bool allLowerCase = trimmed.Equals(trimmed.ToLowerInvariant(), StringComparison.Ordinal);

        if (allUpperCase)
        {
            return "UPPER";
        }

        if (allLowerCase)
        {
            return "lower";
        }

        if (allTitleCase)
        {
            return "Title";
        }

        return "Mixed";
    }

    private static int GetBlockTextLength(DocumentBlock block) => block switch
    {
        ParagraphBlock p => p.Text.Length,
        HeadingBlock h => h.Text.Length,
        ListItemBlock li => li.Text.Length,
        TableBlock t => t.Headers.Sum(h => h.Length) + t.Rows.Sum(r => r.Sum(c => c.Length)),
        _ => 0,
    };

    private static string Truncate(string text, int maxLength)
    {
        if (text.Length <= maxLength)
        {
            return text;
        }

        return string.Concat(text.AsSpan(0, maxLength - 3), "...");
    }

    #endregion
}
