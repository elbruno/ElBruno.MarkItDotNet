using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Drawing = DocumentFormat.OpenXml.Wordprocessing.Drawing;

namespace ElBruno.MarkItDotNet.Converters;

/// <summary>
/// Converts Word documents (.docx) to Markdown using DocumentFormat.OpenXml.
/// Extracts headings, paragraphs, bold/italic formatting, lists, tables,
/// hyperlinks, images, nested lists, and footnotes.
/// </summary>
public class DocxConverter : IMarkdownConverter
{
    /// <inheritdoc />
    public bool CanHandle(string fileExtension) =>
        fileExtension.Equals(".docx", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public Task<string> ConvertAsync(Stream fileStream, string fileExtension, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        using var doc = WordprocessingDocument.Open(fileStream, false);
        var body = doc.MainDocumentPart?.Document?.Body;
        if (body is null)
        {
            return Task.FromResult(string.Empty);
        }

        var mainPart = doc.MainDocumentPart!;
        var orderedNumIds = DetectOrderedNumberings(mainPart);
        var footnotes = ExtractFootnotes(mainPart);
        var footnoteIndex = new Dictionary<long, int>();
        var footnoteCounter = 0;

        var sb = new StringBuilder();

        foreach (var element in body.Elements())
        {
            switch (element)
            {
                case Paragraph paragraph:
                    ProcessParagraph(paragraph, mainPart, orderedNumIds, footnotes, footnoteIndex, ref footnoteCounter, sb);
                    break;
                case Table table:
                    ProcessTable(table, sb);
                    break;
            }
        }

        // Append footnotes at the end
        if (footnoteIndex.Count > 0)
        {
            sb.AppendLine();
            foreach (var kvp in footnoteIndex.OrderBy(k => k.Value))
            {
                if (footnotes.TryGetValue(kvp.Key, out var text))
                {
                    sb.AppendLine($"[^{kvp.Value}]: {text}");
                }
            }
        }

        return Task.FromResult(sb.ToString().TrimEnd());
    }

    private static void ProcessParagraph(
        Paragraph paragraph,
        MainDocumentPart mainPart,
        HashSet<int> orderedNumIds,
        Dictionary<long, string> footnotes,
        Dictionary<long, int> footnoteIndex,
        ref int footnoteCounter,
        StringBuilder sb)
    {
        var styleId = paragraph.ParagraphProperties?.ParagraphStyleId?.Val?.Value;

        // Skip footnote separator styles
        if (styleId is "FootnoteText" or "FootnoteReference")
            return;

        var headingLevel = GetHeadingLevel(styleId);

        var formattedText = GetFormattedText(paragraph, mainPart, footnotes, footnoteIndex, ref footnoteCounter);
        if (string.IsNullOrWhiteSpace(formattedText) && headingLevel == 0)
        {
            sb.AppendLine();
            return;
        }

        if (headingLevel > 0)
        {
            sb.Append(new string('#', headingLevel));
            sb.Append(' ');
        }

        // Check for list items
        var numProps = paragraph.ParagraphProperties?.NumberingProperties;
        if (numProps is not null)
        {
            var level = numProps.NumberingLevelReference?.Val?.Value ?? 0;
            var indent = new string(' ', level * 2);
            sb.Append(indent);

            var numId = numProps.NumberingId?.Val?.Value ?? 0;
            if (orderedNumIds.Contains(numId))
            {
                sb.Append("1. ");
            }
            else
            {
                sb.Append("- ");
            }
        }

        sb.AppendLine(formattedText);
        sb.AppendLine();
    }

    private static string GetFormattedText(
        Paragraph paragraph,
        MainDocumentPart mainPart,
        Dictionary<long, string> footnotes,
        Dictionary<long, int> footnoteIndex,
        ref int footnoteCounter)
    {
        var sb = new StringBuilder();

        // Handle HYPERLINK field codes (used by some Word versions instead of Hyperlink elements)
        if (TryProcessFieldCodeHyperlink(paragraph, out var fieldLinkMarkdown))
        {
            return fieldLinkMarkdown!;
        }

        foreach (var child in paragraph.ChildElements)
        {
            switch (child)
            {
                case Hyperlink hyperlink:
                    ProcessHyperlink(hyperlink, mainPart, sb);
                    break;

                case Run run:
                    ProcessRun(run, footnotes, footnoteIndex, ref footnoteCounter, sb);
                    break;
            }
        }

        return sb.ToString();
    }

    private static void ProcessHyperlink(Hyperlink hyperlink, MainDocumentPart mainPart, StringBuilder sb)
    {
        var linkText = new StringBuilder();
        foreach (var run in hyperlink.Elements<Run>())
        {
            linkText.Append(string.Concat(run.Elements<Text>().Select(t => t.Text)));
        }

        var displayText = linkText.ToString();
        if (string.IsNullOrEmpty(displayText))
            return;

        var relationshipId = hyperlink.Id?.Value;
        if (relationshipId is not null)
        {
            var rel = mainPart.HyperlinkRelationships
                .FirstOrDefault(r => r.Id == relationshipId);
            if (rel is not null)
            {
                sb.Append($"[{displayText}]({EscapeUrlParentheses(rel.Uri.ToString())})");
                return;
            }
        }

        sb.Append(displayText);
    }

    private static bool TryProcessFieldCodeHyperlink(Paragraph paragraph, out string? markdown)
    {
        markdown = null;

        var fieldCode = paragraph.Descendants<FieldCode>().FirstOrDefault();
        if (fieldCode is null)
            return false;

        var codeText = fieldCode.Text.Trim();
        if (!codeText.StartsWith("HYPERLINK", StringComparison.OrdinalIgnoreCase))
            return false;

        // Extract URL from HYPERLINK field code: HYPERLINK "url" or HYPERLINK url
        var rest = codeText["HYPERLINK".Length..].Trim();
        var url = rest.StartsWith("\"")
            ? rest.Split('"').ElementAtOrDefault(1)
            : rest.Split(' ', 2)[0];
        if (string.IsNullOrEmpty(url))
            return false;

        // Extract display text from runs between "separate" and "end" field chars
        var displayText = new StringBuilder();
        var capture = false;
        foreach (var child in paragraph.ChildElements)
        {
            if (child is not Run run) continue;

            var fieldChar = run.Elements<FieldChar>().FirstOrDefault();
            if (fieldChar?.FieldCharType?.Value == FieldCharValues.Separate)
            {
                capture = true;
                continue;
            }
            if (fieldChar?.FieldCharType?.Value == FieldCharValues.End)
            {
                capture = false;
                break;
            }
            if (capture)
            {
                foreach (var txt in run.Elements<Text>())
                    displayText.Append(txt.Text);
            }
        }

        if (displayText.Length == 0)
            return false;

        markdown = $"[{displayText}]({EscapeUrlParentheses(url)})";
        return true;
    }

    private static string EscapeUrlParentheses(string url)
    {
        return url.Replace("(", "%28").Replace(")", "%29");
    }

    private static void ProcessRun(
        Run run,
        Dictionary<long, string> footnotes,
        Dictionary<long, int> footnoteIndex,
        ref int footnoteCounter,
        StringBuilder sb)
    {
        // Check for images (Drawing elements)
        if (run.Descendants<Drawing>().Any())
        {
            sb.Append("![image](embedded-image)");
            return;
        }

        // Check for footnote references
        var footnoteRef = run.Descendants<FootnoteReference>().FirstOrDefault();
        if (footnoteRef?.Id?.Value is not null)
        {
            var fnId = footnoteRef.Id.Value;
            if (footnotes.ContainsKey(fnId) && !footnoteIndex.ContainsKey(fnId))
            {
                footnoteIndex[fnId] = ++footnoteCounter;
            }

            if (footnoteIndex.TryGetValue(fnId, out var idx))
            {
                sb.Append($"[^{idx}]");
            }

            return;
        }

        var runText = string.Concat(run.Elements<Text>().Select(t => t.Text));
        if (string.IsNullOrEmpty(runText))
            return;

        var props = run.RunProperties;
        var isBold = props?.Bold is not null || props?.Bold?.Val?.Value == true;
        var isItalic = props?.Italic is not null || props?.Italic?.Val?.Value == true;

        if (isBold && isItalic)
            sb.Append($"***{runText}***");
        else if (isBold)
            sb.Append($"**{runText}**");
        else if (isItalic)
            sb.Append($"*{runText}*");
        else
            sb.Append(runText);
    }

    private static int GetHeadingLevel(string? styleId)
    {
        if (string.IsNullOrEmpty(styleId))
            return 0;

        // Word heading styles: Heading1, Heading2, ... or heading 1, heading 2, ...
        if (styleId.StartsWith("Heading", StringComparison.OrdinalIgnoreCase) &&
            int.TryParse(styleId.AsSpan(7), out var level) &&
            level is >= 1 and <= 6)
        {
            return level;
        }

        return 0;
    }

    /// <summary>
    /// Detects which numbering IDs correspond to ordered (numbered) lists.
    /// </summary>
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

            // Check the first level's format
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

    /// <summary>
    /// Extracts footnotes from the document's FootnotesPart.
    /// </summary>
    private static Dictionary<long, string> ExtractFootnotes(MainDocumentPart mainPart)
    {
        var result = new Dictionary<long, string>();
        var footnotesPart = mainPart.FootnotesPart;
        if (footnotesPart?.Footnotes is null)
            return result;

        foreach (var footnote in footnotesPart.Footnotes.Elements<Footnote>())
        {
            var id = footnote.Id?.Value;
            // Skip separator/continuation footnotes (ids 0 and -1)
            if (id is null or 0 or -1)
                continue;

            var text = string.Join(" ", footnote.Elements<Paragraph>()
                .Select(p => string.Concat(p.Elements<Run>()
                    .SelectMany(r => r.Elements<Text>())
                    .Select(t => t.Text))))
                .Trim();

            if (!string.IsNullOrEmpty(text))
            {
                result[id.Value] = text;
            }
        }

        return result;
    }

    private static void ProcessTable(Table table, StringBuilder sb)
    {
        var rows = table.Elements<TableRow>().ToList();
        if (rows.Count == 0)
            return;

        // First row as header
        var headerCells = rows[0].Elements<TableCell>().ToList();
        sb.Append('|');
        foreach (var cell in headerCells)
        {
            var text = GetCellText(cell);
            sb.Append($" {text} |");
        }
        sb.AppendLine();

        // Separator
        sb.Append('|');
        foreach (var _ in headerCells)
        {
            sb.Append(" --- |");
        }
        sb.AppendLine();

        // Data rows
        for (var i = 1; i < rows.Count; i++)
        {
            var cells = rows[i].Elements<TableCell>().ToList();
            sb.Append('|');
            foreach (var cell in cells)
            {
                var text = GetCellText(cell);
                sb.Append($" {text} |");
            }
            sb.AppendLine();
        }

        sb.AppendLine();
    }

    private static string GetCellText(TableCell cell)
    {
        return string.Join(" ", cell.Elements<Paragraph>()
            .Select(p => string.Concat(p.Elements<Run>()
                .SelectMany(r => r.Elements<Text>())
                .Select(t => t.Text))))
            .Trim();
    }
}
