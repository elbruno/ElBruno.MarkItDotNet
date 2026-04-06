using ElBruno.MarkItDotNet.CoreModel;

namespace ElBruno.MarkItDotNet.Citations;

/// <summary>
/// Represents the granularity of a citation reference.
/// </summary>
public enum CitationMode
{
    /// <summary>Exact source location is known (block-level or span-level).</summary>
    Exact,

    /// <summary>Only coarse location is available (file or section level).</summary>
    Coarse
}

/// <summary>
/// A single citation pointing to a source location within a document.
/// </summary>
public record CitationReference
{
    /// <summary>Gets the file path of the source document.</summary>
    public string? FilePath { get; init; }

    /// <summary>Gets the page number within the source document.</summary>
    public int? PageNumber { get; init; }

    /// <summary>Gets the section title containing the cited content.</summary>
    public string? SectionTitle { get; init; }

    /// <summary>Gets the full heading path (e.g., "Chapter 1 > Introduction > Overview").</summary>
    public string? HeadingPath { get; init; }

    /// <summary>Gets the unique identifier of the source block.</summary>
    public string? BlockId { get; init; }

    /// <summary>Gets the character span within the source content.</summary>
    public SpanReference? Span { get; init; }

    /// <summary>Gets the citation mode indicating the granularity of the reference.</summary>
    public CitationMode Mode { get; init; } = CitationMode.Exact;
}
