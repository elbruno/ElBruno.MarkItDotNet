// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

namespace ElBruno.MarkItDotNet.CoreModel;

/// <summary>
/// Tracks the origin of a document or block within the source material.
/// </summary>
public record SourceReference
{
    /// <summary>
    /// Path to the source file.
    /// </summary>
    public string? FilePath { get; init; }

    /// <summary>
    /// Page number in the original document (1-based), if applicable.
    /// </summary>
    public int? PageNumber { get; init; }

    /// <summary>
    /// Character span within the source content.
    /// </summary>
    public SpanReference? Span { get; init; }

    /// <summary>
    /// Slash-delimited heading path for hierarchical navigation (e.g., "Chapter 1/Introduction").
    /// </summary>
    public string? HeadingPath { get; init; }
}
