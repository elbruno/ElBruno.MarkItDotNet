// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using ElBruno.MarkItDotNet.CoreModel;

namespace ElBruno.MarkItDotNet.Chunking;

/// <summary>
/// Represents the output of a chunking operation, containing the extracted text content,
/// source references, heading path context, and associated metadata.
/// </summary>
public record ChunkResult
{
    /// <summary>
    /// Gets the unique, deterministic identifier for this chunk.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets the text content of this chunk.
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// Gets the zero-based index of this chunk within the document.
    /// </summary>
    public int Index { get; init; }

    /// <summary>
    /// Gets the source references that identify where this chunk's content originated.
    /// </summary>
    public IReadOnlyList<SourceReference> Sources { get; init; } = [];

    /// <summary>
    /// Gets the heading hierarchy path for this chunk (e.g., "Chapter 1 > Section 1.1").
    /// </summary>
    public string? HeadingPath { get; init; }

    /// <summary>
    /// Gets additional metadata associated with this chunk.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();
}
