// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

namespace ElBruno.MarkItDotNet.VectorData;

/// <summary>
/// Represents the standard record shape for vector stores, mapping chunk content
/// and metadata into a flat structure suitable for vector database ingestion.
/// </summary>
public record VectorRecord
{
    /// <summary>
    /// Gets the unique identifier for this record, derived from the chunk ID.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets the text content of the chunk.
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional embedding vector, populated by an embedding step.
    /// </summary>
    public ReadOnlyMemory<float>? Embedding { get; init; }

    /// <summary>
    /// Gets the source document identifier.
    /// </summary>
    public string? DocumentId { get; init; }

    /// <summary>
    /// Gets the document title from metadata.
    /// </summary>
    public string? DocumentTitle { get; init; }

    /// <summary>
    /// Gets the heading hierarchy path for this chunk.
    /// </summary>
    public string? HeadingPath { get; init; }

    /// <summary>
    /// Gets the page number from the chunk's source references.
    /// </summary>
    public int? PageNumber { get; init; }

    /// <summary>
    /// Gets the file path from the chunk's source references.
    /// </summary>
    public string? FilePath { get; init; }

    /// <summary>
    /// Gets the zero-based index of the chunk within the document.
    /// </summary>
    public int ChunkIndex { get; init; }

    /// <summary>
    /// Gets the extensible tags associated with this record.
    /// </summary>
    public IReadOnlyList<string> Tags { get; init; } = [];

    /// <summary>
    /// Gets the extensible metadata properties associated with this record.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();
}
