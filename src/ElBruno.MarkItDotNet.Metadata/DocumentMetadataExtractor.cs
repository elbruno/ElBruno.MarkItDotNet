// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using System.Text.RegularExpressions;
using ElBruno.MarkItDotNet.CoreModel;

namespace ElBruno.MarkItDotNet.Metadata;

/// <summary>
/// Default implementation of <see cref="IMetadataExtractor"/> that extracts and normalizes
/// metadata from a <see cref="Document"/> using its structure, content, and existing metadata.
/// </summary>
public partial class DocumentMetadataExtractor : IMetadataExtractor
{
    /// <inheritdoc />
    public MetadataResult Extract(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var allBlocks = CollectAllBlocks(document.Sections).ToList();
        var headings = allBlocks.OfType<HeadingBlock>().ToList();
        var paragraphs = allBlocks.OfType<ParagraphBlock>().ToList();

        var title = ExtractTitle(document, headings);
        var author = document.Metadata.Author;
        var normalizedHeadings = NormalizeHeadings(headings);
        var wordCount = CountWords(paragraphs);
        var allText = string.Join(" ", paragraphs.Select(p => p.Text));
        var language = LanguageDetector.Detect(allText);
        var documentType = InferDocumentType(document, headings, paragraphs);
        var sectionCount = document.Sections.Count;

        return new MetadataResult
        {
            Title = title,
            Author = author,
            Language = language,
            CreatedAt = document.Metadata.CreatedAt,
            ModifiedAt = document.Metadata.ModifiedAt,
            DocumentType = documentType,
            HeadingCount = headings.Count,
            SectionCount = sectionCount,
            WordCount = wordCount,
            PageCount = document.Metadata.PageCount,
            NormalizedHeadings = normalizedHeadings,
            Tags = InferTags(documentType, document.Metadata.SourceFormat),
            Custom = new Dictionary<string, object>(
                document.Metadata.Custom.Select(kvp => KeyValuePair.Create(kvp.Key, kvp.Value))),
        };
    }

    private static string? ExtractTitle(Document document, List<HeadingBlock> headings)
    {
        // Priority 1: existing metadata title
        if (!string.IsNullOrWhiteSpace(document.Metadata.Title))
        {
            return document.Metadata.Title.Trim();
        }

        // Priority 2: first H1 heading
        var firstH1 = headings.FirstOrDefault(h => h.Level == 1);
        if (firstH1 is not null && !string.IsNullOrWhiteSpace(firstH1.Text))
        {
            return NormalizeText(firstH1.Text);
        }

        // Priority 3: source filename
        if (document.Source?.FilePath is { } filePath && !string.IsNullOrWhiteSpace(filePath))
        {
            return Path.GetFileNameWithoutExtension(filePath);
        }

        return null;
    }

    private static IReadOnlyList<NormalizedHeading> NormalizeHeadings(List<HeadingBlock> headings)
    {
        var result = new List<NormalizedHeading>(headings.Count);
        for (var i = 0; i < headings.Count; i++)
        {
            var heading = headings[i];
            var normalized = NormalizeText(heading.Text);
            result.Add(new NormalizedHeading(
                Id: $"heading-{i}",
                Text: normalized,
                Level: heading.Level,
                OriginalText: heading.Text));
        }

        return result;
    }

    private static int CountWords(List<ParagraphBlock> paragraphs)
    {
        var count = 0;
        foreach (var paragraph in paragraphs)
        {
            if (!string.IsNullOrWhiteSpace(paragraph.Text))
            {
                count += WordCountRegex().Split(paragraph.Text.Trim())
                    .Count(w => w.Length > 0);
            }
        }

        return count;
    }

    private static DocumentType InferDocumentType(
        Document document,
        List<HeadingBlock> headings,
        List<ParagraphBlock> paragraphs)
    {
        var format = document.Metadata.SourceFormat?.ToLowerInvariant();

        // Format-based inference
        if (format is not null)
        {
            if (format is "pptx" or "ppt" or "odp" or "presentation")
            {
                return DocumentType.Presentation;
            }

            if (format is "xlsx" or "xls" or "ods" or "csv" or "spreadsheet")
            {
                return DocumentType.Spreadsheet;
            }
        }

        // Content-based heuristics
        var headingTexts = headings
            .Select(h => h.Text.ToLowerInvariant())
            .ToList();

        var legalKeywords = new[] { "agreement", "contract", "terms", "clause", "whereas", "hereby" };
        if (headingTexts.Any(h => legalKeywords.Any(k => h.Contains(k, StringComparison.Ordinal))))
        {
            return DocumentType.Legal;
        }

        var manualKeywords = new[] { "installation", "configuration", "troubleshooting", "getting started", "user guide", "setup" };
        if (headingTexts.Any(h => manualKeywords.Any(k => h.Contains(k, StringComparison.Ordinal))))
        {
            return DocumentType.Manual;
        }

        // Check for report-like structure: multiple sections with numbered headings
        var hasNumberedHeadings = headings.Any(h => NumberedHeadingRegex().IsMatch(h.Text));
        if (hasNumberedHeadings && headings.Count >= 3)
        {
            return DocumentType.Report;
        }

        // Simple article heuristic: few headings, mostly paragraphs
        if (headings.Count <= 5 && paragraphs.Count > 0)
        {
            return DocumentType.Article;
        }

        return DocumentType.Unknown;
    }

    private static List<string> InferTags(DocumentType documentType, string? sourceFormat)
    {
        var tags = new List<string>();

        if (documentType != DocumentType.Unknown)
        {
            tags.Add(documentType.ToString().ToLowerInvariant());
        }

        if (!string.IsNullOrWhiteSpace(sourceFormat))
        {
            tags.Add($"format:{sourceFormat.ToLowerInvariant()}");
        }

        return tags;
    }

    private static IEnumerable<DocumentBlock> CollectAllBlocks(IReadOnlyList<DocumentSection> sections)
    {
        foreach (var section in sections)
        {
            if (section.Heading is not null)
            {
                yield return section.Heading;
            }

            foreach (var block in section.Blocks)
            {
                yield return block;
            }

            foreach (var block in CollectAllBlocks(section.SubSections))
            {
                yield return block;
            }
        }
    }

    private static string NormalizeText(string text)
    {
        var trimmed = text.Trim();
        return CollapseWhitespaceRegex().Replace(trimmed, " ");
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex CollapseWhitespaceRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WordCountRegex();

    [GeneratedRegex(@"^\d+[\.\)]\s")]
    private static partial Regex NumberedHeadingRegex();
}
