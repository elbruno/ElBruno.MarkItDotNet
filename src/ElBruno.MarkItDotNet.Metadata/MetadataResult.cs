// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

namespace ElBruno.MarkItDotNet.Metadata;

/// <summary>
/// Normalized and enriched metadata extracted from a <see cref="CoreModel.Document"/>.
/// </summary>
public record MetadataResult
{
    /// <summary>The document title, if available.</summary>
    public string? Title { get; init; }

    /// <summary>The document author, if available.</summary>
    public string? Author { get; init; }

    /// <summary>The detected language of the document content.</summary>
    public string? Language { get; init; }

    /// <summary>When the document was originally created.</summary>
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>When the document was last modified.</summary>
    public DateTimeOffset? ModifiedAt { get; init; }

    /// <summary>The inferred document type classification.</summary>
    public DocumentType DocumentType { get; init; } = DocumentType.Unknown;

    /// <summary>Total number of headings found in the document.</summary>
    public int HeadingCount { get; init; }

    /// <summary>Total number of top-level sections in the document.</summary>
    public int SectionCount { get; init; }

    /// <summary>Word count computed from all paragraph blocks.</summary>
    public int WordCount { get; init; }

    /// <summary>Number of pages in the original document, if applicable.</summary>
    public int? PageCount { get; init; }

    /// <summary>Cleaned and normalized heading hierarchy.</summary>
    public IReadOnlyList<NormalizedHeading> NormalizedHeadings { get; init; } = [];

    /// <summary>Extensible tags associated with the document.</summary>
    public IReadOnlyList<string> Tags { get; init; } = [];

    /// <summary>Extensible custom properties.</summary>
    public IReadOnlyDictionary<string, object> Custom { get; init; } = new Dictionary<string, object>();
}
