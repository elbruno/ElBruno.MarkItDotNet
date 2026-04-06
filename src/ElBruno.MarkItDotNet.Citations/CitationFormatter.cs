using System.Text;

namespace ElBruno.MarkItDotNet.Citations;

/// <summary>
/// Formats <see cref="CitationReference"/> instances as human-readable strings.
/// </summary>
public static class CitationFormatter
{
    /// <summary>
    /// Formats a citation with full detail (e.g., "document.pdf, Page 3, Section 'Introduction'").
    /// </summary>
    /// <param name="citation">The citation reference to format.</param>
    /// <returns>A human-readable citation string.</returns>
    public static string Format(CitationReference citation)
    {
        ArgumentNullException.ThrowIfNull(citation);

        var parts = new List<string>();

        if (citation.FilePath is not null)
        {
            parts.Add(citation.FilePath);
        }

        if (citation.PageNumber.HasValue)
        {
            parts.Add($"Page {citation.PageNumber.Value}");
        }

        if (citation.SectionTitle is not null)
        {
            parts.Add($"Section '{citation.SectionTitle}'");
        }

        if (citation.HeadingPath is not null && citation.SectionTitle is null)
        {
            parts.Add($"Section '{citation.HeadingPath}'");
        }

        if (citation.BlockId is not null)
        {
            parts.Add($"Block {citation.BlockId}");
        }

        return parts.Count > 0 ? string.Join(", ", parts) : "Unknown source";
    }

    /// <summary>
    /// Formats a citation in short form (e.g., "document.pdf p.3").
    /// </summary>
    /// <param name="citation">The citation reference to format.</param>
    /// <returns>A short citation string.</returns>
    public static string FormatShort(CitationReference citation)
    {
        ArgumentNullException.ThrowIfNull(citation);

        var sb = new StringBuilder();

        if (citation.FilePath is not null)
        {
            sb.Append(citation.FilePath);
        }

        if (citation.PageNumber.HasValue)
        {
            if (sb.Length > 0)
            {
                sb.Append(' ');
            }

            sb.Append($"p.{citation.PageNumber.Value}");
        }

        if (citation.SectionTitle is not null)
        {
            if (sb.Length > 0)
            {
                sb.Append(' ');
            }

            sb.Append(citation.SectionTitle);
        }

        return sb.Length > 0 ? sb.ToString() : "Unknown";
    }

    /// <summary>
    /// Formats a citation as a Markdown link (e.g., "[document.pdf, p.3](#page-3)").
    /// </summary>
    /// <param name="citation">The citation reference to format.</param>
    /// <returns>A Markdown-formatted citation string.</returns>
    public static string FormatMarkdown(CitationReference citation)
    {
        ArgumentNullException.ThrowIfNull(citation);

        var label = FormatShort(citation);
        var anchor = BuildAnchor(citation);

        return $"[{label}]({anchor})";
    }

    private static string BuildAnchor(CitationReference citation)
    {
        if (citation.PageNumber.HasValue)
        {
            return $"#page-{citation.PageNumber.Value}";
        }

        if (citation.SectionTitle is not null)
        {
            var slug = citation.SectionTitle.ToLowerInvariant().Replace(' ', '-');
            return $"#{slug}";
        }

        if (citation.HeadingPath is not null)
        {
            var lastHeading = citation.HeadingPath.Split(" > ").Last();
            var slug = lastHeading.ToLowerInvariant().Replace(' ', '-');
            return $"#{slug}";
        }

        if (citation.BlockId is not null)
        {
            return $"#block-{citation.BlockId}";
        }

        return "#";
    }
}
