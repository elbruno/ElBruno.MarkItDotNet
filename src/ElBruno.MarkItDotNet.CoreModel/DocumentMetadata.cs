// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

namespace ElBruno.MarkItDotNet.CoreModel;

/// <summary>
/// Metadata describing a document's origin, authorship, and general properties.
/// </summary>
public record DocumentMetadata
{
    /// <summary>
    /// The document title.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// The document author.
    /// </summary>
    public string? Author { get; init; }

    /// <summary>
    /// Original file format (e.g., "pdf", "docx", "markdown").
    /// </summary>
    public string? SourceFormat { get; init; }

    /// <summary>
    /// When the document was originally created.
    /// </summary>
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>
    /// When the document was last modified.
    /// </summary>
    public DateTimeOffset? ModifiedAt { get; init; }

    /// <summary>
    /// Total number of pages in the original document, if applicable.
    /// </summary>
    public int? PageCount { get; init; }

    /// <summary>
    /// Approximate word count of the document content.
    /// </summary>
    public int? WordCount { get; init; }

    /// <summary>
    /// Extensible dictionary for custom metadata key-value pairs.
    /// </summary>
    public IDictionary<string, object> Custom { get; init; } = new Dictionary<string, object>();
}
