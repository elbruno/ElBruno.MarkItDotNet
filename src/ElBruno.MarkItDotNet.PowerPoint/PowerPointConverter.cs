using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using A = DocumentFormat.OpenXml.Drawing;

namespace ElBruno.MarkItDotNet.PowerPoint;

/// <summary>
/// Converts PowerPoint (.pptx) files to Markdown using the Open XML SDK.
/// </summary>
public class PowerPointConverter : IMarkdownConverter
{
    /// <inheritdoc />
    public bool CanHandle(string fileExtension) =>
        string.Equals(fileExtension, ".pptx", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public Task<string> ConvertAsync(Stream fileStream, string fileExtension, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        using var presentationDocument = PresentationDocument.Open(fileStream, false);
        var presentationPart = presentationDocument.PresentationPart;
        if (presentationPart is null)
        {
            return Task.FromResult(string.Empty);
        }

        var slideIdList = presentationPart.Presentation?.SlideIdList;
        if (slideIdList is null)
        {
            return Task.FromResult(string.Empty);
        }

        var sb = new StringBuilder();
        var slideNumber = 1;

        foreach (var slideId in slideIdList.Elements<SlideId>())
        {
            cancellationToken.ThrowIfCancellationRequested();

            var relationshipId = slideId.RelationshipId?.Value;
            if (relationshipId is null)
            {
                continue;
            }

            var slidePart = (SlidePart)presentationPart.GetPartById(relationshipId);

            sb.AppendLine($"## Slide {slideNumber}");
            sb.AppendLine();

            // Extract text from slide shapes
            var slideText = ExtractTextFromSlide(slidePart);
            if (!string.IsNullOrWhiteSpace(slideText))
            {
                sb.AppendLine(slideText);
                sb.AppendLine();
            }

            // Extract speaker notes
            var notes = ExtractSpeakerNotes(slidePart);
            if (!string.IsNullOrWhiteSpace(notes))
            {
                sb.AppendLine($"> **Notes:** {notes}");
                sb.AppendLine();
            }

            slideNumber++;
        }

        return Task.FromResult(sb.ToString().TrimEnd() + Environment.NewLine);
    }

    private static string ExtractTextFromSlide(SlidePart slidePart)
    {
        if (slidePart.Slide is null)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        var shapes = slidePart.Slide.Descendants<Shape>();

        foreach (var shape in shapes)
        {
            var textBody = shape.TextBody;
            if (textBody is null)
            {
                continue;
            }

            foreach (var paragraph in textBody.Elements<A.Paragraph>())
            {
                var paragraphText = ExtractParagraphText(paragraph);
                if (!string.IsNullOrEmpty(paragraphText))
                {
                    sb.AppendLine(paragraphText);
                }
            }
        }

        return sb.ToString().TrimEnd();
    }

    private static string ExtractParagraphText(A.Paragraph paragraph)
    {
        var sb = new StringBuilder();

        foreach (var run in paragraph.Elements<A.Run>())
        {
            var text = run.Text?.Text ?? string.Empty;
            if (string.IsNullOrEmpty(text))
            {
                continue;
            }

            var isBold = run.RunProperties?.Bold?.Value == true;
            var isItalic = run.RunProperties?.Italic?.Value == true;

            if (isBold && isItalic)
            {
                sb.Append($"***{text}***");
            }
            else if (isBold)
            {
                sb.Append($"**{text}**");
            }
            else if (isItalic)
            {
                sb.Append($"*{text}*");
            }
            else
            {
                sb.Append(text);
            }
        }

        return sb.ToString();
    }

    private static string ExtractSpeakerNotes(SlidePart slidePart)
    {
        var notesSlidePart = slidePart.NotesSlidePart;
        if (notesSlidePart?.NotesSlide is null)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        var shapes = notesSlidePart.NotesSlide.Descendants<Shape>();

        foreach (var shape in shapes)
        {
            var textBody = shape.TextBody;
            if (textBody is null)
            {
                continue;
            }

            foreach (var paragraph in textBody.Elements<A.Paragraph>())
            {
                var text = string.Concat(paragraph.Elements<A.Run>().Select(r => r.Text?.Text ?? string.Empty));
                if (!string.IsNullOrWhiteSpace(text))
                {
                    sb.Append(text).Append(' ');
                }
            }
        }

        return sb.ToString().Trim();
    }
}
